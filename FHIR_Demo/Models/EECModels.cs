using FHIR_Demo.Models;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using static Hl7.Fhir.Model.Observation;

namespace FHIR_Demo.Models
{
    public class EECModels
    {
        public string Id { get; set; }
        public string Meta { get; set; }

        [Display(Name = "Identifier")]
        //[RegularExpression(@"^[A-Z]{1}[A-Da-d1289]{1}[0-9]{8}$", ErrorMessage = "身分證字號錯誤")]
        public Obser_Code_Value identifier { get; set; }

        [Display(Name = "種類")]
        [Required]
        public BundleType type { get; set; }

        [Display(Name = "創建時間")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss.fff}", ApplyFormatInEditMode = true)]
        public DateTime timestamp { get; set; } 

        public List<EntryViewModle> entry { get; set; }

        public EECModels EEC_ICBundleViewModelMapping(Bundle bundle)
        {
            this.Id = bundle.Id;
            this.identifier = new Obser_Code_Value();
            if (bundle.Identifier != null)
            {
                this.identifier.code_code = bundle.Identifier?.Value;
                this.identifier.code_system = bundle.Identifier?.System;
                this.identifier.code_display = bundle.Identifier?.Period?.Start;
            }
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
                            entryViewModel.Resource.eec_icresourcedata = MapPatientResource(entry.Resource as Patient);
                            break;
                        case "Observation":
                            entryViewModel.Resource.eec_icresourcedata = MapObservationResource(entry.Resource as Observation);
                            break;
                        case "Encounter":
                            entryViewModel.Resource.eec_icresourcedata = MapEncounterResource(entry.Resource as Encounter);
                            break;
                        case "Practitioner":
                            entryViewModel.Resource.eec_icresourcedata = MapPractitionerResource(entry.Resource as Practitioner);
                            break;
                        case "Organization":
                            entryViewModel.Resource.eec_icresourcedata = MapOrganizationResource(entry.Resource as Organization);
                            break;
                        case "Specimen":
                            entryViewModel.Resource.eec_icresourcedata = MapSpecimenResource(entry.Resource as Specimen);
                            break;
                        case "Composition":
                            entryViewModel.Resource.eec_icresourcedata = MapCompositionResource(entry.Resource as Composition);
                            break;
                        case "Location":
                            entryViewModel.Resource.eec_icresourcedata = MapLocationResource(entry.Resource as Location);
                            break;
                        case "Procedure":
                            entryViewModel.Resource.eec_icresourcedata = MapProcedureResource(entry.Resource as Procedure);
                            break;
                        case "Condition":
                            entryViewModel.Resource.eec_icresourcedata = MapConditionResource(entry.Resource as Condition);
                            break;
                        case "Media":
                            entryViewModel.Resource.eec_icresourcedata = MapMediaResource(entry.Resource as Media);
                            break;
                        case "ImagingStudy":
                            entryViewModel.Resource.eec_icresourcedata = MapImagingStudyResource(entry.Resource as ImagingStudy);
                            break;
                        case "AllergyIntolerance":
                            entryViewModel.Resource.eec_icresourcedata = MapAllergyIntoleranceResource(entry.Resource as AllergyIntolerance);
                            break;
                        case "ClinicalImpression":
                            entryViewModel.Resource.eec_icresourcedata = MapClinicalImpressionResource(entry.Resource as ClinicalImpression);
                            break;
                        case "Medication":
                            entryViewModel.Resource.eec_icresourcedata = MapMedicationResource(entry.Resource as Medication);
                            break;
                        case "MedicationRequest":
                            entryViewModel.Resource.eec_icresourcedata = MapMedicationRequestResource(entry.Resource as MedicationRequest);
                            break;

                    }
                    // 將 entryViewModel 加入到 EEC_ICModle 的 entry 列表中
                    this.entry.Add(entryViewModel);
                }
            }
            return this;
        }

        //建立Patient需要的ResourceDetail項目
        private EEC_ICResourcrDetail MapPatientResource(Patient patientResource)
        {
            DateTime? date = null;
            if (patientResource.BirthDate != null)
            {
                date = DateTime.Parse(patientResource?.BirthDate);
            }
            var EEC_ICResourcrDetail = new EEC_ICResourcrDetail
            {
                name = patientResource?.Name.FirstOrDefault()?.Text?.ToString(),
                familyname = patientResource?.Name.FirstOrDefault()?.Family,
                givename = patientResource?.Name.FirstOrDefault()?.Given.FirstOrDefault(),
                birthDate = patientResource?.BirthDate,
                gender = patientResource.Gender != null ? (Gender)patientResource.Gender : Gender.不知道,
                identifier = patientResource?.Identifier.FirstOrDefault()?.Value,
                pat_id = patientResource?.Id,
            };

            return EEC_ICResourcrDetail;
        }

        private EEC_ICResourcrDetail MapMediaResource(Media mediaResource)
        {
            var EEC_ICResourcrDetail = new EEC_ICResourcrDetail
            {
                EEC_DMSMedia = new List<MediaEEC>()
            };
            var MediaDetail = new MediaEEC
            {
                media_id = mediaResource?.Id,
                med_iden = new Obser_Code_Value(),
                med_Status = mediaResource?.Status != null ? (EventStatus)mediaResource?.Status : EventStatus.Unknown,
                med_subject = mediaResource?.Subject.ToString(),
                med_contentType = mediaResource?.Content?.ContentType?.ToString(),
                med_title = mediaResource?.Content?.Title?.ToString(),
                med_modality = new Obser_Code_Value(),
                med_type = new Obser_Code_Value(),
            };

            if (mediaResource.Identifier != null)
            {
                var med_iden = new Obser_Code_Value
                {
                    code_code = mediaResource?.Identifier?.FirstOrDefault()?.Value ?? null,
                    code_system = mediaResource?.Identifier?.FirstOrDefault()?.System ?? null
                };
            }

            if (mediaResource.Modality != null)
            {
                var med_mod = new Obser_Code_Value
                {
                    code_code = mediaResource?.Modality?.Coding?.FirstOrDefault()?.Code ?? null,
                    code_system = mediaResource?.Modality?.Coding?.FirstOrDefault()?.System ?? null,
                    code_display = mediaResource?.Modality?.Coding?.FirstOrDefault()?.Display ?? null,
                    code_text = mediaResource?.Modality?.Text ?? null,
                };
            }

            if (mediaResource.Type != null)
            {
                var med_ty = new Obser_Code_Value
                {
                    code_code = mediaResource?.Type?.Coding?.FirstOrDefault()?.Code ?? null,
                    code_system = mediaResource?.Type?.Coding?.FirstOrDefault()?.System ?? null,
                    code_display = mediaResource?.Type?.Coding?.FirstOrDefault()?.Display ?? null,
                    code_text = mediaResource?.Type?.Text ?? null,
                };
            }

            EEC_ICResourcrDetail.EEC_DMSMedia.Add(MediaDetail);
            return EEC_ICResourcrDetail;
        }

        private EEC_ICResourcrDetail MapImagingStudyResource(ImagingStudy ImagingStudyResource)
        {

            var EEC_ICResourcrDetail = new EEC_ICResourcrDetail
            {
                ima_id = ImagingStudyResource?.Id,
                ima_iden = new Obser_Code_Value()
                {
                    code_code = ImagingStudyResource?.Identifier?.FirstOrDefault()?.Value ?? null,
                    code_system = ImagingStudyResource?.Identifier?.FirstOrDefault()?.System ?? null
                },
                ima_status = ImagingStudyResource?.Status != null ? (ImagingStudy.ImagingStudyStatus)ImagingStudyResource.Status : ImagingStudy.ImagingStudyStatus.Unknown,
                ima_reffer = ImagingStudyResource?.Referrer?.Reference.ToString(),
                ima_reason = new Obser_Code_Value()
                {
                    code_code = ImagingStudyResource?.ReasonCode?.FirstOrDefault()?.Coding?.FirstOrDefault()?.Code ?? null,
                    code_display = ImagingStudyResource?.ReasonCode?.FirstOrDefault()?.Coding?.FirstOrDefault()?.Display ?? null,
                    code_system = ImagingStudyResource?.ReasonCode?.FirstOrDefault()?.Coding?.FirstOrDefault()?.System ?? null,
                    code_text = ImagingStudyResource?.ReasonCode?.FirstOrDefault()?.Text ?? null,
                },
                ima_modality = new Obser_Code_Value()
                {
                    code_code = ImagingStudyResource?.Series?.FirstOrDefault()?.Modality?.Code ?? null,
                    code_system = ImagingStudyResource?.Series?.FirstOrDefault()?.Modality?.System ?? null,
                    code_display = ImagingStudyResource?.Series?.FirstOrDefault()?.Modality?.Display ?? null,
                },
                ima_bodysite = new Obser_Code_Value()
                {
                    code_code = ImagingStudyResource?.Series?.FirstOrDefault()?.BodySite?.Code ?? null,
                    code_system = ImagingStudyResource?.Series?.FirstOrDefault()?.BodySite?.System ?? null,
                    code_display = ImagingStudyResource?.Series?.FirstOrDefault()?.BodySite?.Display ?? null,
                },
                ima_subject = ImagingStudyResource?.Subject?.Reference.ToString(),
                img_stared = DateTime.Parse(ImagingStudyResource?.Started?.ToString() ?? null),

            };
            return EEC_ICResourcrDetail;
        }

        private EEC_ICResourcrDetail MapObservationResource(Observation observationResource)
        {
            Obser_Code_Value codevalue = new Obser_Code_Value();

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

            var EEC_ICResourcrDetail = new EEC_ICResourcrDetail
            {
                obs_id = observationResource?.Id,
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
                    code_display = observationResource?.Code?.Coding.FirstOrDefault()?.Display ?? null,
                    code_text = observationResource?.Code?.Text ?? null,
                    code_system = observationResource?.Code?.Coding.FirstOrDefault()?.System ?? null,
                },
                interpretation = new Obser_Code_Value
                {
                    code_system = observationResource?.Interpretation.FirstOrDefault()?.Coding.FirstOrDefault()?.Code ?? null,
                    code_code = observationResource?.Interpretation.FirstOrDefault()?.Coding.FirstOrDefault()?.System ?? null,
                    code_display = observationResource?.Interpretation.FirstOrDefault()?.Coding.FirstOrDefault()?.Display ?? null,
                    code_text = observationResource?.Interpretation.FirstOrDefault()?.Text ?? null,
                },
                note = observationResource?.Note.FirstOrDefault()?.Text.ToString(),

                ob_bodycode = new Obser_Code_Value
                {
                    code_system = observationResource?.BodySite?.Coding.FirstOrDefault()?.System ?? null,
                    code_code = observationResource?.BodySite?.Coding.FirstOrDefault()?.Code ?? null,
                    code_display = observationResource?.BodySite?.Coding.FirstOrDefault()?.Display ?? null,
                    code_text = observationResource?.BodySite?.Text ?? null,
                },
                component = new ComponentComponent[observationResource.Component.Count],
            };

            //effectiontime
            if (observationResource.Effective != null)
            {
                if (observationResource.Effective.TypeName != "Period")
                    EEC_ICResourcrDetail.effectivePeriod_star = DateTime.Parse(observationResource.Effective.ToString());
                else if (observationResource.Effective is Period effectivePeriod)
                {
                    EEC_ICResourcrDetail.effectivePeriod_star = DateTime.Parse(effectivePeriod.Start?.ToString());
                    EEC_ICResourcrDetail.effectivePeriod_end = DateTime.Parse(effectivePeriod.End?.ToString());
                }
            }


            //Component
            if (observationResource.Component != null && observationResource.Component.Count > 0)
            {
                EEC_ICResourcrDetail.component = new ComponentComponent[observationResource.Component.Count];
                for (var i = 0; i < observationResource.Component.Count; i++)
                {
                    EEC_ICResourcrDetail.component[i] = new ComponentComponent();

                    if (observationResource.Component[i]?.Code?.Coding != null && observationResource.Component[i]?.Code?.Coding.Count > 0)
                    {
                        EEC_ICResourcrDetail.component[i].Code = new CodeableConcept();

                        foreach (var codingData in observationResource.Component[i].Code.Coding)
                        {
                            var coding = new Coding
                            {
                                Code = codingData.Code,
                                Display = codingData.Display,
                                System = codingData.System
                            };

                            EEC_ICResourcrDetail.component[i].Code.Coding.Add(coding);
                        }
                    }

                    if (observationResource?.Component?[i]?.Value != null)
                    {
                        if (observationResource.Component[i].Value is Quantity quantity)
                        {
                            EEC_ICResourcrDetail.component[i].Value = new Quantity
                            {
                                Value = quantity.Value,
                                Unit = quantity.Unit
                            };
                        }
                        else if (observationResource.Component[i].Value is CodeableConcept code)
                        {
                            EEC_ICResourcrDetail.component[i].Code = new CodeableConcept
                            {
                                Coding = new List<Coding>
                                {
                                    new Coding{
                                        Code = code.Coding?.FirstOrDefault()?.Code,
                                        System = code.Coding?.FirstOrDefault()?.System,
                                        Display = code.Coding?.FirstOrDefault()?.Display
                                    }
                                }
                            };
                        }
                        else if (observationResource.Component[i].Value is FhirDateTime time)
                        {
                            EEC_ICResourcrDetail.component[i].Value = new FhirDateTime
                            {
                                Value = time.Value.ToString() ?? null
                            };
                        }
                        else
                        {
                            EEC_ICResourcrDetail.component[i].Value = new FhirString
                            {
                                Value = observationResource?.Component[i]?.Value?.ToString()
                            };
                        }
                        // 增加其他數據類型的處理方式
                        // else if (observationResource.Component[i].Value is 其他數據類型)
                        // {
                        //     // 設定相應的值
                        // }
                    }

                    if (observationResource?.Component?[i]?.ReferenceRange != null)
                    {
                        EEC_ICResourcrDetail.component[i].ReferenceRange = new List<ReferenceRangeComponent>
                        {
                            new ReferenceRangeComponent
                            {
                                Low = new Quantity
                                {
                                    Value = observationResource.Component[i]?.ReferenceRange?.FirstOrDefault()?.Low?.Value,
                                    Unit = observationResource.Component[i]?.ReferenceRange?.FirstOrDefault()?.Low?.Unit,
                                },
                                High = new Quantity
                                {
                                    Value = observationResource.Component[i]?.ReferenceRange?.FirstOrDefault()?.High?.Value,
                                    Unit = observationResource.Component[i]?.ReferenceRange?.FirstOrDefault()?.High?.Unit,
                                }
                            }
                        };
                    }
                }
            }

            return EEC_ICResourcrDetail;
        }

        private EEC_ICResourcrDetail MapEncounterResource(Encounter encounterResource)
        {
            DateTime? date = null;

            if (encounterResource.Period != null)
            {
                date = DateTime.Parse(encounterResource?.Period?.Start);
            }
            var EEC_ICResourcrDetail = new EEC_ICResourcrDetail
            {
                enc_id = encounterResource?.Id,
                en_status = encounterResource?.Status != null ? (Encounter_Status)encounterResource?.Status : Encounter_Status.unknown,
                en_class = new Obser_Code_Value
                {
                    code_code = encounterResource?.Class?.Code ?? null,
                    code_system = encounterResource?.Class?.System ?? null,
                    code_display = encounterResource?.Class?.Display ?? null
                },
                //period = DateTime.Parse(encounterResource?.Period?.Start?.ToString()),
                en_servicetype = new Obser_Code_Value
                {
                    code_code = encounterResource?.ServiceType?.Coding.FirstOrDefault()?.Code,
                    code_display = encounterResource?.ServiceType?.Coding.FirstOrDefault()?.Display,
                    code_system = encounterResource?.ServiceType?.Coding.FirstOrDefault()?.System,
                    code_text = encounterResource?.ServiceType?.Text
                },
                en_identifier = new List<Obser_Code_Value>(),
            };

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

                    EEC_ICResourcrDetail.en_identifier.Add(en_Iden);
                }
            }

            if (encounterResource?.Period != null)
            {
                if (encounterResource.Period.TypeName != "Period")
                    EEC_ICResourcrDetail.period = DateTime.Parse(encounterResource.Period.ToString());
                else
                    EEC_ICResourcrDetail.period = DateTime.Parse(((Period)encounterResource.Period).Start);
            }

            return EEC_ICResourcrDetail;
        }

        private EEC_ICResourcrDetail MapSpecimenResource(Specimen specimenResource)
        {
            var EEC_ICResourcrDetail = new EEC_ICResourcrDetail
            {
                spe_code = new Obser_Code_Value()
                {
                    code_code = specimenResource?.Collection?.BodySite?.Coding.FirstOrDefault()?.Code ?? null,
                    code_system = specimenResource?.Collection?.BodySite?.Coding.FirstOrDefault()?.System ?? null,
                    code_display = specimenResource?.Collection?.BodySite?.Coding.FirstOrDefault()?.Display ?? null,
                    code_text = specimenResource?.Collection?.BodySite?.Text ?? null
                },
                spe_subject = specimenResource?.Subject?.Reference.ToString(),
                spe_id = specimenResource?.Id,
            };
            return EEC_ICResourcrDetail;
        }

        private EEC_ICResourcrDetail MapAllergyIntoleranceResource(AllergyIntolerance allergyIntoleranceResource)
        {
            var EEC_ICResourcrDetail = new EEC_ICResourcrDetail
            {
                allergy_id = allergyIntoleranceResource?.Id,
                allergy_iden = new Obser_Code_Value()
                {
                    code_code = allergyIntoleranceResource?.Identifier?.FirstOrDefault()?.Value ?? null,
                    code_system = allergyIntoleranceResource?.Identifier?.FirstOrDefault()?.System ?? null,
                },
                allergy_patient = allergyIntoleranceResource?.Patient?.Reference?.ToString() ?? null,
                allergy_code = new Obser_Code_Value()
                {
                    code_system = allergyIntoleranceResource?.Code?.Coding.FirstOrDefault()?.System?.ToString() ?? null,
                    code_code = allergyIntoleranceResource?.Code?.Coding.FirstOrDefault()?.Code?.ToString() ?? null,
                    code_display = allergyIntoleranceResource?.Code?.Coding.FirstOrDefault()?.Display?.ToString() ?? null,
                    code_text = allergyIntoleranceResource?.Code?.Text ?? null,
                },

            };

            return EEC_ICResourcrDetail;
        }

        private EEC_ICResourcrDetail MapLocationResource(Location locationResource)
        {
            var EEC_ICResourcrDetail = new EEC_ICResourcrDetail
            {
                loc_id = locationResource?.Id,
                loc_iden = new Obser_Code_Value()
            };

            if (locationResource.Identifier != null)
            {
                var loc_iden = new Obser_Code_Value
                {
                    code_code = locationResource?.Identifier?.FirstOrDefault()?.Value ?? null,
                    code_system = locationResource?.Identifier?.FirstOrDefault()?.System ?? null
                };
            }


            return EEC_ICResourcrDetail;
        }

        private EEC_ICResourcrDetail MapMedicationResource(Medication medicationResource)
        {

            var EEC_ICResourcrDetail = new EEC_ICResourcrDetail
            {
                med_id = medicationResource?.Id,
                med_code = new Obser_Code_Value()
                {
                    code_code = medicationResource?.Code?.Coding?.FirstOrDefault()?.Code ?? null,
                    code_display = medicationResource?.Code?.Coding?.FirstOrDefault()?.Display?.ToString() ?? null,
                    code_system = medicationResource?.Code?.Coding?.FirstOrDefault()?.System?.ToString() ?? null,
                    code_text = medicationResource?.Code?.Text ?? null,
                },
                med_form = new Obser_Code_Value()
                {
                    code_code = medicationResource?.Form?.Coding?.FirstOrDefault()?.Code ?? null,
                    code_system = medicationResource?.Form?.Coding?.FirstOrDefault()?.System ?? null,
                    code_display = medicationResource?.Form?.Coding?.FirstOrDefault()?.Display ?? null,
                    code_text = medicationResource?.Form?.Text ?? null,
                }
            };
            return EEC_ICResourcrDetail;
        }

        private EEC_ICResourcrDetail MapPractitionerResource(Practitioner practitionerResource)
        {
            var EEC_ICResourcrDetail = new EEC_ICResourcrDetail
            {
                practitioners = new List<PractitionerEEC>()
            };
            var practitioner = new PractitionerEEC
            {
                pra_name = practitionerResource?.Name.FirstOrDefault()?.Text?.ToString() ?? null,
                pra_id = practitionerResource?.Id,
            };

            EEC_ICResourcrDetail.practitioners.Add(practitioner);
            return EEC_ICResourcrDetail;
        }

        private EEC_ICResourcrDetail MapOrganizationResource(Organization organizationResource)
        {
            var EEC_ICResourcrDetail = new EEC_ICResourcrDetail
            {
                EEC_DMS_Organization = new List<OrganizationEEC_DMS>()
            };

            var OrganizationDetail = new OrganizationEEC_DMS
            {
                organ_identifier = new Obser_Code_Value
                {
                    code_code = organizationResource?.Identifier.FirstOrDefault()?.Type?.Coding.FirstOrDefault()?.Code?.ToString() ?? null,
                    code_system = organizationResource?.Identifier.FirstOrDefault()?.Type?.Coding.FirstOrDefault()?.System?.ToString() ?? null,
                    code_display = organizationResource?.Identifier.FirstOrDefault()?.Type?.Coding.FirstOrDefault()?.Display?.ToString() ?? null,
                    code_text = organizationResource?.Identifier.FirstOrDefault()?.Type?.Text?.ToString() ?? null,
                    ob_value = organizationResource?.Identifier.FirstOrDefault()?.Value?.ToString() ?? null,
                    value_system = organizationResource?.Identifier.FirstOrDefault()?.System?.ToString() ?? null
                },
                organ_name = organizationResource?.Name ?? null,
                org_id = organizationResource?.Id,
            };

            EEC_ICResourcrDetail.EEC_DMS_Organization.Add(OrganizationDetail);
            return EEC_ICResourcrDetail;
        }

        private EEC_ICResourcrDetail MapProcedureResource(Procedure procedureResource)
        {
            var EEC_ICResourcrDetail = new EEC_ICResourcrDetail
            {
                EEC_DMS_Procedure = new List<ProcedureEEC_DMS>()
            };

            var ProcedureDetail = new ProcedureEEC_DMS
            {
                pro_id = procedureResource?.Id,
                pro_idenDetail = new List<Obser_Code_Value>(),
                pro_status = procedureResource?.Status != null ? (EventStatus)procedureResource?.Status : EventStatus.Unknown,
                pro_category = new Obser_Code_Value(),
                pro_codeDetail = new Obser_Code_Value(),
                pro_performer = procedureResource?.Performer?.FirstOrDefault()?.Actor?.Reference?.ToString() ?? null,
                pro_subject = procedureResource?.Subject?.ToString() ?? null,
                
                pro_bodysite = new Obser_Code_Value(),
                pro_followUp = new List<Obser_Code_Value>(),
                pro_noteDetail = new List<Obser_Code_Value>()

            };
            //DateTime? data;
            if (procedureResource?.Performed != null)
            {
                if (procedureResource?.Performed.TypeName != "Period")
                {
                    ProcedureDetail.pro_performed = DateTime.Parse(procedureResource.Performed.ToString());
                }
                else if (procedureResource.Performed is Period performedPeriod)
                {
                    ProcedureDetail.pro_star = DateTime.Parse(performedPeriod.Start?.ToString());
                }
            }
            if (procedureResource.Identifier.Count() != 0)
            {
                foreach (var iden in procedureResource.Identifier)
                {
                    var pro_iden = new Obser_Code_Value
                    {
                        code_system = iden?.System ?? null,
                        code_display = iden?.Value ?? null
                    };

                    ProcedureDetail.pro_idenDetail.Add(pro_iden);
                }

                // EEC_ICResourcrDetail.Procedure.Add(ProcedureDetail);

            };

            if (procedureResource.Category != null)
            {
                var pro_category = new Obser_Code_Value
                {
                    code_code = procedureResource?.Category?.Coding?.FirstOrDefault()?.Code ?? null,
                    code_system = procedureResource?.Category?.Coding?.FirstOrDefault()?.System ?? null,
                    code_display = procedureResource?.Category?.Coding?.FirstOrDefault()?.Display ?? null,
                    code_text = procedureResource?.Category?.Text ?? null,
                };
            }

            if (procedureResource.Code != null)
            {
                var pro_code = new Obser_Code_Value
                {
                    code_code = procedureResource?.Code?.Coding?.FirstOrDefault()?.Code ?? null,
                    code_system = procedureResource?.Code?.Coding?.FirstOrDefault()?.System ?? null,
                    code_display = procedureResource?.Code?.Coding?.FirstOrDefault()?.Display ?? null,
                    code_text = procedureResource?.Code?.Text ?? null,
                };
            }

            if (procedureResource.BodySite.Count() != 0)
            {
                var pro_body = new Obser_Code_Value
                {
                    code_code = procedureResource?.BodySite?.FirstOrDefault().Coding?.FirstOrDefault()?.Code ?? null,
                    code_system = procedureResource?.BodySite?.FirstOrDefault().Coding?.FirstOrDefault()?.System ?? null,
                    code_display = procedureResource?.BodySite?.FirstOrDefault().Coding?.FirstOrDefault()?.Display ?? null,
                    code_text = procedureResource?.BodySite?.FirstOrDefault().Text ?? null,
                };
            }

            if (procedureResource.FollowUp != null)
            {
                foreach (var follow in procedureResource.FollowUp)
                {
                    var follower = new Obser_Code_Value
                    {
                        code_system = follow?.Coding?.FirstOrDefault()?.System ?? null,
                        code_code = follow?.Coding?.FirstOrDefault()?.Code ?? null,
                        code_display = follow?.Coding?.FirstOrDefault()?.Display ?? null,
                        code_text = follow?.Text ?? null,
                    };

                    ProcedureDetail.pro_followUp.Add(follower);
                }
            }

            if (procedureResource.Note != null)
            {
                foreach (var note in procedureResource.Note)
                {
                    var procedure_note = new Obser_Code_Value
                    {
                        markdown_text = note.Text ?? null,
                    };

                    ProcedureDetail.pro_noteDetail.Add(procedure_note);
                }

                // EEC_ICResourcrDetail.Procedure.Add(ProcedureDetail);

            };
            EEC_ICResourcrDetail.EEC_DMS_Procedure.Add(ProcedureDetail);
            return EEC_ICResourcrDetail;

        }

        private EEC_ICResourcrDetail MapConditionResource(Condition conditionResource)
        {
            var EEC_ICResourcrDetail = new EEC_ICResourcrDetail
            {
                EEC_DMSCondition = new List<ConditionEEC>(),
            };
            var conditiondetail = new ConditionEEC
            {
                con_id = conditionResource?.Id,
                con_iden = new List<Obser_Code_Value>(),

                con_verificationStatus = new Obser_Code_Value
                {
                    code_code = conditionResource?.VerificationStatus?.Coding.FirstOrDefault()?.Code,
                    code_system = conditionResource?.VerificationStatus?.Coding.FirstOrDefault()?.System,
                    code_display = conditionResource?.VerificationStatus?.Coding.FirstOrDefault()?.Display,
                },
                con_codeDetail = new Obser_Code_Value()
                {
                    code_code = conditionResource?.Code?.Coding.FirstOrDefault()?.Code ?? null,
                    code_system = conditionResource?.Code?.Coding.FirstOrDefault()?.System ?? null,
                    code_display = conditionResource?.Code?.Coding.FirstOrDefault()?.Display ?? null,
                    code_text = conditionResource?.Code?.Text ?? null
                },
                con_subject = conditionResource?.Subject?.Reference,
                con_category = new Obser_Code_Value()
                {
                    code_code = conditionResource?.Category?.FirstOrDefault()?.Coding.FirstOrDefault()?.Code ?? null,
                    code_system = conditionResource?.Category?.FirstOrDefault()?.Coding.FirstOrDefault()?.System ?? null,
                    code_display = conditionResource?.Category?.FirstOrDefault()?.Coding.FirstOrDefault()?.Display ?? null,
                    code_text = conditionResource?.Category?.FirstOrDefault()?.Text ?? null
                },
                con_clinicalStatus = new Obser_Code_Value()
                {
                    code_code = conditionResource?.ClinicalStatus?.Coding?.FirstOrDefault()?.Code ?? null,
                    code_system = conditionResource?.ClinicalStatus?.Coding?.FirstOrDefault()?.System ?? null,
                    code_display = conditionResource?.ClinicalStatus?.Coding?.FirstOrDefault()?.Display ?? null,
                },
                con_noteDetail = conditionResource?.Note?.FirstOrDefault()?.Text ?? null,


            };
            if (conditionResource?.Identifier.Count != 0)
            {
                foreach (var iden in conditionResource.Identifier)
                {
                    var con_Iden = new Obser_Code_Value
                    {
                        code_system = iden?.System ?? null,
                        code_display = iden?.Value ?? null,

                    };

                    EEC_ICResourcrDetail.EEC_DMSCondition.Add(conditiondetail);
                }
            };

            return EEC_ICResourcrDetail;
        }

        private EEC_ICResourcrDetail MapClinicalImpressionResource(ClinicalImpression ClinicalImpressionResource)
        {
            var EEC_ICResourcrDetail = new EEC_ICResourcrDetail
            {
                EEC_PMRClinicalImpression = new List<ClinicalImpressionEEC>()
            };

            var ClinicalImpressionDetail = new ClinicalImpressionEEC
            {
                cli_id = ClinicalImpressionResource?.Id,
                cli_iden = new Obser_Code_Value()
                {
                    code_system = ClinicalImpressionResource?.Identifier?.FirstOrDefault()?.System ?? null,
                    code_display = ClinicalImpressionResource?.Identifier?.FirstOrDefault()?.Value ?? null,
                },
                cli_code = new Obser_Code_Value()
                {
                    code_code = ClinicalImpressionResource?.Code?.Coding?.FirstOrDefault().Code ?? null,
                    code_system = ClinicalImpressionResource?.Code?.Coding?.FirstOrDefault().System ?? null,
                    code_display = ClinicalImpressionResource?.Code?.Coding?.FirstOrDefault().Display ?? null,
                    code_text = ClinicalImpressionResource?.Code?.Text ?? null,
                },
                cli_note = ClinicalImpressionResource?.Note?.FirstOrDefault()?.Text ?? null,
                cli_subject = ClinicalImpressionResource?.Subject?.Reference ?? null,
                cli_status = ClinicalImpressionResource?.Status != null ? (ClinicalImpression.ClinicalImpressionStatus)ClinicalImpressionResource.Status : ClinicalImpression.ClinicalImpressionStatus.InProgress,
            };

            EEC_ICResourcrDetail.EEC_PMRClinicalImpression.Add(ClinicalImpressionDetail);
            return EEC_ICResourcrDetail;
        }

        private EEC_ICResourcrDetail MapMedicationRequestResource(MedicationRequest medicationRequestResource)
        {

            var EEC_ICResourcrDetail = new EEC_ICResourcrDetail
            {
                medrq_id = medicationRequestResource?.Id,
                medrq_iden = new Obser_Code_Value
                {
                    code_code = medicationRequestResource?.Identifier?.FirstOrDefault()?.Value ?? null,
                    code_system = medicationRequestResource?.Identifier?.FirstOrDefault()?.System ?? null,
                },
                medrq_status = medicationRequestResource?.Status != null ? (MedicationRequest.medicationrequestStatus)medicationRequestResource?.Status : MedicationRequest.medicationrequestStatus.Unknown,
                medrq_category = new Obser_Code_Value
                {
                    code_code = medicationRequestResource?.Category?.FirstOrDefault()?.Coding?.FirstOrDefault()?.Code ?? null,
                    code_display = medicationRequestResource?.Category?.FirstOrDefault()?.Coding?.FirstOrDefault()?.Display ?? null,
                    code_system = medicationRequestResource?.Category?.FirstOrDefault()?.Coding?.FirstOrDefault()?.System ?? null,
                    code_text = medicationRequestResource?.Category?.FirstOrDefault()?.Text ?? null,
                },
                medrq_intent = medicationRequestResource?.Intent != null ? (MedicationRequest.medicationRequestIntent)medicationRequestResource?.Intent : MedicationRequest.medicationRequestIntent.Plan,
                medrq_subject = medicationRequestResource?.Subject?.Reference ?? null,
                medrq_noteDetail = new List<Obser_Code_Value>(),
                medrq_timing = new Obser_Code_Value
                {
                    code_code = medicationRequestResource?.DosageInstruction?.FirstOrDefault()?.Timing?.Code?.Coding?.FirstOrDefault()?.Code ?? null,
                    code_system = medicationRequestResource?.DosageInstruction?.FirstOrDefault()?.Timing?.Code?.Coding?.FirstOrDefault()?.System ?? null,
                    code_display = medicationRequestResource?.DosageInstruction?.FirstOrDefault()?.Timing?.Code?.Coding?.FirstOrDefault()?.Display ?? null,
                    code_text = medicationRequestResource?.DosageInstruction?.FirstOrDefault()?.Timing?.Code?.Text ?? null,
                },
                medrq_route = new Obser_Code_Value
                {
                    code_code = medicationRequestResource?.DosageInstruction?.FirstOrDefault()?.Route?.Coding?.FirstOrDefault()?.Code ?? null,
                    code_system = medicationRequestResource?.DosageInstruction?.FirstOrDefault()?.Route?.Coding?.FirstOrDefault()?.System ?? null,
                    code_display = medicationRequestResource?.DosageInstruction?.FirstOrDefault()?.Route?.Coding?.FirstOrDefault()?.Display ?? null,
                    code_text = medicationRequestResource?.DosageInstruction?.FirstOrDefault()?.Route?.Text ?? null,

                },
                /*medrq_dose = new Obser_Code_Value
                {
                    code_code = medicationRequestResource?.DosageInstruction?.FirstOrDefault()?.DoseAndRate?.FirstOrDefault()?.Dose
                },*/
                medrq_maxdose = new Obser_Code_Value
                {
                    code_code = medicationRequestResource?.DosageInstruction?.FirstOrDefault()?.MaxDosePerAdministration?.Unit ?? null,
                    value = medicationRequestResource?.DosageInstruction?.FirstOrDefault()?.MaxDosePerAdministration?.Value ?? null,
                },
                medrq_quantity = new Obser_Code_Value
                {
                    code_code = medicationRequestResource?.DispenseRequest?.Quantity?.Code ?? null,
                    code_display = medicationRequestResource?.DispenseRequest?.Quantity?.Unit ?? null,
                    code_system = medicationRequestResource?.DispenseRequest?.Quantity?.System ?? null,
                    value = medicationRequestResource?.DispenseRequest?.Quantity?.Value ?? null,

                },
                medrq_durationDetail = new Obser_Code_Value
                {
                    code_code = medicationRequestResource?.DispenseRequest?.ExpectedSupplyDuration?.Code ?? null,
                    code_display = medicationRequestResource?.DispenseRequest?.ExpectedSupplyDuration?.Unit ?? null,
                    code_system = medicationRequestResource?.DispenseRequest?.ExpectedSupplyDuration?.System ?? null,
                    value = medicationRequestResource?.DispenseRequest?.ExpectedSupplyDuration?.Value ?? null,
                }
            };
        

            if (medicationRequestResource?.Note.Count != 0)
            {
                foreach (var mednote in medicationRequestResource.Note)
                {
                    var medNote = new Obser_Code_Value
                    {
                        markdown_text = mednote?.Text ?? null

                    };

                    EEC_ICResourcrDetail.medrq_noteDetail.Add(medNote);
                }
            };
            return EEC_ICResourcrDetail;
        }

        private EEC_ICResourcrDetail MapCompositionResource(Composition compositionResource)
        {
            DateTime? time = null;

            if (compositionResource.Date != null)
            {
                time = DateTime.Parse(compositionResource?.Date);
            }

            var EEC_ICResourcrDetail = new EEC_ICResourcrDetail
            {
                com_id = compositionResource?.Id,
                com_title = compositionResource?.Title ?? null,
                com_status = (CompositionStatus)compositionResource?.Status,
                com_type = new Obser_Code_Value
                {
                    code_system = compositionResource?.Type?.Coding.FirstOrDefault()?.System ?? null,
                    code_code = compositionResource?.Type?.Coding.FirstOrDefault()?.Code ?? null,
                    code_display = compositionResource?.Type?.Coding.FirstOrDefault()?.Display ?? null,
                    code_text = compositionResource?.Type?.Text ?? null
                },
                com_encounter = compositionResource?.Encounter?.Reference ?? null,
                com_subject = compositionResource?.Subject?.Reference ?? null,
                com_author = compositionResource?.Author?.FirstOrDefault()?.Reference ?? null,
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

                    EEC_ICResourcrDetail.section.Add(sectionval);
                }
            }

            return EEC_ICResourcrDetail;
        }
    }


    public class EEC_ICResourcrDetail
    {
        #region  //Organization
        [Display(Name = "Organization Id")]
        public string org_id { get; set; }
        [Display(Name = "醫事機構代碼與名稱 Hospital Id/Name")]
        public Obser_Code_Value organ_identifier { get; set; }
        [Display(Name = "醫事機構名稱 Hospital Name")]
        public string organ_name { get; set; }
        #endregion

        #region  //Patient
        [Display(Name = "Patient Id")]
        public string pat_id { get; set; }
        [Display(Name = "姓名 Name")]
        public string name { get; set; }
        [Display(Name = "姓")]
        public string familyname { get; set; }
        [Display(Name = "名")]
        public string givename { get; set; }

        [Required]
        [Display(Name = "性別")]
        public Gender gender { get; set; }

        [Display(Name = "身分證字號 ID Number")]  //病歷號碼 Chart No.
        //[RegularExpression(@"^[A-Z]{1}[A-Da-d1289]{1}[0-9]{8}$", ErrorMessage = "身分證字號錯誤")]
        public string identifier { get; set; }

        [Display(Name = "生日")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public string birthDate { get; set; }
        #endregion

        #region //Specimen
        [Display(Name = "Specimen Id")]
        public string spe_id { get; set; }
        [Display(Name = "檢體來源 Sampling Source")]
        public Obser_Code_Value spe_code { get; set; }

        [Display(Name = "就醫病患")]
        public string spe_subject { get; set; }
        #endregion

        #region //Practitioner
        public List<PractitionerEEC> practitioners { get; set; }
        #endregion

        #region //Encounter
        [Display(Name = "Encounter Id")]
        public string enc_id { get; set; }
        [Display(Name = "識別碼")]
        public List<Obser_Code_Value> en_identifier { get; set; }

        [Display(Name = "狀態")]
        public Encounter_Status en_status { get; set; }

        [Display(Name = "分類")]
        public Obser_Code_Value en_class { get; set; }
        [Display(Name = "就醫病患")]
        public string enc_subject { get; set; }

        [Display(Name = "開立檢驗醫囑單張日期時間 Date and Time of Medical Order")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime period { get; set; }

        [Display(Name = "住院日期Date of Hospitalization")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime period_start { get; set; }


        [Display(Name = "出院日期Discharge Date")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime period_end { get; set; }

        [Display(Name = "開立檢驗醫囑單張門診 Department of Medical Order Provided")]
        public Obser_Code_Value en_servicetype { get; set; }

        [Display(Name = "開立醫師")]
        public string en_participant { get; set; } 

        [Display(Name = "就醫機構")]
        public string serviceProvider { get; set; }

        //EEC_DMS(還有period的start、end)
        [Display(Name = "physicalType")]
        public Obser_Code_Value physicalType { get; set; }
        [Display(Name = "就醫地點")]
        public string en_location { get; set; }

        #endregion

        #region //Observation
        [Display(Name = "Observation Id")]
        public string obs_id { get; set; }
        [Required]
        [Display(Name = "狀態")]
        public Obser_Status ob_status { get; set; }

        [Display(Name = "就醫病患")]
        public string ob_subject { get; set; }

        [Display(Name = "encounter 開立資料Id")]
        public string encounter { get; set; }

        [Display(Name = "採檢日期時間Sampling Date and Time")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Duration)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime effectivePeriod_star { get; set; }

        [Display(Name = "收件日期時間Delivering Date and Time")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Duration)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime effectivePeriod_end { get; set; }

        [Display(Name = "檢驗單號 Application No.")]
        public Obser_Code_Value ob_identifier { get; set; }

        [Display(Name = "健保檢驗項目 Test item Codes")]
        public Obser_Code_Value ob_code { get; set; }//code

        public Obser_Code_Value Code_value { get; set; }

        [Display(Name = "檢驗項目與報告結果 Test Results diagnostic test and/or laboratory")]
        public Obser_Code_Value interpretation { get; set; }

        [Display(Name = "備註 Remark")]
        public String note { get; set; }

        [Display(Name = "檢體類別與說明 Categories/Categories Description")]
        public Obser_Code_Value ob_bodycode { get; set; } //bodySite.code

        [Display(Name = "檢驗方法 Method")]
        public Obser_Code_Value method { get; set; }

        [Display(Name = "Specimen Id")]
        public string specimen { get; set; }

        [Display(Name = "項次Item Number")]
        public ComponentComponent[] component { get; set; }

        [Display(Name = "Value")]
        public List<string> value { get; set; }


        public ReferenceRange ob_range { get; set; }

        #endregion

        #region //Composition
        [Display(Name = "Composition Id")]
        public string com_id { get; set; }
        [Display(Name = "狀態")]
        public CompositionStatus com_status { get; set; }

        [Display(Name = "Type")]
        public Obser_Code_Value com_type { get; set; }

        [Display(Name = "就醫病患")]
        public string com_subject { get; set; }
        [Display(Name = "開立資料")]
        public string com_encounter { get; set; }
        [Display(Name = "醫事人員Id")]
        public string com_author { get; set; }

        [Display(Name = "報告日期時間 Report Date and Time")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime com_date { get; set; }

        [Display(Name = "報告文件標題")]
        public string com_title { get; set; }

        [Display(Name = "section")]
        public List<SectionViewModle> section { get; set; }

        #endregion

        #region //出院病摘新增

        #region //ImagingStudy
        public string ima_id { get; set; }
        [Display(Name = "Identifier")]
        public Obser_Code_Value ima_iden { get; set; }

        [Display(Name = "Status")]
        public ImagingStudy.ImagingStudyStatus ima_status { get; set; }

        [Display(Name = "影像檢查時間 Imaging Study Time")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime img_stared { get; set; }

        [Display(Name = "檢查醫師")]
        public string ima_reffer { get; set; }

        [Display(Name = "醫療影像檢查發生原因 Imaging Study(ReasonCode)")]
        public Obser_Code_Value ima_reason { get; set; }

        [Display(Name = "醫療影像檢查 Imaging Study")]
        public Obser_Code_Value ima_modality { get; set; }

        [Display(Name = "醫療影像檢查部位 Imaging Study(BodySite)")]
        public Obser_Code_Value ima_bodysite { get; set; }

        [Display(Name = "檢查對象")]
        public string ima_subject { get; set; }
        #endregion


        #region //Location
        public string loc_id { get; set; }
        [Display(Name = "出院床號Bed No.")]
        public Obser_Code_Value loc_iden { get; set; }

        //Observation
        public List<ObservationEEC_DMS> EEC_DMSObservations { get; set; }

        //Organization
        public List<OrganizationEEC_DMS> EEC_DMS_Organization { get; set; }

        //Procedure
        public List<ProcedureEEC_DMS> EEC_DMS_Procedure { get; set; }

        //Condition
        public List<ConditionEEC> EEC_DMSCondition { get; set; }

        //Media
        public List<MediaEEC> EEC_DMSMedia { get; set; }

        #endregion

        #endregion

        #region//門診病歷增加//Procedure

        public string pro_id { get; set; }
        [Display(Name = "Identifier")]
        public List<Obser_Code_Value> pro_identifier { get; set; }

        [Display(Name = "Category")]
        public Obser_Code_Value pro_category { get; set; }

        [Display(Name = "醫療服務人員")]
        public string pro_performer { get; set; }

        [Display(Name = "status")]
        public EventStatus pro_status { get; set; }

        [Display(Name = "處置對象")]
        public string pro_subject { get; set; }

        [Display(Name = "概念")]
        public List<string> pro_followUp { get; set; }

        [Display(Name = "門診病歷註記")]
        public List<string> pro_note { get; set; }

        #region //AllergyIntolerance 
        public string allergy_id { get; set; }
        [Display(Name = "Identifier")]
        public Obser_Code_Value allergy_iden { get; set; }

        [Display(Name = "過敏史患者")]
        public string allergy_patient { get; set; }

        [Display(Name = "代碼")]
        public Obser_Code_Value allergy_code { get; set; }
        #endregion

        #region //Medication
        public string med_id { get; set; }
        [Display(Name = "code")]
        public Obser_Code_Value med_code { get; set; }

        [Display(Name = "磨粉註記powdered與劑型Dosage Form")]
        public Obser_Code_Value med_form { get; set; }

        #endregion

        #region //MedicationRequest

        public string medrq_id { get; set; }
        [Display(Name = "Identifier")]
        public Obser_Code_Value medrq_iden { get; set; }

        [Display(Name = "status")]
        public MedicationRequest.medicationrequestStatus medrq_status { get; set; }

        [Display(Name = "Intent")]
        public MedicationRequest.medicationRequestIntent medrq_intent { get; set; }

        [Display(Name = "Category")]
        public Obser_Code_Value medrq_category { get; set; }

        [Display(Name = "Subject")]
        public string medrq_subject { get; set; }

        [Display(Name = "註記/自費註記 Note/Self-pay note")]
        public Obser_Code_Value medrq_note { get; set; }

        [Display(Name = "註記/自費註記 Note/Self-pay note")]
        public List<Obser_Code_Value> medrq_noteDetail { get; set; } //Detail新增

        [Display(Name = "處方內容頻率 Frequency")]
        public Obser_Code_Value medrq_timing { get; set; }

        [Display(Name = "處方內容給藥途徑 Route of Administration")]
        public Obser_Code_Value medrq_route { get; set; }

        [Display(Name = "處方劑量與單位 Dose and Dose Units")]
        public Obser_Code_Value medrq_dose { get; set; }

        [Display(Name = "實際給藥總量與單位 Actual Amount and Units")]
        public Obser_Code_Value medrq_maxdose { get; set; }

        [Display(Name = "給藥總量與單位 Total Amount and Units")]
        public Obser_Code_Value medrq_quantity { get; set; }

        [Display(Name = "給藥日數 Medication Days")]
        public decimal medrq_duration { get; set; }

        [Display(Name = "給藥日數 Medication Days")]
        public Obser_Code_Value medrq_durationDetail { get; set; }//Detail新增

    #endregion

    #region //ClinicalImpression
    public List<ClinicalImpressionEEC> EEC_PMRClinicalImpression { get; set; }
        #endregion

        #endregion
    }

    public class ClinicalImpressionEEC
    {
        public string cli_id { get; set; }
        [Display(Name = "Subject")]
        public string cli_subject { get; set; }

        [Display(Name = "Status")]
        public ClinicalImpression.ClinicalImpressionStatus cli_status { get; set; }

        [Display(Name = "Identifier")]
        public Obser_Code_Value cli_iden { get; set; }

        [Display(Name = "Code")]
        public Obser_Code_Value cli_code { get; set; }

        [Display(Name = "Note")]
        public Markdown cli_note { get; set; } //病情摘要Subjective名稱為:主觀描述Subjective;Objective為:客觀描述Objective;病人基本資料為:評估Assessment
    }

    public class ConditionEEC
    {
        public string con_id { get; set; }
        [Display(Name = "Identifier")]
        public List<Obser_Code_Value> con_iden { get; set; } //出院病摘主訴 DMS-CC會用到
        [Display(Name = "clinicalStatus")]
        public Obser_Code_Value con_clinicalStatus { get; set; }
        [Display(Name = "verificationStatus")]
        public Obser_Code_Value con_verificationStatus { get; set; } //EEC Condition-DMS-Diagnosis沒用到

        [Display(Name = "出院診斷Discharge Diagnosis")]
        public string con_code { get; set; }

        [Display(Name = "出院診斷Discharge Diagnosis")]
        public Obser_Code_Value con_codeDetail { get; set; } //Detail新增

        [Display(Name = "Category")]
        public Obser_Code_Value con_category { get; set; }

        [Display(Name = "subject")]
        public string con_subject { get; set; }

        [Display(Name = "重大傷病註記Major Illness")]
        public string con_note { get; set; }//門診病歷

        [Display(Name = "重大傷病註記Major Illness")]
        public Markdown con_noteDetail { get; set; }//Detail新增
    }
    public class MediaEEC
    {
        public string media_id { get; set; }
        [Display(Name = "Identifier")]
        public Obser_Code_Value med_iden { get; set; }

        [Display(Name = "Type")]
        public Obser_Code_Value med_type { get; set; }

        [Display(Name = "Status")]
        public EventStatus med_Status { get; set; } //EEC Condition-DMS-Diagnosis沒用到

        [Display(Name = "contentType")]
        public string med_contentType { get; set; }

        [Display(Name = "Title")]
        public string med_title { get; set; }

        [Display(Name = "subject")]
        public string med_subject { get; set; }

        [Display(Name = "醫療影像檢查Imaging Study")]
        public Obser_Code_Value med_modality { get; set; } //Media-DMS-ImagingStudy才有

    }
    public class ObservationEEC_DMS
    {
        [Required]
        [Display(Name = "狀態")]
        public Obser_Status ob_status { get; set; }

        [Display(Name = "就醫病患")]
        public string ob_subject { get; set; }

        [Display(Name = "encounter 開立資料Id")]
        public string encounter { get; set; }

        [Display(Name = "採檢日期時間Sampling Date and Time")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Duration)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime effectivePeriod_star { get; set; }

        [Display(Name = "收件日期時間Delivering Date and Time")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Duration)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime effectivePeriod_end { get; set; }

        [Display(Name = "檢驗單號 Application No.")]
        public Obser_Code_Value ob_identifier { get; set; }

        [Display(Name = "健保檢驗項目 Test item Codes")]
        public Obser_Code_Value ob_code { get; set; }//code

        public Obser_Code_Value Code_value { get; set; }

        [Display(Name = "項次Item Number")]
        public ComponentComponent[] component { get; set; }

        public ReferenceRange ob_range { get; set; }



    }
    public class OrganizationEEC_DMS
    {
        public string org_id { get; set; }
        [Display(Name = "醫事機構代碼與名稱 Hospital Id/Name")]
        public Obser_Code_Value organ_identifier { get; set; }
        [Display(Name = "醫事機構名稱 Hospital Name")]
        public string organ_name { get; set; }

        //出院病摘需要3個Organization
        //Organization 醫事機構，identifier為:醫事機構代碼Hospital Id;name為:醫事機構名稱Hospital Name
        //轉入醫事機構，identifier為:轉入醫事機構代碼Referring Hospital Id;name為:醫事機構名稱Referring Hospital Name
        //轉出醫事機構，identifier為:轉出醫事機構代碼Receiving Hospital Id;name為:醫事機構名稱Receiving Hospital Name
    }
    public class ProcedureEEC_DMS
    {
        public string pro_id { get; set; }
        [Display(Name = "Identifier")]
        public Obser_Code_Value pro_identifier { get; set; }

        //Detail用
        [Display(Name = "Identifier")]
        public List<Obser_Code_Value> pro_idenDetail { get; set; }

        [Display(Name = "Category")]
        public Obser_Code_Value pro_category { get; set; }

        [Display(Name = "出院指示 Instructions on Discharge")]
        public string pro_code { get; set; }

        [Display(Name = "醫療服務人員")]
        public string pro_performer { get; set; }

        [Display(Name = "status")]
        public EventStatus pro_status { get; set; }

        [Display(Name = "處置對象")]
        public string pro_subject { get; set; }

        [Display(Name = "手術日期")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime pro_performed { get; set; }//出院病摘 手術日期及方法會用到

        [Display(Name = "附加資訊")]
        public string pro_note { get; set; }

        [Display(Name = "部位part")]
        public Obser_Code_Value pro_bodysite { get; set; }

        [Display(Name = "followUp 數量/頻率/單位 Amount/Frequency/Units")]
        public List<Obser_Code_Value> pro_followUp { get; set; }

        //Detail用
        [Display(Name = "附加資訊")]
        public List<Obser_Code_Value> pro_noteDetail { get; set; }
        [Display(Name = "出院指示 Instructions on Discharge")]
        public Obser_Code_Value pro_codeDetail { get; set; }

        [Display(Name = "Performed Period")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Duration)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime pro_star { get; set; }
    }



        public class PractitionerEEC
        {
            [Display(Name = "Practitioner Id")]
            public string pra_id { get; set; }
            [Display(Name = "醫事人員姓名 Technician Name")]
            public string pra_name { get; set; }
        }
    
}