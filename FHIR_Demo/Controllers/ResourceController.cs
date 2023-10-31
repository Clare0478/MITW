using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Mvc;
using FHIR_Demo.Models;

namespace FHIR_Demo.Controllers
{
    public class ResourceController : Controller
    {
        private CookiesController cookies = new CookiesController();
        private HttpClientEventHandler handler = new HttpClientEventHandler();
        string entry_type = "";
        string entry_id = "";

        //爬蟲
        private FHIRResourceModel db = new FHIRResourceModel();
        public ActionResult Index(string res, string id, string sele, string isbundle)
        {
            string[] Resource = new string[] { "Patient", "Encounter", "Observation", "MedicationRequest","Composition", "Procedure", "Condition", "DiagnosticReport", "ServiceRequest" };
           
            //if (Resource.Contains(res) && id != null && Request.Cookies["FHIR_url"] != null)
            if (id != null && Request.Cookies["FHIR_url"] != null)
                {
                    //try
                    //{
                    //連結FHIR Server
                    //讓系統通過對於不安全的https連線
                    handler.ServerCertificateCustomValidationCallback += (sender2, cert, chain, sslPolicyErrors) => true;
                    if (cookies.FHIR_Server_Cookie(HttpContext) == "IBM")
                    {
                        //使用Basic 登入
                        handler.OnBeforeRequest += (sender, e) =>
                        {
                            e.RawRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(cookies.FHIR_Token_Cookie(HttpContext))));
                        };
                    }
                    else
                    {
                        //使用Bearer 登入
                        handler.OnBeforeRequest += (sender, e) =>
                        {
                            e.RawRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", cookies.FHIR_Token_Cookie(HttpContext));
                        };
                    }
                    FhirClient client = new FhirClient(cookies.FHIR_URL_Cookie(HttpContext), cookies.settings, handler);

                    //爬蟲資料庫抓到該類型
                    List<resourceinfo> resourceInfos = db.ResourceInfos
                        .Where(r => r.ResourceType == res)
                        .ToList();

                    //*****抓到該選取的該筆類型資料顯示右方(Res_Search)*****
                    string patient_id = "";
                    string composition_id = "";
                    string encounter_id = "";
                    string selerestype = "";
                    string resourcetype = "";//判斷resourcetype的變數

                    //加入isbundle的判斷，讓前端知道我現在的isbundle
                    ViewBag.isbundle = isbundle;
                    ViewBag.ResourceType = res;
                    ViewBag.ResourceId = id;
                    resourcetype = res;
                    selerestype = sele;

                    #region Search_Resource 資料
                    Bundle Res_Bundle = client.SearchById(res, id);
                    List<dynamic> Res_Search = new List<dynamic>();

                    List<dynamic> Bundle_reosurces = new List<dynamic>();
                    foreach (var entry in Res_Bundle.Entry)
                    {
                        var entry_res = (entry.Resource).ToJObject();
                        Res_Search.Add(entry_res);
                        if (res == "Patient")
                        {
                            patient_id = id;
                        }
                        else if (res == "Bundle")
                        {
                            ViewBag.isbundle = "isBundle";
                            Session["composition_id"] = "";

                            var entryvalue = entry_res.GetValue("entry");
                            var meta = entry_res.GetValue("meta");
                            foreach (var entryresource in entryvalue)
                            {
                                var resource_item = entryresource["resource"];
                                if (entryresource["fullUrl"]?.ToString() != null && entryresource["fullUrl"].ToString().StartsWith("urn:uuid:"))
                                {
                                    entryresource["fullUrl"] = entryresource["fullUrl"].ToString().Replace("urn:uuid:", "");
                                    resource_item["id"] = entryresource["fullUrl"].ToString();
                                }
                                if(resource_item != null)
                                {
                                    resource_item["lastUpdated"] = meta["lastUpdated"]?.ToString();
                                    Bundle_reosurces.Add(resource_item);
                                }
                            }
                            Session["bundle_id"] = Bundle_reosurces;
                        }
                        else if (res == "Composition" && ViewBag.isbundle != "isBundle")
                        {
                            composition_id = id;
                            ViewBag.isbundle = "isComposition";
                            Session["composition_id"] = composition_id;
                            Session["bundle_id"] = "";
                        }
                        else if (entry_res["subject"] != null)
                        {
                            patient_id = (entry.Resource).ToJObject()["subject"]["reference"].ToString().Split('/')[1];
                        }

                        if (ViewBag.isbundle == "null")
                        {
                            Session["composition_id"] = "";
                            Session["bundle_id"] = "";
                        }
                        Dictionary<string,object> formattedData = ProcessData(entry_res, resourceInfos);
                        ViewBag.DictionaryData = formattedData;
                    }
                    ViewBag.Res_Search = Res_Search; //右邊的細項 
                    #endregion Resource_detail 所有相關資料


                    #region Resource_detail 資料
                    var res_detail_query = new SearchParams();

                    if (res == "Encounter")
                    {
                        res_detail_query.Where("_id=" + id)
                                .Where("_revinclude=ServiceRequest:encounter")
                                .Where("_revinclude=Observation:encounter")
                                .Where("_revinclude=MedicationRequest:encounter")
                                .Where("_revinclude=Composition:encounter")
                                .Where("_revinclude=Procedure:encounter")
                                .Where("_revinclude=Condition:encounter")
                                .Where("_revinclude=DiagnosticReport:encounter")
                                .Where("_total=accurate"); //顯示總比數
                                                           //.Where("_include=Encounter:practitioner")
                    }
                    else if (res == "Observation")
                    {
                        res_detail_query.Where("patient=" + patient_id);
                        //取的檢驗檢查code
                        if (Res_Search?[0].code?.coding != null)
                        {
                            if (Res_Search[0].code.coding.Count > 0)
                            {
                                res_detail_query.Where($"code={Res_Search[0].code.coding[0].code}");
                            }
                            else
                            {
                                res_detail_query.Where($"code:text={Res_Search[0].code.text}");
                            }
                        }
                        else
                        {
                            res_detail_query.Where($"code:text={Res_Search[0].code.text}");
                        }

                        res_detail_query.Where("_count=200").Where("_total=accurate").OrderBy("date", SortOrder.Ascending);
                    }

                    //*****抓到該Bundle的所有Entry資料(左邊資料 Resource_detail)****** 
                    Bundle Res_detail_Bundle = client.Search(res_detail_query, res);
                    List<dynamic> Res_detail_list = new List<dynamic>();

                    foreach (var entry in Res_detail_Bundle.Entry)
                    {
                        var entry_res = (entry.Resource).ToJObject();
                        Res_detail_list.Add(entry_res);
                        /*if (res == "Patient")
                        {
                            patient_id = id;
                        }
                        else if (entry_res["subject"] != null)
                        {
                            patient_id = entry_res["subject"]["reference"].ToString().Split('/')[1];
                        }*/
                    }

                    ViewBag.Resource_detail = Res_detail_list; //資料都全送但前端只有用obser
                    #endregion Resource_detail 所有相關資料

