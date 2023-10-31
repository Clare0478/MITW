using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using FHIR_Demo.Models;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using static Hl7.Fhir.Model.HumanName;

namespace FHIR_Demo.Models
{
    public class BundleModel
    {
        public string Id { get; set; }
        public string Meta { get; set; }

        [Display(Name = "識別碼")]
        //[RegularExpression(@"^[A-Z]{1}[A-Da-d1289]{1}[0-9]{8}$", ErrorMessage = "身分證字號錯誤")]
        public Obser_Code_Value identifier { get; set; }

        [Display(Name = "種類")]
        [Required]
        public BundleType type { get; set; }

        [Display(Name = "創建時間")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss.fff}", ApplyFormatInEditMode = true)]
        public DateTime timestamp { get; set; } 

        public List<EntryViewModle> entry { get; set; }

        public BundleModel EMSBundleViewModelMapping(Bundle bundle)
        {
            this.Id = bundle.Id;
            //if (bundle.Identifier != null)
            //    this.identifier.value_text = bundle.Identifier.Value;
            this.type = (BundleType)bundle.Type;
            if (bundle.Timestamp != null)
                this.timestamp = DateTime.Parse(bundle?.Timestamp?.ToString());

            //entry
            if (bundle.Entry != null && bundle.Entry.Count > 0)
            {
                this.entry = new List<EntryViewModle>();
                foreach (var entry in bundle.Entry)
                {
                    var entryViewModel = new EntryViewModle
                    {
                        fullurl = entry.FullUrl,
                        Resource = new EntryResourceModel()
                    };

                    entryViewModel.Resource.resourceType = entry.Resource.TypeName;
                    entryViewModel.Resource.id = entry.Resource.Id;
                    switch (entry.Resource.TypeName)
                    {
                        case "Patient":
                            entryViewModel.Resource.resourcedata = MapPatientResource(entry.Resource as Patient);
                            break;
                        case "Observation":
                            entryViewModel.Resource.resourcedata = MapObservationResource(entry.Resource as Observation);
                            break;
                        case "Encounter":
                            entryViewModel.Resource.resourcedata = MapEncounterResource(entry.Resource as Encounter);
                            break;
                        case "Condition":
                            entryViewModel.Resource.resourcedata = MapConditionResource(entry.Resource as Condition);
                            break;
                        case "RiskAssessment":
                            entryViewModel.Resource.resourcedata = MapRiskAssessmentResource(entry.Resource as RiskAssessment);
                            break;
                        case "Practitioner":
                            entryViewModel.Resource.resourcedata = MapPractitionerResource(entry.Resource as Practitioner);
                            break;
                        case "AllergyIntolerance":
                            entryViewModel.Resource.resourcedata = MapAllergyIntoleranceResource(entry.Resource as AllergyIntolerance);
                            break;
                        case "Organization":
                            entryViewModel.Resource.resourcedata = MapOrganizationResource(entry.Resource as Organization);
                            break;
                        case "Composition":
                            entryViewModel.Resource.resourcedata = MapCompositionResource(entry.Resource as Composition);
                            break;

                    }
                    // 將 entryViewModel 加入到 EMSModle 的 entry 列表中
                    this.entry.Add(entryViewModel);
                }
            }
            return this;
        }

        //建立Patient需要的ResourceDetail項目
        private ResourceDetail MapPatientResource(Patient patientResource)
        {
            DateTime? date = null;
            if (patientResource.BirthDate != null)
            {
                date = DateTime.Parse(patientResource?.BirthDate);
            }
            var resourceDetail = new ResourceDetail
            {
                name = patientResource?.Name.FirstOrDefault()?.Text?.ToString(),
                familyname = patientResource?.Name.FirstOrDefault()?.Family,
                givename = patientResource?.Name.FirstOrDefault()?.Given.FirstOrDefault(),
                birthDate = patientResource?.BirthDate,
                gender = patientResource.Gender != null ? (Gender)patientResource.Gender : Gender.不知道,
                active = patientResource?.Active?.ToString() ?? "True",
                identifier = patientResource?.Identifier.FirstOrDefault()?.Value
            };

            return resourceDetail;
        }

