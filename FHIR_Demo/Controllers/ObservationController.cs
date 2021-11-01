using FHIR_Demo.Models;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Mvc;

namespace FHIR_Demo.Controllers
{
    public class ObservationController : Controller
    {
        private CookiesController cookies = new CookiesController();
        private HttpClientEventHandler handler = new HttpClientEventHandler();

        // GET: Observation
        public ActionResult Index()
        {
            handler.OnBeforeRequest += (sender, e) =>
            {
                e.RawRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", cookies.FHIR_Token_Cookie(HttpContext));
            };
            FhirClient client = new FhirClient(cookies.FHIR_URL_Cookie(HttpContext), cookies.settings, handler);
            ViewBag.status = TempData["status"];
            try
            {
                var q = new SearchParams().LimitTo(20);
                Bundle ObservationBundle = client.Search<Observation>(q);
                //var json = PatientSearchBundle.ToJson();
                List<ObservationViewModel> observationViewModels = new List<ObservationViewModel>();
                foreach (var entry in ObservationBundle.Entry)
                {
                    observationViewModels.Add(new ObservationViewModel().ObservationViewModelMapping((Observation)entry.Resource));
                }

                return View(observationViewModels);
            }
            catch (Exception e)
            {
                ViewBag.Error = e.ToString();
                return View();
            }
        }

        [HttpPost]
        public ActionResult GetRecord(string url, string token, string search)
        {
            handler.OnBeforeRequest += (sender, e) =>
            {
                e.RawRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", cookies.FHIR_Token_Cookie(HttpContext, token));
            };
            FhirClient client = new FhirClient(cookies.FHIR_URL_Cookie(HttpContext, url), cookies.settings, handler);
            try
            {
                var q = SearchParams.FromUriParamList(UriParamList.FromQueryString(search)).LimitTo(20);

                Bundle ObservationBundle = client.Search<Observation>(q);

                //var json = PatientSearchBundle.ToJson();
                List<ObservationViewModel> observationViewModels = new List<ObservationViewModel>();

                foreach (var entry in ObservationBundle.Entry)
                {
                    observationViewModels.Add(new ObservationViewModel().ObservationViewModelMapping((Observation)entry.Resource));
                }

                return PartialView("_GetRecord", observationViewModels);
            }
            catch (Exception e)
            {
                ViewBag.Error = e.ToString();
                return PartialView("_GetRecord");
            }
        }

        // GET: Observation/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            handler.OnBeforeRequest += (sender, e) =>
            {
                e.RawRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", cookies.FHIR_Token_Cookie(HttpContext));
            };
            FhirClient client = new FhirClient(cookies.FHIR_URL_Cookie(HttpContext), cookies.settings, handler);
            try
            {
                var Obser = client.Read<Observation>("Observation/" + id);
                var Obser_view = new ObservationViewModel().ObservationViewModelMapping(Obser);

                return View(Obser_view);
            }
            catch
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        // GET: Observation/Create
        public ActionResult Create()
        {
            ViewBag.ObserCode_Select = new SelectList(new ObservationCode().observationCode(), "chinese", "chinese");
            return View();
        }

        // POST: Observation/Create
        [HttpPost]
        public ActionResult Create(ObservationViewModel model)
        {
            ViewBag.ObserCode_Select = new SelectList(new ObservationCode().observationCode(), "chinese", "chinese");

            if (ModelState.IsValid)
            {
                handler.OnBeforeRequest += (sender, e) =>
                {
                    e.RawRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", cookies.FHIR_Token_Cookie(HttpContext));
                };
                FhirClient client = new FhirClient(cookies.FHIR_URL_Cookie(HttpContext), cookies.settings, handler);
                try
                {
                    Observation observation = new Observation()
                    {
                        Status = (ObservationStatus)model.status,
                        BasedOn = new List<ResourceReference>
                        {
                            new ResourceReference
                            {
                                Reference = model.basedOn,
                            }
                        },
                        Subject = new ResourceReference
                        {
                            Reference = model.subject
                        },
                        Effective = new FhirDateTime(model.effectiveDateTime)
                    };

                    var observationCategory_Value = ObservationCode_Select_Switch(model.Code_value, model.component);

                    observation.Category = observationCategory_Value.Category;
                    observation.Code = observationCategory_Value.Code;
                    observation.Value = observationCategory_Value.Value;
                    observation.Component = observationCategory_Value.Component;

                    var observation_ToJson = observation.ToJson();
                    //如果找到同樣資料，會回傳該筆資料，但如果找到多筆資料，會產生Error
                    var created_obser = client.Create<Observation>(observation);
                    TempData["status"] = "Create succcess! Reference url:" + created_obser.Id;
                    return RedirectToAction("Index");
                }
                catch (Exception e)
                {
                    return View(model);
                }
            }
            return View(model);
        }