                    //*****抓到該Patient_id的所有相關資料(Resources)*******
                    #region Patient 所有相關資料
                    List<dynamic> Patient_Search_reosurces = new List<dynamic>();
                    ViewBag.Resources = null;
                    if (ViewBag.isbundle == "null" && Res_Bundle.Entry.Count > 0 && patient_id != "")
                    {
                        var query = new SearchParams()
                        .Where("_id=" + patient_id)
                        .Where("_revinclude=Encounter:subject")
                        .Where("_revinclude=Observation:subject")
                        .Where("_revinclude=MedicationRequest:subject")
                        .Where("_revinclude=Composition:subject")
                        .Where("_revinclude=Procedure:subject")
                        .Where("_revinclude=Condition:subject")
                        .Where("_revinclude=DiagnosticReport:subject")
                        .Where("_total=accurate");//顯示總比數
                        var a = query.ToParameters();
                        try
                        {
                            //抓取該Patient_id的所有類型資料
                            Bundle Patient_Bundle = client.Search(query, "Patient");
                            foreach (var entry in Patient_Bundle.Entry)
                            {
                                Patient_Search_reosurces.Add((entry.Resource).ToJObject());
                            }
                        }
                        catch (Exception ex)
                        {
                            TempData["message"] = "資料太大伺服器錯誤" + "\n" + ex.Message;
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else if (ViewBag.isbundle == "isBundle" && Res_Bundle.Entry.Count > 0 && Session["bundle_id"] != "")
                    {
                        //Res_Bundle = client.SearchById("Bundle", (string)Session["bundle_id"]);
                        Patient_Search_reosurces = (List<dynamic>)Session["bundle_id"];
                    }
                    //Composition
                    else if (ViewBag.isbundle == "isComposition" && Res_Bundle.Entry.Count > 0 && Session["composition_id"] != "")
                    {
                        try
                        {
                            //Session.composition_id = "";
                            //抓取當筆id資料(為了取出所有連結)
                            var x = (string)Session["composition_id"];
                            Bundle Composition_Bundle = client.SearchById("Composition", (string)Session["composition_id"]);
                            List<dynamic> Composition_Search = new List<dynamic>();

                            foreach (var entry in Composition_Bundle.Entry)
                            {
                                //取Composition、subject、encounter的個別資料
                                List<KeyValuePair<string, string>> queryParameters = new List<KeyValuePair<string, string>>()
                                {
                                    new KeyValuePair<string, string>("Composition", "_id=" + (string)Session["composition_id"])
                                };
                                if ((entry.Resource).ToJObject()["subject"] != null)
                                {
                                    patient_id = (entry.Resource).ToJObject()["subject"]["reference"].ToString().Split('/')[1];
                                    queryParameters.Add(new KeyValuePair<string, string>("Patient", "_id=" + patient_id));
                                    
                                }
                                if((entry.Resource).ToJObject()["encounter"] != null)
                                {
                                    encounter_id = (entry.Resource).ToJObject()["encounter"]["reference"].ToString().Split('/')[1];
                                    queryParameters.Add(new KeyValuePair<string, string>("Encounter", "_id=" + encounter_id));
                                }

                                
                                if((entry.Resource).ToJObject()["section"] != null)
                                {   
                                    //section裡面所有層中的連接資料
                                    ExtractEntryReferences((entry.Resource).ToJObject()["section"], queryParameters);
                                }
                                
                                //所有類型和id取出做查詢
                                foreach (var kvp in queryParameters)
                                {
                                    string type = kvp.Key;
                                    string ids = kvp.Value;

                                    var query = new SearchParams().Where(ids);
                                    Bundle resultBundle = client.Search(query, type);

                                    foreach (var res_entry in resultBundle.Entry)
                                    {
                                        Patient_Search_reosurces.Add((res_entry.Resource).ToJObject());
                                    }
                                }
                                
                            }
                        }
                        catch
                        {
                            TempData["message"] = "資料太大伺服器錯誤";
                            return RedirectToAction("Index", "Home");
                        }
                     }
                    else
                    {
                        foreach (var entry in Res_Bundle.Entry)
                        {
                            Patient_Search_reosurces.Add((entry.Resource).ToJObject());
                        }

                    }
                    #endregion

                    ViewBag.Resources = Patient_Search_reosurces; //左圖的
                    // 將 ViewBag.Resources 轉換為 JSON 字串
                    string resourcesJson = JsonConvert.SerializeObject(ViewBag.Resources);

                    // 將 JSON 字串傳遞給前端
                    ViewBag.ResourcesJson = resourcesJson;
                    
                #region
                //************************分開的程式碼
                /*if (selerestype != null)
                {
                    if (patient_id != "" && Res_Bundle.Entry.Count > 0)
                    {
                        if (selerestype == "Procedure")
                        {
                            var query = new SearchParams()
                            .Where("_id=" + patient_id)
                            .Where("_revinclude=Procedure:subject")
                            .Where("_count=10")
                            .Where("_total=accurate");//顯示總比數
                            var a = query.ToParameters();

                            Bundle Patient_Bundle = client.Search(query, "Patient");
                            foreach (var entry in Patient_Bundle.Entry)
                            {
                                Patient_Search_reosurces.Add((entry.Resource).ToJObject());
                            }
                        }
                        else if (selerestype == "Observation")
                        {
                            var query = new SearchParams()
                            .Where("_id=" + patient_id)
                            .Where("_revinclude=Observation:subject")
                            .Where("_total=accurate");//顯示總比數
                            var a = query.ToParameters();

                            Bundle Patient_Bundle = client.Search(query, "Patient");
                            foreach (var entry in Patient_Bundle.Entry)
                            {
                                Patient_Search_reosurces.Add((entry.Resource).ToJObject());
                            }
                        }
                        else if (selerestype == "Patient")
                        {
                            var query = new SearchParams()
                            .Where("_id=" + patient_id)
                            .Where("_total=accurate");//顯示總比數
                            var a = query.ToParameters();

                            Bundle Patient_Bundle = client.Search(query, "Patient");
                            foreach (var entry in Patient_Bundle.Entry)
                            {
                                Patient_Search_reosurces.Add((entry.Resource).ToJObject());
                            }
                        }
                        else if (selerestype == "Encounter")
                        {
                            var query = new SearchParams()
                            .Where("_id=" + patient_id)
                            .Where("_revinclude=Encounter:subject")
                            .Where("_total=accurate");//顯示總比數
                            var a = query.ToParameters();

                            Bundle Patient_Bundle = client.Search(query, "Patient");
                            foreach (var entry in Patient_Bundle.Entry)
                            {
                                Patient_Search_reosurces.Add((entry.Resource).ToJObject());
                            }
                        }
                        else if (selerestype == "MedicationRequest")
                        {
                            var query = new SearchParams()
                            .Where("_id=" + patient_id)
                            .Where("_revinclude=MedicationRequest:subject")
                            .Where("_total=accurate");//顯示總比數
                            var a = query.ToParameters();

                            Bundle Patient_Bundle = client.Search(query, "Patient");
                            foreach (var entry in Patient_Bundle.Entry)
                            {
                                Patient_Search_reosurces.Add((entry.Resource).ToJObject());
                            }
                        }
                        else if (selerestype == "Condition")
                        {
                            var query = new SearchParams()
                            .Where("_id=" + patient_id)
                            .Where("_revinclude=Condition:subject")
                            .Where("_total=accurate");//顯示總比數
                            var a = query.ToParameters();

                            Bundle Patient_Bundle = client.Search(query, "Patient");
                            foreach (var entry in Patient_Bundle.Entry)
                            {
                                Patient_Search_reosurces.Add((entry.Resource).ToJObject());
                            }
                        }
                        else if (selerestype == "DiagnosticReport")
                        {
                            var query = new SearchParams()
                            .Where("_id=" + patient_id)
                            .Where("_revinclude=DiagnosticReport:subject")
                            .Where("_total=accurate");//顯示總比數
                            var a = query.ToParameters();

                            Bundle Patient_Bundle = client.Search(query, "Patient");
                            foreach (var entry in Patient_Bundle.Entry)
                            {
                                Patient_Search_reosurces.Add((entry.Resource).ToJObject());
                            }
                        }
                        else if (selerestype == "Medication")
                        {
                            var query = new SearchParams()
                            .Where("_id=" + patient_id)
                            .Where("_revinclude=Medication:subject")
                            .Where("_total=accurate");//顯示總比數
                            var a = query.ToParameters();

                            Bundle Patient_Bundle = client.Search(query, "Patient");
                            foreach (var entry in Patient_Bundle.Entry)
                            {
                                Patient_Search_reosurces.Add((entry.Resource).ToJObject());
                            }
                        }
                        else if (selerestype == "ServiceRequest")
                        {
                            var query = new SearchParams()
                            .Where("_id=" + patient_id)
                            .Where("_revinclude=ServiceRequest:subject")
                            .Where("_total=accurate");//顯示總比數
                            var a = query.ToParameters();

                            Bundle Patient_Bundle = client.Search(query, "Patient");
                            foreach (var entry in Patient_Bundle.Entry)
                            {
                                Patient_Search_reosurces.Add((entry.Resource).ToJObject());
                            }
                        }
                        else
                        {
                            var query = new SearchParams()
                            .Where("_id=" + patient_id)
                            //.Where("_revinclude=Encounter:subject")
                            //.Where("_revinclude=Observation:subject")
                            //.Where("_revinclude=MedicationRequest:subject")
                            //.Where("_revinclude=Procedure:subject")
                            //.Where("_revinclude=Condition:subject")
                            //.Where("_revinclude=DiagnosticReport:subject")
                            .Where("_total=accurate");//顯示總比數
                            var a = query.ToParameters();

                            Bundle Patient_Bundle = client.Search(query, "Patient");
                            foreach (var entry in Patient_Bundle.Entry)
                            {
                                Patient_Search_reosurces.Add((entry.Resource).ToJObject());
                            }
                            //if (Patient_Search_reosurces.Count == 1)
                            //{
                            //    Patient_Search_reosurces.Clear();
                            //}
                        }

                    }

                    ViewBag.Resources = Patient_Search_reosurces; //左圖的
                    var asd = JsonConvert.SerializeObject(Patient_Search_reosurces);
                    return Json(asd);
                }
                else if (selerestype == null)
                {
                    if (patient_id != "" && Res_Bundle.Entry.Count > 0)
                    {
                        if (resourcetype == "Procedure")
                        {
                            var query = new SearchParams()
                            .Where("_id=" + patient_id)
                            //.Where("_revinclude=Encounter:subject")
                            //.Where("_revinclude=Observation:subject")
                            //.Where("_revinclude=MedicationRequest:subject")
                            .Where("_revinclude=Procedure:subject")
                            //.Where("_revinclude=Condition:subject")
                            //.Where("_revinclude=DiagnosticReport:subject")
                            .Where("_count=10")
                            .Where("_total=accurate");//顯示總比數
                            var a = query.ToParameters();

                            Bundle Patient_Bundle = client.Search(query, "Patient");
                            foreach (var entry in Patient_Bundle.Entry)
                            {
                                Patient_Search_reosurces.Add((entry.Resource).ToJObject());
                            }
                        }
                        else if (resourcetype == "Observation")
                        {
                            var query = new SearchParams()
                            .Where("_id=" + patient_id)
                            .Where("_revinclude=Observation:subject")
                            .Where("_total=accurate");//顯示總比數
                            var a = query.ToParameters();

                            Bundle Patient_Bundle = client.Search(query, "Patient");
                            foreach (var entry in Patient_Bundle.Entry)
                            {
                                Patient_Search_reosurces.Add((entry.Resource).ToJObject());
                            }
                        }
                        else if (resourcetype == "Patient")
                        {
                            var query = new SearchParams()
                            .Where("_id=" + patient_id)
                            .Where("_total=accurate");//顯示總比數
                            var a = query.ToParameters();

                            Bundle Patient_Bundle = client.Search(query, "Patient");
                            foreach (var entry in Patient_Bundle.Entry)
                            {
                                Patient_Search_reosurces.Add((entry.Resource).ToJObject());
                            }
                        }
                        else if (resourcetype == "Encounter")
                        {
                            var query = new SearchParams()
                            .Where("_id=" + patient_id)
                            .Where("_revinclude=Encounter:subject")
                            .Where("_total=accurate");//顯示總比數
                            var a = query.ToParameters();

                            Bundle Patient_Bundle = client.Search(query, "Patient");
                            foreach (var entry in Patient_Bundle.Entry)
                            {
                                Patient_Search_reosurces.Add((entry.Resource).ToJObject());
                            }
                        }
                        else if (resourcetype == "MedicationRequest")
                        {
                            var query = new SearchParams()
                            .Where("_id=" + patient_id)
                            .Where("_revinclude=MedicationRequest:subject")
                            .Where("_total=accurate");//顯示總比數
                            var a = query.ToParameters();

                            Bundle Patient_Bundle = client.Search(query, "Patient");
                            foreach (var entry in Patient_Bundle.Entry)
                            {
                                Patient_Search_reosurces.Add((entry.Resource).ToJObject());
                            }
                        }
                        else if (resourcetype == "Condition")
                        {
                            var query = new SearchParams()
                            .Where("_id=" + patient_id)
                            //.Where("_revinclude=Encounter:subject")
                            //.Where("_revinclude=Observation:subject")
                            //.Where("_revinclude=MedicationRequest:subject")
                            //.Where("_revinclude=Procedure:subject")
                            .Where("_revinclude=Condition:subject")
                            //.Where("_revinclude=DiagnosticReport:subject")
                            .Where("_total=accurate");//顯示總比數
                            var a = query.ToParameters();

                            Bundle Patient_Bundle = client.Search(query, "Patient");
                            foreach (var entry in Patient_Bundle.Entry)
                            {
                                Patient_Search_reosurces.Add((entry.Resource).ToJObject());
                            }
                        }
                        else if (resourcetype == "DiagnosticReport")
                        {
                            var query = new SearchParams()
                            .Where("_id=" + patient_id)
                            //.Where("_revinclude=Encounter:subject")
                            //.Where("_revinclude=Observation:subject")
                            //.Where("_revinclude=MedicationRequest:subject")
                            //.Where("_revinclude=Procedure:subject")
                            //.Where("_revinclude=Condition:subject")
                            .Where("_revinclude=DiagnosticReport:subject")
                            .Where("_total=accurate");//顯示總比數
                            var a = query.ToParameters();

                            Bundle Patient_Bundle = client.Search(query, "Patient");
                            foreach (var entry in Patient_Bundle.Entry)
                            {
                                Patient_Search_reosurces.Add((entry.Resource).ToJObject());
                            }
                        }
                        else if (resourcetype == "Medication")
                        {
                            var query = new SearchParams()
                            .Where("_id=" + patient_id)
                            //.Where("_revinclude=Encounter:subject")
                            //.Where("_revinclude=Observation:subject")
                            //.Where("_revinclude=MedicationRequest:subject")
                            //.Where("_revinclude=Procedure:subject")
                            //.Where("_revinclude=Condition:subject")
                            .Where("_revinclude=Medication:subject")
                            .Where("_total=accurate");//顯示總比數
                            var a = query.ToParameters();

                            Bundle Patient_Bundle = client.Search(query, "Patient");
                            foreach (var entry in Patient_Bundle.Entry)
                            {
                                Patient_Search_reosurces.Add((entry.Resource).ToJObject());
                            }
                        }
                        else if (resourcetype == "ServiceRequest")
                        {
                            var query = new SearchParams()
                            .Where("_id=" + patient_id)
                            //.Where("_revinclude=Encounter:subject")
                            //.Where("_revinclude=Observation:subject")
                            //.Where("_revinclude=MedicationRequest:subject")
                            //.Where("_revinclude=Procedure:subject")
                            //.Where("_revinclude=Condition:subject")
                            .Where("_revinclude=ServiceRequest:subject")
                            .Where("_total=accurate");//顯示總比數
                            var a = query.ToParameters();

                            Bundle Patient_Bundle = client.Search(query, "Patient");
                            foreach (var entry in Patient_Bundle.Entry)
                            {
                                Patient_Search_reosurces.Add((entry.Resource).ToJObject());
                            }
                        }
                        else
                        {
                            var query = new SearchParams()
                            .Where("_id=" + patient_id)
                            //.Where("_revinclude=Encounter:subject")
                            //.Where("_revinclude=Observation:subject")
                            //.Where("_revinclude=MedicationRequest:subject")
                            //.Where("_revinclude=Procedure:subject")
                            //.Where("_revinclude=Condition:subject")
                            //.Where("_revinclude=DiagnosticReport:subject")
                            .Where("_total=accurate");//顯示總比數
                            var a = query.ToParameters();

                            Bundle Patient_Bundle = client.Search(query, "Patient");
                            foreach (var entry in Patient_Bundle.Entry)
                            {
                                Patient_Search_reosurces.Add((entry.Resource).ToJObject());
                            }
                            //if (Patient_Search_reosurces.Count == 1)
                            //{
                            //    Patient_Search_reosurces.Clear();
                            //}
                        }
                    }

                    ViewBag.Resources = Patient_Search_reosurces; //左圖的

                }*/
                //************************分開的程式碼

                #endregion Patient 所有相關資料
                    return View();
                    
                }
            else
                {
                    if (Request.UrlReferrer == null)
                        return RedirectToAction("Index", "Home");
                    else
                        return Redirect(Request.UrlReferrer.ToString());
                }
            
        }

