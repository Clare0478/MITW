﻿using FHIR_Demo.Models;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace FHIR_Demo.Controllers
{
    public class PatientController : Controller
    {
        private CookiesController cookies = new CookiesController();

        // GET: Patient
        public ActionResult Index()
        {
            FhirClient client = new FhirClient(cookies.FHIR_URL_Cookie(HttpContext), cookies.settings);
            ViewBag.status = TempData["status"];
            try
            {
                Bundle PatientSearchBundle = client.Search<Patient>(null);
                //var json = PatientSearchBundle.ToJson();
                List<PatientViewModel> patientViewModels = new List<PatientViewModel>();
                foreach (var entry in PatientSearchBundle.Entry)
                {
                    patientViewModels.Add(new PatientViewModel().PatientViewModelMapping((Patient)entry.Resource));
                }

                return View(patientViewModels);
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
            try
            {
                FhirClient client = new FhirClient(cookies.FHIR_URL_Cookie(HttpContext, url), cookies.settings);

                Bundle PatientSearchBundle = client.Search<Patient>(null);
                var json = PatientSearchBundle.ToJson();
                List<PatientViewModel> patientViewModels = new List<PatientViewModel>();
                foreach (var entry in PatientSearchBundle.Entry)
                {
                    patientViewModels.Add(new PatientViewModel().PatientViewModelMapping((Patient)entry.Resource));
                }

                return PartialView("_GetRecord", patientViewModels);
            }
            catch (Exception e)
            {
                ViewBag.Error = e.ToString();
                return PartialView("_GetRecord");
            }
        }

        // GET: Patient/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FhirClient client = new FhirClient(cookies.FHIR_URL_Cookie(HttpContext), cookies.settings);
            try
            {
                var pat_A = client.Read<Patient>("Patient/" + id);
                var pat_view = new PatientViewModel().PatientViewModelMapping(pat_A);
                return View(pat_view);
            }
            catch
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        // GET: Patient/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Patient/Create
        [HttpPost]
        public ActionResult Create(PatientViewModel model)
        {
            if (ModelState.IsValid)
            {
                FhirClient client = new FhirClient(cookies.FHIR_URL_Cookie(HttpContext), cookies.settings);
                try
                {
                    Patient patient = new Patient()
                    {
                        Name = new List<HumanName>()
                        {
                            new HumanName()
                            {
                                Text = model.name,
                                Given = new List<string>
                                {
                                    model.name,
                                }
                            }
                        },
                        BirthDate = model.birthDate,
                        Gender = (AdministrativeGender)model.Gender,
                        Identifier = new List<Identifier> {
                            new Identifier
                            {
                                Value = model.identifier
                            }
                        },
                        Telecom = new List<ContactPoint>
                        {
                            new ContactPoint
                            {
                                System = ContactPoint.ContactPointSystem.Phone,
                                Value = model.telecom
                            },
                            new ContactPoint
                            {
                                System = ContactPoint.ContactPointSystem.Email,
                                Value = model.email
                            },
                        },
                        Address = new List<Address>
                        {
                            new Address
                            {
                                Text = model.address
                            }
                        },
                        Contact = new List<Patient.ContactComponent>
                        {
                            new Patient.ContactComponent
                            {
                                Name = new HumanName()
                                {
                                    Text = model.contact_name,
                                    Given = new List<string>
                                    {
                                        model.contact_name,
                                    }
                                },
                                Relationship = new List<CodeableConcept>
                                {
                                    new CodeableConcept("http://terminology.hl7.org/CodeSystem/v2-0131", "N", model.contact_relationship)
                                },
                                Telecom = new List<ContactPoint>
                                {
                                    new ContactPoint
                                    {
                                        System = ContactPoint.ContactPointSystem.Phone,
                                        Value = model.contact_telecom
                                    },
                                }

                            },
                        },
                        ManagingOrganization = new ResourceReference
                        {
                            Reference = model.managingOrganization
                        }
                    };

                    var conditions = new SearchParams();
                    conditions.Add("identifier", model.identifier);

                    var patient_ToJson = patient.ToJson();
                    //如果找到同樣資料，會回傳該筆資料，但如果找到多筆資料，會產生Error
                    var created_pat_A = client.Create<Patient>(patient, conditions);
                    TempData["status"] = "Create succcess! Reference url:" + created_pat_A.Id;
                    return RedirectToAction("Index");
                }
                catch (Exception e)
                {
                    return View();
                }
            }
            return View(model);
        }

        // GET: Patient/Edit/5
        public ActionResult Update(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FhirClient client = new FhirClient(cookies.FHIR_URL_Cookie(HttpContext), cookies.settings);
            try
            {
                var pat_A = client.Read<Patient>("Patient/" + id);
                var pat_view = new PatientViewModel().PatientViewModelMapping(pat_A);
                return View(pat_view);
            }
            catch (Exception e)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        // POST: Patient/Edit/5
        [HttpPost]
        public ActionResult Update(string id, PatientViewModel model)
        {
            if (ModelState.IsValid)
            {
                FhirClient client = new FhirClient(cookies.FHIR_URL_Cookie(HttpContext), cookies.settings);
                try
                {
                    var pat_A = client.Read<Patient>("Patient/" + id);
                    pat_A.Name = new List<HumanName>()
                    {
                        new HumanName()
                        {
                            Text = model.name,
                            Given = new List<string>
                            {
                                model.name,
                            }
                        }
                    };
                    pat_A.Text = new Narrative();
                    pat_A.BirthDate = model.birthDate;
                    pat_A.Gender = (AdministrativeGender)model.Gender;
                    pat_A.Identifier = new List<Identifier> {
                        new Identifier
                        {
                            Value = model.identifier
                        }
                    };
                    pat_A.Telecom = new List<ContactPoint>
                    {
                        new ContactPoint
                        {
                            System = ContactPoint.ContactPointSystem.Phone,
                            Value = model.telecom
                        },
                        new ContactPoint
                        {
                            System = ContactPoint.ContactPointSystem.Email,
                            Value = model.email
                        },
                    };
                    pat_A.Address = new List<Address>
                    {
                        new Address
                        {
                            Text = model.address
                        }
                    };
                    pat_A.Contact = new List<Patient.ContactComponent>
                    {
                        new Patient.ContactComponent
                        {
                            Name = new HumanName()
                            {
                                Text = model.contact_name,
                                Given = new List<string>
                                {
                                    model.contact_name,
                                }
                            },
                            Relationship = new List<CodeableConcept>
                            {
                                new CodeableConcept("http://terminology.hl7.org/CodeSystem/v2-0131", "N", model.contact_relationship)
                            },
                            Telecom = new List<ContactPoint>
                            {
                                new ContactPoint
                                {
                                    System = ContactPoint.ContactPointSystem.Phone,
                                    Value = model.contact_telecom
                                },
                            }

                        },
                    };
                    var a = pat_A.ToJson();
                    var updated_pat = client.Update<Patient>(pat_A);

                    return RedirectToAction("Index");
                }
                catch
                {
                    return View(model);
                }
            }
            return View(model);

        }

        //// GET: Patient/Delete/5
        //public ActionResult Delete(int id)
        //{
        //    return View();
        //}

        //// POST: Patient/Delete/5
        //[HttpPost]
        //public ActionResult Delete(int id, FormCollection collection)
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