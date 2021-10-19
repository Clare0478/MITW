using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FHIR_Demo.Models.test
{
    public class MedicationRequestViewModels : MedicationRequest
    {
        [Required]
        [Display(Name = "狀態")]
        public medicationrequestStatus_Ch status { get => (medicationrequestStatus_Ch)Status; set => Status = (medicationrequestStatus)value; }

        [Required]
        [Display(Name = "資料用途")]
        public medicationRequestIntent_Ch intent { get => (medicationRequestIntent_Ch)Intent; set => Intent = (medicationRequestIntent)value; }

        [Display(Name = "類別")]
        public string categorys
        {
            get => new MedicationRequestCategory().MedicationRequestCategory_list().Where(c => c.Code.Contains(Category[0].Coding[0].Code)).First().Chinese;
            set => Category = new MedicationRequestCategory().MedicationRequestCategory_Create(value);
        }
        //public List<CodeableConcept> categorys { get => Category; set => Category = categorys; }

        [Display(Name = "藥物")]
        public string medicationReference
        {
            get
            {
                if (Medication.TypeName == "Reference")
                    return ((ResourceReference)Medication).Url.ToString();
                else
                    return null;
            }
            set => Medication = new ResourceReference(value);
        }

        [Display(Name = "藥物")]
        public Coding medicationCodeableConcept
        {
            get
            {
                if (Medication.TypeName == "Reference")
                    return null;
                else
                    return ((CodeableConcept)Medication).Coding[0];
            }
            set => Medication = new CodeableConcept(value.System, value.Code, value.Display);
        }

        [Required]
        [Display(Name = "患者")]
        public string subject
        {
            get => Subject.Url.ToString();

            set => Subject = new ResourceReference(value);
        }

        [Required]
        [Display(Name = "給藥日期")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public string authoredOn
        {
            get => AuthoredOn;
            set => AuthoredOn = value;
        }

        //public MedDosage dosageInstruction

        public string dispenseRequest 
        {
            get => DispenseRequest.ToJson();
            set => DispenseRequest = new FhirJsonParser().Parse<DispenseRequestComponent>(value);
        }

    }

    public class MedDosage : Dosage 
    {
        //給藥次序
        public int? Sequence { get; set; }
        //用藥描述
        public string Text { get; set; }
        //用藥方式
        public Code Timing_Code { get; set; }
        //用藥途徑
        public CodeableConcept Route { get; set; }

    }

    public class MedicationRequestCategory
    {
        public string System { get => "http://terminology.hl7.org/CodeSystem/medicationrequest-category"; }
        public string Code { get; set; }
        public string Display { get; set; }
        public string Chinese { get; set; }

        public List<MedicationRequestCategory> MedicationRequestCategory_list()
        {
            var obser_code_lists = new List<MedicationRequestCategory>
            {
                new MedicationRequestCategory {Code = "inpatient", Display = "Inpatient", Chinese = "住院病人"},
                new MedicationRequestCategory {Code = "outpatient", Display = "Outpatient", Chinese = "門診病人"},
                new MedicationRequestCategory {Code = "community", Display = "Community", Chinese = "機構/家中"},
                new MedicationRequestCategory {Code = "discharge", Display = "Discharge", Chinese = "出院"},
            };
            return obser_code_lists;
        }

        public List<CodeableConcept> MedicationRequestCategory_Create(string Category)
        {
            var MedicationRequestCategory = MedicationRequestCategory_list().Where(c => c.Chinese.Contains(Category)).First();
            return new List<CodeableConcept>{new CodeableConcept(MedicationRequestCategory.System, MedicationRequestCategory.Code, MedicationRequestCategory.Display, null)};
        }
    }

    public enum medicationrequestStatus_Ch
    {
        [Display(Name = "1 Star")]
        活躍 = 0,
        保持 = 1,
        取消 = 2,
        完成 = 3,
        輸入錯誤 = 4,
        停止 = 5,
        草稿 = 6,
        未知 = 7
    }

    public enum medicationRequestIntent_Ch
    {
        提案 = 0,
        計劃 = 1,
        訂單 = 2,
        原始訂單 = 3,
        反射順序 = 4,
        填充順序 = 5,
        實例順序 = 6,
        選項 = 7
    }
}