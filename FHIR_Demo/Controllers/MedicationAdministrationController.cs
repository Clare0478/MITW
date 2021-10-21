﻿using FHIR_Demo.Models;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static Hl7.Fhir.Model.MedicationAdministration;

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
        public ActionResult GetRecord(string url)
        {
            FhirClient client = new FhirClient(cookies.FHIR_URL_Cookie(HttpContext, url), cookies.settings);
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
            return View();
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
                FhirClient client = new FhirClient(cookies.FHIR_URL_Cookie(HttpContext), cookies.settings);

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
                    TempData["status"] = "Create succcess! Reference url:"+ created_MedAdmin.Id;
                    return RedirectToAction("Index");
                }
                catch
                {
                    return View(model);
                }
            }
            return View(model);
        }

        // GET: MedicationAdministration/Edit/5
        public ActionResult Edit(string id)
        {
            return View();
        }

        // POST: MedicationAdministration/Edit/5
        [HttpPost]
        public ActionResult Edit(string id, MedicationAdministrationViewModel model)
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
