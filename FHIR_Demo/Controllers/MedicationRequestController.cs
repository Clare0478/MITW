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
using static Hl7.Fhir.Model.MedicationRequest;

namespace FHIR_Demo.Controllers
{
    public class MedicationRequestController : Controller
    {
        private CookiesController cookies = new CookiesController();
        private HttpClientEventHandler handler = new HttpClientEventHandler();

        // GET: MedicationRequest
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
                Bundle MedicationRequestBundle = client.Search<MedicationRequest>(null);
                //var json = PatientSearchBundle.ToJson();
                List<MedicationRequestViewModel> medicationRequestViewModels = new List<MedicationRequestViewModel>();
                foreach (var entry in MedicationRequestBundle.Entry)
                {
                    medicationRequestViewModels.Add(new MedicationRequestViewModel().MedicationRequestViewModelMapping((MedicationRequest)entry.Resource));
                }

                return View(medicationRequestViewModels);
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

                Bundle MedicationRequestBundle = client.Search<MedicationRequest>(q);
                //var json = PatientSearchBundle.ToJson();
                List<MedicationRequestViewModel> medicationRequestViewModels = new List<MedicationRequestViewModel>();
                foreach (var entry in MedicationRequestBundle.Entry)
                {
                    medicationRequestViewModels.Add(new MedicationRequestViewModel().MedicationRequestViewModelMapping((MedicationRequest)entry.Resource));
                }

                return PartialView("_GetRecord", medicationRequestViewModels);
            }
            catch (Exception e)
            {
                ViewBag.Error = e.ToString();
                return PartialView("_GetRecord");
            }
        }

        // GET: MedicationRequest/Details/5
        public ActionResult Details(string id)
        {
            return View();
        }

        // GET: MedicationRequest/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: MedicationRequest/Create
        [HttpPost]
        public ActionResult Create(MedicationRequestViewModel model)
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
                    MedicationRequest medicationRequest = new MedicationRequest();
                    medicationRequest.Status = (medicationrequestStatus)model.status;
                    medicationRequest.Intent = (medicationRequestIntent)model.intent;
                    medicationRequest.Category = model.categorys.ToList();
                    if (model.medicationReference != null)
                        medicationRequest.Medication = new ResourceReference(model.medicationReference);
                    if (model.medicationCodeableConcept != null)
                        medicationRequest.Medication = new CodeableConcept(model.medicationCodeableConcept.System, model.medicationCodeableConcept.Code, model.medicationCodeableConcept.Display);
                    medicationRequest.Subject = new ResourceReference(model.subject);
                    medicationRequest.AuthoredOn = model.authoredOn;
                    foreach (var dosag in model.dosageInstruction) 
                    {
                        medicationRequest.DosageInstruction.Add(new Dosage 
                        {
                            Sequence = dosag.sequence,
                            Text = dosag.text,
                            Timing = new Timing 
                            {
                                Code = new CodeableConcept(dosag.timing_Code.System, dosag.timing_Code.Code, dosag.timing_Code.Display, null)
                            },
                            Route = new CodeableConcept(dosag.route.System, dosag.route.Code, dosag.route.Display, null)
                        });
                    }
                    medicationRequest.DispenseRequest = new DispenseRequestComponent
                    {
                        ValidityPeriod = model.dispenseRequest.validityPeriod,
                        Quantity = model.dispenseRequest.quantity,
                        ExpectedSupplyDuration = model.dispenseRequest.expectedSupplyDuration
                    };

                    var created_MedReq = client.Create<MedicationRequest>(medicationRequest);
                    TempData["status"] = "Create succcess! Reference url:" + created_MedReq.Id;
                    return RedirectToAction("Index");
                }
                catch (Exception e)
                {
                    return View(model);
                }
            }
            return View(model);
        }

        // GET: MedicationRequest/Update/5
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
                var med = client.Read<MedicationRequest>("MedicationRequest/" + id);
                var med_view = new MedicationRequestViewModel().MedicationRequestViewModelMapping(med);
                return View(med_view);
            }
            catch (Exception e)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        // POST: MedicationRequest/Update/5
        [HttpPost]
        public ActionResult Update(string id, MedicationRequestViewModel model)
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
                    var medicationRequest = client.Read<MedicationRequest>("MedicationRequest/" + id);
                    medicationRequest.Status = (medicationrequestStatus)model.status;
                    medicationRequest.Intent = (medicationRequestIntent)model.intent;
                    medicationRequest.Category = model.categorys.ToList();
                    if (model.medicationReference != null)
                        medicationRequest.Medication = new ResourceReference(model.medicationReference);
                    if (model.medicationCodeableConcept != null)
                        medicationRequest.Medication = new CodeableConcept(model.medicationCodeableConcept.System, model.medicationCodeableConcept.Code, model.medicationCodeableConcept.Display);
                    medicationRequest.Subject = new ResourceReference(model.subject);
                    medicationRequest.AuthoredOn = model.authoredOn;
                    foreach (var dosag in model.dosageInstruction)
                    {
                        medicationRequest.DosageInstruction.Add(new Dosage
                        {
                            Sequence = dosag.sequence,
                            Text = dosag.text,
                            Timing = new Timing
                            {
                                Code = new CodeableConcept(dosag.timing_Code.System, dosag.timing_Code.Code, dosag.timing_Code.Display, null)
                            },
                            Route = new CodeableConcept(dosag.route.System, dosag.route.Code, dosag.route.Display, null)
                        });
                    }
                    medicationRequest.DispenseRequest = new DispenseRequestComponent
                    {
                        ValidityPeriod = model.dispenseRequest.validityPeriod,
                        Quantity = model.dispenseRequest.quantity,
                        ExpectedSupplyDuration = model.dispenseRequest.expectedSupplyDuration
                    };

                    var created_MedReq = client.Update<MedicationRequest>(medicationRequest);
                    TempData["status"] = "Create Update! Reference url:" + created_MedReq.Id;

                    return RedirectToAction("Index");
                }
                catch
                {
                    return View(model);
                }
            }
            return View(model);
        }

        // GET: MedicationRequest/Delete/5
        public ActionResult Delete(string id)
        {
            return View();
        }

        // POST: MedicationRequest/Delete/5
        [HttpPost]
        public ActionResult Delete(string id, MedicationRequestViewModel model)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
