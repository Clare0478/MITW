using FHIR_Demo.Models;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;
using LinqKit;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;//NRE
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;//NEW
using System.Net.Http;//NEW
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks; //NEW
using System.Web;
using System.Web.Mvc;


namespace FHIR_Demo.Controllers
{
    public class HomeController : Controller
    {
        private CookiesController cookies = new CookiesController();
        private HttpClientEventHandler handler = new HttpClientEventHandler();

        public ActionResult Index()
        {
            //預設Cookie值
            cookies.Init_Cookie(HttpContext);

            //設定Server選單
            var Server_selectList = new List<SelectListItem>()
            {
                new SelectListItem {Text="IBM", Value="IBM" },
                new SelectListItem {Text="HAPi", Value="HAPi" },
            };
            //預設選擇哪一筆
            Server_selectList.Where(q => q.Value == cookies.FHIR_Server_Cookie(HttpContext)).First().Selected = true;
            ViewBag.Server_selectList = Server_selectList;

            return View();
        }

        //index2
        public ActionResult Index2()
        {
            return View();
        }

        //初始
        public async Task<ActionResult> Index2_3()
        {

            //var Getomi_json = "'entry': [{'fullUrl': 'http://10.40.8.45:8080/fhir/Patient/C04DA5FB362ACBE0D8B8E889364A10C9DA0E6E76'} ]";
            var Getomi_json = await Get_MultipleSearch_3();
            ViewBag.getjson = Getomi_json;
            return Json(Getomi_json);

        }

        //Index2_3
        [HttpGet]
        public async Task<string> Get_MultipleSearch_3()
        {
            //a = Request.Form["sendAlltext"];
            var url = ConfigurationManager.AppSettings.Get("FHIRAPI"); //改FHIRAPI
            //var Authorization = ConfigurationManager.AppSettings.Get("Authorization");

            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;//憑證一定要通過
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;//版本
            HttpClient client = new HttpClient(); //請求
                                                  //client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", Authorization);
            var Username = ConfigurationManager.AppSettings.Get("Username");
            var Password = ConfigurationManager.AppSettings.Get("Password");
            //var response = await client.GetAsync(url);
            var byteArray = Encoding.ASCII.GetBytes($"{Username}:{Password}");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            var response = await client.GetAsync(url);
            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)/*回傳500*/
            {
                return "500";
            }
            else
            {
                var result = response.Content.ReadAsStringAsync().Result;
                return result;
            }

            //var result = response.Content.ReadAsStringAsync().Result;
            //return result;

        }

        [HttpGet]
        public async Task<string> Get_MultipleSearch(string sendalltext)
        {
            //a = Request.Form["sendAlltext"];
            var url = ConfigurationManager.AppSettings.Get("FHIRAPI") + "/" + sendalltext; //改FHIRAPI
            //var Authorization = ConfigurationManager.AppSettings.Get("Authorization");

            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;//憑證一定要通過
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;//版本
            HttpClient client = new HttpClient(); //請求
                                                  //client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", Authorization);
            var Username = ConfigurationManager.AppSettings.Get("Username");
            var Password = ConfigurationManager.AppSettings.Get("Password");
            //var response = await client.GetAsync(url);
            var byteArray = Encoding.ASCII.GetBytes($"{Username}:{Password}");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            var response = await client.GetAsync(url);
            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)/*回傳500*/
            {
                return "500";
            }
            else
            {
                var result = response.Content.ReadAsStringAsync().Result;
                return result;
            }

