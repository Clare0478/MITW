using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
                List<MedicationRequest> MedicationRequests = new List<MedicationRequest>();
                foreach (var entry in MedicationRequestBundle.Entry)
                {
                    //string a;
                    //if (((MedicationRequest)entry.Resource).Medication.TypeName == "Reference")
                    //    a = ((ResourceReference)((MedicationRequest)entry.Resource).Medication).Url.ToString();
                    //else
                    //    a = ((CodeableConcept)((MedicationRequest)entry.Resource).Medication).Coding[0].Display;
                    MedicationRequests.Add((MedicationRequest)entry.Resource);
                }

                return View(MedicationRequests);
            }
            catch (Exception e)
            {
                ViewBag.Error = e.ToString();
                return View();
            }
        }

        // GET: MedicationRequest/Details/5
        public ActionResult Details(int id)
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
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: MedicationRequest/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: MedicationRequest/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
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
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: MedicationRequest/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
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
