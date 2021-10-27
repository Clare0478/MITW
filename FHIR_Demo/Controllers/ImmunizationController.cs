using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Mvc;

namespace FHIR_Demo.Controllers
{
    public class ImmunizationController : Controller
    {
        private CookiesController cookies = new CookiesController();
        private HttpClientEventHandler handler = new HttpClientEventHandler();

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
            if (ModelState.IsValid)
            {
                handler.OnBeforeRequest += (sender, e) =>
                {
                    e.RawRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", cookies.FHIR_Token_Cookie(HttpContext));
                };
                FhirClient client = new FhirClient(cookies.FHIR_URL_Cookie(HttpContext), cookies.settings, handler);
                try
                {

                    //如果找到同樣資料，會回傳該筆資料，但如果找到多筆資料，會產生Error
                    //var created_obser = client.Create<>();
                    //TempData["status"] = "Create succcess! Reference url:" + created_obser.Id;
                    return RedirectToAction("Index");
                }
                catch (Exception e)
                {
                    return View(model);
                }
            }
            return View(model);
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