            //var result = response.Content.ReadAsStringAsync().Result;
            //return result;

        }

        [HttpPost]
        public async Task<ActionResult> Index2(string sendalltext)
        {
            //var Getomi_json = "'entry': [{'fullUrl': 'http://10.40.8.45:8080/fhir/Patient/C04DA5FB362ACBE0D8B8E889364A10C9DA0E6E76'} ]";
            var Getomi_json = await Get_MultipleSearch(sendalltext);
            ViewBag.getjson = Getomi_json;
            if (Getomi_json == "500")
            {
                return Json(500);
            }
            else
            {
                return Json(Getomi_json);
            }

        }


        #region Datatable 表單功能
        private string FHIR_url;
        private string FHIR_Server;
        private string FHIR_token;
        private string FHIR_Resource;
        private string SearchParameter_query;

        public dynamic GetResource(DataTableAjaxPostModel model, string server, string url, string token, string resource , string search = null)
        {
            FHIR_url = url;
            FHIR_Server = server;
            FHIR_token = token;
            FHIR_Resource = resource;
            SearchParameter_query = search;

            // action inside a standard controller
            int filteredResultsCount;
            int totalResultsCount;
            var res = YourCustomSearchFunc(model, out filteredResultsCount, out totalResultsCount);

            var result = new List<dynamic>(res.Count);
            foreach (var s in res)
            {
                // simple remapping adding extra info to found dataset
                result.Add(s);
            };

            return JsonConvert.SerializeObject(Json(new
            {
                // this is what datatables wants sending back
                draw = model.draw,
                recordsTotal = totalResultsCount,
                recordsFiltered = filteredResultsCount,
                data = result
            }).Data);
            
        }

        public IList<dynamic> YourCustomSearchFunc(DataTableAjaxPostModel model, out int filteredResultsCount, out int totalResultsCount)
        {
            var searchBy = (model.search != null) ? model.search.value : null;
            //取得幾筆資料
            var take = model.length;
            //第幾筆開始
            var skip = model.start;
            //第幾頁
            var page = model.start / model.length + 1;
            string sortBy = "";
            bool sortDir = true;

            if (model.order != null)
            {
                // in this example we just default sort on the 1st column
                sortBy = model.columns[model.order[0].column].name;
                sortDir = model.order[0].dir.ToLower() == "asc";
            }

            // search the dbase taking into consideration table sorting and paging
            var result = GetDataFromDbase(searchBy, take, skip, page, sortBy, sortDir, out filteredResultsCount, out totalResultsCount);
            if (result == null)
            {
                // empty collection...
                return new List<dynamic>();
            }
            return result;
        }

        public List<dynamic> GetDataFromDbase(string searchBy, int take, int skip, int page, string sortBy, bool sortDir, out int filteredResultsCount, out int totalResultsCount)
        {
            //修改searchparameter
            sortBy = FHIRSearchParameters_Chagne(DatatablesObjectDisplay_Change(sortBy));
            var q = new SearchParams();
            if (SearchParameter_query != null)
                q = SearchParams.FromUriParamList(UriParamList.FromQueryString(SearchParameter_query));
            q.Where("_total=accurate") //顯示總比數
                .LimitTo(take); //抓取幾筆資料

            if (String.IsNullOrEmpty(searchBy))
            {
                if (String.IsNullOrEmpty(sortBy)) 
                {
                    // if we have an empty search then just order the results by Id ascending
                    sortBy = "_id";
                    sortDir = true;
                }
            }
            else
                q.Where("_content=" + searchBy ?? "");

            q.OrderBy(sortBy, (sortDir == true) ? SortOrder.Ascending : SortOrder.Descending);

            //讓系統通過對於不安全的https連線
            handler.ServerCertificateCustomValidationCallback += (sender2, cert, chain, sslPolicyErrors) => true;
            if (cookies.FHIR_Server_Cookie(HttpContext, FHIR_Server) == "IBM")
            {
                //換頁//重要
                q.Where("_page=" + page);

                //使用Basic 登入
                handler.OnBeforeRequest += (sender, e) =>
                {
                    e.RawRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(cookies.FHIR_Token_Cookie(HttpContext, FHIR_token))));
                };
            }
            else 
            {
                //換頁
                q.Where("_getpagesoffset=" + skip);

                //使用Bearer 登入
                handler.OnBeforeRequest += (sender, e) =>
                {
                    e.RawRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", cookies.FHIR_Token_Cookie(HttpContext, FHIR_token));
                };
            }

            FhirClient client = new FhirClient(cookies.FHIR_URL_Cookie(HttpContext, FHIR_url), cookies.settings, handler);

            //查看數值使用變數
            var a = q.ToParameters();

            Bundle PatientSearchBundle = client.Search(q, FHIR_Resource);

            Bundle PatientBundle_total = client.Search(new SearchParams().Where("_total=accurate"), FHIR_Resource);

            List<dynamic> Patients = new List<dynamic>();

            foreach (var entry in PatientSearchBundle.Entry)
            {
                Patients.Add((entry.Resource).ToJObject());
            }

            filteredResultsCount = PatientSearchBundle.Total ?? 0;

            totalResultsCount = PatientBundle_total.Total ?? 0;

            return Patients;
        }
        #endregion Datatable 表單功能

        /// <summary>
        /// 更改Search搜尋字串關鍵字
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public string FHIRSearchParameters_Chagne(string parameter) 
        {
            parameter = parameter.ToLower();
            switch (parameter) 
            {
                case "id":
                    parameter = "_id";
                    break;
                case "lastupdated":
                    parameter = "_lastUpdated";
                    break;
                case "tag":
                    parameter = "_tag";
                    break;
                case "profile":
                    parameter = "_profile";
                    break;
                case "security":
                    parameter = "_security";
                    break;
                case "text":
                    parameter = "_text";
                    break;
                case "content":
                    parameter = "_content";
                    break;
                case "list":
                    parameter = "_list";
                    break;
                case "has":
                    parameter = "_has";
                    break;
                case "type":
                    parameter = "_type";
                    break;
                case "query":
                    parameter = "_query";
                    break;
            }
            return parameter;
        }

        public string DatatablesObjectDisplay_Change (string orderName) 
        {
            string Pattern = @"\[, ].+";
            Regex regex = new Regex(Pattern);
            return regex.Replace(orderName, "")??"";
        }
    }
}