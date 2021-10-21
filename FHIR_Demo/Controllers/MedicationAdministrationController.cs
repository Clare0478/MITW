using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FHIR_Demo.Controllers
{
    public class MedicationAdministrationController : Controller
    {
        private CookiesController cookies = new CookiesController();

        // GET: MedicationAdministration
        public ActionResult Index()
        {
            FhirClient client = new FhirClient(cookies.FHIR_URL_Cookie(HttpContext), cookies.settings);
            ViewBag.status = TempData["status"];
            try
            {
                Bundle MedicationAdministrationBundle = client.Search<MedicationAdministration>(null);
                //var json = PatientSearchBundle.ToJson();
                //List<PatientViewModel> patientViewModels = new List<PatientViewModel>();
                List<MedicationAdministration> MedicationAdministrations = new List<MedicationAdministration>();
                foreach (var entry in MedicationAdministrationBundle.Entry)
                {
                    var b = new FhirDateTime().TypeName;
                    var a = ((MedicationAdministration)entry.Resource).Effective;
                    MedicationAdministrations.Add((MedicationAdministration)entry.Resource);
                }

                return View(MedicationAdministrations);
            }
            catch (Exception e)
            {
                ViewBag.Error = e.ToString();
                return View();
            }
        }

        // GET: MedicationAdministration/Details/5
        public ActionResult Details(string id)
        {
            return View();
        }

        // GET: MedicationAdministration/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: MedicationAdministration/Create
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

        // GET: MedicationAdministration/Edit/5
        public ActionResult Edit(string id)
        {
            return View();
        }

        // POST: MedicationAdministration/Edit/5
        [HttpPost]
        public ActionResult Edit(string id, FormCollection collection)
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

        // GET: MedicationAdministration/Delete/5
        public ActionResult Delete(string id)
        {
            return View();
        }

        // POST: MedicationAdministration/Delete/5
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
