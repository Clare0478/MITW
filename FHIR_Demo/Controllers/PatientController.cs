using FHIR_Demo.Models;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace FHIR_Demo.Controllers
{
    public class PatientController : Controller
    {
        private CookiesController cookies = new CookiesController();
        private HttpClientEventHandler handler = new HttpClientEventHandler();


        // GET: Patient
        public ActionResult Index()
        {
            //讓系統通過對於不安全的https連線
            handler.ServerCertificateCustomValidationCallback += (sender2, cert, chain, sslPolicyErrors) => true;
            //不知道錯哪裡
            if (cookies.FHIR_Server_Cookie(HttpContext) == "IBM")
            {
                //使用Basic 登入
                handler.OnBeforeRequest += (sender, e) =>
                {
                    e.RawRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(cookies.FHIR_Token_Cookie(HttpContext))));
                };
            }
            else
            {
                handler.OnBeforeRequest += (sender, e) =>
                {
                    e.RawRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", cookies.FHIR_Token_Cookie(HttpContext));
                };
            }
            
            FhirClient client = new FhirClient(cookies.FHIR_URL_Cookie(HttpContext), cookies.settings, handler);
            ViewBag.status = TempData["status"];
            ViewBag.Error = TempData["Error"];
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
                //ViewBag.Error = e.ToString();
                ViewBag.Error = "發生錯誤";
                return View();
            }
        }

        [HttpPost]
        public ActionResult GetRecord(string url, string token, string search)
        {
            //讓系統通過對於不安全的https連線
            handler.ServerCertificateCustomValidationCallback += (sender2, cert, chain, sslPolicyErrors) => true;
            if (cookies.FHIR_Server_Cookie(HttpContext) == "IBM")
            {
                //使用Basic 登入
                handler.OnBeforeRequest += (sender, e) =>
                {
                    e.RawRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(cookies.FHIR_Token_Cookie(HttpContext))));
                };
            }
            else
            {
                handler.OnBeforeRequest += (sender, e) =>
                {
                    e.RawRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", cookies.FHIR_Token_Cookie(HttpContext));
                };
            }
            FhirClient client = new FhirClient(cookies.FHIR_URL_Cookie(HttpContext, url), cookies.settings, handler);

            try
            {
                var q = SearchParams.FromUriParamList(UriParamList.FromQueryString(search)).LimitTo(20);

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
                //ViewBag.Error = "發生錯誤";
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
            //讓系統通過對於不安全的https連線
            handler.ServerCertificateCustomValidationCallback += (sender2, cert, chain, sslPolicyErrors) => true;
            if (cookies.FHIR_Server_Cookie(HttpContext) == "IBM")
            {
                //使用Basic 登入
                handler.OnBeforeRequest += (sender, e) =>
                {
                    e.RawRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(cookies.FHIR_Token_Cookie(HttpContext))));
                };
            }
            else
            {
                handler.OnBeforeRequest += (sender, e) =>
                {
                    e.RawRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", cookies.FHIR_Token_Cookie(HttpContext));
                };
            }
            //handler.OnBeforeRequest += (sender, e) =>
            //{
            //    e.RawRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", cookies.FHIR_Token_Cookie(HttpContext));
            //};
            FhirClient client = new FhirClient(cookies.FHIR_URL_Cookie(HttpContext), cookies.settings, handler);
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
        public async Task<ActionResult>  Create(PatientViewModel model)
        {
            if (ModelState.IsValid)
            {
                //讓系統通過對於不安全的https連線
                handler.ServerCertificateCustomValidationCallback += (sender2, cert, chain, sslPolicyErrors) => true;
                if (cookies.FHIR_Server_Cookie(HttpContext) == "IBM")
                {
                    //使用Basic 登入
                    handler.OnBeforeRequest += (sender, e) =>
                    {
                        e.RawRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(cookies.FHIR_Token_Cookie(HttpContext))));
                    };
                }
                else
                {
                    handler.OnBeforeRequest += (sender, e) =>
                    {
                        e.RawRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", cookies.FHIR_Token_Cookie(HttpContext));
                    };
                }
                
                FhirClient client = new FhirClient(cookies.FHIR_URL_Cookie(HttpContext), cookies.settings, handler);
                try
                {
                    //製造Patient
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

                    patient.Meta = new Meta
                    {
                        Profile = new List<string> { model.meta }
                    };

                    var conditions = new SearchParams();
                    conditions.Add("identifier", model.identifier);
                    var b = patient.Identifier;
                    var patient_ToJson = patient.ToJson();
                    //如果找到同樣資料，會回傳該筆資料，但如果找到多筆資料，會產生Error
                    //var created_pat_A = client.Create<Patient>(patient, conditions);
                    if (cookies.FHIR_Server_Cookie(HttpContext) == "IBM")
                    {
                        var resullt = await GetandShare_Block(patient_ToJson);
                        int resulltnumber1 = resullt.IndexOf("/Patient/") + 9;
                        int resulltnumber2 = resullt.IndexOf("/_history");
                        int length = resulltnumber2 - resulltnumber1;
                        var resulltid = resullt.Substring(resulltnumber1, length);
                        TempData["status"] = "Create succcess! Reference url:" + resulltid;

                    }
                    else
                    {
                        var created_pat_A = client.Create<Patient>(patient, conditions);
                        TempData["status"] = "Create succcess! Reference url:" + created_pat_A.Id;
                    }

                    //var created_pat_A = client.Create<Patient>(patient);
                    //TempData["status"] = "Create succcess! Reference url:" + created_pat_A.Id;
                    return RedirectToAction("Index");
                }
                catch (Exception e)
                {
                    //TempData["Error"] = e.ToString();
                    TempData["Error"] = "發生錯誤或該資料已存在";
                    return RedirectToAction("Index");
                    //return View();
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
            if (cookies.FHIR_Server_Cookie(HttpContext) == "IBM")
            {
                //讓系統通過對於不安全的https連線
                handler.ServerCertificateCustomValidationCallback += (sender2, cert, chain, sslPolicyErrors) => true;
                //使用Basic 登入
                handler.OnBeforeRequest += (sender, e) =>
                {
                    e.RawRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(cookies.FHIR_Token_Cookie(HttpContext))));
                };
            }
            else
            {
                handler.OnBeforeRequest += (sender, e) =>
                {
                    e.RawRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", cookies.FHIR_Token_Cookie(HttpContext));
                };
            }
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

        // POST: Patient/Edit/5
        [HttpPost]
        public async Task<ActionResult> Update(string id, PatientViewModel model)
        {
            if (ModelState.IsValid)
            {
                //讓系統通過對於不安全的https連線
                handler.ServerCertificateCustomValidationCallback += (sender2, cert, chain, sslPolicyErrors) => true;
                if (cookies.FHIR_Server_Cookie(HttpContext) == "IBM")
                {
                    //使用Basic 登入
                    handler.OnBeforeRequest += (sender, e) =>
                    {
                        e.RawRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(cookies.FHIR_Token_Cookie(HttpContext))));
                    };
                }
                else
                {
                    handler.OnBeforeRequest += (sender, e) =>
                    {
                        e.RawRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", cookies.FHIR_Token_Cookie(HttpContext));
                    };
                }
                //handler.OnBeforeRequest += (sender, e) =>
                //{
                //    e.RawRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", cookies.FHIR_Token_Cookie(HttpContext));
                //};
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
                    var a = pat_A.ToJson();
                    var updated_pat = client.Update<Patient>(pat_A);
                    //if (cookies.FHIR_Server_Cookie(HttpContext) == "IBM")
                    //{
                    //    var resullt = await GetandShare_Block(a);
                    //    int resulltnumber1 = resullt.IndexOf("/Patient/") + 9;
                    //    int resulltnumber2 = resullt.IndexOf("/_history");
                    //    int length = resulltnumber2 - resulltnumber1;
                    //    var resulltid = resullt.Substring(resulltnumber1, length);
                    //    TempData["status"] = "Create succcess! Reference url:" + resulltid;

                    //}
                    //else
                    //{
                    //    var updated_pat = client.Update<Patient>(pat_A);
                    //}

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


        //測試
        [HttpPost]
        public async Task<dynamic> GetandShare_Block(string bundlejson)
        {
            //var json = JsonConvert.SerializeObject(Post_data);
            var data = new StringContent(bundlejson, Encoding.UTF8, "application/json");

            //var url = "http://localhost:12904/api/Geth/" + Request_Url;
            var url = cookies.FHIR_URL_Cookie(HttpContext)+"/Patient";
            var Token = cookies.FHIR_Token_Cookie(HttpContext);

            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            HttpClient client = new HttpClient();

            //specify to use TLS 1.2 as default connection
            var byteArray = Encoding.ASCII.GetBytes(Token);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            //POST資料
            var response = await client.PostAsync(url, data);
            var resultheader = response.Headers.Location.LocalPath.ToString();
            return resultheader;
        }
    }
}
