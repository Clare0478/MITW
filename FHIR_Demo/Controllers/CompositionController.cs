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
    public class CompositionController : Controller
    {
        private CookiesController cookies = new CookiesController();
        private HttpClientEventHandler handler = new HttpClientEventHandler();
        // GET: Composition
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
                Bundle CompositionSearchBundle = client.Search<Composition>(null);
                //var json = PatientSearchBundle.ToJson();
                List<CompositionViewModel> CompositionViewModels = new List<CompositionViewModel>();
                foreach (var entry in CompositionSearchBundle.Entry)
                {
                    CompositionViewModels.Add(new CompositionViewModel().CompositionViewModelMapping((Composition)entry.Resource));
                }

                return View(CompositionViewModels);
            }
            catch (Exception e)
            {
                //ViewBag.Error = e.ToString();
                ViewBag.Error = "發生錯誤";
                return View();
            }
        }
        public ActionResult Create()
        {
            return View();
        }

        // POST: Patient/Create
        [HttpPost]
        public async Task<ActionResult> Create(CompositionViewModel model)
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
                    //製造Compition
                    Composition composition = new Composition()
                    {
                        Status = CompositionStatus.Final,
                        Date =model.date,
                        Title=model.title,
                        Subject=new ResourceReference
                        {
                            Reference=model.subject
                        },
                        Author=new List<ResourceReference>()
                        {
                            new ResourceReference()
                            {
                                Reference=model.author
                            }
                        },
                        Section=new List<Composition.SectionComponent>()
                        {
                            new Composition.SectionComponent()
                            {
                                Title="Patient",
                                Entry=new List<ResourceReference>()
                                {
                                    new ResourceReference()
                                    {
                                        Reference=model.section_Patient
                                    }
                                }
                            },
                            new Composition.SectionComponent()
                            {
                                Title="Practitioner",
                                Entry=new List<ResourceReference>()
                                {
                                    new ResourceReference()
                                    {
                                        Reference=model.section_Practitioner
                                    }
                                }
                            },
                            new Composition.SectionComponent()
                            {
                                Title="Encounter",
                                Entry=new List<ResourceReference>()
                                {
                                    new ResourceReference()
                                    {
                                        Reference=model.section_Encounter
                                    }
                                }
                            },
                            new Composition.SectionComponent()
                            {
                                Title="Observation",
                                Entry=new List<ResourceReference>()
                                {
                                    new ResourceReference()
                                    {
                                        Reference=model.section_Observation1
                                    }
                                }
                            },
                            new Composition.SectionComponent()
                            {
                                Title="Observation",
                                Entry=new List<ResourceReference>()
                                {
                                    new ResourceReference()
                                    {
                                        Reference=model.section_Observation2
                                    }
                                }
                            },
                            new Composition.SectionComponent()
                            {
                                Title="Observation",
                                Entry=new List<ResourceReference>()
                                {
                                    new ResourceReference()
                                    {
                                        Reference=model.section_Observation3
                                    }
                                }
                            },
                            new Composition.SectionComponent()
                            {
                                Title="Observation",
                                Entry=new List<ResourceReference>()
                                {
                                    new ResourceReference()
                                    {
                                        Reference=model.section_Observation4
                                    }
                                }
                            },
                            new Composition.SectionComponent()
                            {
                                Title="Observation",
                                Entry=new List<ResourceReference>()
                                {
                                    new ResourceReference()
                                    {
                                        Reference=model.section_Observation5
                                    }
                                }
                            },
                            new Composition.SectionComponent()
                            {
                                Title="Observation",
                                Entry=new List<ResourceReference>()
                                {
                                    new ResourceReference()
                                    {
                                        Reference=model.section_Observation6
                                    }
                                }
                            },
                            new Composition.SectionComponent()
                            {
                                Title="Observation",
                                Entry=new List<ResourceReference>()
                                {
                                    new ResourceReference()
                                    {
                                        Reference=model.section_Observation7
                                    }
                                }
                            },
                            new Composition.SectionComponent()
                            {
                                Title="Observation",
                                Entry=new List<ResourceReference>()
                                {
                                    new ResourceReference()
                                    {
                                        Reference=model.section_Observation8
                                    }
                                }
                            },
                            new Composition.SectionComponent()
                            {
                                Title="AllergyIntolerance",
                                Entry=new List<ResourceReference>()
                                {
                                    new ResourceReference()
                                    {
                                        Reference=model.section_AllergyIntolerance
                                    }
                                }
                            },
                            new Composition.SectionComponent()
                            {
                                Title="Condition",
                                Entry=new List<ResourceReference>()
                                {
                                    new ResourceReference()
                                    {
                                        Reference=model.section_Condition1
                                    }
                                }
                            },
                            new Composition.SectionComponent()
                            {
                                Title="Condition",
                                Entry=new List<ResourceReference>()
                                {
                                    new ResourceReference()
                                    {
                                        Reference=model.section_Condition2
                                    }
                                }
                            },
                        },
                        Type=new CodeableConcept
                        {
                            Coding = new List<Coding>
                            {
                                new Coding
                                {
                                    Code = "11502-2",
                                    System = "http://loinc.org",
                                    Display = "Laboratory report.total"
                                },
                            }
                        }
                    };
                    composition.Meta = new Meta
                    {
                        Profile = new List<string> { model.meta }
                    };

                    //var conditions = new SearchParams();
                    //conditions.Add("identifier", model.identifier);
                    //var b = patient.Identifier;
                    var composition_ToJson = composition.ToJson();
                    //如果找到同樣資料，會回傳該筆資料，但如果找到多筆資料，會產生Error
                    //var created_pat_A = client.Create<Patient>(patient, conditions);
                    if (cookies.FHIR_Server_Cookie(HttpContext) == "IBM")
                    {
                        var resullt = await GetandShare_Block(composition_ToJson);
                        int resulltnumber1 = resullt.IndexOf("/Composition/") + 9;
                        int resulltnumber2 = resullt.IndexOf("/_history");
                        int length = resulltnumber2 - resulltnumber1;
                        var resulltid = resullt.Substring(resulltnumber1, length);
                        TempData["status"] = "Create succcess! Reference url:" + resulltid;

                    }
                    else
                    {
                        var created_comp_A = client.Create<Composition>(composition);
                        TempData["status"] = "Create succcess! Reference url:" + created_comp_A.Id;
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


        [HttpPost]
        public async Task<dynamic> GetandShare_Block(string bundlejson)
        {
            //var json = JsonConvert.SerializeObject(Post_data);
            var data = new StringContent(bundlejson, Encoding.UTF8, "application/json");

            //var url = "http://localhost:12904/api/Geth/" + Request_Url;
            var url = cookies.FHIR_URL_Cookie(HttpContext) + "/Patient";
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