        private ResourceDetail MapObservationResource(Observation observationResource)
        {
            DateTime? effectiontime = null;
            Obser_Code_Value codevalue = new Obser_Code_Value();
            if (observationResource?.Effective?.TypeName == null)
            {
                effectiontime = null;
            }
            else if (observationResource?.Effective?.TypeName != "Period")
            {
                effectiontime = DateTime.Parse(observationResource?.Effective?.ToString());
            }
            else
            {
                effectiontime = DateTime.Parse(((Period)observationResource?.Effective)?.Start);
            }

            if (observationResource?.Value?.GetType() == typeof(Quantity))
            {
                codevalue.ob_value = ((Quantity)observationResource?.Value)?.Value ?? null;
                codevalue.unit = ((Quantity)observationResource?.Value)?.Unit ?? null;
            }//要增加datetime跟布林值
            else if (observationResource?.Value?.GetType() == typeof(DateTime))
            {
                codevalue.ob_value = DateTime.Parse(observationResource?.Value?.ToString() ?? null);
            }
            else if (observationResource?.Value is CodeableConcept code)
            {
                codevalue.code_code = code.Coding.FirstOrDefault()?.Code ?? null;
                codevalue.code_system = code.Coding.FirstOrDefault()?.System ?? null;
                codevalue.code_display = code.Coding.FirstOrDefault()?.Display ?? null;
            }
            else /*if (observationResource?.Value?.GetType() == typeof(FhirBoolean))*/
            {
                codevalue.ob_value = observationResource?.Value?.ToString() ?? null;
            }

            var resourceDetail = new ResourceDetail
            {
                Observations = new List<ObservationEMS>()
            };

            var Ob_Detail = new ObservationEMS
            {
                ob_status = observationResource?.Status != null ? (Obser_Status)observationResource.Status : Obser_Status.未知,
                ob_subject = observationResource?.Subject?.Reference.ToString(),
                Code_value = new Obser_Code_Value
                {//code跟value[x]
                    code_display = codevalue?.code_display ?? null,
                    code_code = codevalue?.code_code ?? null,
                    code_system = codevalue?.code_system ?? null,
                    ob_value = codevalue?.ob_value,
                    unit = codevalue?.unit
                },
                ob_code = new Obser_Code_Value
                {
                    code_code = observationResource?.Code?.Coding.FirstOrDefault()?.Code ?? null,
                    code_system = observationResource?.Code?.Coding.FirstOrDefault()?.System ?? null,
                    code_display = observationResource?.Code?.Coding.FirstOrDefault()?.Display ?? null
                },
                component = new Obser_Code_Value[observationResource.Component.Count],
            };

            if (observationResource.Effective != null)
            {
                if (observationResource.Effective.TypeName != "Period")
                    Ob_Detail.effectiveDateTime = DateTime.Parse(observationResource.Effective.ToString());
                else
                    Ob_Detail.effectiveDateTime = DateTime.Parse(((Period)observationResource.Effective).Start);
            }

            if (observationResource.Component != null && observationResource.Component.Count > 0)
            {
                Ob_Detail.component = new Obser_Code_Value[observationResource.Component.Count];

                for (var i = 0; i < observationResource.Component.Count; i++)
                {
                    Ob_Detail.component[i] = new Obser_Code_Value();

                    Ob_Detail.component[i].code_system = observationResource.Component[i]?.Code?.Coding.FirstOrDefault()?.System ?? null;
                    Ob_Detail.component[i].code_display = observationResource.Component[i]?.Code?.Coding.FirstOrDefault()?.Display ?? null;
                    Ob_Detail.component[i].code_code = observationResource.Component[i]?.Code?.Coding.FirstOrDefault()?.Code ?? null;

                    if (observationResource?.Component?[i]?.Value != null)
                    {
                        if (observationResource.Component[i].Value is Quantity quantity)
                        {
                            Ob_Detail.component[i].ob_value = quantity.Value;
                            Ob_Detail.component[i].unit = quantity.Unit;
                        }
                        else if (observationResource.Component[i].Value is CodeableConcept code)
                        {
                            Ob_Detail.component[i].value_code = code.Coding?.FirstOrDefault()?.Code;
                            Ob_Detail.component[i].value_system = code.Coding?.FirstOrDefault()?.System;
                            Ob_Detail.component[i].value_display = code.Coding?.FirstOrDefault()?.Display;
                            Ob_Detail.component[i].value_text = code?.Text ?? null;
                        }
                        else if (observationResource.Component[i].Value is FhirDateTime time)
                        {
                            Ob_Detail.component[i].ob_value = time.Value ?? null;
                        }
                        else
                        {
                            Ob_Detail.component[i].ob_value = observationResource?.Component[i]?.Value?.ToString() ?? null;
                        }
                        // 增加其他數據類型的處理方式
                        // else if (observationResource.Component[i].Value is 其他數據類型)
                        // {
                        //     // 設定相應的值
                        // }
                    }
                }
            }

            resourceDetail.Observations.Add(Ob_Detail);
            return resourceDetail;
        }

