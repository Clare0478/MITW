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
using static Hl7.Fhir.Model.Encounter;
using static Hl7.Fhir.Model.AllergyIntolerance;
using static Hl7.Fhir.Model.HumanName;

namespace FHIR_Demo.Controllers
{
    public class EMSController : Controller
    {
        private CookiesController cookies = new CookiesController();
        private HttpClientEventHandler handler = new HttpClientEventHandler();

        // GET: EMS
        public ActionResult Index(PatientViewModel model)
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
                List<BundleModel> bundleViewModels = new List<BundleModel>();
                foreach (var entry in BundleSearchBundle.Entry)
                {
                    bundleViewModels.Add(new BundleModel().EMSBundleViewModelMapping((Bundle)entry?.Resource));
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

        // GET: EMS/Details/5
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
                var bun_view = new BundleModel().EMSBundleViewModelMapping(bun_A);

                var z = new SearchParams().Where("subject=Bundle/" + id);

                return View(bun_view);

            }
            catch(Exception ex)
            {
                var reeor = ex;
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        // GET: EMS/Create
        public ActionResult Create()
        {
            ViewBag.ObserCode_Select = new SelectList(new ObservationCode().observationCode(), "chinese", "chinese");
            BundleModel model = new BundleModel();
            model.entry = new List<EntryViewModle>
            {
                new EntryViewModle
                    {
                        fullurl = string.Empty,
                        Resource = new EntryResourceModel
                        {
                            resourceType = string.Empty,
                            id = string.Empty,
                            resourcedata = new ResourceDetail
                            {
                                name = string.Empty,
                                birthDate = null,
                                gender = Gender.不知道,
                                Observations = new List<ObservationEMS>()
                            }
                        }
                    }
            };
            return View(model);
        }

        // POST: EMS/Create
        [HttpPost]
        public ActionResult Create(BundleModel model)
        {
            if (ModelState.IsValid)
            {
                ViewBag.ObserCode_Select = new SelectList(new ObservationCode().observationCode(), "chinese", "chinese");
                Servercookie(cookies.FHIR_Server_Cookie(HttpContext));
                FhirClient client = new FhirClient(cookies.FHIR_URL_Cookie(HttpContext), cookies.settings, handler);
                try
                {
                    //★資料裡必須先有一筆Location(預設ID是10)
                    List<string> Obserentry = new List<string>();

                    Bundle bundle = new Bundle
                    {
                        Identifier = new Identifier
                        {
                            Value = "MI11111",
                            System = "https://www.glc.tw/"
                        },
                        Timestamp = model.timestamp,
                        Type = (Bundle.BundleType)model.type,
                        Entry = new List<Bundle.EntryComponent>()
                    };

                    bundle.Meta = new Meta
                    {
                        Profile = new List<string> { "https://standard-interoperability-lab.com/fhir/StructureDefinition/EMS-Bundle" }
                    };

                    #region//Patient創建

                    Patient patient = new Patient()
                    {
                        Name = new List<HumanName>()
                        {
                            new HumanName()
                            {
                                Text = model.entry[0].Resource.resourcedata.name
                            }
                        },
                        Active = Convert.ToBoolean(model.entry[0].Resource.resourcedata.active),
                        BirthDate = model.entry[0].Resource.resourcedata.birthDate,
                        Gender = (AdministrativeGender)model.entry[0].Resource.resourcedata.gender,
                        Identifier = new List<Identifier> {
                            new Identifier
                            {
                                Value = model.entry[0].Resource.resourcedata.identifier
                            }
                        }
                    };
                    patient.Meta = new Meta
                    {
                        Profile = new List<string> { "https://standard-interoperability-lab.com/fhir/StructureDefinition/EMS-Patient" }
                    };

                    var conditions = new SearchParams();
                    conditions.Add("identifier", model.entry[0].Resource.resourcedata.identifier);
                    //var patient_ToJson = patient.ToJson();
                    var created_pat_A = client.Create<Patient>(patient, conditions);
                    //Bundle
                    Bundle.EntryComponent patientEntry = new Bundle.EntryComponent
                    {
                        FullUrl =  "Patient/" + created_pat_A.Id,
                        Resource = patient
                    };
                    bundle.Entry.Add(patientEntry);
                    //Composition
                    Obserentry.Add("Patient/" + created_pat_A.Id);

                    #endregion

                    #region //Practitioner

                    Practitioner practitioner = new Practitioner()
                    {
                        Name = new List<HumanName>
                        {
                            new HumanName
                            {
                                Text = model.entry[0].Resource.resourcedata.pra_name
                            }
                        }
                    };
                    practitioner.Meta = new Meta
                    {
                        Profile = new List<string> { "https://standard-interoperability-lab.com/fhir/StructureDefinition/EMS-Practitioner" }
                    };

                    //var practitioner_ToJson = practitioner.ToJson();
                    var created_practitioner = client.Create<Practitioner>(practitioner);
                    //Composition要用到的
                    Obserentry.Add("Practitioner/" + created_practitioner.Id);

                    Bundle.EntryComponent practitionerEntry = new Bundle.EntryComponent
                    {
                        FullUrl =  "Practitioner/" + created_practitioner.Id,
                        Resource = practitioner
                    };
                    bundle.Entry.Add(practitionerEntry);

                    #endregion

                    #region //Observation
                    ViewBag.ObserCode_Select = new SelectList(new ObservationCode().observationCode(), "chinese", "chinese");

                    //foreach (var observationdetail in model.entry[0].Resource.resourcedata.Observations)
                    for (int i = 0; i < model.entry[0].Resource.resourcedata.Observations.Count; i++)
                    {
                        var observationdetail = model.entry[0].Resource.resourcedata.Observations[i];
                        Observation observation = new Observation()
                        {
                            Status = (ObservationStatus)observationdetail.ob_status,
                            Subject = new ResourceReference
                            {
                                Reference = "Patient/" + created_pat_A.Id
                            },
                            Performer = new List<ResourceReference>
                            {
                                new ResourceReference
                                {
                                    Reference = "Practitioner/" + created_practitioner.Id
                                }
                            },
                            Effective = new FhirDateTime(observationdetail?.effectiveDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ")),
                            Code = new CodeableConcept
                            {
                                Coding = new List<Coding>
                                {
                                    new Coding
                                    {
                                        System = observationdetail.ob_code.code_system,
                                        Code = observationdetail.ob_code.code_code,
                                        Display = observationdetail.ob_code.code_display
                                    }
                                }
                            },
                        };

                        string x = "";
                        switch (i)
                        {
                            case 0: //流程
                                x = "https://standard-interoperability-lab.com/fhir/StructureDefinition/EMS-Observation-MissionTime";
                                break;
                            case 1:  //心肺
                                x = "https://standard-interoperability-lab.com/fhir/StructureDefinition/EMS-Observation-OHCA";
                                break;
                            case 2:  //VT
                                x = "https://standard-interoperability-lab.com/fhir/StructureDefinition/EMS-Observation-VitalSigns";
                                break;
                            case 3:  //GCS
                                x = "https://standard-interoperability-lab.com/fhir/StructureDefinition/EMS-Observation-GCS";
                                break;
                            case 4:  //VT
                                x = "https://twcore.mohw.gov.tw/ig/twcore/StructureDefinition/Observation-bloodPressure-twcore";
                                break;
                        }
                        observation.Meta = new Meta
                        {
                            Profile = new List<string> { x }
                        };

                        for (int n = 0; n < model.entry[0].Resource.resourcedata.Observations[i].component.Length; n++)
                        {
                            var observationValue = observationdetail.component[n].ob_value;
                            DataType value = null;

                            var obValueArray = observationdetail.component[n].ob_value as string[];
                            if (obValueArray != null && obValueArray.Length > 0)
                            {
                                string firstValue = obValueArray[0];

                                if (double.TryParse(firstValue, out double doubleValue))
                                {
                                    value = new Quantity
                                    {
                                        Value = (decimal?)doubleValue
                                    };
                                }
                                else if (DateTime.TryParse(firstValue, out DateTime dateTimeValue))
                                {
                                    value = new FhirDateTime(dateTimeValue.ToString("yyyy-MM-ddTHH:mm:sszzz"));
                                }
                                else if(Boolean.TryParse(firstValue, out Boolean boolValue)){
                                    value = new FhirBoolean(boolValue);
                                }
                                else if (observationdetail.component[n].value_code != null)
                                {
                                    break;
                                }
                                else
                                {
                                    value = new FhirString(firstValue);
                                }
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
                                                System = observationdetail.component[n].code_system,
                                                Code = observationdetail.component[n].code_code,
                                                Display = observationdetail.component[n].code_display
                                            }
                                        },
                                    },
                                    Value = value
                                }

                            };


                        }

                        var observation_ToJson = observation.ToJson();
                        var created_obser = client.Create<Observation>(observation);

                        //Bundle
                        Bundle.EntryComponent obserEntry = new Bundle.EntryComponent
                        {
                            FullUrl =  "Observation/" + created_obser.Id,
                            Resource = observation
                        };
                        bundle.Entry.Add(obserEntry);

                        //Composition要用到的
                        Obserentry.Add("Observation/" + created_obser.Id);

                    }


                    //TempData["status"] = "Create succcess! Reference url:" + created_obser.Id;
                    //return RedirectToAction("Index");
                    #endregion

                    #region //Condition
                    for (int i = 0; i < model.entry[0].Resource.resourcedata.Conditions.Count; i++)
                    {
                        Condition condition = new Condition()
                        {
                            VerificationStatus = new CodeableConcept
                            {
                                Coding = new List<Coding>
                                {
                                    new Coding
                                    {
                                        System = model?.entry[0]?.Resource?.resourcedata?.Conditions[i]?.con_verificationStatus?.code_system,
                                        Code = model?.entry[0]?.Resource.resourcedata?.Conditions[i]?.con_verificationStatus?.code_code,
                                        Display = model?.entry[0]?.Resource.resourcedata?.Conditions[i]?.con_verificationStatus?.code_display
                                    }
                                }
                            },
                            Code = new CodeableConcept
                            {
                                Coding = new List<Coding>
                                {
                                    new Coding
                                    {
                                        System = model.entry[0].Resource.resourcedata.Conditions[i].con_code.code_system,
                                        Code = model.entry[0].Resource.resourcedata.Conditions[i].con_code.code_code,
                                        Display = model.entry[0].Resource.resourcedata.Conditions[i].con_code.code_display
                                    }
                                }
                            },
                            Subject = new ResourceReference
                            {
                                Reference = "Patient/" + created_pat_A.Id
                            }
                        };
                        string x = "";
                        switch (i)
                        {
                            case 0: //現場狀況
                                x = "https://standard-interoperability-lab.com/fhir/StructureDefinition/EMS-Condition-Situation";
                                break;
                            case 1:  //過去病史
                                x = "https://standard-interoperability-lab.com/fhir/StructureDefinition/EMS-Condition-History";
                                break;

                        }
                        condition.Meta = new Meta
                        {
                            Profile = new List<string> { x }
                        };

                        //var condition_ToJson = condition.ToJson();
                        var created_condition = client.Create<Condition>(condition);
                        //Composition要用到的
                        Obserentry.Add("Condition/" + created_condition.Id);

                        Bundle.EntryComponent conditionEntry = new Bundle.EntryComponent
                        {
                            FullUrl =  "Condition/" + created_condition.Id,
                            Resource = condition
                        };
                        bundle.Entry.Add(conditionEntry);
                    }
                    #endregion

                    #region //Organization

                    Organization organization = new Organization()
                    {
                        Identifier = new List<Identifier>
                        {
                                new Identifier
                                {
                                    Type = new CodeableConcept(){
                                    Text = model.entry[0].Resource.resourcedata.organ_identifier.code_text,
                                    Coding = new List<Coding>
                                    {
                                        new Coding
                                        {
                                            System = model.entry[0].Resource.resourcedata.organ_identifier.code_system,
                                            Code = model.entry[0].Resource.resourcedata.organ_identifier.code_code
                                        }
                                    }
                                    },
                                    Value = model.entry[0].Resource.resourcedata.organ_identifier.code_display
                                }
                        },
                        Active = model.entry[0].Resource.resourcedata.organ_active,
                        Name = model.entry[0].Resource.resourcedata.organ_name
                    };
                    //var organization_ToJson = organization.ToJson();
                    var created_organization = client.Create<Organization>(organization);
                    //Composition要用到的
                    Obserentry.Add("Organization/" + created_organization.Id);

                    Bundle.EntryComponent organEntry = new Bundle.EntryComponent
                    {
                        FullUrl =  "Organization/" + created_organization.Id,
                        Resource = organization
                    };
                    bundle.Entry.Add(organEntry);
                    #endregion

                    #region //Encounter

                    Encounter encounter = new Encounter()
                    {
                        Identifier = new List<Identifier>(),
                        Status = (EncounterStatus)model.entry[0].Resource.resourcedata.en_status,
                        Class = new Coding
                        {
                            System = model.entry[0].Resource.resourcedata.en_class.code_system,
                            Code = model.entry[0].Resource.resourcedata.en_class.code_code,
                            Display = model.entry[0].Resource.resourcedata.en_class.code_display
                        },
                        Participant = new List<ParticipantComponent>
                        {
                            new ParticipantComponent
                            {
                                Individual = new ResourceReference
                                {
                                    Reference = "Practitioner/" + created_practitioner.Id
                                    //Reference = "Practitioner/106"
                                }
                            }
                        },
                        Period = new Period
                        {
                            Start = new FhirDateTime(model.entry[0].Resource.resourcedata.en_period).ToString()
                        },
                        Location = new List<LocationComponent>
                        {
                            new LocationComponent
                            {
                                Location = new ResourceReference
                                {
                                    Reference = "Location/37d7a272-935a-416e-87d9-e67bc03c6c48"
                                }
                            }
                        },
                        ServiceProvider = new ResourceReference
                        {
                            Reference = "Organization/" + created_organization.Id,
                            //Reference = "Organization/98"
                        }
                    };
                    //Encounter的identifier要兩個
                    for (int i = 0; i < model.entry[0].Resource.resourcedata.en_identifier.Count; i++)
                    {
                        Identifier Identifier = new Identifier
                        {
                            Type = new CodeableConcept()
                            {
                                Text = model.entry[0].Resource.resourcedata.en_identifier[i].code_text,
                            },
                            System = model.entry[0].Resource.resourcedata.en_identifier[i].code_system,
                            Value = model.entry[0].Resource.resourcedata.en_identifier[i].code_display
                        };

                        encounter.Identifier.Add(Identifier);
                    }

                    //var encounter_ToJson = encounter.ToJson();
                    var created_encoun = client.Create<Encounter>(encounter);
                    //Composition要用到的
                    Obserentry.Add("Encounter/" + created_encoun.Id);
                    //Bundle
                    Bundle.EntryComponent encounterEntry = new Bundle.EntryComponent
                    {
                        FullUrl =  "Encounter/" + created_encoun.Id,
                        Resource = encounter
                    };
                    bundle.Entry.Add(encounterEntry);

                    #endregion

                    #region //RiskAssessment
                    for (int i = 0; i < model.entry[0].Resource.resourcedata.RiskAssessments.Count; i++)
                    {
                        RiskAssessment riskAssessment = new RiskAssessment()
                        {
                            Status = (ObservationStatus)model.entry[0].Resource.resourcedata.RiskAssessments[i].risk_status,
                            Code = new CodeableConcept
                            {
                                Coding = new List<Coding> {
                                    new Coding
                                    {
                                        System = model.entry[0].Resource.resourcedata.RiskAssessments[i].risk_code.code_system,
                                        Code = model.entry[0].Resource.resourcedata.RiskAssessments[i].risk_code.code_code,
                                        Display = model.entry[0].Resource.resourcedata.RiskAssessments[i].risk_code.code_display,
                                    }
                                }
                            },
                            Occurrence = new FhirDateTime(model.entry[0].Resource.resourcedata.RiskAssessments[i].occurrenceDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ")),
                            Prediction = new List<RiskAssessment.PredictionComponent>
                            {
                                new RiskAssessment.PredictionComponent
                                {
                                    Outcome = new CodeableConcept
                                    {
                                        Text = model.entry[0].Resource.resourcedata.RiskAssessments[i].risk_prediction
                                    },
                                    Probability = new FhirDecimal(2)
                                }
                            },
                            Subject = new ResourceReference
                            {
                                Reference = "Patient/" + created_pat_A.Id
                            }
                        };

                        var riskAssessment_ToJson = riskAssessment.ToJson();
                        var created_riskAssessment = client.Create<RiskAssessment>(riskAssessment);
                        //Composition要用到的
                        Obserentry.Add("RiskAssessment/" + created_riskAssessment.Id);

                        Bundle.EntryComponent riskEntry = new Bundle.EntryComponent
                        {
                            FullUrl =  "RiskAssessment/" + created_riskAssessment.Id,
                            Resource = riskAssessment
                        };
                        bundle.Entry.Add(riskEntry);
                    }

                    #endregion

                    #region //AllergyIntolerance

                    AllergyIntolerance allergyIntolerance = new AllergyIntolerance()
                    {
                        Patient = new ResourceReference
                        {
                            Reference = "Patient/" + created_pat_A.Id,
                        },
                        ClinicalStatus = new CodeableConcept
                        {
                            Coding = new List<Coding>
                            {
                                new Coding
                                {
                                    System = "http://terminology.hl7.org/CodeSystem/allergyintolerance-clinical",
                                    Code = "active",
                                    Display = "Active"
                                }
                            }
                        },
                        VerificationStatus = new CodeableConcept
                        {
                            Coding = new List<Coding>
                            {
                                new Coding
                                {
                                    System = model.entry[0].Resource.resourcedata.allergy_status.code_system,
                                    Code = model.entry[0].Resource.resourcedata.allergy_status.code_code,
                                    Display = model.entry[0].Resource.resourcedata.allergy_status.code_display,
                                }
                            }
                        },
                        Category = new List<AllergyIntoleranceCategory?>
                        {
                            AllergyIntoleranceCategory.Food
                        },
                        Code = new CodeableConcept
                        {
                            Coding = new List<Coding>
                            {
                                new Coding
                                {
                                    System = model.entry[0].Resource.resourcedata.allergy_code.code_system,
                                    Code = model.entry[0].Resource.resourcedata.allergy_code.code_code,
                                    Display = model.entry[0].Resource.resourcedata.allergy_code.code_display,
                                }
                            }
                        }
                    };
                    var allergyIntolerance_ToJson = allergyIntolerance.ToJson();
                    var created_allergyIntolerance = client.Create<AllergyIntolerance>(allergyIntolerance);
                    //Composition要用到的
                    Obserentry.Add("AllergyIntolerance/" + created_allergyIntolerance.Id);

                    Bundle.EntryComponent allerEntry = new Bundle.EntryComponent
                    {
                        FullUrl =  "AllergyIntolerance/" + created_allergyIntolerance.Id,
                        Resource = allergyIntolerance
                    };
                    bundle.Entry.Add(allerEntry);

                    #endregion

                    #region //Composition

                    Composition.SectionComponent sectionComponent = new Composition.SectionComponent
                    {
                        Entry = new List<ResourceReference>()
                    };
                    foreach (var obserId in Obserentry)
                    {
                        //把所有種類連結加進Composition的entry中
                        sectionComponent.Entry.Add(new ResourceReference
                        {
                            Reference = obserId
                        });
                    }

                    Composition composition = new Composition
                    {
                        Title = model.entry[0].Resource.resourcedata.com_title,
                        Status = (CompositionStatus)CompositionStatus.Final,
                        Type = new CodeableConcept
                        {
                            Coding = new List<Coding>
                            {
                                new Coding
                                {
                                    System = "https://loinc.org",
                                    Code = "67796-3",
                                    Display = "EMS patient care report - version 3 Document NEMSIS"
                                }
                            }
                        },
                        Date = new FhirDateTime(model.entry[0].Resource.resourcedata.com_date).ToString(),
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
                                Reference = "Practitioner/" + created_practitioner.Id
                            }
                        },
                        Section = new List<Composition.SectionComponent> {
                            sectionComponent
                        },

                    };
                    var composition_ToJson = composition.ToJson();
                    //composition.Section[0] = sectionComponent;

