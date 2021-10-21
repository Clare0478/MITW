using FHIR_Demo.Models;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static Hl7.Fhir.Model.MedicationRequest;

namespace FHIR_Demo.Controllers
{
    public class MedicationRequestController : Controller
    {
        private CookiesController cookies = new CookiesController();

        // GET: MedicationRequest
        public ActionResult Index()
        {
            FhirClient client = new FhirClient(cookies.FHIR_URL_Cookie(HttpContext), cookies.settings);
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
                FhirClient client = new FhirClient(cookies.FHIR_URL_Cookie(HttpContext), cookies.settings);
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

                    TempData["status"] = "Create succcess! Reference url:";
                    return RedirectToAction("Index");
                }
                catch (Exception e)
                {
                    return View(model);
                }
            }
            return View(model);
        }

        // GET: MedicationRequest/Edit/5
        public ActionResult Edit(string id)
        {
            return View();
        }

        // POST: MedicationRequest/Edit/5
        [HttpPost]
        public ActionResult Edit(string id, MedicationRequestViewModel model)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: MedicationRequest/Delete/5
        public ActionResult Delete(string id)
        {
            return View();
        }

        // POST: MedicationRequest/Delete/5
        [HttpPost]
        public ActionResult Delete(string id, FormCollection collection)
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