        private ResourceDetail MapEncounterResource(Encounter encounterResource)
        {
            DateTime? date = null;

            if (encounterResource.Period != null)
            {
                date = DateTime.Parse(encounterResource?.Period?.Start);
            }
            var resourceDetail = new ResourceDetail
            {
                en_identifier = new List<Obser_Code_Value>(),
                en_status = encounterResource?.Status != null ? (Encounter_Status)encounterResource?.Status : Encounter_Status.unknown,
                en_class = new Obser_Code_Value
                {
                    code_code = encounterResource?.Class?.Code ?? null,
                    code_system = encounterResource?.Class?.System ?? null,
                    code_display = encounterResource?.Class?.Display ?? null
                },
                en_participant = new Encounter_Participant_Code
                {
                    en_reference = encounterResource?.Participant.FirstOrDefault()?.Individual?.Reference ?? null,
                    en_display = encounterResource?.Participant.FirstOrDefault()?.Individual?.Display ?? null
                },
                serviceProvider = encounterResource?.ServiceProvider?.Reference,
                location = encounterResource?.Location.FirstOrDefault()?.Location?.Reference
            };

            if (encounterResource?.Period != null)
            {
                if (encounterResource.Period.TypeName != "Period")
                    resourceDetail.en_period = DateTime.Parse(encounterResource.Period.ToString());
                else
                    resourceDetail.en_period = DateTime.Parse(((Period)encounterResource.Period).Start);
            }

            if (encounterResource?.Identifier.Count != 0)
            {
                foreach (var iden in encounterResource.Identifier)
                {
                    var en_Iden = new Obser_Code_Value
                    {
                        code_system = iden?.System ?? null,
                        code_display = iden?.Value ?? null,
                        code_text = iden.Type?.Text ?? null
                    };

                    resourceDetail.en_identifier.Add(en_Iden);
                }
            }

            return resourceDetail;
        }

        private ResourceDetail MapConditionResource(Condition conditionResource)
        {
            var resourceDetail = new ResourceDetail
            {
                Conditions = new List<ConditionEMS>(),
            };
            var conditiondetail = new ConditionEMS
            {
                con_verificationStatus = new Obser_Code_Value
                {
                    code_code = conditionResource?.VerificationStatus?.Coding.FirstOrDefault()?.Code,
                    code_system = conditionResource?.VerificationStatus?.Coding.FirstOrDefault()?.System,
                    code_display = conditionResource?.VerificationStatus?.Coding.FirstOrDefault()?.Display,
                },
                con_code = new Obser_Code_Value()
                {
                    code_code = conditionResource?.Code?.Coding.FirstOrDefault()?.Code ?? conditionResource?.Code?.Coding.FirstOrDefault()?.Code ?? null,
                    code_system = conditionResource?.Code?.Coding.FirstOrDefault()?.Code ?? conditionResource?.Code?.Coding.FirstOrDefault()?.System ?? null,
                    code_display = conditionResource?.Code?.Coding.FirstOrDefault()?.Code ?? conditionResource?.Code?.Coding.FirstOrDefault()?.Display ?? null,
                    code_text = conditionResource?.Code?.Text ?? null
                },
                subject = conditionResource?.Subject?.Reference
            };

            resourceDetail.Conditions.Add(conditiondetail);

            return resourceDetail;
        }

