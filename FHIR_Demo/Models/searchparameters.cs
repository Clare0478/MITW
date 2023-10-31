namespace FHIR_Demo.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("searchparameters")]
    public partial class searchparameters
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string ResourceType { get; set; }

        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(300)]
        public string Type { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [StringLength(500)]
        public string Expression { get; set; }

        [StringLength(200)]
        public string InCommon { get; set; }
    }
}
