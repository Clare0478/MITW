using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FHIR_Demo.Controllers
{
    public class MedicationAdministrationController : Controller
    {
        // GET: MedicationAdministration
        public ActionResult Index()
        {
            return View();
        }

        // GET: MedicationAdministration/Details/5
        public ActionResult Details(int id)
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
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: MedicationAdministration/Edit/5
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

        // GET: MedicationAdministration/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: MedicationAdministration/Delete/5
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