        private ResourceDetail MapRiskAssessmentResource(RiskAssessment riskAssessmentResource)
        {
            DateTime? date = null;
            if (riskAssessmentResource.Occurrence != null && riskAssessmentResource.Occurrence.GetType() == typeof(FhirDateTime))
            {
                date = DateTime.Parse(riskAssessmentResource?.Occurrence?.ToString());
            }


            var resourceDetail = new ResourceDetail
            {
                RiskAssessments = new List<RiskAssessmentEMS>()
            };
            var RiskAssessmentDetail = new RiskAssessmentEMS
            {
                risk_status = riskAssessmentResource?.Status != null ? (Obser_Status)riskAssessmentResource.Status : Obser_Status.未知,
                risk_code = new Obser_Code_Value()
                {
                    code_code = riskAssessmentResource?.Code?.Coding.FirstOrDefault()?.Code ?? null,
                    code_system = riskAssessmentResource?.Code?.Coding.FirstOrDefault()?.System ?? null,
                    code_display = riskAssessmentResource?.Code?.Coding.FirstOrDefault()?.Display ?? null,
                    code_text = riskAssessmentResource?.Code?.Text ?? null
                },
                risk_prediction = riskAssessmentResource?.Prediction.FirstOrDefault()?.Outcome?.Text ?? null,
                risk_encounter = riskAssessmentResource?.Encounter?.Reference,
                risk_subject = riskAssessmentResource?.Subject?.Reference
            };
            if (riskAssessmentResource?.Occurrence != null)
            {
                RiskAssessmentDetail.occurrenceDateTime = DateTime.Parse(riskAssessmentResource?.Occurrence?.ToString());
            }

            resourceDetail.RiskAssessments.Add(RiskAssessmentDetail);

            return resourceDetail;
        }

        private ResourceDetail MapPractitionerResource(Practitioner practitionerResource)
        {
            var resourceDetail = new ResourceDetail
            {
                pra_name = practitionerResource?.Name.FirstOrDefault()?.Text?.ToString() ?? null,
                pra_familyname = practitionerResource?.Name.FirstOrDefault()?.Family?.ToString() ?? null
            };

            return resourceDetail;
        }

        private ResourceDetail MapAllergyIntoleranceResource(AllergyIntolerance allergyIntoleranceResource)
        {
            var resourceDetail = new ResourceDetail
            {
                allergy_status = new Obser_Code_Value()
                {
                    code_code = allergyIntoleranceResource?.VerificationStatus?.Coding.FirstOrDefault()?.Code?.ToString() ?? null,
                    code_system = allergyIntoleranceResource?.VerificationStatus?.Coding.FirstOrDefault()?.System?.ToString() ?? null,
                    code_display = allergyIntoleranceResource?.VerificationStatus?.Coding.FirstOrDefault()?.Display?.ToString() ?? null,
                    code_text = allergyIntoleranceResource?.VerificationStatus?.Text?.ToString() ?? null,
                },
                allergy_patient = allergyIntoleranceResource?.Patient?.Reference?.ToString() ?? null,
                allergy_code = new Obser_Code_Value()
                {
                    code_system = allergyIntoleranceResource?.Code?.Coding.FirstOrDefault()?.System?.ToString() ?? null,
                    code_code = allergyIntoleranceResource?.Code?.Coding.FirstOrDefault()?.Code?.ToString() ?? null,
                    code_display = allergyIntoleranceResource?.Code?.Coding.FirstOrDefault()?.Display?.ToString() ?? null,
                },
                allergy_category = allergyIntoleranceResource?.Category?.FirstOrDefault() != null ? (Allergy_Category)allergyIntoleranceResource.Category.FirstOrDefault() : Allergy_Category.food
        };

            return resourceDetail;
        }

