using FHIR_Demo.Models;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using static Hl7.Fhir.Model.MedicationAdministration;

namespace FHIR_Demo.Models
{
    public class PatientViewModel
    {
        public string Id { get; set; }

        [Required]
        [Display(Name = "姓名")]
        public string name { get; set; }

        [Required]
        [Display(Name = "生日")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public string birthDate { get; set; }

        [Required]
        [Display(Name = "性別")]
        public Gender Gender { get; set; }

        [Required]
        [Display(Name = "身分證/病歷號")]
        //[RegularExpression(@"^[A-Z]{1}[A-Da-d1289]{1}[0-9]{8}$", ErrorMessage = "身分證字號錯誤")]
        public string identifier { get; set; }

        //以後要可以多個
        [Display(Name = "連絡電話")]
        public string telecom { get; set; }

        [Display(Name = "聯絡地址")]
        public string address { get; set; }
        public string city { get; set; }
        public string town { get; set; }
        public string zipcode { get; set; }

        [Display(Name = "緊急聯絡人")]
        public string contact_name { get; set; }

        [Display(Name = "關係")]
        public string contact_relationship { get; set; }
        /*
         * C	緊急聯繫人	
         * E	Employer 雇主
         * F	Federal Agency	聯邦機構
         * I	Insurance Company	保險公司
         * N	Next-of-Kin	近親
         * S	State Agency    國家機關
         * U	Unknown 未知	
         */

        [Display(Name = "聯絡人聯絡電話")]
        public string contact_telecom { get; set; }

        [EmailAddress]
        [Display(Name = "電子信箱")]
        public string email { get; set; }

        [Display(Name = "組織")]
        public string managingOrganization { get; set; }

        [Display(Name = "是否死亡")]
        public string deceased { get; set; }



        public PatientViewModel PatientViewModelMapping(Patient patient)
        {
            this.Id = patient.Id;
            if (patient.Name.Count > 0)
                this.name = patient.Name[0].ToString();
            this.birthDate = patient.BirthDate ?? "";
            //switch (patient.Gender) 
            //{
            //    case AdministrativeGender.Male:
            //        patientView.Gender = Gender.男;
            //        break;
            //    case AdministrativeGender.Female:
            //        patientView.Gender = Gender.女;
            //        break;
            //    case AdministrativeGender.Other:
            //        patientView.Gender = Gender.其他;
            //        break;
            //    default:
            //        patientView.Gender = Gender.不知道;
            //        break;
            //}
            this.Gender = patient.Gender != null ? (Gender)patient.Gender : Gender.不知道;
            if (patient.Identifier.Count > 0)
                this.identifier = patient.Identifier[0].Value;
            if (patient.Telecom.Count > 0)
            {
                foreach (var telecom in patient.Telecom)
                {
                    if (telecom.System == ContactPoint.ContactPointSystem.Phone)
                        this.telecom = telecom.Value;
                    else if (telecom.System == ContactPoint.ContactPointSystem.Email)
                        this.email = telecom.Value;
                }
            }
            if (patient.Address.Count > 0) 
            {
                if (patient.Address[0].Line.Count() > 0) 
                    this.address = patient.Address[0].Line.First();
                else 
                    this.address = patient.Address[0].Text;
                this.city = patient.Address[0].City;
                this.town = patient.Address[0].District;
                this.zipcode = patient.Address[0].PostalCode;
            }
            if (patient.Contact.Count > 0)
            {
                if (patient.Contact[0].Name != null)
                    this.contact_name = patient.Contact[0].Name.ToString();
                if (patient.Contact[0].Relationship.Count > 0)
                    if (patient.Contact[0].Relationship[0].Coding.Count > 0)
                        //this.contact_relationship = patient.Contact[0].Relationship[0].Coding[0].Code ?? "";
                        this.contact_relationship = patient.Contact[0].Relationship[0].Text ?? "";
                if (patient.Contact[0].Telecom.Count > 0)
                    this.contact_telecom = patient.Contact[0].Telecom[0].Value;
            }
            if (patient.ManagingOrganization != null)
                this.managingOrganization = patient.ManagingOrganization.Reference;

            if (patient.Deceased != null)
                if (patient.Deceased.TypeName == "boolean")
                    this.deceased = ((FhirBoolean)patient.Deceased).Value.ToString();
                else if (patient.Deceased.TypeName == "dateTime")
                    this.deceased = ((FhirDateTime)patient.Deceased).ToString();
            return this;
        }

    }

