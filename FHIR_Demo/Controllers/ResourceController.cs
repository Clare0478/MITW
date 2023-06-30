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

namespace FHIR_Demo.Controllers
{
    public class ResourceController : Controller
    {
        private CookiesController cookies = new CookiesController();
        private HttpClientEventHandler handler = new HttpClientEventHandler();
        string entry_type = "";
        string entry_id = "";
        public ActionResult Index(string res, string id, string sele,bool isbundle = false)
           {
            string[] Resource = new string[] { "Patient", "Encounter", "Observation", "MedicationRequest","Composition", "Procedure", "Condition", "DiagnosticReport", "ServiceRequest" };
           
            if (Resource.Contains(res) && id != null && Request.Cookies["FHIR_url"] != null)
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


                    //*****抓到該選取的該筆類型資料顯示右方(Res_Search)*****
                    string patient_id = "";
                    string composition_id = "";
                    string encounter_id = "";
                    string selerestype = "";
                    string resourcetype = "";//判斷resourcetype的變數

                    //加入isbundle的判斷，讓前端知道我現在的isbundle
                    ViewBag.isbundle = isbundle;
                    resourcetype = res;
                    selerestype = sele;
                    #region Search_Resource 資料
                     Bundle Res_Bundle = client.SearchById(res, id);
                    List<dynamic> Res_Search = new List<dynamic>();
                    foreach (var entry in Res_Bundle.Entry)
                    {
                        var entry_res = (entry.Resource).ToJObject();
                        Res_Search.Add(entry_res);
                        if (res == "Patient")
                        {
                            patient_id = id;
                        }
                        else if (res == "Composition")
                        {
                            composition_id = id;
                            ViewBag.isbundle = true;
                            Session["composition_id"] = composition_id;
                        }
                        else if (entry_res["subject"] != null)
                        {
                            if (ViewBag.isbundle == false)
                            {
                                Session["composition_id"] = "";
                            }
                            patient_id = (entry.Resource).ToJObject()["subject"]["reference"].ToString().Split('/')[1];
                        }
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
                    if (ViewBag.isbundle == false && Res_Bundle.Entry.Count > 0 )
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
                    else if(ViewBag.isbundle == true && Res_Bundle.Entry.Count > 0)
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
                    
                    //}
                    //catch (Exception e)
                    //{
                    //    if (Request.UrlReferrer == null)
                    //        return RedirectToAction("Index", "Home");
                    //    else
                    //        return Redirect(Request.UrlReferrer.ToString());
                    //}
                }
                else
                {
                    if (Request.UrlReferrer == null)
                        return RedirectToAction("Index", "Home");
                    else
                        return Redirect(Request.UrlReferrer.ToString());
                }
            
        }

        //抓取多層section
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








    }
}