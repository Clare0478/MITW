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

    [Table("datatypes")]
    public class datatypes
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(80)]
        public string Datatype { get; set; }

        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(50)]
        public string Flags { get; set; }

        [StringLength(50)]
        public string Card { get; set; }

        [StringLength(300)]
        public string Type { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }
    }
}