    public class ObservationViewModel
    {
        public string Id { get; set; }

        [Required]
        [Display(Name = "狀態")]
        public Obser_Status status { get; set; }

        [Display(Name = "類別")]
        public string catogory { get; set; }

        [Display(Name = "基於")]
        public string basedOn { get; set; }

        [Required]
        [Display(Name = "受試者")]
        public string subject { get; set; }

        [Required]
        [Display(Name = "檢測時間")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime effectiveDateTime { get; set; }

        public Obser_Code_Value Code_value { get; set; }

        [Display(Name = "組合數值")]
        public Obser_Code_Value[] component { get; set; }

        public ObservationViewModel ObservationViewModelMapping(Observation observation)
        {
            this.Id = observation.Id;
            this.status = (Obser_Status)observation.Status;
            if (observation.BasedOn.Count > 0)
                this.basedOn = observation.BasedOn[0].Reference;
            if (observation.Category.Count > 0)
                if (observation.Category[0].Coding.Count > 0)
                    this.catogory = observation.Category[0].Coding[0].Display;
            this.subject = observation.Subject.Reference ?? "";
            if (observation.Effective != null)
                this.effectiveDateTime = DateTime.Parse(observation.Effective.ToString());
            this.Code_value = new Obser_Code_Value();
            if (observation.Code != null)
            {
                if (observation.Code.Coding.Count > 0)
                {
                    //this.Code_value.code_display = new ObservationCode().observationCode().
                    //    Where(o => o.code.Contains(observation.Code.Coding[0].Code)).FirstOrDefault()?.chinese ?? observation.Code.Coding[0].Display ?? "";
                    this.Code_value.code_display = observation.Code.Coding[0].Code ?? observation.Code.Coding[0].Display ?? "";
                }
            }
            //this.Code_value.code_display = observation.Code.Coding[0].Display ?? "";
            if (observation.Value != null) 
            {
                if (observation.Value.GetType() == typeof(Quantity))
                {
                    this.Code_value.value = ((Quantity)observation.Value).Value;
                    this.Code_value.unit = ((Quantity)observation.Value).Unit;
                }
            }
            if (observation.Component.Count > 0)
            {
                this.component = new Obser_Code_Value[observation.Component.Count];
                for (var i = 0; i < observation.Component.Count; i++)
                {
                    this.component[i] = new Obser_Code_Value();
                    if (observation.Component[i].Code.Coding.Count > 0) 
                    {
                        //this.component[i].code_display = new ObservationCode().observationCode().
                        //    Where(o => o.code.Contains(observation.Component[i].Code.Coding[0].Code)).FirstOrDefault()?.chinese ?? observation.Component[i].Code.Coding[0].Display ?? "";

                        this.component[i].code_display = observation.Component[i].Code.Coding[0].Code ?? observation.Component[i].Code.Coding[0].Display ?? "";

                    }
                    if (observation.Component[i].Value != null) 
                    {
                        if (observation.Component[i].Value.GetType() == typeof(Quantity))
                        {
                            this.component[i].value = ((Quantity)observation.Component[i].Value).Value;
                            this.component[i].unit = ((Quantity)observation.Component[i].Value).Unit;
                        }
                    }
                }
            }

            return this;
        }
    }

    public class Obser_Code_Value
    {
        [Display(Name = "檢驗項目")]
        public string code_display { get; set; }

        [Display(Name = "數值")]
        public decimal? value { get; set; }

        public string unit { get; set; }
    }

    public class ObservationCategory_Value
    {
        public List<CodeableConcept> Category { get; private set; }
        public CodeableConcept Code { get; private set; }
        public Quantity Value { get; private set; }
        public List<Observation.ComponentComponent> Component { get; private set; }