        private ResourceDetail MapOrganizationResource(Organization organizationResource)
        {
            var resourceDetail = new ResourceDetail
            {
                organ_identifier = new Obser_Code_Value
                {
                    code_code = organizationResource?.Identifier.FirstOrDefault()?.Type?.Coding.FirstOrDefault()?.Code?.ToString() ?? null,
                    code_system = organizationResource?.Identifier.FirstOrDefault()?.Type?.Coding.FirstOrDefault()?.System?.ToString() ?? null,
                    code_text = organizationResource?.Identifier.FirstOrDefault()?.Type?.Text?.ToString() ?? null,
                    ob_value = organizationResource?.Identifier.FirstOrDefault()?.Value?.ToString() ?? null
                },
                organ_active = organizationResource?.Active,
                organ_name = organizationResource?.Name ?? null
            };

            return resourceDetail;
        }

        private ResourceDetail MapCompositionResource(Composition compositionResource)
        {
            DateTime? time = null;

            //if (compositionResource?.Date != null)
            //{
            //    time = DateTime.Parse(compositionResource?.Date);
            //}

            var resourceDetail = new ResourceDetail
            {
                com_title = compositionResource.Title,
                com_status = (CompositionStatus)compositionResource?.Status,
                com_type = new Obser_Code_Value
                {
                    code_system = compositionResource?.Type?.Coding.FirstOrDefault()?.System ?? null,
                    code_code = compositionResource?.Type?.Coding.FirstOrDefault()?.Code ?? null,
                    code_display = compositionResource?.Type?.Coding.FirstOrDefault()?.Display ?? null,
                },
                com_encounter = compositionResource?.Encounter?.Reference,
                com_subject = compositionResource?.Subject?.Reference,
                com_author = compositionResource?.Author.FirstOrDefault()?.Reference,
                //com_date = DateTime.Parse(compositionResource?.Date?.ToString()),
                section = new List<SectionViewModle>()
            };

            if (compositionResource?.Section?.Count != 0)
            {
                foreach (var val in compositionResource?.Section)
                {
                    //第一層(Section)
                    var sectionval = new SectionViewModle
                    {
                        title = val?.Title ?? null,
                        section_code = new Obser_Code_Value
                        {
                            code_code = val?.Code?.Coding.FirstOrDefault()?.Code ?? null,
                            code_system = val?.Code?.Coding.FirstOrDefault()?.System ?? null,
                            code_display = val?.Code?.Coding.FirstOrDefault()?.Display ?? null
                        },
                        section = new List<SectionViewModle>(),
                        entry = new List<String>()
                    };
                    //第一層(Entry)
                    if (val.Entry.Count != 0)
                    {
                        foreach (var entryval in val.Entry)
                        {
                            var enval = entryval?.Reference;
                            sectionval.entry.Add(enval);
                        }
                    }

                    //第二層Section
                    if (val.Section.Count != 0)
                    {
                        foreach (var twosection in val.Section)
                        {
                            var twosectionval = new SectionViewModle
                            {
                                title = twosection?.Title ?? null,
                                section_code = new Obser_Code_Value
                                {
                                    code_code = twosection?.Code?.Coding.FirstOrDefault()?.Code ?? null,
                                    code_system = twosection?.Code?.Coding.FirstOrDefault()?.System ?? null,
                                    code_display = twosection?.Code?.Coding.FirstOrDefault()?.Display ?? null
                                },
                                entry = new List<string>()
                            };
                            if (twosection.Entry.Count != 0)
                            {
                                foreach (var twoentry in twosection.Entry)
                                {
                                    var twoentryval = twoentry?.Reference;
                                    twosectionval.entry.Add(twoentryval);
                                }
                            }

                            sectionval.section.Add(twosectionval);
                        }
                    }

                    resourceDetail.section.Add(sectionval);
                }
            }

            return resourceDetail;
        }
    }

    public class EntryViewModle
    {
        public string fullurl { get; set; }

        public EntryResourceModel Resource { get; set; }
    }

    public class EntryResourceModel
    {
        public string resourceType { get; set; }

        public string id { get; set; }

        public ResourceDetail resourcedata { get; set; }//EMS

