using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;
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

        public ActionResult Index(string res, string id)
        {
            string[] Resource = new string[] { "Patient", "Encounter", "Observation", "MedicationRequest", "Procedure", "Condition", "DiagnosticReport", "ServiceRequest" };
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

                string patient_id = "";
                string resourcetype = "";
                resourcetype = res;
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
                    else if (entry_res["subject"] != null)
                    {
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
                    if (Res_Search[0].code.coding != null)
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

                Bundle Res_detail_Bundle = client.Search(res_detail_query, res);
                List<dynamic> Res_detail_list = new List<dynamic>();

                foreach (var entry in Res_detail_Bundle.Entry)
                {
                    var entry_res = (entry.Resource).ToJObject();
                    Res_detail_list.Add(entry_res);
                    //if (res == "Patient") 
                    //{
                    //    patient_id = id;
                    //}
                    //else if (entry_res["subject"] != null)
                    //{
                    //    patient_id = entry_res["subject"]["reference"].ToString().Split('/')[1];
                    //}
                }

                ViewBag.Resource_detail = Res_detail_list; //資料都全送但前端只有用obser
                #endregion Resource_detail 所有相關資料


                #region Patient 所有相關資料
                List<dynamic> Patient_Search_reosurces = new List<dynamic>();
                
                if (patient_id != "" && Res_Bundle.Entry.Count > 0)
                {
                    if (resourcetype == "Procedure")
                    {
                        var query = new SearchParams()
                        .Where("_id=" + patient_id)
                        //.Where("_revinclude=Encounter:patient")
                        //.Where("_revinclude=Observation:patient")
                        //.Where("_revinclude=MedicationRequest:patient")
                        .Where("_revinclude=Procedure:patient")
                        //.Where("_revinclude=Condition:patient")
                        //.Where("_revinclude=DiagnosticReport:patient")
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
                        //.Where("_revinclude=Encounter:patient")
                        .Where("_revinclude=Observation:patient")
                        //.Where("_revinclude=MedicationRequest:patient")
                        //.Where("_revinclude=Procedure:patient")
                        //.Where("_revinclude=Condition:patient")
                        //.Where("_revinclude=DiagnosticReport:patient")
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
                        //.Where("_revinclude=Encounter:patient")
                        //.Where("_revinclude=Observation:patient")
                        //.Where("_revinclude=MedicationRequest:patient")
                        //.Where("_revinclude=Procedure:patient")
                        //.Where("_revinclude=Condition:patient")
                        //.Where("_revinclude=DiagnosticReport:patient")
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
                        .Where("_revinclude=Encounter:patient")
                        //.Where("_revinclude=Observation:patient")
                        //.Where("_revinclude=MedicationRequest:patient")
                        //.Where("_revinclude=Procedure:patient")
                        //.Where("_revinclude=Condition:patient")
                        //.Where("_revinclude=DiagnosticReport:patient")
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
                        //.Where("_revinclude=Encounter:patient")
                        //.Where("_revinclude=Observation:patient")
                        .Where("_revinclude=MedicationRequest:patient")
                        //.Where("_revinclude=Procedure:patient")
                        //.Where("_revinclude=Condition:patient")
                        //.Where("_revinclude=DiagnosticReport:patient")
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
                        //.Where("_revinclude=Encounter:patient")
                        //.Where("_revinclude=Observation:patient")
                        //.Where("_revinclude=MedicationRequest:patient")
                        //.Where("_revinclude=Procedure:patient")
                        .Where("_revinclude=Condition:patient")
                        //.Where("_revinclude=DiagnosticReport:patient")
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
                        //.Where("_revinclude=Encounter:patient")
                        //.Where("_revinclude=Observation:patient")
                        //.Where("_revinclude=MedicationRequest:patient")
                        //.Where("_revinclude=Procedure:patient")
                        //.Where("_revinclude=Condition:patient")
                        .Where("_revinclude=DiagnosticReport:patient")
                        .Where("_total=accurate");//顯示總比數
                        var a = query.ToParameters();

                        Bundle Patient_Bundle = client.Search(query, "Patient");
                        foreach (var entry in Patient_Bundle.Entry)
                        {
                            Patient_Search_reosurces.Add((entry.Resource).ToJObject());
                        }
                    }

                }


                ViewBag.Resources = Patient_Search_reosurces; //左圖的
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
    }
}