        //抓取多層section(Composition)
        private void ExtractEntryReferences(JToken section, List<KeyValuePair<string, string>> queryParameters)
        {
            foreach (var sectioninfo in section)
            {
                if (sectioninfo["entry"] != null)
                {
                    foreach (var info in sectioninfo["entry"])
                    {
                        entry_type = info["reference"].ToString().Split('/')[0];
                        entry_id = info["reference"].ToString().Split('/')[1];
                        queryParameters.Add(new KeyValuePair<string, string>(entry_type, "_id=" + entry_id));
                    }
                }

                if (sectioninfo["section"] != null)
                {
                    ExtractEntryReferences(sectioninfo["section"], queryParameters);
                }
            }
        }


        //處理每筆資料對照爬蟲
        private Dictionary<string, object> ProcessData(dynamic data, List<resourceinfo> resourceinfos)
        {
            Dictionary<string, object> formattedData = new Dictionary<string, object>();
            List<object> propertyValue = new List<object>();

            //骨幹結構
            string startName = "";
            Dictionary<string, (string Type, string Card)> tempDataName = new Dictionary<string, (string Type, string Card)>();
            //JArray BackValues;

            for (int i = 0; i < resourceinfos.Count; i++)
            {
                var resourceinfo = resourceinfos[i];
                string Name = resourceinfo.Name;
                string card = resourceinfo.Card;
                string type = resourceinfo.Type;

                //Backbone屬性
                int pointnum = Name.IndexOf('.');

                //if (type == "see referenceRange")
                //{
                //    //ENTRY裡的resource骨幹要修
                //    type = "BackboneElement";
                //}

                //處理型別頁面沒有的例外(處理type)
                if (type.ToString().Contains("Reference"))
                {
                    // 找到左括號的位置
                    int leftBracketIndex = type.IndexOf('(');
                    string referencePart = "";
                    if (leftBracketIndex >= 0)
                    {
                        // 從開頭到左括號位置的子字串即為所需的 Reference 部分
                        referencePart = type.Substring(0, leftBracketIndex).Trim();
                        type = referencePart;
                    }
                }
                else if (type == "SimpleQuantity")
                {
                    type = "Money";
                }
                else if (type == "BackboneElement" )
                {
                    if (Name.Contains('.'))
                    {
                        startName = Name.Substring(0, pointnum);
                        int startindex = pointnum + 1;
                        string BackName = Name.Substring(startindex);
                        tempDataName[BackName] = (type, card);
                    }
                    else
                    {
                        startName = Name;
                    }
                    continue;
                }

                //骨幹結構中的內容(要修型別)
                if (Name.Contains('.') && Name.StartsWith(startName) && ((JObject)data).ContainsKey(startName))
                {
                    int startindex = 0;
                    string BackName = "";

                    // 跳過第一個點前面的字串
                    startindex = pointnum + 1;
                    BackName = Name.Substring(startindex);

                    //儲存骨幹架構下的元素資料
                    tempDataName[BackName] = (type,card);

                    if(i + 1 < resourceinfos.Count && resourceinfos[i + 1].Name.StartsWith(startName) )
                    {//如果下一個資料不為底部或是開頭還是母元素名稱時繼續往下抓
                        continue;
                    }
                    else
                    {
                        //JToken BackValues = (JToken)data[startName];
                        JToken BackValues = data.SelectToken(startName);

                        if (BackValues != null)
                        {
                            if(BackValues is JArray)
                            {
                                List<object> propertytypeValues = new List<object>();
                                foreach (var contact in BackValues)
                                {
                                    List<object> combinedList = GetSectionData(contact, tempDataName, startName);
                                    propertytypeValues.Add(combinedList);
                                }
                                formattedData[startName] = propertytypeValues;
                            }
                            else
                            {
                                List<object> combinedList = GetSectionData(BackValues, tempDataName, startName);
                                formattedData[startName] = combinedList;
                            }
                        }
                    }
                    
                }

                #region
                //while (Name.Contains('.') && startName == Name.Substring(0, pointnum))
                //{
                //    //startName = Name.Substring(0, pointnum);
                //    BackValues = (JArray)data[startName];

                //    if (BackValues != null)
                //    {
                //        // 找每個的 contact元素
                //        foreach (JToken contact in BackValues)
                //        {
                //            var inBackData = getBackName(contact,pointnum, Name, type, card);
                //            tempData.Add(inBackData);


                //            if (inBackData.Count != 0)
                //            {
                //                // 使用臨時列表 tempDataList 保存 inBackData(不然會被最後一個取代)
                //                List<object> tempDataList = new List<object>();
                //                if (formattedData.TryGetValue(startName, out object existingValue) && existingValue is List<object> existingList)
                //                {
                //                    tempDataList.AddRange(existingList);
                //                }
                //                tempDataList.Add(inBackData);
                //                formattedData[startName] = tempDataList;
                //            }
                //        }
                //    }
                //    break;//跑過型別處理方法不用跑下面
                //}
                #endregion

                if (((JObject)data).ContainsKey(Name))//檢查資料中是否有此欄位(最外層)
                {
                    //判斷Card
                    if (card.Contains('*'))
                    {
                        JArray layerValues = (JArray)data[Name];
                        List<object> propertyValues = new List<object>();

                        //為了讓JArray轉成JObject才能在格式處理方法中取裡面特定內容
                        foreach (var value in layerValues)
                        {
                            var processedValue = ForDataType(type, value);
                            propertyValues.Add(processedValue);
                        }
                        formattedData[Name] = propertyValues;
                    }
                    else
                    {
                        if (data[Name] is JObject)
                        {
                            //假設是0..1、1..1時但卻是特殊格式的狀況
                            JToken innerDataValue = data.GetValue(Name);
                            var processedValue = ForDataType(type, innerDataValue);
                            formattedData[Name] = processedValue;
                        }
                        else
                        {
                            string value = data[Name]?.ToString();
                            formattedData[Name] = value;
                        }
                    }
                }
            }
            return formattedData;
            
        }

