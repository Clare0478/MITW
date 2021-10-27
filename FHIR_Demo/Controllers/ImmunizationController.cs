using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FHIR_Demo.Controllers
{
    public class ImmunizationController : Controller
    {
        // GET: Immunization
        public ActionResult Index()
        {
            Immunization a = new Immunization();
            Composition composition = new Composition();
            return View();
        }

        // GET: Immunization/Details/5
        public ActionResult Details(string id)
        {
            return View();
        }

        // GET: Immunization/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Immunization/Create
        [HttpPost]
        public ActionResult Create(Immunization model)
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

        // GET: Immunization/Edit/5
        public ActionResult Edit(string id)
        {
            return View();
        }

        // POST: Immunization/Edit/5
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

        // GET: Immunization/Delete/5
        public ActionResult Delete(string id)
        {
            return View();
        }

        // POST: Immunization/Delete/5
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
