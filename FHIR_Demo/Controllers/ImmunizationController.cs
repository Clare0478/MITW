using FHIR_Demo.Models;
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
            handler.OnBeforeRequest += (sender, e) =>
            {
                e.RawRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", cookies.FHIR_Token_Cookie(HttpContext));
            };
            FhirClient client = new FhirClient(cookies.FHIR_URL_Cookie(HttpContext), cookies.settings, handler);
            ViewBag.status = TempData["status"];
            ViewBag.Error = TempData["Error"];
            try
            {
                Bundle ImmunizationSearchBundle = client.Search<Immunization>(null);
                //var json = PatientSearchBundle.ToJson();
                List<ImmunizationViewModel> immunizationViews = new List<ImmunizationViewModel>();
                foreach (var entry in ImmunizationSearchBundle.Entry)
                {
                    immunizationViews.Add(new ImmunizationViewModel().ImmunizationViewModelMapping((Immunization)entry.Resource));
                }

                return View(immunizationViews);
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
                var q = SearchParams.FromUriParamList(UriParamList.FromQueryString(search)).LimitTo(20);

                Bundle ImmunizationSearchBundle = client.Search<Immunization>(null);
                //var json = PatientSearchBundle.ToJson();
                List<ImmunizationViewModel> immunizationViews = new List<ImmunizationViewModel>();
                foreach (var entry in ImmunizationSearchBundle.Entry)
                {
                    immunizationViews.Add(new ImmunizationViewModel().ImmunizationViewModelMapping((Immunization)entry.Resource));
                }

                return PartialView("_GetRecord", immunizationViews);
            }
            catch (Exception e)
            {
                ViewBag.Error = e.ToString();
                return PartialView("_GetRecord");
            }
        }

        // GET: Immunization/Details/5
        public ActionResult Details(string id)
        {
            return View();
        }

        // GET: Immunization/Create
        public ActionResult Create()
        {
            ViewBag.status = TempData["status"];
            ViewBag.Error = TempData["Error"];
            return View();
        }

        // POST: Immunization/Create
        [HttpPost]
        public ActionResult Create(ImmunizationViewModel model)
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
                    var patient = client.Read<Patient>(model.Patient.Reference);
                    var organization = client.Read<Organization>(model.Hospital.Reference);

                    Composition composition = new Composition();
                    Observation observation = new Observation();
                    Immunization immunization = new Immunization();
                    Bundle bundle = new Bundle();

                    bundle.Identifier = new Identifier
                    {
                        System = "https://www.vghtc.gov.tw",
                        Value = $"TW.{organization.Identifier[0].Value}.{new FhirDateTime(model.Date).Value}",
                        Period = new Period(new FhirDateTime(model.Date), new FhirDateTime(model.Date.AddYears(5)))
                    };
                    bundle.Type = Bundle.BundleType.Document;
                    bundle.Timestamp = model.Date;
                    bundle.Entry = new List<Bundle.EntryComponent>();

                    composition.Status = CompositionStatus.Final;
                    composition.Type = new CodeableConcept
                    {
                        Coding = new List<Coding>
                        {
                            new Coding
                            {
                                System = "http://loinc.org"
                            }
                        }
                    };

                    composition.Subject = model.Patient;
                    composition.Date = new FhirDateTime(model.Date).Value;
                    composition.Author = new List<ResourceReference>
                    {
                        model.Hospital
                    };

                    composition.Section = new List<Composition.SectionComponent> {
                        new Composition.SectionComponent
                        {
                            Entry = new List<ResourceReference>
                            {
                                new ResourceReference($"{organization.TypeName}/{organization.Id}"),
                                new ResourceReference($"{patient.TypeName}/{patient.Id}")
                            }
                        }
                    };

                    if (model.Type == "疫苗")
                    {
                        composition.Type.Coding[0].Code = "82593-5";
                        composition.Type.Coding[0].Display = "Immunization summary report";

                        immunization.Patient = model.Patient;
                        immunization.VaccineCode = model.Imm_VaccineCode;
                        immunization.Manufacturer = model.Imm_Manufacturer;
                        immunization.ProtocolApplied = new List<Immunization.ProtocolAppliedComponent>
                        {
                            new Immunization.ProtocolAppliedComponent
                            {
                                TargetDisease = model.Imm_ProtocolApplied.TargetDisease,
                                DoseNumber = new FhirString(model.Imm_ProtocolApplied.DoseNumber),
                                SeriesDoses = new FhirString(model.Imm_ProtocolApplied.SeriesDoses)
                            }
                        };
                        immunization.LotNumber = model.Imm_LotNumber;
                        immunization.Occurrence = new FhirDateTime(model.Date);
                        immunization.Performer = new List<Immunization.PerformerComponent>
                        {
                            new Immunization.PerformerComponent
                            {
                                Actor = model.Hospital
                            },
                            new Immunization.PerformerComponent
                            {
                                Actor = new ResourceReference
                                {
                                    Display = model.Imm_Performer_acotr_display
                                }
                            }
                        };

                        //新增疫苗資料
                        immunization = client.Create<Immunization>(immunization);

                        composition.Section[0].Entry.Add(new ResourceReference($"{immunization.TypeName}/{immunization.Id}"));
                    }
                    else
                    {
                        if (model.Type == "PCR")
                        {
                            composition.Type.Coding[0].Code = "LP6464-4";
                            composition.Type.Coding[0].Display = "Nucleic acid amplification with probe detection";
                        }
                        else
                        {
                            composition.Type.Coding[0].Code = "LP217198-3";
                            composition.Type.Coding[0].Display = "Rapid immunoassay";
                        }
                        observation.Status = ObservationStatus.Final;
                        observation.Subject = model.Patient;
                        observation.Code = model.Obs_Coding;
                        observation.Effective = new Period(new FhirDateTime(model.Date), new FhirDateTime(model.Date));
                        observation.Value = new FhirBoolean(bool.Parse(model.value));
                        observation.Performer = new List<ResourceReference> { model.Hospital };

                        //新增檢驗資料
                        observation = client.Create<Observation>(observation);
                        composition.Section[0].Entry.Add(new ResourceReference($"{observation.TypeName}/{observation.Id}"));
                    }

                    //新增Composition資料
                    composition = client.Create<Composition>(composition);
                    //bundle新增
                    bundle.Entry.Add(new Bundle.EntryComponent { FullUrl = composition.ResourceIdentity().WithoutVersion().ToString(), Resource = composition });
                    bundle.Entry.Add(new Bundle.EntryComponent { FullUrl = organization.ResourceIdentity().WithoutVersion().ToString(), Resource = organization });
                    bundle.Entry.Add(new Bundle.EntryComponent { FullUrl = patient.ResourceIdentity().WithoutVersion().ToString(), Resource = patient });
                    if (model.Type == "疫苗")
                        bundle.Entry.Add(new Bundle.EntryComponent { FullUrl = immunization.ResourceIdentity().WithoutVersion().ToString(), Resource = immunization });
                    else
                        bundle.Entry.Add(new Bundle.EntryComponent { FullUrl = observation.ResourceIdentity().WithoutVersion().ToString(), Resource = observation });

                    bundle = client.Create<Bundle>(bundle);
                    //如果找到同樣資料，會回傳該筆資料，但如果找到多筆資料，會產生Error
                    //var created_obser = client.Create<>();
                    TempData["status"] = "Create succcess! Reference url:" + bundle.Id;
                    ViewBag.status = TempData["status"];
                    return RedirectToAction("Index");
                }
                catch (Exception e)
                {
                    TempData["Error"] = e.Message;
                    ViewBag.Error = TempData["Error"];
                    return View(model);
                }
            }
            return View(model);
        }

        // GET: Immunization/Edit/5
        public ActionResult Update(string id)
        {
            return View();
        }

        // POST: Immunization/Edit/5
        [HttpPost]
        public ActionResult Update(string id, FormCollection collection)
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