        bool isSee = false;
        //處理Section和骨幹的多層結構迴圈
        private List<object> GetSectionData(dynamic data, Dictionary<string, (string Type, string Card)> tempDataName, string startname)
        {
            JToken relationshipArray = null;
            List<object> combinedList = new List<object>();
            JToken sectionInnerDataValue = null;
            List<object> propertytypeValues = new List<object>();
            //骨幹結構
            string startName = "";
            Dictionary<string, (string Type, string Card)> nectDataName = new Dictionary<string, (string Type, string Card)>();

            foreach (var concent in tempDataName)
            {
                string backname = concent.Key;
                string backtype = concent.Value.Type;
                string backcard = concent.Value.Card;
                
                //Backbone屬性
                int pointnum = backname.IndexOf('.');
                

                if (backtype == "see referenceRange" && isSee == false)
                {
                    isSee = true;
                    propertytypeValues.Add(GetSectionData(data[backname], tempDataName, backname));
                }
                else if (backtype == "BackboneElement")
                {
                    if (backname.Contains('.') && backname.StartsWith(startName))
                    {
                        startName = backname.Substring(0, pointnum);

                        int startindex = pointnum + 1;
                        string BackName = backname.Substring(startindex);
                        nectDataName[BackName] = (backtype, backcard);
                    }
                    else
                    {
                        if(nectDataName.Count != 0)
                        {
                            JToken BackValues = data.SelectToken(startName);

                            if (BackValues != null)
                            {
                                foreach (var contact in BackValues)
                                {
                                    List<object> bakeboneList = GetSectionData(contact, nectDataName, startName);
                                    propertytypeValues.Add(bakeboneList);
                                }
                            }
                            startName = backname;
                            nectDataName = new Dictionary<string, (string Type, string Card)>();
                        }
                        startName = backname;
                    }
                    continue;
                }

                if (backname.Contains(".") && backname.StartsWith(startName))//不只兩層時的處理情況
                {
                    int backpoint = backname.IndexOf('.');
                    backname = backname.Substring(backpoint + 1);
                    nectDataName[backname] = (backtype,backcard);
                    continue;
                }
                else if (backname.Contains("."))//不只兩層時的處理情況
                {
                    int backpoint = backname.IndexOf('.');
                    string backname1 = backname.Substring(0, backpoint);
                    if (((JObject)data).ContainsKey(backname1))
                    {
                        if (backcard.Contains('*'))
                        {
                            relationshipArray = (JArray)data[backname1];
                            backname = backname.Substring(backpoint + 1);
                            relationshipArray = (JArray)data[backname];
                            if (relationshipArray != null)
                            {
                                // 跑內層內容
                                foreach (JToken relationship in relationshipArray)
                                {
                                    var BackData = ForDataType(backtype, relationship);
                                    propertytypeValues.Add(BackData);
                                }
                            }
                        }
                        else
                        {
                            JToken innerDataValue = data[backname1];
                            backname = backname.Substring(backpoint + 1);
                            foreach (var val in innerDataValue)
                            {
                                innerDataValue = val[backname];
                                var processedValue = ForDataType(backtype, innerDataValue);
                                propertytypeValues.Add(processedValue);
                            }
                        }
                    }
                }
                //else if (((JObject)contact).ContainsKey(backname))
                //else if (data is JObject)
                else
                {
                    if (backname == "section")
                    {
                        //if (contact[backname] is JArray sectionArray)
                        //{
                        //    foreach (JToken sectionItem in sectionArray)
                        //    {
                        //        var sectionPropertyValues = GetSectionData(sectionItem, tempDataName);
                        //        propertytypeValues.AddRange(sectionPropertyValues);
                        //    }
                        //}
                        if (data is JObject contactObj)
                        {
                            if (contactObj[backname] is JArray sectionArray)
                            {
                                foreach (JToken sectionItem in sectionArray)
                                {
                                    // 利用 tempDataName 循環處理 sectionItem 
                                    List<object> sectionPropertyValues = new List<object>();
                                    foreach (var sectionConcent in tempDataName)
                                    {
                                        string sectionBackname = sectionConcent.Key;
                                        string sectionBacktype = sectionConcent.Value.Type;
                                        string sectionBackcard = sectionConcent.Value.Card;

                                        // 根據 sectionBackname 獲取 sectionItem 值做處理
                                        if ((((JObject)sectionItem).ContainsKey(sectionBackname)))
                                        {
                                            sectionInnerDataValue = sectionItem[sectionBackname];
                                            if (sectionBackcard.Contains('*'))
                                            {
                                                if (sectionInnerDataValue != null)
                                                {
                                                    // 跑內層內容
                                                    foreach (JToken sectionInnerData in sectionInnerDataValue)
                                                    {
                                                        var sectionProcessedValue = ForDataType(sectionBacktype, sectionInnerData);
                                                        propertytypeValues.Add(sectionProcessedValue);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                var sectionProcessedValue = ForDataType(sectionBacktype, sectionInnerDataValue);
                                                sectionPropertyValues.Add(sectionProcessedValue);
                                            }
                                        }

                                    }

                                    propertytypeValues.Add(sectionPropertyValues);
                                }
                            }
                        }
                    }
                    // 確保 contact 是 JObject
                    else
                    {
                        if (backcard.Contains('*'))
                        {
                            if (data is JArray)//不為特殊格式時
                            {
                                foreach (var val in data)
                                {
                                    relationshipArray = val[backname];
                                }
                            }
                            else
                            {
                                relationshipArray = data[backname];
                            }

                            if (relationshipArray != null)
                            {
                                // 跑內層內容
                                foreach (JToken relationship in relationshipArray)
                                {
                                    var BackData = ForDataType(backtype, relationship);
                                    propertytypeValues.Add(BackData);
                                }
                            }
                        }
                        else
                        {
                            JToken innerDataValue = null;
                            if (data is JArray)//不為特殊格式時
                            {
                                foreach (var val in data)
                                {
                                    innerDataValue = val[backname];
                                }
                            }
                            else
                            {
                                innerDataValue = data[backname];
                            }

                            if (innerDataValue != null)
                            {
                                Dictionary<string, object> formattedinterData = new Dictionary<string, object>();
                                if (backtype == "Resource" && backname == "resource")
                                {
                                    string resourcetype = innerDataValue["resourceType"].ToString();
                                    if (resourcetype != "")
                                    {
                                        List<resourceinfo> resourceinterInfos = db.ResourceInfos
                                        .Where(r => r.ResourceType == resourcetype)
                                        .ToList();
                                        formattedinterData["resource"] = ProcessData(innerDataValue, resourceinterInfos);
                                        List<object> key = new List<object>();
                                        key.Add(innerDataValue["resourceType"]?.ToString());
                                        key.Add(innerDataValue["id"]?.ToString());
                                        key.Add(FindValuesByValue(formattedinterData["resource"]));
                                        propertytypeValues.Add(key);
                                        continue;
                                    }
                                }
                                var processedValue = ForDataType(backtype, innerDataValue);
                                propertytypeValues.Add(processedValue);
                            }
                            //else
                            //{
                            //    string str = contact[backname]?.ToString();
                            //    propertytypeValues.Add(str);
                            //    //AlldatatypeData = $"{str}";
                            //}
                        }

                        //if (propertytypeValues.Count != 0)
                        //{
                        //    // 使用臨時列表 tempdatalist 保存 inbackdata(不然會被最後一個取代)
                        //    List<object> tempdatalist = new List<object>();
                        //    if (formattedData.TryGetValue(startName, out object existingvalue) && existingvalue is List<object> existinglist)
                        //    {
                        //        tempdatalist.AddRange(existinglist);
                        //    }
                        //    tempdatalist.Add(propertytypeValues);
                        //    formattedData[startName] = tempdatalist;
                        //}
                    }
                }
            }

            if(nectDataName.Count != 0)
            {
                JToken BackValues = data.SelectToken(startName);

                if (BackValues != null)
                {
                    if (BackValues is JArray)
                    {
                        foreach (var contact in BackValues)
                        {
                            List<object> bakeboneList = GetSectionData(contact, nectDataName, startName);
                            propertytypeValues.Add(bakeboneList);
                        }
                    }
                    else
                    {
                        List<object> bakeboneList = GetSectionData(BackValues, nectDataName, startName);
                        propertytypeValues.Add(bakeboneList);
                    }
                }
            }
            combinedList.Add(propertytypeValues);
            // 找每個的 contact元素

            #region
            //foreach (JToken contact in data)
            //{
            //    List<object> propertytypeValues = new List<object>();
            //    foreach (var concent in tempDataName)
            //    {
            //        string backname = concent.Key;
            //        string backtype = concent.Value.Type;
            //        string backcard = concent.Value.Card;


            //        if (backname.Contains("."))//不只兩層時的處理情況
            //        {
            //            int backpoint = backname.IndexOf('.');
            //            string backname1 = backname.Substring(0, backpoint);
            //            if (contact.Contains(backname1))
            //            {
            //                if (backcard.Contains('*'))
            //                {
            //                    relationshipArray = (JArray)contact[backname1];
            //                    backname = backname.Substring(backpoint + 1);
            //                    relationshipArray = (JArray)contact[backname];
            //                    if (relationshipArray != null)
            //                    {
            //                        // 跑每个联系人的 內層內容
            //                        foreach (JToken relationship in relationshipArray)
            //                        {
            //                            var BackData = ForDataType(backtype, relationship);
            //                            propertytypeValues.Add(BackData);
            //                        }
            //                    }
            //                }
            //                else
            //                {
            //                    JToken innerDataValue = contact[backname];
            //                    backname = backname.Substring(backpoint + 1);
            //                    innerDataValue = contact[backname];

            //                    var processedValue = ForDataType(backtype, innerDataValue);
            //                    propertytypeValues.Add(processedValue);
            //                }
            //            }

            //        }
            //        //else if (((JObject)contact).ContainsKey(backname))
            //        else if (contact is JObject)
            //        {
            //            if (backname == "section" )
            //            {
            //                //if (contact[backname] is JArray sectionArray)
            //                //{
            //                //    foreach (JToken sectionItem in sectionArray)
            //                //    {
            //                //        var sectionPropertyValues = GetSectionData(sectionItem, tempDataName);
            //                //        propertytypeValues.AddRange(sectionPropertyValues);
            //                //    }
            //                //}
            //                if (contact is JObject contactObj)
            //                {
            //                    if (contactObj[backname] is JArray sectionArray)
            //                    {
            //                        foreach (JToken sectionItem in sectionArray)
            //                        {
            //                            // 再次运行 tempDataName 的循环来处理 sectionItem 数据
            //                            List<object> sectionPropertyValues = new List<object>();
            //                            foreach (var sectionConcent in tempDataName)
            //                            {
            //                                string sectionBackname = sectionConcent.Key;
            //                                string sectionBacktype = sectionConcent.Value.Type;
            //                                string sectionBackcard = sectionConcent.Value.Card;

            //                                // 根据 sectionBackname 获取 sectionItem 的值並處理
            //                                if ((((JObject)sectionItem).ContainsKey(sectionBackname)))
            //                                {
            //                                    sectionInnerDataValue = sectionItem[sectionBackname];
            //                                    if (sectionBackcard.Contains('*'))
            //                                    {
            //                                        if (sectionInnerDataValue != null)
            //                                        {
            //                                            // 跑每个联系人的 內層內容
            //                                            foreach (JToken sectionInnerData in sectionInnerDataValue)
            //                                            {
            //                                                var sectionProcessedValue = ForDataType(sectionBacktype, sectionInnerData);
            //                                                propertytypeValues.Add(sectionProcessedValue);
            //                                            }
            //                                        }
            //                                    }
            //                                    else
            //                                    {
            //                                        var sectionProcessedValue = ForDataType(sectionBacktype, sectionInnerDataValue);
            //                                        sectionPropertyValues.Add(sectionProcessedValue);
            //                                    }
            //                                }

            //                            }

            //                            propertytypeValues.Add(sectionPropertyValues);
            //                        }
            //                    }
            //                }
            //            }
            //            // 確保contact 是一个 JObject
            //            else 
            //            {
            //                if (backcard.Contains('*'))
            //                {
            //                    relationshipArray = (JArray)contact[backname];
            //                    if (relationshipArray != null)
            //                    {
            //                        // 跑每个联系人的 內層內容
            //                        foreach (JToken relationship in relationshipArray)
            //                        {
            //                            var BackData = ForDataType(backtype, relationship);
            //                            propertytypeValues.Add(BackData);
            //                        }
            //                    }
            //                }
            //                else
            //                {
            //                    //if (contact[backname] is JArray)//不為特殊格式時
            //                    //{
            //                        JToken innerDataValue = contact[backname];
            //                        var processedValue = ForDataType(backtype, innerDataValue);
            //                        propertytypeValues.Add(processedValue);
            //                    //}
            //                    //else
            //                    //{
            //                    //    string str = contact[backname]?.ToString();
            //                    //    propertytypeValues.Add(str);
            //                    //    //AlldatatypeData = $"{str}";
            //                    //}
            //                }

            //                //if (propertytypeValues.Count != 0)
            //                //{
            //                //    // 使用臨時列表 tempdatalist 保存 inbackdata(不然會被最後一個取代)
            //                //    List<object> tempdatalist = new List<object>();
            //                //    if (formattedData.TryGetValue(startName, out object existingvalue) && existingvalue is List<object> existinglist)
            //                //    {
            //                //        tempdatalist.AddRange(existinglist);
            //                //    }
            //                //    tempdatalist.Add(propertytypeValues);
            //                //    formattedData[startName] = tempdatalist;
            //                //}
            //            }
            //        }
            //        else if(contact is JProperty)
            //        {
            //            JProperty property = (JProperty)contact;
            //            JObject jObject = (JObject)property.Value;

            //            JToken propertyValue = jObject.GetValue(backname);

            //            if (propertyValue != null)
            //            {
            //                // 在这里处理 propertyValue
            //            }
            //        }
            //    }

            //    combinedList.Add(propertytypeValues);

            //}
            #endregion
            return combinedList;

        }

        //抓字典VALUE項
        private List<object> FindValuesByValue(object value)
        {
            List<object> values = new List<object>();

            if (value is Dictionary<string, object> dictionary)
            {
                foreach (var pair in dictionary)
                {
                    values.AddRange(FindValuesByValue(pair.Value));
                }
            }
            else if (value is JValue jValue)
            {
                values.Add(jValue.Value);
            }
            else if (value is List<object> list)
            {
                foreach (var item in list)
                {
                    values.AddRange(FindValuesByValue(item));
                }
            }
            else
            {
                values.Add(value);
            }
            return values;
        }

        #region //處理骨幹元素的名字階層
        //private dynamic getBackName(JToken contact, int pointnum, string Name,string type,string card)
        //{
        //    string BackName = "";
        //    JArray relationshipArray = null;
        //    List<object> propertytypeValues = new List<object>();


        //    // 確保contact 是一个 JObject
        //    if (contact is JObject contactObj)
        //    {
        //        int startindex = 0;
        //        int endIndex;

        //        // 找到第一個點的位置
        //        int firstIndex = Name.IndexOf('.');

        //        if (firstIndex != -1 && firstIndex < Name.Length - 1)
        //        {
        //            // 跳過第一個點前面的字串
        //            startindex = firstIndex + 1;

        //            while ((endIndex = Name.IndexOf('.', startindex)) != -1)//查找Name中從startindex位置開始找起的.位置
        //            {
        //                // 取到兩個.間的字串
        //                BackName = Name.Substring(startindex, endIndex - startindex);

        //                // 獲取內層元素
        //                relationshipArray = (JArray)contactObj[BackName];

        //                // 跑內層內容
        //                foreach (JToken relationship in relationshipArray)
        //                {
        //                    // 现在 relationship 包含了 relationship 区块的内容
        //                    Console.WriteLine(relationship.ToString());
        //                }

        //                var BackData = ForDataType(type, relationshipArray);

        //                startindex = endIndex + 1; // 移動到下一.位置
        //            }

        //            // 處理最後面的部份
        //            if (startindex < Name.Length)
        //            {
        //                BackName = Name.Substring(startindex);
        //                if (((JObject)contactObj).ContainsKey(BackName))//檢查資料中是否有此欄位(最外層)
        //                { 
        //                    if (card.Contains('*'))
        //                    {
        //                        relationshipArray = (JArray)contactObj[BackName];
        //                        if (relationshipArray != null)
        //                        {
        //                            // 跑內層內容
        //                            foreach (JToken relationship in relationshipArray)
        //                            {
        //                                var BackData = ForDataType(type, relationship);
        //                                propertytypeValues.Add(BackData);

        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        JToken BackData = contactObj?.GetValue(BackName);
        //                        //var BackData1 = ForDataType(type, BackData);

        //                        if (BackData != null)
        //                        {
        //                            //var BackData1 = ForDataType(type, BackData);
        //                            propertytypeValues.Add(BackData.ToString());
        //                        }
        //                    }
        //                }
                            
        //            }
        //        }
        //    }
        //    return propertytypeValues;
        //}
        #endregion

        //抓取內層型別(特殊格式處理)
        private dynamic ForDataType(string datatypestr, JToken value)
        {
            List<object> propertytypeValues = new List<object>();
            //抓特殊型別內容
            var Datatypes = db.datatypes
                .Where(x => x.Datatype == datatypestr)
                .ToList();

            if (Datatypes.Count == 0)
            {//假設型別表中沒有此類(不是特殊型別但也可能多層)
                if (value is JArray nestedEnumerableValue)
                {
                    foreach (var item in nestedEnumerableValue)
                    {
                        // 使用遞迴處理內層資料
                        var innerData = ForDataType(datatypestr, item);
                        propertytypeValues.Add(innerData);
                    }
                }
                //if (value is JObject nestedValue)
                //{
                //    var processedValue = ForDataType(datatypestr, nestedValue);
                //    propertytypeValues.Add(processedValue);
                //}
                else
                {
                    propertytypeValues.Add(value.ToString());
                }
            }
            else
            {
                List<object> propertytypeValue = new List<object>();
                //為特殊格式時將型別內容取出比對處理
                foreach (var datatype in Datatypes)
                {
                    string name = datatype.Name;
                    string card = datatype.Card;
                    string type = datatype.Type;

                    string AlldatatypeData = "";
                    JObject AlldatatypeObj = (JObject)value;//可能要修
                        
                    if(type == "SimpleQuantity")
                    {
                        type = "Money";
                    }
                    else if (type == "Element")
                    {
                        continue;
                    }

                    //先檢查內層行別是否為特殊型別
                    bool HasMatchDatatype = db.datatypes.Any(x => x.Datatype == type);
                    if (HasMatchDatatype)
                    {
                        JToken innerDataValue = null;
                        //為特殊格式時進去判斷
                        if (name.Contains("."))//不只兩層時的處理情況
                        {
                            int backpoint = name.IndexOf('.');
                            string backname1 = name.Substring(0, backpoint);
                            if (((JObject)AlldatatypeObj).ContainsKey(backname1))
                            {
                                innerDataValue = AlldatatypeObj.GetValue(backname1);
                                name = name.Substring(backpoint + 1);
                                foreach (var innerval in innerDataValue)
                                {
                                    innerDataValue = innerval[name];
                                }
                            }

                        }
                        else
                        {
                            innerDataValue = AlldatatypeObj.GetValue(name);
                        }

                        if(innerDataValue != null)
                        {
                            if (innerDataValue is JArray)
                            {
                                JArray innerDataArray = (JArray)innerDataValue;
                                foreach (var innerValue in innerDataArray)
                                {
                                    var inData = ForDataType(type, innerValue);
                                    propertytypeValues.Add(inData);
                                }
                            }
                            else
                            {
                                var inData = ForDataType(type, innerDataValue);
                                propertytypeValues.Add(inData);
                            }
                        }

                    }
                    else//不為特殊格式時
                    {
                        if (card != null && card.Contains('*'))
                        {//陣列處理
                            JArray layerValues = (JArray)AlldatatypeObj[name];
                            List<object> propertyValues = new List<object>();
                            if(layerValues != null)
                            {
                                foreach (var avalue in layerValues)
                                {
                                    //object processedValue = ProcessValue(type, value);
                                    var processedValue = ForDataType(type, avalue);

                                    propertytypeValues.Add(processedValue);
                                }
                            }
                            
                        }
                        else
                        {
                            if (AlldatatypeObj.ContainsKey(name))
                            {
                                //假設是0..1、1..1時但卻是特殊格式的狀況
                                JToken innerDataValue = AlldatatypeObj.GetValue(name);
                                var processedValue = ForDataType(type, innerDataValue);
                                propertytypeValues.Add(processedValue);
                            }
                            else
                            {
                                string str = AlldatatypeObj[name]?.ToString();
                                if(str != null)
                                {
                                    AlldatatypeData = $"{str}";
                                    propertytypeValue.Add(AlldatatypeData);
                                }
                            }
                        }


                    }

                    //if (AlldatatypeData != "")
                    //{
                    //    propertytypeValue.Add(AlldatatypeData);
                    //}

                }

                if (propertytypeValue.Count != 0)
                {
                    propertytypeValues.Add(propertytypeValue);
                }
            }
            return propertytypeValues;
        }

        
        private object ProcessValue(string type, JToken value)
        {
            if (type == "Identifier")
            {
                JObject identifierObj = (JObject)value;

                string use = identifierObj.GetValue("use")?.ToString();
                string system = identifierObj.GetValue("system")?.ToString();
                string identifierValue = identifierObj.GetValue("value")?.ToString();

                string identifierData = $"{use}, {system}, {identifierValue}";

                return identifierData;
            }
            else if (type == "CodeableConcept")
            {
                JObject codeableConceptObj = (JObject)value;

                string code = codeableConceptObj.GetValue("code")?.ToString();
                string display = codeableConceptObj.GetValue("display")?.ToString();

                string CodeableConceptrData = $"{code}, {display}";

                return CodeableConceptrData;
            }
            else if (type == "Address")
            {
                JObject addressObj = (JObject)value;

                List<string> addressValues = new List<string>();

                string[] properties = { "use", "type", "text", "line", "city", "district", "state", "postalCode", "country", "period" };
                foreach (string property in properties)
                {
                    string res = addressObj.GetValue(property)?.ToString();
                    if (!string.IsNullOrEmpty(res))
                    {
                        addressValues.Add(res);
                    }
                }

                JArray linesArray = addressObj.GetValue("line") as JArray;
                string[] lines = linesArray?.ToObject<string[]>();
                if (lines != null && lines.Length > 0)
                {
                    addressValues.AddRange(lines);
                }

                string addressData = string.Join(", ", addressValues);

                return addressData;

            }
            else if (type == "HumanName")
            {
                JObject nameObj = (JObject)value;

                string use = nameObj.GetValue("use")?.ToString();
                string text = nameObj.GetValue("text")?.ToString();
                string family = nameObj.GetValue("family")?.ToString();
                JArray givenArray = nameObj.GetValue("given") as JArray;
                string[] given = givenArray?.ToObject<string[]>();
                JArray prefixArray = nameObj.GetValue("prefix") as JArray;
                string[] prefix = prefixArray?.ToObject<string[]>();
                JArray suffixArray = nameObj.GetValue("suffix") as JArray;
                string[] suffix = suffixArray?.ToObject<string[]>();
                string period = nameObj.GetValue("period")?.ToString();

                List<string> nameValues = new List<string>
                {
                    use, text, family
                };

                nameValues.AddRange(given ?? Enumerable.Empty<string>());
                nameValues.AddRange(prefix ?? Enumerable.Empty<string>());
                nameValues.AddRange(suffix ?? Enumerable.Empty<string>());

                nameValues.RemoveAll(string.IsNullOrEmpty);

                string nameData = string.Join(", ", nameValues);

                return nameData;
            }
            else if (type == "ContactPoint")
            {
                JObject contactPointObj = (JObject)value;

                string system = contactPointObj.GetValue("system")?.ToString();
                string contactValue = contactPointObj.GetValue("value")?.ToString();
                string use = contactPointObj.GetValue("use")?.ToString();
                string rank = contactPointObj.GetValue("rank")?.ToString();
                string period = contactPointObj.GetValue("period")?.ToString();

                // 根據需求將這些值組合成所需的格式
                string contactPointData = $"{system}, {contactValue}, {use}, {rank}, {period}";

                return contactPointData;
            }
            else if (type == "Attachment")
            {
                JObject attachmentObj = (JObject)value;

                string contentType = attachmentObj.GetValue("contentType")?.ToString();
                string language = attachmentObj.GetValue("language")?.ToString();
                string data = attachmentObj.GetValue("data")?.ToString();
                string url = attachmentObj.GetValue("url")?.ToString();
                string size = attachmentObj.GetValue("size")?.ToString();
                string hash = attachmentObj.GetValue("hash")?.ToString();
                string title = attachmentObj.GetValue("title")?.ToString();
                string creation = attachmentObj.GetValue("creation")?.ToString();

                // 根據需求將這些值組合成所需的格式
                string attachmentData = $"{contentType}, {language}, {data}, {url}, {size}, {hash}, {title}, {creation}";

                return attachmentData;
            }
            else if (type == "BackboneElement")//要改
            {
                JObject backboneElementObj = (JObject)value;

                JArray modifierExtensions = backboneElementObj.GetValue("modifierExtension") as JArray;

                // 創建一個列表來存儲 modifierExtension 的值
                List<string> modifierExtensionValues = new List<string>();

                if (modifierExtensions != null)
                {
                    foreach (JObject modifierExtensionObj in modifierExtensions)
                    {
                        // 處理每個 modifierExtension 物件的值
                        // ...

                        // 將值加入列表中
                        //modifierExtensionValues.Add(modifierExtensionValue);
                    }
                }

                //if (referencePart == "Reference")
                //{
                //    JObject contactPointObj = data.GetValue(Name); ;

                //    string reference = contactPointObj.GetValue("reference")?.ToString();
                //    string type1 = contactPointObj.GetValue("type")?.ToString();
                //    string identifier = contactPointObj.GetValue("identifier	")?.ToString();
                //    string display = contactPointObj.GetValue("display")?.ToString();

                //    // 根據需求將這些值組合成所需的格式
                //    string contactPointData = $"{reference}, {type1}, {identifier}, {display}";

                //    formattedData[Name] = contactPointData;
                //}
                //else
                //{
                //    string value = data[Name]?.ToString();
                //    formattedData[Name] = value;
                //}

                // 將 modifierExtension 的值組合成所需的格式
                string modifierExtensionData = string.Join(", ", modifierExtensionValues);

                return modifierExtensionData;
            }
            return null;
        }





    }
}