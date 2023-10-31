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

        private FHIRResourceModel db = new FHIRResourceModel();
        public ActionResult Index()
        {
            //if (Session["UserName"] == "" || Session["UserName"] == null)
            //{
            //    TempData["message"] = "尚未登入，請登入後再做連線";
            //    return RedirectToAction("Login","Account");
            //}

            // 從 Session 中取得 token 值
            //if (Session["Token"] != null || Session["Token"] != "")
            //{
            //    ViewBag.token = Session["Token"] as string;
            //    ViewBag.fhirServerUrl = Session["FhirServerUrl"] as string;
            //}

            //預設Cookie值
            cookies.Init_Cookie(HttpContext);
            //if(isLoggedIn == false)
            //{
            //    TempData["message"] = "未登入無法使用連線!";
            //}

            //設定Server選單
            var Server_selectList = new List<SelectListItem>()
            {
                new SelectListItem {Text="IBM", Value="IBM" },
                new SelectListItem {Text="HAPi", Value="HAPi" },
                new SelectListItem {Text="Smart on Fhir", Value="Smart" },
            };
            //預設選擇哪一筆
            Server_selectList.Where(q => q.Value == cookies.FHIR_Server_Cookie(HttpContext)).First().Selected = true;
            ViewBag.Server_selectList = Server_selectList;

            //下拉ResourceType
            var resourceTypes = db.ResourceInfos.Select(r => r.ResourceType).Distinct().ToList();
            if (resourceTypes.Contains("Patient"))
            {
                resourceTypes.Remove("Patient");
                resourceTypes.Insert(0, "Patient");
            }
            //ViewBag.SelectedResourceType = "Patient";
            ViewBag.ResourceTypes = resourceTypes;

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
        
        private string FHIR_Server;
        private string FHIR_token;
        private string FHIR_url;
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

        //尋找所有種類下面的必要欄位顯示
        public List<string> GetRequiredColumns(string resourcetype)
        {
            List<string> requiredcolumnsData = new List<string>();

            List<resourceinfo> resourceinfos = db.ResourceInfos
                .Where(c => c.ResourceType == resourcetype)
                .ToList();

            foreach(var info in resourceinfos)
            {
                //尋找開頭為1的欄位且不為第二層的欄位
                if (info.Card.StartsWith("1") && !info.Name.Contains('.') && !info.Card.Contains('*'))
                {
                    requiredcolumnsData.Add(info.Name);
                }
            }
            return requiredcolumnsData;
        }

        public JsonResult GetRequiredColumnsForData(string resourceType)
        {
            List<string> requiredColumns = GetRequiredColumns(resourceType);
            return Json(requiredColumns, JsonRequestBehavior.AllowGet);
        }

        //尋找每種ResourceType的進階搜尋欄位
        public JsonResult GetSearchColumns(string resourcetype)
        {
            List<searchparameters> searchinfos = db.searchparameters
                .Where(c => c.ResourceType == resourcetype)
                .ToList();

            Dictionary<string, dynamic> paramterJson = new Dictionary<string, dynamic>
            {
                {"resource", resourcetype },
                {"searchParameters",new List<Dictionary<string, dynamic>>()}
            };
            // 先加入第一筆資料
            var firstSearchParameter = new Dictionary<string, dynamic>
            {
                { "ch_Name", "id" },
                { "parameter_name", "_id" },
                { "selected", false },
                { "input_content", new Dictionary<string, dynamic> { { "type", "text" }, { "value", "" } } },
                { "switched", true }
            };
            paramterJson["searchParameters"].Add(firstSearchParameter);

            if (resourcetype == "Composition")
            {
                var AddSearchParameter = new Dictionary<string, dynamic>
                {
                    { "ch_Name", "Patient_Name" },
                    { "parameter_name", "_patient.name" },
                    { "selected", false },
                    { "input_content", new Dictionary<string, dynamic> { { "type", "text" }, { "value", "" } } },
                    { "switched", true }
                };
                paramterJson["searchParameters"].Add(AddSearchParameter);

                var AddSearchParameter2 = new Dictionary<string, dynamic>
                {
                    { "ch_Name", "Patient_Identifier" },
                    { "parameter_name", "_patient.identifier" },
                    { "selected", false },
                    { "input_content", new Dictionary<string, dynamic> { { "type", "text" }, { "value", "" } } },
                    { "switched", true }
                };
                paramterJson["searchParameters"].Add(AddSearchParameter2);

                var AddSearchParameter3 = new Dictionary<string, dynamic>
                {
                    { "ch_Name", "Organization" },
                    { "parameter_name", "_organization.id" },
                    { "selected", false },
                    { "input_content", new Dictionary<string, dynamic> { { "type", "text" }, { "value", "" } } },
                    { "switched", true }
                };
                paramterJson["searchParameters"].Add(AddSearchParameter3);
            }
            else if (resourcetype == "Bundle")
            {
                var AddSearchParameter = new Dictionary<string, dynamic>
                {
                    { "ch_Name", "Patient_ID" },
                    { "parameter_name", "_patient.id" },
                    { "selected", false },
                    { "input_content", new Dictionary<string, dynamic> { { "type", "text" }, { "value", "" } } },
                    { "switched", true }
                };
                var AddSearchParameter2 = new Dictionary<string, dynamic>
                {
                    { "ch_Name", "Date" },
                    { "parameter_name", "_date" },
                    { "selected", false },
                    { "input_content", new Dictionary<string, dynamic>{
                        { "type","date"},
                        { "key", new dynamic[]{
                            new Dictionary<string, dynamic>
                            {
                                {"oncheck", true },
                                {"input_content",new Dictionary<string, dynamic>
                                    {
                                        {"type","select"},
                                        {"value_list", new[]
                                            {
                                                new Dictionary<string, dynamic> { { "key", "等於(=)" }, { "value", "eq" }, { "status", false } },
                                                new Dictionary<string, dynamic> { { "key", "大於(>)" }, { "value", "gt" }, { "status", false } },
                                                new Dictionary<string, dynamic> { { "key", "大於或等於(>=)" }, { "value", "ge" }, { "status", true } },
                                                new Dictionary<string, dynamic> { { "key", "小於(<)" }, { "value", "lt" }, { "status", false } },
                                                new Dictionary<string, dynamic> { { "key", "小於或等於(<=)" }, { "value", "le" }, { "status", false } }
                                            }
                                        },
                                        {"value",""}
                                    }
                                }
                            },
                            new  Dictionary<string, dynamic>
                            {
                                { "oncheck", true },
                                { "input_content", new Dictionary<string, dynamic>
                                    {
                                        { "type", "select" },
                                        { "value_list", new[]
                                            {
                                                new Dictionary<string, dynamic> { { "key", "等於(=)" }, { "value", "eq" }, { "status", false } },
                                                new Dictionary<string, dynamic> { { "key", "大於(>)" }, { "value", "gt" }, { "status", false } },
                                                new Dictionary<string, dynamic> { { "key", "大於或等於(>=)" }, { "value", "ge" }, { "status", false } },
                                                new Dictionary<string, dynamic> { { "key", "小於(<)" }, { "value", "lt" }, { "status", false } },
                                                new Dictionary<string, dynamic> { { "key", "小於或等於(<=)" }, { "value", "le" }, { "status", true } }
                                            } },
                                        { "value", "" }
                                    }
                                }
                            }
                        } }
                        }
                    },
                    { "switched", true }
                };
                var AddSearchParameter3 = new Dictionary<string, dynamic>
                {
                    { "ch_Name", "Organization" },
                    { "parameter_name", "_organization.id" },
                    { "selected", false },
                    { "input_content", new Dictionary<string, dynamic> { { "type", "text" }, { "value", "" } } },
                    { "switched", true }
                };
                paramterJson["searchParameters"].Add(AddSearchParameter2);
                paramterJson["searchParameters"].Add(AddSearchParameter3);
                paramterJson["searchParameters"].Add(AddSearchParameter);
            }

            foreach (var info in searchinfos)
            {
                //處理Name
                if (info.Name.Contains("TU"))
                {
                    info.Name = info.Name.Replace("TU", "");
                }
                if (info.Name.Contains("-"))
                {
                    continue;
                }

                //reference跳過處理(資料格式不一難取)
                if (info.Type == "reference" && info.Name != "subject" && info.Name != "organization")
                {
                    continue;
                }


                var searchParameter = new Dictionary<string, dynamic>
                {
                    { "ch_Name", info.Name},
                    { "parameter_name", info.Name},
                    { "selected", false },
                    { "input_content", new Dictionary<string, dynamic>()}
                };

                //處理type
                if (info.Name == "gender")
                {
                    searchParameter["input_content"] = new Dictionary<string, dynamic>
                    {
                        {"type","radio" },
                        {"value_list", new dynamic[]{ 
                            new Dictionary<string,string>{{"key","男生"},{"value","male" } },
                            new Dictionary<string,string>{{"key","女生"},{"value","female" } }
                        } },
                        { "value",""}
                    };
                }
                else if(info.Type == "date")
                {
                    searchParameter["input_content"] = new Dictionary<string, dynamic>
                    {
                        { "type","date"},
                        { "key", new dynamic[]{
                            new Dictionary<string, dynamic>
                            {
                                {"oncheck", true },
                                {"input_content",new Dictionary<string, dynamic>
                                    {
                                        {"type","select"},
                                        {"value_list", new[]
                                            {
                                                new Dictionary<string, dynamic> { { "key", "等於(=)" }, { "value", "eq" }, { "status", false } },
                                                new Dictionary<string, dynamic> { { "key", "大於(>)" }, { "value", "gt" }, { "status", false } },
                                                new Dictionary<string, dynamic> { { "key", "大於或等於(>=)" }, { "value", "ge" }, { "status", true } },
                                                new Dictionary<string, dynamic> { { "key", "小於(<)" }, { "value", "lt" }, { "status", false } },
                                                new Dictionary<string, dynamic> { { "key", "小於或等於(<=)" }, { "value", "le" }, { "status", false } }
                                            } 
                                        },
                                        {"value",""}
                                    } 
                                }
                            },
                            new  Dictionary<string, dynamic>
                            {
                                { "oncheck", true },
                                { "input_content", new Dictionary<string, dynamic>
                                    {
                                        { "type", "select" },
                                        { "value_list", new[]
                                            {
                                                new Dictionary<string, dynamic> { { "key", "等於(=)" }, { "value", "eq" }, { "status", false } },
                                                new Dictionary<string, dynamic> { { "key", "大於(>)" }, { "value", "gt" }, { "status", false } },
                                                new Dictionary<string, dynamic> { { "key", "大於或等於(>=)" }, { "value", "ge" }, { "status", false } },
                                                new Dictionary<string, dynamic> { { "key", "小於(<)" }, { "value", "lt" }, { "status", false } },
                                                new Dictionary<string, dynamic> { { "key", "小於或等於(<=)" }, { "value", "le" }, { "status", true } }
                                            } },
                                        { "value", "" }
                                    }
                                }
                            }
                        } }
                    };
                }
                else
                {
                    searchParameter["input_content"] = new Dictionary<string, string>
                    {
                        { "type", "text" },
                        { "value", "" }
                    };
                }
                searchParameter["switched"] = true;
                paramterJson["searchParameters"].Add(searchParameter);
            }


            return Json(paramterJson, JsonRequestBehavior.AllowGet);
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

            Bundle PatientSearchBundle = client.Search(q, FHIR_Resource);//返回符合搜尋條件的資源(以Bundle)

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

        public string DatatablesObjectDisplay_Change (string orderName) //使用正則表達替換移除字符串中的特定部分[,]
        {
            string Pattern = @"\[, ].+";
            Regex regex = new Regex(Pattern);
            return regex.Replace(orderName, "")??"";
        }
    }
}