        private ObservationCategory_Value ObservationCategory_Data(string category_code, string category_display, string coding_code, string coding_display, string unit, decimal? value)
        {
            Category = new List<CodeableConcept>
            {
                new CodeableConcept
                {
                    Coding = new List<Coding>
                    {
                        new Coding
                        {
                            System = "http://terminology.hl7.org/CodeSystem/observation-category",
                            Code = category_code,
                            Display = category_display
                        }
                    }
                }
            };
            Code = new CodeableConcept
            {
                Coding = new List<Coding>
                    {
                        new Coding
                        {
                            System = "http://loinc.org",
                            Code = coding_code,
                            Display = coding_display
                        }
                    }
            };
            Value = new Quantity
            {
                Unit = unit,
                System = "http://unitsofmeasure.org",
                Code = unit,
                Value = value
            };
            return this;
        }
        private ObservationCategory_Value ObservationCategory_Data(string category_code, string category_display, string coding_code, string coding_display, string unit, string coding_code_Systolic, string coding_display_Systolic, decimal? value_Systolic, string coding_code_Distolic, string coding_display_Distolic, decimal? value_Distolic)
        {
            Category = new List<CodeableConcept>
            {
                new CodeableConcept
                {
                    Coding = new List<Coding>
                    {
                        new Coding
                        {
                            System = "http://terminology.hl7.org/CodeSystem/observation-category",
                            Code = category_code,
                            Display = category_display
                        }
                    }
                }
            };
            Code = new CodeableConcept
            {
                Coding = new List<Coding>
                    {
                        new Coding
                        {
                            System = "http://loinc.org",
                            Code = coding_code,
                            Display = coding_display
                        }
                    }
            };
            Value = new Quantity
            {
                Unit = unit,
                System = "http://unitsofmeasure.org",
                Code = unit,
            };

            Component = new List<Observation.ComponentComponent>
            {
                new Observation.ComponentComponent
                {
                    Code = new CodeableConcept
                    {
                        Coding = new List<Coding>
                            {
                                new Coding
                                {
                                    System = "http://loinc.org",
                                    Code = coding_code_Systolic,
                                    Display = coding_display_Systolic
                                }
                            }
                    },
                    Value = new Quantity
                    {
                        Unit = unit,
                        System = "http://unitsofmeasure.org",
                        Code = unit,
                        Value = value_Systolic
                    }
                },
                new Observation.ComponentComponent
                {
                    Code = new CodeableConcept
                    {
                        Coding = new List<Coding>
                            {
                                new Coding
                                {
                                    System = "http://loinc.org",
                                    Code = coding_code_Distolic,
                                    Display = coding_display_Distolic
                                }
                            }
                    },
                    Value = new Quantity
                    {
                        Unit = unit,
                        System = "http://unitsofmeasure.org",
                        Code = unit,
                        Value = value_Distolic
                    }
                },
            };
            return this;
        }


        public ObservationCategory_Value Body_Height(decimal? value) { return ObservationCategory_Data("vital-signs", "Vital Signs", "3137-7", "Body Height", "cm", value); }
        public ObservationCategory_Value Body_Weight(decimal? value) { return ObservationCategory_Data("vital-signs", "Vital Signs", "29463-7", "Body Weight", "kg", value); }
        public ObservationCategory_Value Body_Temperature(decimal? value) { return ObservationCategory_Data("vital-signs", "Vital Signs", "8310-5", "Body Temperature", "Cel", value); }
        public ObservationCategory_Value Blood_Glucose_Post_Meal(decimal? value) { return ObservationCategory_Data("laboratory", "Laboratory", "87422-2", "Blood Glucose Post Meal", "mg/dL", value); }
        public ObservationCategory_Value Blood_Glucose_Pre_Meal(decimal? value) { return ObservationCategory_Data("laboratory", "Laboratory", "88365-2", "Blood Glucose Pre Meal", "mg/dL", value); }
        public ObservationCategory_Value Percentage_of_body_fat_Measured(decimal? value) { return ObservationCategory_Data("vital-signs", "Vital Signs", "41982-0", "Percentage of body fat Measured", "%", value); }
        public ObservationCategory_Value Grip_strength_Hand_right_Dynamometer(decimal? value) { return ObservationCategory_Data("vital-signs", "Vital Signs", "83174-3", "Grip strength Hand - right Dynamometer", "kg", value); }
        public ObservationCategory_Value Oxygen_saturation_in_Arterial_blood_by_Pulse_oximetry(decimal? value) { return ObservationCategory_Data("vital-signs", "Vital Signs", "59408-5", "Oxygen saturation in Arterial blood by Pulse oximetry", "%", value); }
        public ObservationCategory_Value Heart_Rate(decimal? value) { return ObservationCategory_Data("vital-signs", "Vital Signs", "8867-4", "Heart Rate", "{beats}/min", value); }
        public ObservationCategory_Value Blood_Pressure_Panel(decimal? value_Systolic, decimal? value_Distolic) { return ObservationCategory_Data("vital-signs", "Vital Signs", "35094-2", "Blood Pressure Panel", "mmHg", "8480-6", "Systolic Blood Pressure", value_Systolic, "8462-4", "Distolic Blood Pressure", value_Distolic); }
        public ObservationCategory_Value Systolic_Blood_Pressure(decimal? value_Systolic) { return ObservationCategory_Data("vital-signs", "Vital Signs", "8480-6", "Systolic Blood Pressure", "mmHg", value_Systolic); }
        public ObservationCategory_Value Distolic_Blood_Pressure(decimal? value_Distolic) { return ObservationCategory_Data("vital-signs", "Vital Signs", "8462-4", "Distolic Blood Pressure", "mmHg", value_Distolic); }

