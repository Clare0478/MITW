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
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime birthDate { get; set; }
        [Required]
        [Display(Name = "性別")]
        public Gender Gender { get; set; }
        
        [Required]
        [Display(Name = "身分證")]
        [RegularExpression(@"^[A-Z]{1}[A-Da-d1289]{1}[0-9]{8}$", ErrorMessage = "身分證字號錯誤")]
        public string identifier { get; set; }

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

        [Required]
        [Display(Name = "關係")]
        public string contact_telecom { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "電子信箱")]
        public string Email { get; set; }
    }

    public enum Gender
    {
        男 = 0,
        女 = 1,
    }
}