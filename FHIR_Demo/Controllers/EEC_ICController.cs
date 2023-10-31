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
using System.Text;
using static Hl7.Fhir.Model.Observation;
using static Hl7.Fhir.Model.Encounter;

namespace FHIR_Demo.Controllers
{
    public class EEC_ICController : Controller
    {
        private CookiesController cookies = new CookiesController();
        private HttpClientEventHandler handler = new HttpClientEventHandler();

        public void Servercookie(String Server)
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
        }

        // GET: EEC
        public ActionResult Index(EECModels model)
        {
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
            ViewBag.status = TempData["status"];
            try
            {
                var q = new SearchParams().LimitTo(20);
                //.Where("organization=MITW.ForEMS")

                Bundle BundleSearchBundle = client.Search<Bundle>(q);
                //var json = PatientSearchBundle.ToJson();
                List<EECModels> bundleViewModels = new List<EECModels>();
                foreach (var entry in BundleSearchBundle.Entry)
                {
                    bundleViewModels.Add(new EECModels().EEC_ICBundleViewModelMapping((Bundle)entry?.Resource));
                }

                return View(bundleViewModels);
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
            handler.ServerCertificateCustomValidationCallback += (sender2, cert, chain, sslPolicyErrors) => true;

            if(cookies.FHIR_Server_Cookie(HttpContext) == "IBM")
            {
                handler.OnBeforeRequest += (sender, e) =>
                {
                    e.RawRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(cookies.FHIR_Token_Cookie(HttpContext))));
                };
            }
            else
            {
                //使用Bearer 登入
                handler.OnBeforeRequest += (sender, e) =>
                {
                    e.RawRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", cookies.FHIR_Token_Cookie(HttpContext, token));
                };
            }

            FhirClient client = new FhirClient(cookies.FHIR_URL_Cookie(HttpContext, url), cookies.settings, handler);