        public ObservationCategory_Value Heart_Rate_EMS(decimal? value) { return ObservationCategory_Data("vital-signs", "Vital Signs", "8889-8", "Heart rate by Pulse oximeter", "{beats}/min", value); }
        public ObservationCategory_Value Respiratory_Rate_EMS(decimal? value) { return ObservationCategory_Data("vital-signs", "Vital Signs", "9279-1", "Respiratory Rate", "{breaths}/min;{counts/min}", value); }
        public ObservationCategory_Value Capillary_refill_of_Nail_bed_EMS(decimal? value) { return ObservationCategory_Data("exam", "Exam", "44963-7", "Capillary refill [Time] of Nail bed", "s", value); }
        public ObservationCategory_Value Glucose_in_Blood_EMS(decimal? value) { return ObservationCategory_Data("laboratory", "Laboratory", "2339-0", "Glucose [Mass/volume] in Blood", "mg/dL", value); }


    }

    public class MedicationRequestViewModel
    {
        public string Id { get; set; }

        [Required]
        [Display(Name = "狀態")]
        public medicationrequestStatus_Ch status { get; set; }

        [Required]
        [Display(Name = "資料用途")]
        public medicationRequestIntent_Ch intent { get; set; }

        [Display(Name = "類別")]
        public CodeableConcept[] categorys { get; set; }
        //public List<CodeableConcept> categorys { get => Category; set => Category = categorys; }

        [Display(Name = "藥物連結")]
        public string medicationReference { get; set; }

        [Display(Name = "藥物資訊")]
        public Coding medicationCodeableConcept { get; set; }

        [Required]
        [Display(Name = "患者")]
        public string subject { get; set; }