        public ObservationCategory_Value ObservationCode_Select_Switch(Obser_Code_Value obser_Code_Value, Obser_Code_Value[] obser_component)
        {
            ObservationCategory_Value observationCategory_Value = new ObservationCategory_Value();
            switch (obser_Code_Value.code_display)
            {
                case "身高":
                    observationCategory_Value = new ObservationCategory_Value().Body_Height(obser_Code_Value.value);
                    break;
                case "體重":
                    observationCategory_Value = new ObservationCategory_Value().Body_Weight(obser_Code_Value.value);
                    break;
                case "體溫":
                    observationCategory_Value = new ObservationCategory_Value().Body_Temperature(obser_Code_Value.value);
                    break;
                case "餐後血糖":
                    observationCategory_Value = new ObservationCategory_Value().Blood_Glucose_Post_Meal(obser_Code_Value.value);
                    break;
                case "餐前血糖":
                    observationCategory_Value = new ObservationCategory_Value().Blood_Glucose_Pre_Meal(obser_Code_Value.value);
                    break;
                case "體指百分率":
                    observationCategory_Value = new ObservationCategory_Value().Percentage_of_body_fat_Measured(obser_Code_Value.value);
                    break;
                case "握力":
                    observationCategory_Value = new ObservationCategory_Value().Grip_strength_Hand_right_Dynamometer(obser_Code_Value.value);
                    break;
                case "SPO2血氧飽和濃度":
                    observationCategory_Value = new ObservationCategory_Value().Oxygen_saturation_in_Arterial_blood_by_Pulse_oximetry(obser_Code_Value.value);
                    break;
                case "心率":
                    observationCategory_Value = new ObservationCategory_Value().Heart_Rate(obser_Code_Value.value);
                    break;
                case "收縮壓":
                    observationCategory_Value = new ObservationCategory_Value().Systolic_Blood_Pressure(obser_Code_Value.value);
                    break;
                case "舒張壓":
                    observationCategory_Value = new ObservationCategory_Value().Distolic_Blood_Pressure(obser_Code_Value.value);
                    break;
                case "血壓":
                    decimal? Systolic = obser_component.Where(o => o.code_display == "收縮壓").FirstOrDefault().value; //收縮壓
                    decimal? Distolic = obser_component.Where(o => o.code_display == "舒張壓").FirstOrDefault().value; //舒張壓
                    observationCategory_Value = new ObservationCategory_Value().Blood_Pressure_Panel(Systolic, Distolic);
                    break;
                case "心率(EMS)":
                    observationCategory_Value = new ObservationCategory_Value().Heart_Rate_EMS(obser_Code_Value.value);
                    break;
                case "呼吸頻率(EMS)":
                    observationCategory_Value = new ObservationCategory_Value().Respiratory_Rate_EMS(obser_Code_Value.value);
                    break;
                case "微血管充填時間(EMS)":
                    observationCategory_Value = new ObservationCategory_Value().Capillary_refill_of_Nail_bed_EMS(obser_Code_Value.value);
                    break;
                case "血糖(EMS)":
                    observationCategory_Value = new ObservationCategory_Value().Glucose_in_Blood_EMS(obser_Code_Value.value);
                    break;

                    //default: 保持原來資料
            }

            return observationCategory_Value;
        }

        //// GET: Observation/Update/5
        //public ActionResult Update(string id)
        //{
        //    return View();
        //}

        //// POST: Observation/Update/5
        //[HttpPost]
        //public ActionResult Update(string id, FormCollection collection)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    FhirClient client = new FhirClient(cookies.FHIR_URL_Cookie(HttpContext), cookies.settings, handler);
        //    try
        //    {
        //        var Obser = client.Read<Observation>("Observation/" + id);
        //        var Obser_view = new ObservationViewModel().ObservationViewModelMapping(Obser);
        //        return View(Obser_view);
        //    }
        //    catch (Exception e)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //}

        //// GET: Observation/Delete/5
        //public ActionResult Delete(string id)
        //{
        //    return View();
        //}

        //// POST: Observation/Delete/5
        //[HttpPost]
        //public ActionResult Delete(string id, FormCollection collection)
        //{
        //    try
        //    {
        //        // TODO: Add delete logic here

        //        return RedirectToAction("Index");
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}
    }
}
