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

namespace FHIR_Demo.Controllers
{
    public class EEC_DMSController : Controller
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
                        var obvalue = obdetail.Code_value.value_text;

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
                            Effective = new FhirDateTime("2023-10-01T16:52:00+08:00"),
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
                            Value = new FhirString { 
                                Value = obvalue
                            },
                            //Encounter = new ResourceReference
                            //{
                            //    Reference = "Encounter/" + created_pat_A.Id
                            //}

                        };

                        var observation_ToJson = observation.ToJson();
                        var created_obser = client.Create<Observation>(observation);

                        //Bundle
                        Bundle.EntryComponent obserEntry = new Bundle.EntryComponent
                        {
                            //FullUrl = cookies.FHIR_URL_Cookie(HttpContext) + "Observation/" + created_obser.Id,
                            FullUrl =  "Observation/" + created_obser.Id,
                            Resource = observation
                        };
                        bundle.Entry.Add(obserEntry);

                        //Composition要用到的
                        entryId.Add("Observation/" + created_obser.Id);

                    }

                    #endregion

                    #region //Procedure

                    foreach (var prodetail in model.entry[0].Resource.eec_icresourcedata.EEC_DMS_Procedure)
                    {
                        var PerformedTime = new FhirDateTime();
                        if (prodetail?.pro_performed.ToString() != null && !prodetail.pro_performed.Equals(DateTime.MinValue))
                        {
                            PerformedTime = new FhirDateTime(prodetail.pro_performed);
                        }
                        else
                        {
                            PerformedTime = null;
                        }

                        Procedure procedure = new Procedure()
                        {
                            Status = (EventStatus)prodetail.pro_status,
                            Subject = new ResourceReference
                            {
                                Reference = "Patient/" + created_pat_A.Id
                            },
                            Code = new CodeableConcept
                            {
                                Text = prodetail?.pro_code
                            },
                            Identifier = new List<Identifier>
                            {
                                new Identifier
                                {
                                    System = prodetail?.pro_identifier?.code_system,
                                    Value = prodetail?.pro_identifier?.value_text,
                                }
                            },
                            Category = new CodeableConcept
                            {
                                Coding = new List<Coding>
                                {
                                    new Coding
                                    {
                                        Code = prodetail?.pro_category?.code_code,
                                        System = prodetail?.pro_category?.code_system,
                                        Display = prodetail?.pro_category?.code_display,
                                    }
                                },
                                Text = prodetail?.pro_category?.code_text
                            },
                            Performed = PerformedTime,
                            Note = new List<Annotation> { 
                                new Annotation
                                {
                                    Text = new Markdown(prodetail?.pro_note)
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

                    }

                    #endregion

                    #region //Condition
                    for (int i = 0; i < model.entry[0].Resource.eec_icresourcedata.EEC_DMSCondition.Count; i++)
                    {
                        Condition condition = new Condition()
                        {
                            Identifier = new List<Identifier>(),
                            Category = new List<CodeableConcept>
                            {
                                new CodeableConcept
                                {
                                    Coding = new List<Coding>
                                    {
                                        new Coding
                                        {
                                            System = model?.entry[0]?.Resource?.eec_icresourcedata?.EEC_DMSCondition[i]?.con_category?.code_system,
                                            Code = model?.entry[0]?.Resource.eec_icresourcedata?.EEC_DMSCondition[i]?.con_category?.code_code,
                                            Display = model?.entry[0]?.Resource.eec_icresourcedata?.EEC_DMSCondition[i]?.con_category?.code_display
                                        }
                                    },
                                    Text = model?.entry[0]?.Resource.eec_icresourcedata?.EEC_DMSCondition[i]?.con_category?.code_text
                                }
                            },
                            ClinicalStatus = new CodeableConcept
                            {
                                Coding = new List<Coding>
                                {
                                    new Coding
                                    {
                                        System = model?.entry[0]?.Resource?.eec_icresourcedata?.EEC_DMSCondition[i]?.con_clinicalStatus?.code_system,
                                        Code = model?.entry[0]?.Resource.eec_icresourcedata?.EEC_DMSCondition[i]?.con_clinicalStatus?.code_code,
                                        Display = model?.entry[0]?.Resource.eec_icresourcedata?.EEC_DMSCondition[i]?.con_clinicalStatus?.code_display
                                    }
                                }
                            },
                            Code = new CodeableConcept
                            {
                                Text = model?.entry[0]?.Resource.eec_icresourcedata?.EEC_DMSCondition[i]?.con_code
                            },
                            Subject = new ResourceReference
                            {
                                Reference = "Patient/" + created_pat_A.Id
                            }
                        };

                        //Condition的合併症與併發症identifier要兩個
                        foreach (var conidentifier in model?.entry[0]?.Resource?.eec_icresourcedata?.EEC_DMSCondition[i]?.con_iden)
                        {
                            Identifier Identifier = new Identifier
                            {
                                System = conidentifier.code_system,
                                Value = conidentifier.value_text
                            };

                            condition.Identifier.Add(Identifier);
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
                    foreach (var organdetail in model.entry[0].Resource.eec_icresourcedata.EEC_DMS_Organization)
                    {
                        Organization organization = new Organization()
                        {
                            Identifier = new List<Identifier> { 
                                new Identifier
                                {
                                    System = organdetail?.organ_identifier?.code_system,
                                    Value = organdetail?.organ_identifier?.value_text
                                }
                            },
                            Name = organdetail?.organ_name
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
                    }

                    #endregion

                    #region //Location

                    Location location = new Location()
                    {
                        Identifier = new List<Identifier> {
                            new Identifier
                            {
                                Value = model.entry[0].Resource.eec_icresourcedata?.loc_iden?.value_text
                            }
                        }
                    };
                    var location_ToJson = location.ToJson();
                    var created_location = client.Create<Location>(location);
                    //Composition要用到的
                    entryId.Add("Location/" + created_location.Id);

                    Bundle.EntryComponent locationEntry = new Bundle.EntryComponent
                    {
                        //FullUrl = cookies.FHIR_URL_Cookie(HttpContext) + "Location/" + created_location.Id,
                        FullUrl =  "Location/" + created_location.Id,
                        Resource = location
                    };
                    bundle.Entry.Add(locationEntry);


                    #endregion

                    #region //Encounter

                    Encounter encounter = new Encounter()
                    {
                        Identifier = new List<Identifier> (),
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
                            Start = new FhirDateTime(model.entry[0].Resource.eec_icresourcedata.period_start).ToString(),
                            End = new FhirDateTime(model.entry[0].Resource.eec_icresourcedata.period_end).ToString()
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
                        },
                        Location = new List<LocationComponent>
                        {
                            new LocationComponent
                            {
                                Location = new ResourceReference{
                                    Reference = "Location/" + created_location.Id
                                },
                                PhysicalType = new CodeableConcept
                                {
                                    Coding = new List<Coding>
                                    {
                                        new Coding
                                        {
                                            System = model.entry[0].Resource.eec_icresourcedata.physicalType.code_system,
                                            Code = model.entry[0].Resource.eec_icresourcedata.physicalType.code_code,
                                            Display = model.entry[0].Resource.eec_icresourcedata.physicalType.code_display
                                        }
                                    },
                                    Text = model.entry[0].Resource.eec_icresourcedata.physicalType.code_text
                                }
                            }
                        }
                    };

                    //Encounter的identifier要兩個
                    for (int i = 0; i < model.entry[0].Resource.eec_icresourcedata.en_identifier.Count; i++)
                    {
                        Identifier Identifier = new Identifier
                        {
                            Type = new CodeableConcept()
                            {
                                Text = model.entry[0].Resource.eec_icresourcedata.en_identifier[i].code_text,
                            },
                            System = model.entry[0].Resource.eec_icresourcedata.en_identifier[i].code_system,
                            Value = model.entry[0].Resource.eec_icresourcedata.en_identifier[i].value_text
                        };

                        encounter.Identifier.Add(Identifier);
                    }

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

                    #region //ImagingStudy

                    ImagingStudy imagingStudy = new ImagingStudy()
                    {
                        Identifier = new List<Identifier> {
                            new Identifier
                            {
                                System = model.entry[0].Resource.eec_icresourcedata?.ima_iden?.code_system,
                                Value = model.entry[0].Resource.eec_icresourcedata?.ima_iden?.value_text
                            }
                        },
                        Status = (ImagingStudy.ImagingStudyStatus)model.entry[0].Resource.eec_icresourcedata.ima_status,
                        ReasonCode = new List<CodeableConcept>
                        {
                            new CodeableConcept
                            {
                                Text = model.entry[0].Resource.eec_icresourcedata?.ima_reason?.code_text
                            }
                        },
                        Series = new List<ImagingStudy.SeriesComponent>
                        {
                            new ImagingStudy.SeriesComponent
                            {
                                Uid = "1",
                                Modality = new Coding
                                {
                                    Display = model.entry[0].Resource.eec_icresourcedata?.ima_modality?.code_display
                                },
                                BodySite = new Coding
                                {
                                    Display = model.entry[0].Resource.eec_icresourcedata?.ima_bodysite?.code_display
                                }
                            }
                        },
                        Referrer = new ResourceReference
                        {
                            Reference = "Practitioner/" + created_practitioner.Id
                        },
                        Started = new FhirDateTime(model.entry[0].Resource.eec_icresourcedata.img_stared).ToString(),
                        Subject = new ResourceReference
                        {
                            Reference = "Patient/" + created_pat_A.Id
                        }
                    };

                    var imagingStudy_ToJson = imagingStudy.ToJson();
                    var created_imagingStudy = client.Create<ImagingStudy>(imagingStudy);
                    //Composition要用到的
                    entryId.Add("ImagingStudy/" + created_imagingStudy.Id);
                    //Bundle
                    Bundle.EntryComponent imagingStudyEntry = new Bundle.EntryComponent
                    {
                        //FullUrl = cookies.FHIR_URL_Cookie(HttpContext) + "ImagingStudy/" + created_imagingStudy.Id,
                        FullUrl =  "ImagingStudy/" + created_imagingStudy.Id,
                        Resource = imagingStudy
                    };
                    bundle.Entry.Add(imagingStudyEntry);

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
                                ContentType = meddetail?.med_contentType,
                                Title = meddetail?.med_title
                            },
                            Identifier = new List<Identifier>
                            {
                                new Identifier
                                {
                                    System = meddetail?.med_iden?.code_system,
                                    Value = meddetail?.med_iden?.value_text,
                                }
                            },
                            Modality = new CodeableConcept
                            {
                                Coding = new List<Coding>
                                {
                                    new Coding
                                    {
                                        Code = meddetail?.med_modality?.code_code
                                    }
                                },
                                Text = meddetail?.med_modality?.code_text
                            }
                        };

                        var media_ToJson = media.ToJson();
                        var created_media = client.Create<Media>(media);

                        //Bundle
                        Bundle.EntryComponent mediaEntry = new Bundle.EntryComponent
                        {
                            //FullUrl = cookies.FHIR_URL_Cookie(HttpContext) + "Media/" + created_media.Id,
                            FullUrl =  "Media/" + created_media.Id,
                            Resource = media
                        };
                        bundle.Entry.Add(mediaEntry);

                        //Composition要用到的
                        entryId.Add("Media/" + created_media.Id);

                    }

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
                            Text = model.entry[0].Resource.eec_icresourcedata.com_type.code_text
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
                    TempData["status"] = "Create EEC succcess! Reference url:" + createdBundle.Id;
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