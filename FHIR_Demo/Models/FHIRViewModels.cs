using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FHIR_Demo.Models
{
    public class PatientViewModel 
    {
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
         * S	State Agency	國家機關
         * U	Unknown未知	
         */

        [Required]
        [Display(Name = "聯絡人聯絡電話")]
        public string contact_telecom { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "電子信箱")]
        public string email { get; set; }

        public PatientViewModel(Patient patient) 
        {
            this.name = patient.Name[0].ToString();
            this.birthDate = patient.BirthDate;
            //switch (patient.Gender) 
            //{
            //    case AdministrativeGender.Male:
            //        this.Gender = Gender.男;
            //        break;
            //    case AdministrativeGender.Female:
            //        this.Gender = Gender.女;
            //        break;
            //    case AdministrativeGender.Other:
            //        this.Gender = Gender.其他;
            //        break;
            //    default:
            //        this.Gender = Gender.不知道;
            //        break;
            //}
            this.Gender = (Gender)patient.Gender;
            this.identifier = patient.Identifier[0].ToString();
            if(patient.Telecom.Count > 0) 
            {
                foreach(var telecom in patient.Telecom) 
                {
                    if(telecom.System == ContactPoint.ContactPointSystem.Phone)
                        this.telecom = telecom.ToString();
                    else if (telecom.System == ContactPoint.ContactPointSystem.Email)
                        this.email = telecom.ToString();
                }
            }
            this.address = patient.Address[0].Text;
            this.contact_name = patient.Contact[0].Name.ToString();
            this.contact_relationship = patient.Contact[0].Relationship[0].Coding[0].Code;
            this.contact_telecom = patient.Contact[0].Telecom[0].ToString();
        }
    }

    public enum Gender
    {
        男 = 0,
        女 = 1,
        其他 = 2,
        不知道 = 3
    }
}