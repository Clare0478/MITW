﻿using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

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
        [Display(Name = "身分證")]
        [RegularExpression(@"^[A-Z]{1}[A-Da-d1289]{1}[0-9]{8}$", ErrorMessage = "身分證字號錯誤")]
        public string identifier { get; set; }

        //以後要可以多個
        [Required]
        [Display(Name = "連絡電話")]
        public string telecom { get; set; }

        [Required]
        [Display(Name = "聯絡地址")]
        public string address { get; set; }

        [Required]
        [Display(Name = "緊急聯絡人")]
        public string contact_name { get; set; }

        [Required]
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

        [Required]
        [Display(Name = "聯絡人聯絡電話")]
        public string contact_telecom { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "電子信箱")]
        public string email { get; set; }

        [Required]
        [Display(Name = "組織")]
        public string managingOrganization { get; set; }




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
                this.address = patient.Address[0].Text;
            if (patient.Contact.Count > 0)
            {
                if (patient.Contact[0].Name != null)
                    this.contact_name = patient.Contact[0].Name.ToString();
                if (patient.Contact[0].Relationship.Count > 0)
                    if (patient.Contact[0].Relationship[0].Coding.Count > 0)
                        this.contact_relationship = patient.Contact[0].Relationship[0].Coding[0].Code ?? "";
                if (patient.Contact[0].Telecom.Count > 0)
                    this.contact_telecom = patient.Contact[0].Telecom[0].Value;
            }
            if (patient.ManagingOrganization != null)
                this.managingOrganization = patient.ManagingOrganization.Reference;

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
            this.effectiveDateTime = DateTime.Parse(observation.Effective.ToString());
            this.Code_value = new Obser_Code_Value();
            if (observation.Code.Coding.Count > 0) 
            {
                this.Code_value.code_display = new ObservationCode().observationCode().
                    Where(o=> o.code.Contains(observation.Code.Coding[0].Code)).FirstOrDefault().chinese ?? observation.Code.Coding[0].Display ?? "";
            }
                //this.Code_value.code_display = observation.Code.Coding[0].Display ?? "";
            if (observation.Value.GetType() == typeof(Quantity))
            {
                this.Code_value.value = ((Quantity)observation.Value).Value;
                this.Code_value.unit = ((Quantity)observation.Value).Unit;
            }
            if (observation.Component.Count > 0)
            {
                this.component = new Obser_Code_Value[observation.Component.Count];
                for (var i = 0; i < observation.Component.Count; i++)
                {
                    this.component[i] = new Obser_Code_Value();
                    if (observation.Component[i].Code.Coding.Count > 0)
                        this.component[i].code_display = new ObservationCode().observationCode().
                             Where(o => o.code.Contains(observation.Component[i].Code.Coding[0].Code)).FirstOrDefault().chinese ?? observation.Component[i].Code.Coding[0].Display ?? "";
                    if (observation.Component[i].Value.GetType() == typeof(Quantity))
                    {
                        this.component[i].value = ((Quantity)observation.Component[i].Value).Value;
                        this.component[i].unit = ((Quantity)observation.Component[i].Value).Unit;
                    }
                }
            }

            return this;
        }
    }

    public class Obser_Code_Value
    {
        [Required]
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
        public ObservationCategory_Value Distolic_Blood_Pressure(decimal? value_Distolic) { return ObservationCategory_Data("vital-signs", "Vital Signs",  "8462-4", "Distolic Blood Pressure", "mmHg", value_Distolic); }

    }

}