        public EEC_ICResourcrDetail eec_icresourcedata { get; set; }//EEC_檢驗檢查


    }

    //EMS
    public class ResourceDetail
    {
        #region  //PatientDetail
        public string meta { get; set; }

        [Display(Name = "Identifier Code")]
        public string identifier_code { get; set; }

        //以後要可以多個
        [Display(Name = "連絡電話")]
        public string telecom { get; set; }

        [Display(Name = "聯絡地址")]
        public string address { get; set; }
        public string city { get; set; }
        public string town { get; set; }
        public string zipcode { get; set; }

        [Display(Name = "緊急聯絡人名字")]
        public string contact_name { get; set; }
        [Display(Name = "緊急聯絡人姓氏")]
        public string contact_familyname { get; set; }

        [Display(Name = "關係")]
        public string contact_relationship { get; set; }

        [Display(Name = "聯絡人聯絡電話")]
        public string contact_telecom { get; set; }

        [EmailAddress]
        [Display(Name = "電子信箱")]
        public string email { get; set; }

        [Display(Name = "組織")]
        public string managingOrganization { get; set; }

        public NameUse nameuse { get; set; }

        //EMS

        [Required]
        [Display(Name = "全名")]
        public string name { get; set; }

        [Display(Name = "姓氏")]
        public string familyname { get; set; }

        [Display(Name = "名字")]
        public string givename { get; set; }