        [Required]
        [Display(Name = "給藥日期")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public string authoredOn { get; set; }

        [Display(Name = "用量說明")]
        public MedReqDosage[] dosageInstruction { get; set; }

        [Display(Name = "配藥說明")]
        public DispenseRequest dispenseRequest { get; set; } = new DispenseRequest();

        public MedicationRequestViewModel MedicationRequestViewModelMapping(MedicationRequest medicationRequest)
        {
            this.Id = medicationRequest.Id;
            this.status = (medicationrequestStatus_Ch)medicationRequest.Status;
            this.intent = (medicationRequestIntent_Ch)medicationRequest.Intent;
            this.categorys = medicationRequest.Category.ToArray();
            if (medicationRequest.Medication != null)
            {
                if (medicationRequest.Medication.TypeName == "Reference")
                    this.medicationReference = ((ResourceReference)medicationRequest.Medication).Url.ToString();
                if (medicationRequest.Medication.TypeName == "CodeableConcept")
                    this.medicationCodeableConcept = ((CodeableConcept)medicationRequest.Medication).Coding[0];
            }

            this.subject = medicationRequest.Subject.Url.ToString();
            this.authoredOn = medicationRequest.AuthoredOn;
            var MedReqDosage_list = new List<MedReqDosage>();
            foreach (var dosage in medicationRequest.DosageInstruction)
            {
                var med_dosage = new MedReqDosage();
                med_dosage.sequence = dosage.Sequence;
                med_dosage.text = dosage.Text;
                if (dosage.Timing != null)
                    if (dosage.Timing.Code != null)
                        med_dosage.timing_Code = dosage.Timing.Code.Coding[0];
                if (dosage.Route != null)
                    med_dosage.route = dosage.Route.Coding[0];
                MedReqDosage_list.Add(med_dosage);
            }
            this.dosageInstruction = MedReqDosage_list.ToArray();
            if (medicationRequest.DispenseRequest != null)
            {
                if (medicationRequest.DispenseRequest.ValidityPeriod != null)
                    this.dispenseRequest.validityPeriod = medicationRequest.DispenseRequest.ValidityPeriod;
                if (medicationRequest.DispenseRequest.NumberOfRepeatsAllowed != null)
                    this.dispenseRequest.numberOfRepeatsAllowed = medicationRequest.DispenseRequest.NumberOfRepeatsAllowed;
                if (medicationRequest.DispenseRequest.Quantity != null)
                    this.dispenseRequest.quantity = medicationRequest.DispenseRequest.Quantity;
                if (medicationRequest.DispenseRequest.ExpectedSupplyDuration != null)
                    this.dispenseRequest.expectedSupplyDuration = medicationRequest.DispenseRequest.ExpectedSupplyDuration;
            }

            return this;
        }


    }

    public class MedReqDosage
    {
        //給藥次序
        [Display(Name = "用藥順序")]
        public int? sequence { get; set; }
        //用藥描述
        [Display(Name = "用藥描述")]
        public string text { get; set; }
        //用藥方式
        [Display(Name = "用藥方式")]
        public Coding timing_Code { get; set; }
        //用藥途徑
        [Display(Name = "用藥途徑")]
        public Coding route { get; set; }

    }

    public class DispenseRequest
    {
        public Period validityPeriod { get; set; } = new Period();

