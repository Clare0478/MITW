using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FHIR_Demo.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    [Table("User")]
    public class User
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Display(Name = "帳號")]
        [Required(ErrorMessage = "{0}欄位是必要項")]
        public string UserName { get; set; }
        
        [Display(Name = "密碼")]
        [Required(ErrorMessage = "{0} 是必要項")]
        [RegularExpression(@"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])[a-zA-Z0-9]+$", ErrorMessage = "{0} 必須包含至少一個數字、一個小寫字母和一個大寫字母，並且只能由英文字母和數字組成。")]
        [StringLength(40,ErrorMessage ="{0}的長度至少必須為{2}個字元",MinimumLength =6)]
        public string Password { get; set; }

        [Display(Name = "記住密碼")]
        public bool RememberMe { get; set; }

        [Required]
        [Display(Name ="回傳網址")]
        public string AuthorizeUrl { get; set; }
        public string Role { get; set; }
    }
}