using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FHIR_Demo.Models
{
    //FHIR Resource
    public class Bundle
    {
        public string meta { get; set; } //Myhealth bank download's date
        public string id { get; set; } //uploader's id 
    }

    public class Patient
    {
        public string id { get; set; }
        public string[] valueDate { get; set; }
        public string[] valuestring { get; set; }
        public string name { get; set; }
        public string gender { get; set; }
        public string birthDate { get; set; }
    }

    public class Composition
    {
        public string id { get; set; }
        public string patient { get; set; }
        public string encounter { get; set; }
        public string date { get; set; }
        public string author { get; set; }
        public string title { get; set; }
        //public string [] section_title{ get; set; }
        public string[] section_entry { get; set; }
    }

    public class Encounter
    {
        public string id { get; set; }
        public string en_class { get; set; }
        public string[] type_code { get; set; } //r2_1.4
        public string[] type_display { get; set; }//r2_1.5   r2_1.6
        public string subject { get; set; }
        public string period_start { get; set; }
        public string period_end { get; set; }
        public string[] reasonReference { get; set; }
        public string[] diagnosis { get; set; }
        public string Organization { get; set; }
    }

    public class Condition
    {
        public string id { get; set; }
        public string code_code { get; set; }
        public string code_diplay { get; set; }
        public string subject { get; set; }
        public string encounter { get; set; }
        public string recoredDate { get; set; }
    }

    //未來可以將組織資料存進資料庫  減少重複輸入
    public class Observation
    {
        public string id { get; set; }
        public string category { get; set; }
        public string code_code { get; set; }
        public string code_diplay { get; set; }
        public string subject { get; set; }
        public string encounter { get; set; }
        public string effectiveDateTime { get; set; }
        public string performer { get; set; }//organization
                                             //POST in component 
        public string[] com_code { get; set; }
        public string[] com_code_display { get; set; }
        public string[] valueQuantity { get; set; }
        //public string valuePeriod{ get; set; }
        public string[] interpretation { get; set; }//r10 r11
        public string note { get; set; }
        public string[] referenceRange_text { get; set; }//r7
    }

    public class Procedure
    {
        public string id { get; set; }
        public string code_code { get; set; }
        public string code_diplay { get; set; }
        public string subject { get; set; }
        public string encounter { get; set; }
        public string performedPeriod_start { get; set; }
        public string performedPeriod_end { get; set; }
        public string actor { get; set; }//organization
        public string reasonReference { get; set; } //condition
        public string bodySite_code { get; set; }
        public string bodySite_display { get; set; }
    }

    public class MedicationRequest
    {
        public string id { get; set; }
        public string Medication_id { get; set; }  //藥品後續可新增資料庫  減少重複性
        public string Medication_code { get; set; }
        public string Medication_display { get; set; }
        public string medicationReference { get; set; }
        public string subject { get; set; }
        public string encounter { get; set; }
        public string authoredOn { get; set; }//date
        public string quantity { get; set; } //藥量
        public string expectedSupplyDuration { get; set; } //天數
    }

    public class AllergyIntolerance
    {
        public string id { get; set; }
        public string code { get; set; }
        public string patient { get; set; }
        public string recordedDate { get; set; }
        public string recorder { get; set; } //doctor
    }

    public class Immunization
    {
        public string id { get; set; }
        public string vaccineCode { get; set; } //疫苗名稱
        public string patient { get; set; }
        public string encounter { get; set; }
        public string occurrenceDateTime { get; set; }
    }

    public class Coverage
    {
        public string id { get; set; }
        public string subscriber { get; set; } //patient
        public string beneficiary { get; set; } //patient
        public string period_start { get; set; }
        public string period_end { get; set; }
        public string payor { get; set; } // patient or organization
        public string valueMoney { get; set; } //money <value value="20.00"/>  <currency value = "USD" />
    }

    public class Organization
    {
        public string id { get; set; }
        public string type { get; set; } //r1.2
        public string name { get; set; }
        public string telecom { get; set; }
        public string address { get; set; }
    }
}