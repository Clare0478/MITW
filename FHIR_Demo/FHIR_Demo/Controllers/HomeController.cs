using FHIR_Demo.Models;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;
using LinqKit;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
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
            cookies.Init_Cookie(HttpContext);
            return View();
        }

        #region Datatable 表單功能
        private string FHIR_url;
        private string FHIR_token;
        private string FHIR_Resource;
        private string SearchParameter_query;

        public dynamic GetResource(DataTableAjaxPostModel model, string url, string token, string resource , string search = null)
        {
            FHIR_url = url;
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
            var take = model.length;
            var skip = model.start;

            string sortBy = "";
            bool sortDir = true;

            if (model.order != null)
            {
                // in this example we just default sort on the 1st column
                sortBy = model.columns[model.order[0].column].name;
                sortDir = model.order[0].dir.ToLower() == "asc";
            }

            // search the dbase taking into consideration table sorting and paging
            var result = GetDataFromDbase(searchBy, take, skip, model.draw, sortBy, sortDir, out filteredResultsCount, out totalResultsCount);
            if (result == null)
            {
                // empty collection...
                return new List<dynamic>();
            }
            return result;
        }

        public List<dynamic> GetDataFromDbase(string searchBy, int take, int skip, int page, string sortBy, bool sortDir, out int filteredResultsCount, out int totalResultsCount)
        {
            // the example datatable used is not supporting multi column ordering
            // so we only need get the column order from the first column passed to us.        
            //var whereClause = BuildDynamicWhereClause(Db, searchBy);

            //修改searchparameter
            sortBy = FHIRSearchParameters_Chagne(DatatablesObjectDisplay_Change(sortBy));
            var q = new SearchParams();
            if (SearchParameter_query != null)
                q = SearchParams.FromUriParamList(UriParamList.FromQueryString(SearchParameter_query));
            q.Where("_total=accurate") //顯示總比數
                .LimitTo(take) //抓取幾筆資料
                //.Where("_getpagesoffset="+ skip); //略過幾筆資料 IBM不能使用
                .Where("_page=" + page);

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

            

            handler.OnBeforeRequest += (sender, e) =>
            {
                //e.RawRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", cookies.FHIR_Token_Cookie(HttpContext, FHIR_token));
                e.RawRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(cookies.FHIR_Token_Cookie(HttpContext, FHIR_token))));
            };
            FhirClient client = new FhirClient(cookies.FHIR_URL_Cookie(HttpContext, FHIR_url), cookies.settings, handler);

            ////查看數值使用變數
            var a = q.ToParameters();

            Bundle PatientSearchBundle = client.Search(q, FHIR_Resource);

            Bundle PatientBundle_total = client.Search(new SearchParams().Where("_total=accurate"), FHIR_Resource);

            //var json = PatientSearchBundle.ToJson();
            //List<PatientViewModel> patientViewModels = new List<PatientViewModel>();
            List<dynamic> Patients = new List<dynamic>();

            foreach (var entry in PatientSearchBundle.Entry)
            {
                //patientViewModels.Add(new PatientViewModel().PatientViewModelMapping((Patient)entry.Resource));

                Patients.Add((entry.Resource).ToJObject());
            }

            // now just get the count of items (without the skip and take) - eg how many could be returned with filtering
            //filteredResultsCount = Db.DatabaseTableEntity.AsExpandable().Where(whereClause).Count();
            filteredResultsCount = PatientSearchBundle.Total ?? 0;

            totalResultsCount = PatientBundle_total.Total ?? 0;

            return Patients;
        }
        #endregion Datatable 表單功能

        //更改Search搜尋字串
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