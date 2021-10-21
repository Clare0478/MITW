using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FHIR_Demo.Models
{
    public enum Gender
    {
        男 = 0,
        女 = 1,
        其他 = 2,
        不知道 = 3
    }

    public enum Obser_Status
    {
        註冊 = 0,
        初步 = 1,
        最終 = 2,
        修正 = 3,
        更正 = 4,
        取消 = 5,
        輸入錯誤 = 6,
        未知 = 7
    }

    public class ObservationCode
    {
        public string code { get; set; }
        public string display { get; set; }
        public string chinese { get; set; }

        public List<ObservationCode> observationCode()
        {
            var obser_code_lists = new List<ObservationCode>
            {
                new ObservationCode {code = "3137-7", display = "Body Height", chinese = "身高"},
                new ObservationCode {code = "29463-7", display = "Body Weight", chinese = "體重"},
                new ObservationCode {code = "8310-5", display = "Body Temperature", chinese = "體溫"},
                new ObservationCode {code = "87422-2", display = "Blood Glucose Post Meal", chinese = "餐後血糖"},
                new ObservationCode {code = "88365-2", display = "Blood Glucose Pre Meal", chinese = "餐前血糖"},
                new ObservationCode {code = "41982-0", display = "Percentage of body fat Measured", chinese = "體指百分率"},
                new ObservationCode {code = "83174-3", display = "Grip strength Hand - right Dynamometer", chinese = "握力"},
                new ObservationCode {code = "59408-5", display = "Oxygen saturation in Arterial blood by Pulse oximetry", chinese = "SPO2血氧飽和濃度"},
                new ObservationCode {code = "8867-4", display = "Heart Rate", chinese = "心率"},
                new ObservationCode {code = "35094-2", display = "Blood Pressure Panel", chinese = "血壓"},
                new ObservationCode {code = "8480-6", display = "Systolic Blood Pressure", chinese = "收縮壓"},
                new ObservationCode {code = "8462-4", display = "Distolic Blood Pressure", chinese = "舒張壓"},
            };
            return obser_code_lists;
        }
    }

    //public enum ObservationCode
    //{
    //    身高,
    //    體重,
    //    體溫,
    //    餐後血糖,
    //    餐前血糖,
    //    體指百分率,
    //    握力_右手測力計,
    //    動脈血氧飽和度通過脈搏血氧飽和度,
    //    心率,
    //    血壓
    //}

    //public enum ObservationCode_ch
    //{
    //    身高,
    //    體重,
    //    體溫,
    //    餐後血糖,
    //    餐前血糖,
    //    體指百分率,
    //    握力_右手測力計,
    //    動脈血氧飽和度通過脈搏血氧飽和度,
    //    心率,
    //    血壓
    //}

    //public enum Observation_blood_Code
    //{
    //    //身高,
    //    //體重,
    //    //體溫,
    //    //餐前血糖,
    //    //餐後血糖,
    //    //體指百分率,
    //    //握力_右手測力計,
    //    //動脈血氧飽和度通過脈搏血氧飽和度,
    //    //心率,
    //    收縮壓,  //Systolic blood pressure
    //    舒張壓   //Distolic blood pressure
    //}

    //public enum Observation_blood_Code_ch
    //{
    //    //身高,
    //    //體重,
    //    //體溫,
    //    //餐前血糖,
    //    //餐後血糖,
    //    //體指百分率,
    //    //握力_右手測力計,
    //    //動脈血氧飽和度通過脈搏血氧飽和度,
    //    //心率,
    //    收縮壓,  //Systolic blood pressure
    //    舒張壓   //Distolic blood pressure
    //}

    public enum medicationrequestStatus_Ch
    {
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
        藥囑 = 2,
        原始訂單 = 3,
        反射順序 = 4,
        填充順序 = 5,
        實例順序 = 6,
        選項 = 7
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
            return new List<CodeableConcept> { new CodeableConcept(MedicationRequestCategory.System, MedicationRequestCategory.Code, MedicationRequestCategory.Display, null) };
        }
    }

    public enum MedicationAdministrationStatusCodes_Ch
    {
        進行中 = 0,
        未完成 = 1,
        保持 = 2,
        完成 = 3,
        輸入錯誤 = 4,
        停止 = 5,
        未知 = 6
    }




}