            try
            {
                var q = SearchParams.FromUriParamList(UriParamList.FromQueryString(search)).LimitTo(20);

                Bundle SearchBundle = client.Search<Bundle>(q);
                var json = SearchBundle.ToJson();
                List<EECModels> bundleViewModels = new List<EECModels>();
                foreach (var entry in SearchBundle.Entry)
                {
                    bundleViewModels.Add(new EECModels().EEC_ICBundleViewModelMapping((Bundle)entry.Resource));
                }

                return PartialView("_GetRecord", bundleViewModels);
            }
            catch (Exception e)
            {
                ViewBag.Error = e.ToString();
                return PartialView("_GetRecord");
            }
        }

        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            //讓系統通過對於不安全的https連線
            Servercookie(cookies.FHIR_Server_Cookie(HttpContext));

            FhirClient client = new FhirClient(cookies.FHIR_URL_Cookie(HttpContext), cookies.settings, handler);
            try
            {
                var bun_A = client.Read<Bundle>("Bundle/" + id);
                var bun_view = new EECModels().EEC_ICBundleViewModelMapping(bun_A);

                var z = new SearchParams().Where("subject=Bundle/" + id);
                return View(bun_view);
            }
            catch (Exception ex)
            {
                var reeor = ex;
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        // GET: EEC_IC/Create
        public ActionResult Create()
        {
            ViewBag.ObserCode_Select = new SelectList(new ObservationCode().observationCode(), "chinese", "chinese");
            EECModels model = new EECModels();
            model.entry = new List<EntryViewModle>
            {
                new EntryViewModle
                    {
                        fullurl = string.Empty,
                        Resource = new EntryResourceModel
                        {
                            resourceType = string.Empty,
                            id = string.Empty,
                            eec_icresourcedata = new EEC_ICResourcrDetail
                            {
                                name = string.Empty,
                                gender = Gender.不知道,
                                component = new ComponentComponent[]
                                {
                                    new ComponentComponent(),
                                    new ComponentComponent(),
                                    new ComponentComponent(),
                                    new ComponentComponent(),
                                    new ComponentComponent(),
                                    new ComponentComponent()
                                    //new Obser_Code_Value{
                                    //    ob_value = DateTime.MinValue.ToString(),
                                    //},
                                    //new Obser_Code_Value{
                                    //    ob_value = string.Empty,
                                    //},
                                    //new Obser_Code_Value(),
                                    //new Obser_Code_Value(),
                                    //new Obser_Code_Value(),
                                    //new Obser_Code_Value(),
                                },
                            }
                        }
                    }
            };
            return View(model);
        }

        // POST: EEC_IC/Create
        [HttpPost]
        public ActionResult Create(EECModels model)
        {
            ViewBag.ObserCode_Select = new SelectList(new ObservationCode().observationCode(), "chinese", "chinese");

            if (ModelState.IsValid)
            {
                Servercookie(cookies.FHIR_Server_Cookie(HttpContext));
                FhirClient client = new FhirClient(cookies.FHIR_URL_Cookie(HttpContext), cookies.settings, handler);
                try
                {
                    List<string> entryId = new List<string>();//最後Composition統整id用

                    Bundle bundle = new Bundle
                    {
                        Identifier = new Identifier
                        {
                            Value = model.identifier.code_code,
                            System = "https://www.glc.tw/"
                        },
                        Timestamp = model.timestamp,
                        Type = (Bundle.BundleType)model.type,
                        Entry = new List<Bundle.EntryComponent>()
                    };

                    #region//Patient創建

                    Patient patient = new Patient()
                    {
                        Name = new List<HumanName>()
                        {
                            new HumanName()
                            {
                                Text = model.entry[0].Resource.eec_icresourcedata.name
                            }
                        },
                        BirthDate = model.entry[0].Resource.eec_icresourcedata.birthDate,
                        Gender = (AdministrativeGender)model.entry[0].Resource.eec_icresourcedata.gender,
                        Identifier = new List<Identifier> {
                            new Identifier
                            {
                                Type = new CodeableConcept
                                {
                                    Coding = new List<Coding>
                                    {
                                        new Coding
                                        {
                                            System = "https://terminology.hl7.org/3.1.0/CodeSystem-v2-0203.html",
                                            Code = "MR",
                                            Display = "Medical record number"
                                        }
                                    }
                                },
                                System = "https://www.ntuh.gov.tw/",
                                Value = model.entry[0].Resource.eec_icresourcedata.identifier
                            },
                            new Identifier
                            {
                                Type = new CodeableConcept
                                {
                                    Coding = new List<Coding>
                                    {
                                        new Coding
                                        {
                                            System = "http://terminology.hl7.org/CodeSystem/v2-0203",
                                            Code = "NNxxx",
                                            Display = "National Person Identifier where the xxx is the ISO table 3166 3-character (alphabetic) country code"
                                        }
                                    }
                                },
                                System = "http://www.moi.gov.tw/",
                                Value = "Z123456789"
                            }
                        }
                    };

                    var conditions = new SearchParams();
                    conditions.Add("identifier", model.entry[0].Resource.eec_icresourcedata.identifier);
                    //var patient_ToJson = patient.ToJson();
                    var created_pat_A = client.Create<Patient>(patient, conditions);
                    //Bundle
                    Bundle.EntryComponent patientEntry = new Bundle.EntryComponent
                    {
                        //FullUrl = cookies.FHIR_URL_Cookie(HttpContext) + "Patient/" + created_pat_A.Id,
                        FullUrl = "Patient/" + created_pat_A.Id,
                        Resource = patient
                    };
                    bundle.Entry.Add(patientEntry);
                    //Composition
                    entryId.Add("Patient/" + created_pat_A.Id);

                    #endregion

                    #region //Specimen

                    Specimen specimen = new Specimen()
                    {
                        Collection = new Specimen.CollectionComponent
                        {
                            BodySite = new CodeableConcept
                            {
                                Coding = new List<Coding>
                                {
                                    new Coding
                                    {
                                        System = model.entry[0].Resource.eec_icresourcedata.spe_code.code_system,
                                        Code = model.entry[0].Resource.eec_icresourcedata.spe_code.code_code,
                                        Display = model.entry[0].Resource.eec_icresourcedata.spe_code.code_display
                                    }
                                },
                                Text = model.entry[0].Resource.eec_icresourcedata.spe_code.code_text
                            }
                        },
                        Subject = new ResourceReference
                        {
                            Reference = "Patient/" + created_pat_A.Id
                        }
                    };

                    var specimen_ToJson = specimen.ToJson();
                    var created_specimen = client.Create<Specimen>(specimen);
                    //Composition要用到的
                    entryId.Add("Specimen/" + created_specimen.Id);
                    //Bundle
                    Bundle.EntryComponent specimenEntry = new Bundle.EntryComponent
                    {
                        //FullUrl = cookies.FHIR_URL_Cookie(HttpContext) + "Specimen/" + created_specimen.Id,
                        FullUrl = "Specimen/" + created_specimen.Id,
                        Resource = specimen
                    };
                    bundle.Entry.Add(specimenEntry);

                    #endregion

                    #region //Practitioner
                    var practitionerid = "";
                    for (int i = 0; i < model.entry[0].Resource.eec_icresourcedata.practitioners.Count; i++)
                    {
                        Practitioner practitioner = new Practitioner()
                        {
                            Name = new List<HumanName>
                            {
                                new HumanName
                                {
                                    Text = model.entry[0].Resource.eec_icresourcedata.practitioners[i].pra_name
                                }
                            }
                        };
                        var practitioner_ToJson = practitioner.ToJson();
                        var created_practitioner = client.Create<Practitioner>(practitioner);
                        practitionerid = created_practitioner.Id;
                        //Composition要用到的
                        entryId.Add("Practitioner/" + created_practitioner.Id);

                        Bundle.EntryComponent practitionerEntry = new Bundle.EntryComponent
                        {
                            //FullUrl = cookies.FHIR_URL_Cookie(HttpContext) + "Practitioner/" + created_practitioner.Id,
                            FullUrl = "Practitioner/" + created_practitioner.Id,
                            Resource = practitioner
                        };
                        bundle.Entry.Add(practitionerEntry);
                    }


                    #endregion

                    #region //Observation
                    ViewBag.ObserCode_Select = new SelectList(new ObservationCode().observationCode(), "chinese", "chinese");

                    Observation observation = new Observation()
                    {
                        Status = (ObservationStatus)model.entry[0].Resource.eec_icresourcedata.ob_status,
                        Subject = new ResourceReference
                        {
                            Reference = "Patient/" + created_pat_A.Id
                        },
                        Performer = new List<ResourceReference>
                        {
                            new ResourceReference
                            {
                                Reference = "Practitioner/" + practitionerid
                            }
                        },
                        Effective = new Period
                        {
                            Start = model.entry[0].Resource.eec_icresourcedata?.effectivePeriod_star.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                            End = model.entry[0].Resource.eec_icresourcedata?.effectivePeriod_end.ToString("yyyy-MM-ddTHH:mm:ssZ"),

                        },
                        Code = new CodeableConcept
                        {
                            Coding = new List<Coding>
                            {
                                new Coding
                                {
                                    System = model.entry[0].Resource.eec_icresourcedata?.ob_code?.code_system,
                                    Code = model.entry[0].Resource.eec_icresourcedata?.ob_code?.code_code,
                                    Display = model.entry[0].Resource.eec_icresourcedata?.ob_code?.code_display
                                }
                            }
                        },
                        BodySite = new CodeableConcept
                        {
                            Coding = new List<Coding>
                            {
                                new Coding
                                {
                                    Code = model.entry[0].Resource.eec_icresourcedata?.ob_bodycode?.code_code,
                                    System = model.entry[0].Resource.eec_icresourcedata?.ob_bodycode?.code_system,
                                    Display = model.entry[0].Resource.eec_icresourcedata?.ob_bodycode?.code_display
                                }
                            }
                        },
                        Method = new CodeableConcept
                        {
                            Coding = new List<Coding>
                            {
                                new Coding
                                {
                                    Code = model.entry[0].Resource.eec_icresourcedata?.method?.code_code,
                                    System = model.entry[0].Resource.eec_icresourcedata?.method?.code_system,
                                    Display = model.entry[0].Resource.eec_icresourcedata?.method?.code_display
                                }
                            }
                        },
                        Note = new List<Annotation>
                        {
                            new Annotation
                            {
                                Text = new Markdown(model.entry[0].Resource.eec_icresourcedata.note)
                            }
                        },
                        Interpretation = new List<CodeableConcept>
                        {
                            new CodeableConcept
                            {
                                Coding = new List<Coding>
                                {
                                    new Coding
                                    {
                                        Code = model.entry[0].Resource.eec_icresourcedata?.interpretation?.code_code,
                                        System = model.entry[0].Resource.eec_icresourcedata?.interpretation?.code_system,
                                        Display = model.entry[0].Resource.eec_icresourcedata?.interpretation?.code_display
                                    }
                                }
                            }
                        },
                        Identifier = new List<Identifier>
                        {
                            new Identifier
                            {
                                Type = new CodeableConcept
                                {
                                    Coding = new List<Coding>
                                    {
                                        new Coding
                                        {
                                            Code = model.entry[0].Resource.eec_icresourcedata?.ob_identifier.code_code,
                                            System = model.entry[0].Resource.eec_icresourcedata?.ob_identifier?.code_system,
                                            Display = model.entry[0].Resource.eec_icresourcedata?.ob_identifier?.code_display
                                        }
                                    }
                                },
                                System = "https://www.vghtpe.gov.tw/Index.action",
                                Value = "9876"
                            }
                        },
                        Specimen = new ResourceReference
                        {
                            Reference = "Specimen/" + created_specimen.Id
                        },
                        //Encounter = new ResourceReference
                        //{
                        //    Reference = "Encounter/" + created_pat_A.Id
                        //}
                    };

                    //component
                    for (int n = 0; n < model.entry[0].Resource.eec_icresourcedata.component.Length; n++)
                    {
                        var newComponent = new ComponentComponent();
                        var observationValue = model.entry[0].Resource.eec_icresourcedata?.Code_value;
                        if (observationValue != null)
                        {
                            newComponent.Value = new Quantity
                            {
                                Value = observationValue.value,
                                Unit = observationValue.unit
                            };
                        }

                        observation.Component = new List<Observation.ComponentComponent>
                        {
                            new Observation.ComponentComponent
                            {
                                Code = new CodeableConcept
                                {
                                    Coding = new List<Coding>
                                    {
                                        new Coding
                                        {
                                            System = model.entry[0].Resource.eec_icresourcedata.component[n].Code.Coding[0].System,
                                            Code = model.entry[0].Resource.eec_icresourcedata.component[n].Code.Coding[0].Code,
                                            Display = model.entry[0].Resource.eec_icresourcedata.component[n].Code.Coding[0].Display,
                                        },
                                        new Coding
                                        {
                                            System = model.entry[0].Resource.eec_icresourcedata.component[n].Code.Coding[1].System,
                                            Code = model.entry[0].Resource.eec_icresourcedata.component[n].Code.Coding[1].Code,
                                            Display = model.entry[0].Resource.eec_icresourcedata.component[n].Code.Coding[1].Display,
                                        },
                                    },
                                },
                                ReferenceRange = new List<ReferenceRangeComponent>{
                                    new ReferenceRangeComponent
                                    {
                                        Low = new Quantity
                                        {
                                            Value = model.entry[0].Resource.eec_icresourcedata.ob_range.Low,
                                            Unit = model.entry[0].Resource.eec_icresourcedata.ob_range.unit
                                        },
                                        High = new Quantity
                                        {
                                            Value = model.entry[0].Resource.eec_icresourcedata.ob_range.High,
                                            Unit = model.entry[0].Resource.eec_icresourcedata.ob_range.unit
                                        },
                                    }
                                },
                                Value = newComponent.Value
                            }
                        };



                        var observation_ToJson = observation.ToJson();
                        var created_obser = client.Create<Observation>(observation);

                        //Bundle
                        Bundle.EntryComponent obserEntry = new Bundle.EntryComponent
                        {
                            //FullUrl = cookies.FHIR_URL_Cookie(HttpContext) + "Observation/" + created_obser.Id,
                            FullUrl = "Observation/" + created_obser.Id,
                            Resource = observation
                        };
                        bundle.Entry.Add(obserEntry);

                        //Composition要用到的
                        entryId.Add("Observation/" + created_obser.Id);

                    }


                    #endregion

                    #region //Organization
                    var organizationValue = model.entry[0]?.Resource?.eec_icresourcedata?.organ_identifier?.ob_value as string[];
                    Organization organization = new Organization()
                    {
                        Identifier = new List<Identifier>(),
                        Name = model.entry[0].Resource.eec_icresourcedata.organ_name
                    };
                    if (model.entry[0]?.Resource?.eec_icresourcedata?.organ_identifier != null)
                    {
                        var or_Identifier = new Identifier
                        {
                            Type = new CodeableConcept()
                            {
                                Coding = new List<Coding>
                                    {
                                        new Coding
                                        {
                                            System = model.entry[0].Resource.eec_icresourcedata.organ_identifier.code_system,
                                            Code = model.entry[0].Resource.eec_icresourcedata.organ_identifier.code_code,
                                            Display = model.entry[0].Resource.eec_icresourcedata.organ_identifier.code_display
                                        }
                                    }
                            },
                            Value = organizationValue[0],
                            System = model.entry[0].Resource.eec_icresourcedata.organ_identifier.value_system
                        };
                        organization.Identifier.Add(or_Identifier);
                    }
                    var organization_ToJson = organization.ToJson();
                    var created_organization = client.Create<Organization>(organization);
                    //Composition要用到的
                    entryId.Add("Organization/" + created_organization.Id);

                    Bundle.EntryComponent organEntry = new Bundle.EntryComponent
                    {
                        //FullUrl = cookies.FHIR_URL_Cookie(HttpContext) + "Organization/" + created_organization.Id,
                        FullUrl = "Organization/" + created_organization.Id,
                        Resource = organization
                    };
                    bundle.Entry.Add(organEntry);
                    #endregion

                    #region //Encounter

                    Encounter encounter = new Encounter()
                    {
                        Identifier = new List<Identifier> {
                            new Identifier
                            {
                                System =  model.entry[0].Resource.eec_icresourcedata.en_identifier[0].code_system,
                                Value =  model.entry[0].Resource.eec_icresourcedata.en_identifier[0].code_code
                            }
                        },
                        Status = (EncounterStatus)model.entry[0].Resource.eec_icresourcedata.en_status,
                        Class = new Coding
                        {
                            System = model.entry[0].Resource.eec_icresourcedata.en_class.code_system,
                            Code = model.entry[0].Resource.eec_icresourcedata.en_class.code_code,
                            Display = model.entry[0].Resource.eec_icresourcedata.en_class.code_display
                        },
                        Participant = new List<ParticipantComponent>
                        {
                            new ParticipantComponent
                            {
                                Individual = new ResourceReference
                                {
                                    //Reference = "Practitioner/" + created_practitioner.Id
                                    Reference = "Practitioner/" + practitionerid
                                }
                            }
                        },
                        Period = new Period
                        {
                            Start = new FhirDateTime(model.entry[0].Resource.eec_icresourcedata.period).ToString()
                        },
                        ServiceType = new CodeableConcept
                        {
                            Coding = new List<Coding>
                            {
                                new Coding
                                {
                                    System = model.entry[0].Resource.eec_icresourcedata.en_servicetype.code_system,
                                    Code = model.entry[0].Resource.eec_icresourcedata.en_servicetype.code_code,
                                    Display = model.entry[0].Resource.eec_icresourcedata.en_servicetype.code_display
                                }
                            }
                        },
                        ServiceProvider = new ResourceReference
                        {
                            Reference = "Organization/" + created_organization.Id,
                            //Reference = "Organization/98"
                        },
                        Subject = new ResourceReference
                        {
                            Reference = "Patient/" + created_pat_A.Id
                        }
                    };

                    var encounter_ToJson = encounter.ToJson();
                    var created_encoun = client.Create<Encounter>(encounter);
                    //Composition要用到的
                    entryId.Add("Encounter/" + created_encoun.Id);
                    //Bundle
                    Bundle.EntryComponent encounterEntry = new Bundle.EntryComponent
                    {
                        //FullUrl = cookies.FHIR_URL_Cookie(HttpContext) + "Encounter/" + created_encoun.Id,
                        FullUrl = "Encounter/" + created_encoun.Id,
                        Resource = encounter
                    };
                    bundle.Entry.Add(encounterEntry);

                    #endregion



                    #region //Composition

                    Composition.SectionComponent sectionComponent = new Composition.SectionComponent
                    {
                        Entry = new List<ResourceReference>()
                    };
                    foreach (var obserId in entryId)
                    {
                        //把所有種類連結加進Composition的entry中
                        sectionComponent.Entry.Add(new ResourceReference
                        {
                            Reference = obserId
                        });
                    }

                    Composition composition = new Composition
                    {
                        Title = model.entry[0].Resource.eec_icresourcedata.com_title,
                        Status = (CompositionStatus)CompositionStatus.Final,
                        Type = new CodeableConcept
                        {
                            Coding = new List<Coding>
                            {
                                new Coding
                                {
                                    System = model.entry[0].Resource.eec_icresourcedata.com_type.code_system,
                                    Code = model.entry[0].Resource.eec_icresourcedata.com_type.code_code,
                                    Display = model.entry[0].Resource.eec_icresourcedata.com_type.code_display
                                }
                            }
                        },
                        Date = new FhirDateTime(model.entry[0].Resource.eec_icresourcedata.com_date).ToString(),
                        Subject = new ResourceReference
                        {
                            Reference = "Patient/" + created_pat_A.Id
                        },
                        Encounter = new ResourceReference
                        {
                            Reference = "Encounter/" + created_encoun.Id
                        },
                        Author = new List<ResourceReference>
                        {
                            new ResourceReference{
                                Reference = "Practitioner/" + practitionerid
                            }
                        },
                        Section = new List<Composition.SectionComponent> {
                            sectionComponent
                        },

                    };
                    var composition_ToJson = composition.ToJson();
                    composition.Section[0] = sectionComponent;

                    var createdComposition = client.Create<Composition>(composition);
                    Bundle.EntryComponent compositionEntry = new Bundle.EntryComponent
                    {
                        //FullUrl = cookies.FHIR_URL_Cookie(HttpContext) + "Composition/" + createdComposition.Id,
                        FullUrl =  "Composition/" + createdComposition.Id,
                        Resource = composition
                    };

                    // 將 Composition 添加到 Bundle 的最前面
                    //bundle.Entry.Insert(0, compositionEntry);

                    #endregion

                    //Bundle統整
                    var bundle_ToJson = bundle.ToJson();
                    var createdBundle = client.Create<Bundle>(bundle);
                    TempData["status"] = "Create EEC succcess! Reference url:" + createdBundle.Id;
                    return RedirectToAction("Index");
                }catch (Exception ex)
                {
                    return View(model);
                }
            }
            else if (!ModelState.IsValid)
            {
                var errorMessages = ModelState.Values.SelectMany(v => v.Errors)
                                                     .Select(e => e.ErrorMessage)
                                                     .ToList();
            }

            return View(model);
        }

        // GET: EEC/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }
    }
}