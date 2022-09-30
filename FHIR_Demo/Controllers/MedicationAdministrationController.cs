using FHIR_Demo.Models;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Mvc;
using static Hl7.Fhir.Model.MedicationAdministration;

namespace FHIR_Demo.Controllers
{
    public class MedicationAdministrationController : Controller
    {
        private CookiesController cookies = new CookiesController();
        private HttpClientEventHandler handler = new HttpClientEventHandler();

        // GET: MedicationAdministration
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
                Bundle MedicationAdministrationBundle = client.Search<MedicationAdministration>(null);
                //var json = PatientSearchBundle.ToJson();
                //List<PatientViewModel> patientViewModels = new List<PatientViewModel>();
                List<MedicationAdministrationViewModel> medicationAdministrationViewModels = new List<MedicationAdministrationViewModel>();
                foreach (var entry in MedicationAdministrationBundle.Entry)
                {
                    medicationAdministrationViewModels.Add(new MedicationAdministrationViewModel().MedicationAdministrationViewModelMapping((MedicationAdministration)entry.Resource));
                }

                return View(medicationAdministrationViewModels);
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

                Bundle MedicationAdministrationBundle = client.Search<MedicationAdministration>(q);
                //var json = PatientSearchBundle.ToJson();
                //List<PatientViewModel> patientViewModels = new List<PatientViewModel>();
                List<MedicationAdministrationViewModel> medicationAdministrationViewModels = new List<MedicationAdministrationViewModel>();
                foreach (var entry in MedicationAdministrationBundle.Entry)
                {
                    medicationAdministrationViewModels.Add(new MedicationAdministrationViewModel().MedicationAdministrationViewModelMapping((MedicationAdministration)entry.Resource));
                }

                return PartialView("_GetRecord", medicationAdministrationViewModels);
            }
            catch (Exception e)
            {
                ViewBag.Error = e.ToString();
                return PartialView("_GetRecord");
            }
        }

        // GET: MedicationAdministration/Details/5
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
                var MedA = client.Read<MedicationAdministration>("MedicationAdministration/" + id);
                var MedA_view = new MedicationAdministrationViewModel().MedicationAdministrationViewModelMapping(MedA);

                return View(MedA_view);
            }
            catch
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        // GET: MedicationAdministration/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: MedicationAdministration/Create
        [HttpPost]
        public ActionResult Create(MedicationAdministrationViewModel model)
        {
            if (ModelState.IsValid)
            {
                handler.OnBeforeRequest += (sender, e) =>
                {
                    e.RawRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", cookies.FHIR_Token_Cookie(HttpContext));
                };
                FhirClient client = new FhirClient(cookies.FHIR_URL_Cookie(HttpContext), cookies.settings, handler);

                try
                {
                    MedicationAdministration medicationAdministration = new MedicationAdministration();
                    medicationAdministration.Status = (MedicationAdministrationStatusCodes)model.status;
                    if (model.medicationReference != null)
                        medicationAdministration.Medication = new ResourceReference(model.medicationReference);
                    if (model.medicationCodeableConcept != null)
                        medicationAdministration.Medication = new CodeableConcept(model.medicationCodeableConcept.System, model.medicationCodeableConcept.Code, model.medicationCodeableConcept.Display);
                    medicationAdministration.Subject = new ResourceReference(model.subject);
                    medicationAdministration.Effective = model.effectivePeriod;
                    medicationAdministration.Request = new ResourceReference(model.request);
                    medicationAdministration.Dosage = model.dosage;

                    var created_MedAdmin = client.Create<MedicationAdministration>(medicationAdministration);
                    TempData["status"] = "Create succcess! Reference url:" + created_MedAdmin.Id;
                    return RedirectToAction("Index");
                }
                catch
                {
                    return View(model);
                }
            }
            return View(model);
        }

        // GET: MedicationAdministration/Update/5
        public ActionResult Update(string id)
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
                var med = client.Read<MedicationAdministration>("MedicationAdministration/" + id);
                var med_view = new MedicationAdministrationViewModel().MedicationAdministrationViewModelMapping(med);
                return View(med_view);
            }
            catch (Exception e)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        // POST: MedicationAdministration/Update/5
        [HttpPost]
        public ActionResult Update(string id, MedicationAdministrationViewModel model)
        {
            if (ModelState.IsValid)
            {
                handler.OnBeforeRequest += (sender, e) =>
                {
                    e.RawRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", cookies.FHIR_Token_Cookie(HttpContext));
                };
                FhirClient client = new FhirClient(cookies.FHIR_URL_Cookie(HttpContext), cookies.settings, handler);

                try
                {
                    var medicationAdministration = client.Read<MedicationAdministration>("MedicationAdministration/" + id);
                    medicationAdministration.Status = (MedicationAdministrationStatusCodes)model.status;
                    if (model.medicationReference != null)
                        medicationAdministration.Medication = new ResourceReference(model.medicationReference);
                    if (model.medicationCodeableConcept != null)
                        medicationAdministration.Medication = new CodeableConcept(model.medicationCodeableConcept.System, model.medicationCodeableConcept.Code, model.medicationCodeableConcept.Display);
                    medicationAdministration.Subject = new ResourceReference(model.subject);
                    medicationAdministration.Effective = model.effectivePeriod;
                    medicationAdministration.Request = new ResourceReference(model.request);
                    medicationAdministration.Dosage = model.dosage;

                    var Update_MedAdmin = client.Update<MedicationAdministration>(medicationAdministration);
                    TempData["status"] = "Create Update! Reference url:" + Update_MedAdmin.Id;

                    return RedirectToAction("Index");
                }
                catch
                {
                    return View(model);
                }
            }
            return View(model);
        }

        //// GET: MedicationAdministration/Delete/5
        //public ActionResult Delete(string id)
        //{
        //    return View();
        //}

        //// POST: MedicationAdministration/Delete/5
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