                    var createdComposition = client.Create<Composition>(composition);
                    Bundle.EntryComponent compositionEntry = new Bundle.EntryComponent
                    {
                        FullUrl =  "Composition/" + createdComposition.Id,
                        Resource = composition
                    };
                    bundle.Entry.Insert(0, compositionEntry);

                    #endregion

                    //Bundle統整
                    var bundle_ToJson = bundle.ToJson();
                    var createdBundle = client.Create<Bundle>(bundle);
                    TempData["status"] = "Create EMS succcess! Reference url:" + createdBundle.Id;
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    return View(model);
                }
            }
            return View(model);
        }

        public ActionResult Create_bundle()
        {
            ViewBag.ObserCode_Select = new SelectList(new ObservationCode().observationCode(), "chinese", "chinese");
            BundleModel model = new BundleModel();
            model.entry = new List<EntryViewModle>
            {
                new EntryViewModle
                    {
                        fullurl = string.Empty,
                        Resource = new EntryResourceModel
                        {
                            resourceType = string.Empty,
                            id = string.Empty,
                            resourcedata = new ResourceDetail
                            {
                                name = string.Empty,
                                birthDate = null,
                                gender = Gender.不知道,
                                Observations = new List<ObservationEMS>()
                            }
                        }
                    }
            };
            return View(model);
        }

        // POST: EMS/Create
        [HttpPost]
        public ActionResult Create_bundle(BundleModel model)
        {
            if (ModelState.IsValid)
            {
                ViewBag.ObserCode_Select = new SelectList(new ObservationCode().observationCode(), "chinese", "chinese");
                Servercookie(cookies.FHIR_Server_Cookie(HttpContext));
                FhirClient client = new FhirClient(cookies.FHIR_URL_Cookie(HttpContext), cookies.settings, handler);
                try
                {
                    //★資料裡必須先有一筆Location(預設ID是10)
                    List<string> Obserentry = new List<string>();

                    Bundle bundle = new Bundle
                    {
                        Identifier = new Identifier
                        {
                            Value = "MI11111",
                            System = "https://www.glc.tw/"
                        },
                        Timestamp = model.timestamp,
                        Type = (Bundle.BundleType)model.type,
                        Entry = new List<Bundle.EntryComponent>()
                    };

                    //bundle.Meta = new Meta
                    //{
                    //    Profile = new List<string> { "https://standard-interoperability-lab.com/fhir/StructureDefinition/EMS-Bundle" }
                    //};

                    #region//Patient創建
                    //製造Patient
                    Patient patient = new Patient()
                    {
                        Id = "pat-mitw-1003test1",
                        Name = new List<HumanName>()
                        {
                            new HumanName()
                            {
                                Use= (NameUse)model.entry[0].Resource.resourcedata.nameuse,
                                Text = model.entry[0].Resource.resourcedata.familyname+model.entry[0].Resource.resourcedata.name,
                                Family=model.entry[0].Resource.resourcedata.familyname,
                                Given = new List<string>
                                {
                                    model.entry[0].Resource.resourcedata.name,
                                }
                            }
                        },
                        Communication = new List<Patient.CommunicationComponent>
                        {
                            new Patient.CommunicationComponent
                            {
                                Language = new CodeableConcept
                                {
                                    Coding = new List<Coding>
                                    {
                                        new Coding
                                        {
                                            System = "urn:ietf:bcp:47",
                                            Code = "zh-TW"
                                        }
                                    }
                                }
                            }
                        },
                        Active = true,
                        BirthDate = model.entry[0].Resource.resourcedata.birthDate,
                        Gender = (AdministrativeGender)model.entry[0].Resource.resourcedata.gender,
                        Identifier = new List<Identifier> {
                            new Identifier
                            {
                                Type = new CodeableConcept
                                {
                                    Coding = new List<Coding>
                                    {
                                        new Coding
                                        {
                                            System = "http://terminology.hl7.org/CodeSystem/v2-0203",
                                            Code = model.entry[0].Resource.resourcedata.identifier_code
                                        }
                                    }
                                },
                                Use=Identifier.IdentifierUse.Official,
                                System = "https://www.tph.mohw.gov.tw/",
                                Value = model.entry[0].Resource.resourcedata.identifier,
                            }
                        },
                        Telecom = new List<ContactPoint>
                        {
                            new ContactPoint
                            {
                                System = ContactPoint.ContactPointSystem.Phone,
                                Value = model.entry[0].Resource.resourcedata.telecom,
                                Use=ContactPoint.ContactPointUse.Mobile,
                            },
                            new ContactPoint
                            {
                                System = ContactPoint.ContactPointSystem.Email,
                                Value = model.entry[0].Resource.resourcedata.email
                            },
                            new ContactPoint
                            {
                                System = ContactPoint.ContactPointSystem.Url,
                                Value = "https://line.me/ti/p/34b2c384l5",
                            },
                        },
                        Address = new List<Address>
                        {
                            new Address
                            {
                                Text = model.entry[0].Resource.resourcedata.zipcode+" 臺灣"+model.entry[0].Resource.resourcedata.city+model.entry[0].Resource.resourcedata.town+model.entry[0].Resource.resourcedata.address,
                                Country = "臺灣",
                                PostalCode = model.entry[0].Resource.resourcedata.zipcode,
                                City = model.entry[0].Resource.resourcedata.city,
                                District = model.entry[0].Resource.resourcedata.town,
                                Line = new List<string>
                                {
                                    model.entry[0].Resource.resourcedata.address
                                }
                            }
                        },
                        Contact = new List<Patient.ContactComponent>
                        {
                            new Patient.ContactComponent
                            {
                                Name = new HumanName()
                                {
                                    Use=HumanName.NameUse.Official,
                                    Text = model.entry[0].Resource.resourcedata.contact_familyname+model.entry[0].Resource.resourcedata.contact_name,
                                    Family=model.entry[0].Resource.resourcedata.contact_familyname,
                                    Given = new List<string>
                                    {
                                        model.entry[0].Resource.resourcedata.contact_name,
                                    }
                                },
                                Relationship = new List<CodeableConcept>
                                {
                                    new CodeableConcept("http://terminology.hl7.org/CodeSystem/v2-0131", "N", model.entry[0].Resource.resourcedata.contact_relationship)
                                },
                                Telecom = new List<ContactPoint>
                                {
                                    new ContactPoint
                                    {
                                        System = ContactPoint.ContactPointSystem.Phone,
                                        Value = model.entry[0].Resource.resourcedata.contact_telecom,
                                        Use=ContactPoint.ContactPointUse.Mobile,
                                    },
                                    new ContactPoint
                                    {
                                        System = ContactPoint.ContactPointSystem.Url,
                                        Value = "https://line.me/ti/p/34b2c384l5",
                                    },

                                }

                            },
                        },
                        ManagingOrganization = new ResourceReference
                        {
                            Reference = model.entry[0].Resource.resourcedata.managingOrganization
                        }
                    };

                    patient.Meta = new Meta
                    {
                        Profile = new List<string> { "https://twcore.mohw.gov.tw/ig/twcore/StructureDefinition/Patient-twcore" }
                    };

                    var conditions = new SearchParams();
                    conditions.Add("identifier", model.entry[0].Resource.resourcedata.identifier);
                    //var patient_ToJson = patient.ToJson();
                    var created_pat_A = client.Create<Patient>(patient, conditions);
                    //Bundle
                    Bundle.EntryComponent patientEntry = new Bundle.EntryComponent
                    {
                        FullUrl = "Patient/" + created_pat_A.Id,
                        Resource = patient
                    };
                    bundle.Entry.Add(patientEntry);
                    //Composition
                    Obserentry.Add("Patient/" + created_pat_A.Id);

                    #endregion

                    #region //Practitioner

                    Practitioner practitioner = new Practitioner()
                    {
                        Name = new List<HumanName>
                        {
                            new HumanName
                            {
                                Text = model.entry[0].Resource.resourcedata.pra_name
                            }
                        },
                        Identifier = new List<Identifier>
                        {
                            new Identifier
                            {
                                //System = model.entry[0].Resource.resourcedata.pra_identifier.value_system,
                                //Value = model.entry[0].Resource.resourcedata.pra_identifier.value_text
                                System = "https://www.tph.mohw.gov.tw",
                                Value = model.entry[0].Resource.resourcedata.pra_identifier.value_text
                            }
                        }
                    };
                    practitioner.Meta = new Meta
                    {
                        Profile = new List<string> { "https://twcore.mohw.gov.tw/ig/twcore/StructureDefinition/Practitioner-twcore" }
                    };
                    //var practitioner_ToJson = practitioner.ToJson();
                    var created_practitioner = client.Create<Practitioner>(practitioner);
                    //Composition要用到的
                    Obserentry.Add("Practitioner/" + created_practitioner.Id);

                    Bundle.EntryComponent practitionerEntry = new Bundle.EntryComponent
                    {
                        FullUrl = "Practitioner/" + created_practitioner.Id,
                        Resource = practitioner
                    };
                    bundle.Entry.Add(practitionerEntry);

                    #endregion

                    #region //Organization

                    Organization organization = new Organization()
                    {
                        Identifier = new List<Identifier>
                        {
                            new Identifier
                            {
                                Type = new CodeableConcept(){
                                    Text = model.entry[0].Resource.resourcedata.organ_identifier.code_text,
                                    Coding = new List<Coding>
                                    {
                                        new Coding
                                        {
                                            System = "http://terminology.hl7.org/CodeSystem/v2-0203",
                                            Code = "PRN"
                                        }
                                    }
                                },
                                System = " https://twcore.mohw.gov.tw/ig/twcore/CodeSystem/organization-identifier-tw",
                                Value = model.entry[0].Resource.resourcedata.organ_identifier.value_text
                            }
                        },
                        Active = model.entry[0].Resource.resourcedata.organ_active,
                        Name = model.entry[0].Resource.resourcedata.organ_name
                    };
                    organization.Meta = new Meta
                    {
                        Profile = new List<string> { "https://twcore.mohw.gov.tw/ig/twcore/StructureDefinition/Organization-twcore" }
                    };

                    var organization_ToJson = organization.ToJson();
                    var created_organization = client.Create<Organization>(organization);
                    //Composition要用到的
                    Obserentry.Add("Organization/" + created_organization.Id);

                    Bundle.EntryComponent organEntry = new Bundle.EntryComponent
                    {
                        FullUrl = "Organization/" + created_organization.Id,
                        Resource = organization
                    };
                    bundle.Entry.Add(organEntry);
                    #endregion

                    #region //Composition

                    Composition.SectionComponent sectionComponent = new Composition.SectionComponent
                    {
                        Entry = new List<ResourceReference>()
                    };
                    foreach (var obserId in Obserentry)
                    {
                        //把所有種類連結加進Composition的entry中
                        sectionComponent.Entry.Add(new ResourceReference
                        {
                            Reference = obserId
                        });
                    }

                    Composition composition = new Composition
                    {
                        Title = model.entry[0].Resource.resourcedata.com_title,
                        Status = (CompositionStatus)CompositionStatus.Final,
                        Type = new CodeableConcept
                        {
                            Coding = new List<Coding>
                            {
                                new Coding
                                {
                                    System = "https://loinc.org",
                                    Code = "67796-3",
                                    Display = "EMS patient care report - version 3 Document NEMSIS"
                                }
                            },
                        },
                        Date = new FhirDateTime(model.entry[0].Resource.resourcedata.com_date).ToString(),
                        Subject = new ResourceReference
                        {
                            Reference = "Patient/" + created_pat_A.Id
                        },
                        Author = new List<ResourceReference>
                        {
                            new ResourceReference{
                                Reference = "Practitioner/" + created_practitioner.Id
                            }
                        },
                        Section = new List<Composition.SectionComponent> {
                            sectionComponent
                        },

                    };

                    //composition.Meta = new Meta
                    //{
                    //    Profile = new List<string> { "https://twcore.mohw.gov.tw/ig/twcore/StructureDefinition/Organization-twcore" }
                    //};

                    var composition_ToJson = composition.ToJson();
                    //composition.Section[0] = sectionComponent;

                    var createdComposition = client.Create<Composition>(composition);
                    Bundle.EntryComponent compositionEntry = new Bundle.EntryComponent
                    {
                        FullUrl = "Composition/" + createdComposition.Id,
                        Resource = composition
                    };
                    bundle.Entry.Insert(0, compositionEntry);

                    #endregion

                    //Bundle統整
                    var bundle_ToJson = bundle.ToJson();
                    var createdBundle = client.Create<Bundle>(bundle);
                    TempData["status"] = "Create EMS succcess! Reference url:" + createdBundle.Id;
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    return View(model);
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
                handler.ServerCertificateCustomValidationCallback += (sender2, cert, chain, sslPolicyErrors) => true;
                handler.OnBeforeRequest += (sender, e) =>
                {
                    e.RawRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(cookies.FHIR_Token_Cookie(HttpContext))));
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
                    observation.Performer = new List<ResourceReference> { new ResourceReference("Organization/MITW.ForEMS") };

                    observation.Meta = new Meta
                    {
                        Profile = new List<string> { model.meta.ob_value.ToString() }
                    };


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