        [Required]
        [Display(Name = "配藥開始日期")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public string validityPeriod_start { get => validityPeriod.Start; set => validityPeriod.Start = value; }
        //{
        //    get => ((Period)Effective).Start;
        //    set => ((Period)Effective).Start = value;
        //}

        [Required]
        [Display(Name = "配藥結束日期")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public string validityPeriod_end { get => validityPeriod.End; set => validityPeriod.End = value; }

        public int? numberOfRepeatsAllowed { get; set; }
        [Display(Name = "配藥總量")]
        public Quantity quantity { get; set; }
        [Display(Name = "配藥天數")]
        public Duration expectedSupplyDuration { get; set; }
    }

    public class MedicationAdministrationViewModel
    {
        public string Id { get; set; }

        [Required]
        [Display(Name = "狀態")]
        public MedicationAdministrationStatusCodes_Ch status { get; set; }

        [Display(Name = "藥物連結")]
        public string medicationReference { get; set; }

        [Display(Name = "藥物資訊")]
        public Coding medicationCodeableConcept { get; set; }

        [Required]
        [Display(Name = "患者")]
        public string subject { get; set; }
        //{
        //    get => Subject.Url.ToString();

        //    set => Subject = new ResourceReference(value);
        //}

        public Period effectivePeriod { get; set; } = new Period();

        [Required]
        [Display(Name = "服藥開始日期")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public string effectivePeriod_start { get => effectivePeriod.Start; set => effectivePeriod.Start = value; }
        //{
        //    get => ((Period)Effective).Start;
        //    set => ((Period)Effective).Start = value;
        //}

        [Required]
        [Display(Name = "服藥結束日期")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public string effectivePeriod_end { get => effectivePeriod.End; set => effectivePeriod.End = value; }
        //{
        //    get => ((Period)Effective).End;
        //    set => ((Period)Effective).End = value;
        //}

        [Required]
        [Display(Name = "處方籤")]
        public string request { get; set; }
        //{
        //    get => Request.Url.ToString();

        //    set => Request = new ResourceReference(value);
        //}

        //重新思考
        [Required]
        [Display(Name = "劑量")]
        public DosageComponent dosage { get; set; }
        //{
        //    get => Dosage;

        //    set => Dosage = new DosageComponent() { Text = value.Text, Dose = value.Dose };
        //}

        public MedicationAdministrationViewModel MedicationAdministrationViewModelMapping(MedicationAdministration medicationAdministration)
        {
            this.Id = medicationAdministration.Id;
            this.status = (MedicationAdministrationStatusCodes_Ch)medicationAdministration.Status;
            if (medicationAdministration.Medication != null)
            {
                if (medicationAdministration.Medication.TypeName == "Reference")
                    this.medicationReference = ((ResourceReference)medicationAdministration.Medication).Url.ToString();
                if (medicationAdministration.Medication.TypeName == "CodeableConcept")
                    this.medicationCodeableConcept = ((CodeableConcept)medicationAdministration.Medication).Coding[0];
            }

            this.subject = medicationAdministration.Subject.Url.ToString();
            if (medicationAdministration.Effective.TypeName == "Period")
                this.effectivePeriod = (Period)medicationAdministration.Effective;
            if (medicationAdministration.Effective.TypeName == "dateTime")
            {
                this.effectivePeriod.Start = medicationAdministration.Effective.ToString();
                this.effectivePeriod.End = medicationAdministration.Effective.ToString();
            }
            if (medicationAdministration.Request != null)
                this.request = medicationAdministration.Request.Url.ToString();
            if (medicationAdministration.Dosage != null)
                this.dosage = medicationAdministration.Dosage;

            return this;
        }
    }

    public class ImmunizationViewModel
    {
        //public string Com_Id { get; set; }

        [Required]
        [Display(Name = "類別")]
        public string Type { get; set; }

        [Required]
        [Display(Name = "患者")]
        public ResourceReference Patient { get; set; } = new ResourceReference();

        [Required]
        [Display(Name = "時間")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }

        [Display(Name = "醫事機構")]
        public ResourceReference Hospital { get; set; } = new ResourceReference();


        //public string Obs_Id { get; set; }

        [Display(Name = "檢驗代碼與名稱")]
        public CodeableConcept Obs_Coding { get; set; } = new CodeableConcept();

        [Display(Name = "檢驗結果")]
        public string value { get; set; }


        //public string Imm_Id { get; set; }

        //[Display(Name = "疫苗或預防措施")]
        //public List<Identifier> Imm_Identifier { get; set; }

        //[Display(Name = "施打患者")]
        //public ResourceReference Imm_Patient { get; set; } = new ResourceReference();

        //[Required]
        [Display(Name = "疫苗代碼")]
        public CodeableConcept Imm_VaccineCode { get; set; } = new CodeableConcept();

        //[Required]
        [Display(Name = "製造商")]
        public ResourceReference Imm_Manufacturer { get; set; } = new ResourceReference();

        //[Required]
        [Display(Name = "批號")]
        public string Imm_LotNumber { get; set; }

        //[Required]
        //[Display(Name = "接種日期")]
        //public string Imm_Occurrence { get; set; }

        //[Required]
        //[Display(Name = "注射單位")]
        //public ResourceReference Location { get; set; }

        [Display(Name = "相關資訊")]
        public ProtocolAppliedComponent Imm_ProtocolApplied { get; set; } =  new ProtocolAppliedComponent() ;

        [Display(Name = "醫事人員姓名")]
        public string Imm_Performer_acotr_display { get; set; }

    }

    public class ProtocolAppliedComponent
    {
        //[Required]
        //[Display(Name = "疫苗系列名稱")]
        //public string Series { get; set; }

        //[Display(Name = "誰負責發佈建議")] 
        //public ResourceReference Authority { get; set; }

        [Display(Name = "疫苗可預防疾病的")]
        public List<CodeableConcept> TargetDisease { get; set; } = new List<CodeableConcept> { new CodeableConcept { Coding = new List<Coding> { new Coding { } } } };

        //[Required]
        [Display(Name = "劑別")]
        public string DoseNumber { get; set; }

        [Display(Name = "疫苗的完整劑數")]
        public string SeriesDoses { get; set; }
    }

    //public class PerformerComponent
    //{      
    //    [Required]
    //    [Display(Name = "注射單位")]
    //    public ResourceReference Actor { get; set; }
    //}

}