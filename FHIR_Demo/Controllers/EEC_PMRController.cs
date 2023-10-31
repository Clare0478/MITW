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
using FHIR_Demo.Models;
using Hl7.Fhir.Model;
using static Hl7.Fhir.Model.AllergyIntolerance;

namespace FHIR_Demo.Controllers
{
    public class EEC_PMRController : Controller
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
            //讓系統通過對於不安全的https連線
            Servercookie(cookies.FHIR_Server_Cookie(HttpContext));
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
            //讓系統通過對於不安全的https連線
            Servercookie(cookies.FHIR_Server_Cookie(HttpContext));
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

        // GET: EEC_DMS/Create
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

        // POST: EEC_DMS/Create
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
                            System = model.identifier.code_system
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
                                System = "http://www.moi.gov.tw/",
                                Value = "Z123456789"
                            },
                            new Identifier
                            {
                                System = "https://loinc.org/",
                                Value = "29762-2"
                            }
                        }
                    };

                    var conditions = new SearchParams();
                    conditions.Add("identifier", model.entry[0].Resource.eec_icresourcedata.identifier);
                    var patient_ToJson = patient.ToJson();
                    var created_pat_A = client.Create<Patient>(patient, conditions);
                    //Bundle
                    Bundle.EntryComponent patientEntry = new Bundle.EntryComponent
                    {
                        //FullUrl = cookies.FHIR_URL_Cookie(HttpContext) + "Patient/" + created_pat_A.Id,
                        FullUrl =  "Patient/" + created_pat_A.Id,
                        Resource = patient
                    };
                    bundle.Entry.Add(patientEntry);
                    //Composition
                    entryId.Add("Patient/" + created_pat_A.Id);

                    #endregion

                    #region //Practitioner

                    Practitioner practitioner = new Practitioner()
                    {
                        Name = new List<HumanName>
                        {
                            new HumanName
                            {
                                Text = model.entry[0].Resource.eec_icresourcedata.practitioners[0].pra_name
                            }
                        }
                    };
                    var practitioner_ToJson = practitioner.ToJson();
                    var created_practitioner = client.Create<Practitioner>(practitioner);
                    //Composition要用到的
                    entryId.Add("Practitioner/" + created_practitioner.Id);

                    Bundle.EntryComponent practitionerEntry = new Bundle.EntryComponent
                    {
                        //FullUrl = cookies.FHIR_URL_Cookie(HttpContext) + "Practitioner/" + created_practitioner.Id,
                        FullUrl = "Practitioner/" + created_practitioner.Id,
                        Resource = practitioner
                    };
                    bundle.Entry.Add(practitionerEntry);


                    #endregion

                    #region //Observation
                    ViewBag.ObserCode_Select = new SelectList(new ObservationCode().observationCode(), "chinese", "chinese");

                    foreach (var obdetail in model.entry[0].Resource.eec_icresourcedata.EEC_DMSObservations)
                    {
                        string obvalue = obdetail?.Code_value?.value_text;

                        Observation observation = new Observation()
                        {
                            Status = (ObservationStatus)obdetail.ob_status,
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
                            Effective = new FhirDateTime((DateTime)(obdetail?.effectivePeriod_star)),
                            //Effective = new FhirDateTime("2023-10-01T14:55:02"),
                            Code = new CodeableConcept
                            {
                                Coding = new List<Coding>
                                {
                                    new Coding
                                    {
                                        System = obdetail?.ob_code?.code_system,
                                        Code = obdetail?.ob_code?.code_code,
                                        Display = obdetail?.ob_code?.code_display
                                    }
                                },
                                Text = obdetail?.ob_code?.code_text
                            },
                            Identifier = new List<Identifier>
                            {
                                new Identifier
                                {
                                    System = obdetail?.ob_identifier?.code_system,
                                    Value = obdetail?.ob_identifier?.value_text,
                                }
                            },
                            Value = new FhirString
                            {
                                Value = obvalue
                            }
                            //Encounter = new ResourceReference
                            //{
                            //    Reference = "Encounter/" + created_pat_A.Id
                            //}

                        };

                        //component
                        if(obdetail?.component != null)
                        {
                            for (int n = 0; n < obdetail.component.Length; n++)
                            {
                                var AddComponent = new Observation.ComponentComponent
                                {
                                    Code = new CodeableConcept
                                    {
                                        Coding = new List<Coding>
                                        {
                                            new Coding
                                            {
                                                System = obdetail.component[n].Code.Coding[0].System,
                                                Code = obdetail.component[n].Code.Coding[0].Code,
                                                Display = obdetail.component[n].Code.Coding[0].Display,
                                            },
                                        },
                                    },
                                    Value = new FhirString{
                                        Value = model.entry[0].Resource.eec_icresourcedata.value[n]
                                    }
                                };

                                observation.Component.Add(AddComponent);
                            }
                        }
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

                    #region //Procedure
                    Procedure procedure = new Procedure()
                    {
                        Status = (EventStatus)model.entry[0].Resource.eec_icresourcedata.pro_status,
                        Subject = new ResourceReference
                        {
                            Reference = "Patient/" + created_pat_A.Id
                        },
                        Code = new CodeableConcept
                        {
                            Coding = new List<Coding>
                            {
                                new Coding
                                {
                                    System =  "https://www.mohw.gov.tw",
                                    Code =  "19009C",
                                    Display =  "腹部超音波，追蹤性"
                                }
                            }
                        },
                        Identifier = new List<Identifier>
                        {
                            new Identifier
                            {
                                System =  model.entry[0].Resource.eec_icresourcedata?.pro_identifier?[0]?.code_system,
                                Value =  model.entry[0].Resource.eec_icresourcedata?.pro_identifier?[0]?.value_text,
                            },
                            new Identifier
                            {
                                 System =  model.entry[0].Resource.eec_icresourcedata?.pro_identifier?[1]?.code_system,
                                Value =  model.entry[0].Resource.eec_icresourcedata?.pro_identifier?[1]?.value_text,
                            }
                        },
                        Category = new CodeableConcept
                        {
                            Coding = new List<Coding>
                            {
                                new Coding
                                {
                                    Code =  model.entry[0].Resource.eec_icresourcedata?.pro_category?.code_code,
                                    System =  model.entry[0].Resource.eec_icresourcedata?.pro_category?.code_system,
                                    Display =  model.entry[0].Resource.eec_icresourcedata?.pro_category?.code_display,
                                }
                            },
                            Text =  model.entry[0].Resource.eec_icresourcedata?.pro_category?.code_text
                        },
                        Note = new List<Annotation> {
                            new Annotation
                            {
                                Text = new Markdown( model.entry[0].Resource.eec_icresourcedata?.pro_note[0])
                            },
                            new Annotation
                            {
                                Text = new Markdown( model.entry[0].Resource.eec_icresourcedata?.pro_note[1])
                            }
                        },
                        FollowUp = new List<CodeableConcept>
                        {
                            new CodeableConcept
                            {
                                Text = model.entry[0].Resource.eec_icresourcedata.pro_followUp[0]
                            },
                            new CodeableConcept
                            {
                                Text = model.entry[0].Resource.eec_icresourcedata.pro_followUp[1]
                            },
                            new CodeableConcept
                            {
                                Text = model.entry[0].Resource.eec_icresourcedata.pro_followUp[2]
                            }
                        }

                    };

                    var procedure_ToJson = procedure.ToJson();
                    var created_procedure = client.Create<Procedure>(procedure);

                    //Bundle
                    Bundle.EntryComponent procedureEntry = new Bundle.EntryComponent
                    {
                        //FullUrl = cookies.FHIR_URL_Cookie(HttpContext) + "Procedure/" + created_procedure.Id,
                        FullUrl = "Procedure/" + created_procedure.Id,
                        Resource = procedure
                    };
                    bundle.Entry.Add(procedureEntry);

                    //Composition要用到的
                    entryId.Add("Procedure/" + created_procedure.Id);

                    

                    #endregion

                    #region //Condition
                    for (int i = 0; i < model.entry[0].Resource.eec_icresourcedata.EEC_DMSCondition.Count; i++)
                    {
                        Condition condition = new Condition()
                        {
                            Identifier = new List<Identifier> { 
                                new Identifier
                                {
                                    System =  model.entry[0].Resource.eec_icresourcedata?.EEC_DMSCondition[i].con_iden[0].code_system,
                                    Value =  model.entry[0].Resource.eec_icresourcedata?.EEC_DMSCondition[i].con_iden[0].value_text
                                }
                            },
                            Code = new CodeableConcept(),
                            Subject = new ResourceReference
                            {
                                Reference = "Patient/" + created_pat_A.Id
                            },
                            Note = new List<Annotation>
                            {
                                new Annotation
                                {
                                    Text = new Markdown( model.entry[0].Resource.eec_icresourcedata?.EEC_DMSCondition[i].con_note)
                                }
                            }
                        };

                        if(i == 0)
                        {
                            condition.Code = new CodeableConcept
                            {
                                Coding = new List<Coding>
                                {
                                    new Coding
                                    {
                                        System =  "https://loinc.org/",
                                        Code =  "11338-1",
                                        Display =  "History of major illnesses and injuries"
                                    }
                                },
                                Text = "History of major illnesses and injuries"
                            };
                        }
                        else if(i == 1)
                        {
                            condition.Code = new CodeableConcept
                            {
                                Coding = new List<Coding>
                                {
                                    new Coding
                                    {
                                        System =  "http://terminology.hl7.org/CodeSystem/condition-clinical",
                                        Code =  "active",
                                        Display =  "Active"
                                    }
                                },
                                Text = "診斷"
                            };
                        }


                        var condition_ToJson = condition.ToJson();
                        var created_condition = client.Create<Condition>(condition);
                        //Composition要用到的
                        entryId.Add("Condition/" + created_condition.Id);

                        Bundle.EntryComponent conditionEntry = new Bundle.EntryComponent
                        {
                            //FullUrl = cookies.FHIR_URL_Cookie(HttpContext) + "Condition/" + created_condition.Id,
                            FullUrl = "Condition/" + created_condition.Id,
                            Resource = condition
                        };
                        bundle.Entry.Add(conditionEntry);
                    }
                    #endregion

                    #region //Organization
                    var organizationid = "";
                    Organization organization = new Organization()
                    {
                        Identifier = new List<Identifier> {
                            new Identifier
                            {
                                System = model.entry[0].Resource.eec_icresourcedata?.organ_identifier?.code_system,
                                Value = model.entry[0].Resource.eec_icresourcedata?.organ_identifier?.value_text
                            }
                        },
                        Name = model.entry[0].Resource.eec_icresourcedata?.organ_name
                    };


                    var organization_ToJson = organization.ToJson();
                    var created_organization = client.Create<Organization>(organization);
                    organizationid = "Organization/" + created_organization.Id;
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
                        Identifier = new List<Identifier>
                        {
                            new Identifier
                            {
                                System = "https://www.vghtpe.gov.tw/Index.action",
                                Value =  "Encounter唯一碼"

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
                                    Reference = "Practitioner/" + created_practitioner.Id
                                    //Reference = "Practitioner/106"
                                }
                            }
                        },
                        Period = new Period
                        {
                            Start = new FhirDateTime(model.entry[0].Resource.eec_icresourcedata.period_start).ToString()
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
                            Reference = organizationid,
                            //Reference = "Organization/98"
                        },
                        Subject = new ResourceReference
                        {
                            //Reference = "Patient/7"                           
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

                    #region //AllergyIntolerance

                    AllergyIntolerance allergyIntolerance = new AllergyIntolerance()
                    {
                        Identifier = new List<Identifier>
                        {
                            new Identifier
                            {
                                System = model.entry[0].Resource.eec_icresourcedata?.allergy_iden?.code_system,
                                Value = model.entry[0].Resource.eec_icresourcedata?.allergy_iden?.value_text
                            }
                        },
                        Patient = new ResourceReference
                        {
                            Reference = "Patient/" + created_pat_A.Id 
                        },
                        Code = new CodeableConcept
                        {
                            Text = model.entry[0].Resource.eec_icresourcedata.allergy_code.code_text
                        },
                        VerificationStatus = new CodeableConcept
                        {
                            Coding = new List<Coding>
                            {
                                new Coding
                                {
                                    System = "http://terminology.hl7.org/CodeSystem/allergyintolerance-verification",
                                    Code = "unconfirmed",
                                    Display = "unconfirmed",
                                }
                            }
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
                        }
                    };
                    var allergyIntolerance_ToJson = allergyIntolerance.ToJson();
                    var created_allergyIntolerance = client.Create<AllergyIntolerance>(allergyIntolerance);
                    //Composition要用到的
                    entryId.Add("AllergyIntolerance/" + created_allergyIntolerance.Id);

                    Bundle.EntryComponent allerEntry = new Bundle.EntryComponent
                    {
                        //FullUrl = cookies.FHIR_URL_Cookie(HttpContext) + "AllergyIntolerance/" + created_allergyIntolerance.Id,
                        FullUrl = "AllergyIntolerance/" + created_allergyIntolerance.Id,
                        Resource = allergyIntolerance
                    };
                    bundle.Entry.Add(allerEntry);

                    #endregion

                    #region //ClinicalImpression

                    foreach (var clidetail in model.entry[0].Resource.eec_icresourcedata.EEC_PMRClinicalImpression)
                    {
                        ClinicalImpression clinicalImpression = new ClinicalImpression()
                        {
                            Status = (ClinicalImpression.ClinicalImpressionStatus)clidetail.cli_status,
                            Subject = new ResourceReference
                            {
                                Reference = "Patient/" + created_pat_A.Id
                            },
                            Code = new CodeableConcept
                            {
                                Coding = new List<Coding>
                                {
                                    new Coding
                                    {
                                        System = clidetail?.cli_code?.code_system,
                                        Code = clidetail?.cli_code?.code_code,
                                        Display = clidetail?.cli_code?.code_display
                                    }
                                },
                                Text = clidetail?.cli_code?.code_text
                            },
                            Identifier = new List<Identifier>
                            {
                                new Identifier
                                {
                                    System = clidetail?.cli_iden?.code_system,
                                    Value = clidetail?.cli_iden?.value_text,
                                }
                            },
                            Note = new List<Annotation>
                            {
                                new Annotation
                                {
                                    Text = new Markdown(clidetail?.cli_note.Value)
                                }
                            }

                        };

                        var clinicalImpression_ToJson = clinicalImpression.ToJson();
                        var created_clinicalImpression = client.Create<ClinicalImpression>(clinicalImpression);

                        //Bundle
                        Bundle.EntryComponent clinicalImpressionEntry = new Bundle.EntryComponent
                        {
                            //FullUrl = cookies.FHIR_URL_Cookie(HttpContext) + "ClinicalImpression/" + created_clinicalImpression.Id,
                            FullUrl = "ClinicalImpression/" + created_clinicalImpression.Id,
                            Resource = clinicalImpression
                        };
                        bundle.Entry.Add(clinicalImpressionEntry);

                        //Composition要用到的
                        entryId.Add("ClinicalImpression/" + created_clinicalImpression.Id);

                    }

                    #endregion

                    #region //Media

                    foreach (var meddetail in model.entry[0].Resource.eec_icresourcedata.EEC_DMSMedia)
                    {
                        Media media = new Media()
                        {
                            Status = (EventStatus)meddetail.med_Status,
                            Subject = new ResourceReference
                            {
                                Reference = "Patient/" + created_pat_A.Id
                            },
                            Content = new Attachment
                            {
                                Title = meddetail?.med_title
                            },
                            Type = new CodeableConcept
                            {
                                Coding = new List<Coding>
                                    {
                                        new Coding
                                        {
                                            System = meddetail.med_type?.code_system,
                                            Code = meddetail.med_type?.code_code,
                                            Display = meddetail.med_type?.code_display
                                        }
                                    },
                                Text = meddetail.med_type?.code_text
                            },
                            Identifier = new List<Identifier>
                            {
                                new Identifier
                                {
                                    System = meddetail?.med_iden?.code_system,
                                    Value = meddetail?.med_iden?.value_text,
                                }
                            }
                        };

                        var media_ToJson = media.ToJson();
                        var created_media = client.Create<Media>(media);

                        //Bundle
                        Bundle.EntryComponent mediaEntry = new Bundle.EntryComponent
                        {
                            //FullUrl = cookies.FHIR_URL_Cookie(HttpContext) + "Media/" + created_media.Id,
                            FullUrl = "Media/" + created_media.Id,
                            Resource = media
                        };
                        bundle.Entry.Add(mediaEntry);

                        //Composition要用到的
                        entryId.Add("Media/" + created_media.Id);

                    }

                    #endregion

                    #region //Medication

                    Medication medication = new Medication()
                    {
                        Code = new CodeableConcept
                        {
                            Coding = new List<Coding>
                                {
                                    new Coding
                                    {
                                        System = model.entry[0].Resource.eec_icresourcedata?.med_code?.code_system,
                                        Code = model.entry[0].Resource.eec_icresourcedata?.med_code?.code_code,
                                        Display = model.entry[0].Resource.eec_icresourcedata?.med_code?.code_display
                                    }
                                },
                            Text = model.entry[0].Resource.eec_icresourcedata?.med_code?.code_text
                        },
                        Form = new CodeableConcept
                        {
                            Coding = new List<Coding>
                                {
                                    new Coding
                                    {
                                        System = model.entry[0].Resource.eec_icresourcedata?.med_form?.code_system,
                                        Code = model.entry[0].Resource.eec_icresourcedata?.med_form?.code_code,
                                        Display = model.entry[0].Resource.eec_icresourcedata?.med_form?.code_display
                                    }
                                },
                            Text = model.entry[0].Resource.eec_icresourcedata?.med_form?.code_text
                        }
                    };


                    var medication_ToJson = medication.ToJson();
                    var created_medication = client.Create<Medication>(medication);
                    //Composition要用到的
                    entryId.Add("Medication/" + created_medication.Id);
                    //Bundle
                    Bundle.EntryComponent medicationEntry = new Bundle.EntryComponent
                    {
                        //FullUrl = cookies.FHIR_URL_Cookie(HttpContext) + "Medication/" + created_medication.Id,
                        FullUrl = "Medication/" + created_medication.Id,
                        Resource = medication
                    };
                    bundle.Entry.Add(medicationEntry);

                    #endregion

                    #region //MedicationRequest

                    MedicationRequest medicationRequest = new MedicationRequest()
                    {
                        Status = (MedicationRequest.medicationrequestStatus)model.entry[0].Resource.eec_icresourcedata.medrq_status,
                        Intent = (MedicationRequest.medicationRequestIntent)model.entry[0].Resource.eec_icresourcedata.medrq_intent,
                        Identifier = new List<Identifier> {
                            new Identifier
                            {
                                System =  model.entry[0].Resource.eec_icresourcedata?.medrq_iden.code_system,
                                Value =  model.entry[0].Resource.eec_icresourcedata?.medrq_iden.value_text
                            }
                        },
                        Category = new List<CodeableConcept>
                        {
                            new CodeableConcept
                            {
                                Coding = new List<Coding>
                                {
                                    new Coding
                                    {
                                        System = model.entry[0].Resource.eec_icresourcedata?.medrq_category?.code_system,
                                        Code = model.entry[0].Resource.eec_icresourcedata?.medrq_category?.code_code,
                                        Display = model.entry[0].Resource.eec_icresourcedata?.medrq_category?.code_display
                                    }
                                }
                            }
                        },
                        Note = new List<Annotation>
                        {
                            new Annotation
                            {
                                Text = new Markdown(model.entry[0].Resource.eec_icresourcedata.medrq_note.code_text)
                            },
                            new Annotation
                            {
                                Text = new Markdown("自費註記Self-pay note")
                            }
                        },
                        Medication = new CodeableConcept
                        {
                            Coding = new List<Coding>
                            {
                                new Coding
                                {
                                    System = "https://www1.nhi.gov.tw/QueryN/Query1.aspx",
                                    Code = "AB55028100",
                                    Display = "IRBETAN F.C. TABLETS 150 MG"
                                }
                            }
                        },
                        DosageInstruction = new List<Dosage>
                        {
                            new Dosage
                            {
                                Timing = new Timing
                                {
                                    Code = new CodeableConcept
                                    {
                                        Coding = new List<Coding>
                                        {
                                            new Coding
                                            {
                                                Code = model.entry[0].Resource.eec_icresourcedata.medrq_timing.code_code,
                                                System = model.entry[0].Resource.eec_icresourcedata.medrq_timing.code_system,
                                                Display = model.entry[0].Resource.eec_icresourcedata.medrq_timing.code_display,
                                            }
                                        },
                                        Text = model.entry[0].Resource.eec_icresourcedata.medrq_timing.code_text
                                    }
                                },
                                Route = new CodeableConcept
                                {
                                    Coding = new List<Coding>
                                    {
                                        new Coding
                                        {
                                            Code = model.entry[0].Resource.eec_icresourcedata.medrq_route.code_code,
                                            System = model.entry[0].Resource.eec_icresourcedata.medrq_route.code_system,
                                            Display = model.entry[0].Resource.eec_icresourcedata.medrq_route.code_display,
                                        }
                                    },
                                    Text = model.entry[0].Resource.eec_icresourcedata.medrq_route.code_text
                                },
                                DoseAndRate = new List<Dosage.DoseAndRateComponent>
                                {
                                    new Dosage.DoseAndRateComponent
                                    {
                                        Dose = new Quantity
                                        {
                                            Value = model.entry[0].Resource.eec_icresourcedata.medrq_dose.value,
                                            Unit = model.entry[0].Resource.eec_icresourcedata.medrq_dose.unit
                                        }
                                    }
                                },
                                MaxDosePerAdministration = new Quantity
                                {
                                    Value = model.entry[0].Resource.eec_icresourcedata.medrq_maxdose.value,
                                    Unit = model.entry[0].Resource.eec_icresourcedata.medrq_maxdose.unit
                                }
                            }
                        },
                        DispenseRequest = new MedicationRequest.DispenseRequestComponent
                        {
                            Quantity = new Quantity
                            {
                                Value = model.entry[0].Resource.eec_icresourcedata.medrq_quantity.value,
                                Unit = model.entry[0].Resource.eec_icresourcedata.medrq_quantity.unit
                            },
                            ExpectedSupplyDuration = new Duration
                            {
                                Value = model.entry[0].Resource.eec_icresourcedata.medrq_duration
                            }
                        },
                        Subject = new ResourceReference
                        {
                            Reference = "Patient/" + created_pat_A.Id
                        }
                    };

                    var medicationRequest_ToJson = medicationRequest.ToJson();
                    var created_medicationRequest = client.Create<MedicationRequest>(medicationRequest);
                    //Composition要用到的
                    entryId.Add("MedicationRequest/" + created_medicationRequest.Id);
                    //Bundle
                    Bundle.EntryComponent medicationRequestEntry = new Bundle.EntryComponent
                    {
                        //FullUrl = cookies.FHIR_URL_Cookie(HttpContext) + "MedicationRequest/" + created_medicationRequest.Id,
                        FullUrl = "MedicationRequest/" + created_medicationRequest.Id,
                        Resource = medicationRequest
                    };
                    bundle.Entry.Add(medicationRequestEntry);

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
                            },
                            Text = model.entry[0].Resource.eec_icresourcedata.com_type.code_display
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
                                Reference = "Practitioner/" + created_practitioner.Id
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
                        FullUrl = "Composition/" + createdComposition.Id,
                        Resource = composition
                    };
                    bundle.Entry.Insert(0, compositionEntry);

                    #endregion

                    //Bundle統整
                    var createdBundle = client.Create<Bundle>(bundle);
                    TempData["status"] = "Create Bundle succcess! Reference url:" + createdBundle.Id;
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
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