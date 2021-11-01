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


namespace FHIR_Demo.Controllers
{
    public class EMSController : Controller
    {
        private CookiesController cookies = new CookiesController();
        private HttpClientEventHandler handler = new HttpClientEventHandler();

        // GET: EMS
        public ActionResult Index(PatientViewModel model)
        {
            handler.OnBeforeRequest += (sender, e) =>
            {
                e.RawRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", cookies.FHIR_Token_Cookie(HttpContext));
            };
            FhirClient client = new FhirClient(cookies.FHIR_URL_Cookie(HttpContext), cookies.settings, handler);
            ViewBag.status = TempData["status"];
            try
            {

                var q = new SearchParams().Where("organization=MITW.ForEMS");
                Bundle PatientSearchBundle = client.Search<Patient>(q);
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
        public ActionResult GetRecord(string url, string token, string search)
        {
            handler.OnBeforeRequest += (sender, e) =>
            {
                e.RawRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", cookies.FHIR_Token_Cookie(HttpContext, token));
            };
            FhirClient client = new FhirClient(cookies.FHIR_URL_Cookie(HttpContext, url), cookies.settings, handler);

            try
            {
                var q = SearchParams.FromUriParamList(UriParamList.FromQueryString(search)).Where("organization=MITW.ForEMS").LimitTo(20);

                Bundle PatientSearchBundle = client.Search<Patient>(q);
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

        // GET: EMS/Details/5
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
                var pat_A = client.Read<Patient>("Patient/" + id);
                var pat_view = new PatientViewModel().PatientViewModelMapping(pat_A);

                var z = new SearchParams().Where("subject=Patient/" + id);
                Bundle ObservationBundle = client.Search<Observation>(z);
                //var json = PatientSearchBundle.ToJson();
                List<ObservationViewModel> observationViewModels = new List<ObservationViewModel>();
                foreach (var entry in ObservationBundle.Entry)
                {
                    observationViewModels.Add(new ObservationViewModel().ObservationViewModelMapping((Observation)entry.Resource));
                }
                ViewBag.Obser_view = observationViewModels;

                return View(pat_view);


            }
            catch
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        // GET: EMS/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: EMS/Create
        [HttpPost]
        public ActionResult Create(PatientViewModel model)
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
                        Active = true,
                        BirthDate = model.birthDate,
                        Gender = (AdministrativeGender)model.Gender,
                        Identifier = new List<Identifier> {
                            new Identifier
                            {
                                System = "https://www.dicom.org.tw/cs/identityCardNumber_tw",
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
                                Text = model.zipcode+" 臺灣省"+model.city+model.town+model.address,
                                Country = "臺灣省",
                                PostalCode = model.zipcode,
                                City = model.city,
                                District = model.town,
                                Line = new List<string>
                                {
                                    model.address
                                }
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
                                    new CodeableConcept("http://terminology.hl7.org/CodeSystem/v2-0131", "C", model.contact_relationship)
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
                        },
                        Deceased = new FhirBoolean
                        {
                            Value = Convert.ToBoolean(model.deceased)
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

        // GET: EMS/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

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
                var pat_A = client.Read<Patient>("Patient/" + id);
                var pat_view = new PatientViewModel().PatientViewModelMapping(pat_A);
                return View(pat_view);
            }
            catch (Exception e)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        // POST: EMS/Edit/5
        [HttpPost]
        public ActionResult Update(string id, PatientViewModel model)
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
                            System = "https://www.dicom.org.tw/cs/identityCardNumber_tw",
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
                            Text = model.zipcode+" 臺灣省"+model.city+model.town+model.address,
                            Country = "臺灣省",
                            PostalCode = model.zipcode,
                            City = model.city,
                            District = model.town,
                            Line = new List<string>
                            {
                                model.address
                            }
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
                                new CodeableConcept("http://hl7.org/fhir/ValueSet/patient-contactrelationship", "N", model.contact_relationship)
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
                    pat_A.ManagingOrganization = new ResourceReference
                    {
                        Reference = model.managingOrganization
                    };
                    pat_A.Deceased = new FhirBoolean
                    {
                        Value = Convert.ToBoolean(model.deceased)
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

        // GET: EMS/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: EMS/Delete/5
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

        // GET: EMS/CreateObservation
        public ActionResult CreateObservation(string Patient)
        {
            ViewBag.ObserCode_Select = new SelectList(new ObservationCode().observationCode(), "chinese", "chinese");
            ViewBag.Subject_ID = Patient;
            return View();
        }

        // POST: EMS/CreateObservation
        [HttpPost]
        public ActionResult CreateObservation(ObservationViewModel model)
        {
            ViewBag.ObserCode_Select = new SelectList(new ObservationCode().observationCode(), "chinese", "chinese");

            if (ModelState.IsValid)
            {
                handler.OnBeforeRequest += (sender, e) =>
                {
                    e.RawRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", cookies.FHIR_Token_Cookie(HttpContext));
                };
                FhirClient client = new FhirClient(cookies.FHIR_URL_Cookie(HttpContext), cookies.settings, handler);
                try
                {
                    Observation observation = new Observation()
                    {
                        Status = (ObservationStatus)model.status,
                        BasedOn = new List<ResourceReference>
                        {
                            new ResourceReference
                            {
                                Reference = model.basedOn,
                            }
                        },
                        Subject = new ResourceReference
                        {
                            Reference = model.subject
                        },
                        Effective = new FhirDateTime(model.effectiveDateTime)
                    };
                    var observationCategory_Value = new ObservationController().ObservationCode_Select_Switch(model.Code_value, model.component);

                    observation.Category = observationCategory_Value.Category;
                    observation.Code = observationCategory_Value.Code;
                    observation.Value = observationCategory_Value.Value;
                    observation.Component = observationCategory_Value.Component;

                    var observation_ToJson = observation.ToJson();
                    //如果找到同樣資料，會回傳該筆資料，但如果找到多筆資料，會產生Error
                    var created_obser = client.Create<Observation>(observation);
                    TempData["status"] = "Create succcess! Reference url:" + created_obser.Id;
                    return RedirectToAction("Index");
                }
                catch (Exception e)
                {
                    return View(model);
                }
            }
            return View(model);
        }

        //private ObservationCategory_Value ObservationCode_Select_Switch(Obser_Code_Value obser_Code_Value, Obser_Code_Value[] obser_component)
        //{
        //    ObservationCategory_Value observationCategory_Value = new ObservationCategory_Value();
        //    switch (obser_Code_Value.code_display)
        //    {
        //        case "身高":
        //            observationCategory_Value = new ObservationCategory_Value().Body_Height(obser_Code_Value.value);
        //            break;
        //        case "體重":
        //            observationCategory_Value = new ObservationCategory_Value().Body_Weight(obser_Code_Value.value);
        //            break;
        //        case "體溫":
        //            observationCategory_Value = new ObservationCategory_Value().Body_Temperature(obser_Code_Value.value);
        //            break;
        //        case "餐後血糖":
        //            observationCategory_Value = new ObservationCategory_Value().Blood_Glucose_Post_Meal(obser_Code_Value.value);
        //            break;
        //        case "餐前血糖":
        //            observationCategory_Value = new ObservationCategory_Value().Blood_Glucose_Pre_Meal(obser_Code_Value.value);
        //            break;
        //        case "體指百分率":
        //            observationCategory_Value = new ObservationCategory_Value().Percentage_of_body_fat_Measured(obser_Code_Value.value);
        //            break;
        //        case "握力_右手測力計":
        //            observationCategory_Value = new ObservationCategory_Value().Grip_strength_Hand_right_Dynamometer(obser_Code_Value.value);
        //            break;
        //        case "動脈血氧飽和度":
        //            observationCategory_Value = new ObservationCategory_Value().Oxygen_saturation_in_Arterial_blood_by_Pulse_oximetry(obser_Code_Value.value);
        //            break;
        //        case "心率":
        //            observationCategory_Value = new ObservationCategory_Value().Heart_Rate(obser_Code_Value.value);
        //            break;
        //        case "收縮壓":
        //            observationCategory_Value = new ObservationCategory_Value().Systolic_Blood_Pressure(obser_Code_Value.value);
        //            break;
        //        case "舒張壓":
        //            observationCategory_Value = new ObservationCategory_Value().Distolic_Blood_Pressure(obser_Code_Value.value);
        //            break;
        //        case "血壓":
        //            decimal? Systolic = obser_component.Where(o => o.code_display == "收縮壓").FirstOrDefault().value; //收縮壓
        //            decimal? Distolic = obser_component.Where(o => o.code_display == "舒張壓").FirstOrDefault().value; //舒張壓
        //            observationCategory_Value = new ObservationCategory_Value().Blood_Pressure_Panel(Systolic, Distolic);
        //            break;
        //            //default: 保持原來資料
        //    }

        //    return observationCategory_Value;
        //}
    }
}