        [Required]
        [Display(Name = "生日")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public string birthDate { get; set; }

        [Required]
        [Display(Name = "性別")]
        public Gender gender { get; set; }

        [Display(Name = "是否使用")]
        public string active { get; set; } = "True";

        [Required]
        [Display(Name = "身分證/病歷號")]
        //[RegularExpression(@"^[A-Z]{1}[A-Da-d1289]{1}[0-9]{8}$", ErrorMessage = "身分證字號錯誤")]
        public string identifier { get; set; }
        #endregion

        public List<ObservationEMS> Observations { get; set; }


        #region //EncounterDetsil
        [Display(Name = "識別碼")]
        public List<Obser_Code_Value> en_identifier { get; set; }

        [Display(Name = "狀態")]
        public Encounter_Status en_status { get; set; }

        [Display(Name = "分類")]
        public Obser_Code_Value en_class { get; set; }

        [Display(Name = "參與人員")]
        public Encounter_Participant_Code en_participant { get; set; }

        [Display(Name = "派遣日期")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime en_period { get; set; } 

        [Display(Name = "出勤單位")]
        public string serviceProvider { get; set; }

        [Display(Name = "發生地點")]
        public string location { get; set; }
        #endregion

        #region //ConditionDetail
        public List<ConditionEMS> Conditions { get; set; }

        #endregion

        #region //PractitionerDetail
        [Display(Name = "救護人員")]
        public string pra_name { get; set; }

        [Display(Name = "識別碼")]
        public Obser_Code_Value pra_identifier { get; set; }

        [Display(Name = "姓氏")]
        public string pra_familyname { get; set; }
        #endregion

        #region //RiskAssessmentDetail
        public List<RiskAssessmentEMS> RiskAssessments { get; set; }
        #endregion

        #region //AllergyIntolerance
        [Display(Name = "狀態")]
        public Obser_Code_Value allergy_status { get; set; }

        [Display(Name = "過敏史患者")]
        public string allergy_patient { get; set; }

        [Display(Name = "過敏項目")]
        public Allergy_Category allergy_category { get; set; }

        [Display(Name = "代碼")]
        public Obser_Code_Value allergy_code { get; set; }
        #endregion

        #region //Organization
        [Display(Name = "識別碼")]
        public Obser_Code_Value organ_identifier { get; set; }

        [Display(Name = "使用與否")]
        public Boolean? organ_active { get; set; }

        [Display(Name = "組織名")]
        public string organ_name { get; set; }
        #endregion

        #region //Composition

        [Display(Name = "狀態")]
        public CompositionStatus com_status { get; set; }

        [Display(Name = "類型")]
        public Obser_Code_Value com_type { get; set; }

        [Display(Name = "主題")]
        public string com_title { get; set; }


        [Display(Name = "編輯時間")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime com_date { get; set; }


        [Display(Name = "subject")]
        public string com_subject { get; set; }


        [Display(Name = "encounter")]
        public string com_encounter { get; set; }


        [Display(Name = "救護人員")]
        public string com_author { get; set; }

        [Display(Name = "section")]
        public List<SectionViewModle> section { get; set; }
        #endregion
    }

    public class ObservationEMS
    {
        #region //ObservationDetail
        //[Required]
        [Display(Name = "狀態")]
        public Obser_Status ob_status { get; set; }

        //Observation-生命跡象
        //[Required]
        [Display(Name = "檢測時間")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime effectiveDateTime { get; set; } = DateTime.Parse("2023-08-21T09:00");
        //= DateTime.Parse("2023-08-21T09:00");

        //[Required]
        [Display(Name = "受試者")]
        public string ob_subject { get; set; }

        [Display(Name = "encounter")]
        public string encounter { get; set; }

        [Display(Name = "代碼")]
        public Obser_Code_Value ob_code { get; set; }//code
        public Obser_Code_Value Code_value { get; set; }//value

        public Obser_Code_Value[] component { get; set; }
        #endregion
    }

    public class RiskAssessmentEMS
    {
        [Display(Name = "狀態")]
        public Obser_Status risk_status { get; set; }

        [Display(Name = "代碼")]
        public Obser_Code_Value risk_code { get; set; }

        [Display(Name = "encounter")]
        public string risk_encounter { get; set; }

        [Display(Name = "評估時間")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime occurrenceDateTime { get; set; } = DateTime.Parse("2023-08-21T09:00");

        [Display(Name = "傷病患連結")]
        public string risk_subject { get; set; }

        [Display(Name = "風險等級")]
        public string risk_prediction { get; set; }
    }

    public class ConditionEMS
    {
        [Display(Name = "狀態")]
        public Obser_Code_Value con_verificationStatus { get; set; }

        public Obser_Code_Value con_code { get; set; }

        [Display(Name = "subject")]
        public string subject { get; set; }
    }


    //創建EMS_Observation_composition需要的必要值
    public class ObservationEntry_value
    {
        public string code { get; set; }

        public string display { get; set; }
        public string chinese { get; set; }

        public List<ObservationEntry_value> observationEntry_Values()
        {
            var obser_entry_lists = new List<ObservationEntry_value> {
                new ObservationEntry_value{ code="",display="",chinese="受理時間"},
                new ObservationEntry_value{ code="410429000",display="isCardiacArrest",chinese="心肺功能"},
                new ObservationEntry_value{ code="11454-6",display="avpu",chinese="意識狀態"},
                new ObservationEntry_value{ code="9279-1",display="respiratory",chinese=" 呼吸"},
                new ObservationEntry_value{ code="8867-4",display="heartRate",chinese="脈搏"},
                new ObservationEntry_value{ code="8310-5",display="bodyTemperature",chinese="體溫"},
                new ObservationEntry_value{ code="2339-0",display="glucose",chinese="血糖"},
                new ObservationEntry_value{ code="2708-6",display="spo2",chinese="血氧"},
                new ObservationEntry_value{ code="8480-6",display="Systolic",chinese="收縮壓"},
                new ObservationEntry_value{ code="8462-4",display="Diastolic",chinese="舒張壓"},
                new ObservationEntry_value{ code="9267-6",display="eyeOpening",chinese="睜眼反應"},
                new ObservationEntry_value{ code="9270-0",display="verbal",chinese="言語反應"},
                new ObservationEntry_value{ code="9268-4",display="motor",chinese="動作反應"},
            };
            return obser_entry_lists;
        }

    }


    public class SectionViewModle
    {
        [Display(Name = "代碼")]
        public Obser_Code_Value section_code { get; set; }

        [Display(Name = "主題")]
        public string title { get; set; }

        [Display(Name = "連結")]
        public List<string> entry { get; set; }

        public List<SectionViewModle> section { get; set; }
    }


}