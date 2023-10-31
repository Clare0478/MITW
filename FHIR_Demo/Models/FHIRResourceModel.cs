using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace FHIR_Demo.Models
{
    public class FHIRResourceModel : DbContext
    {
        public FHIRResourceModel()
            : base("FHIRResourceModel")
        {
            //Database.SetInitializer<FHIRResourceModel>(null);//清空Entity Framework更改動作
        }

        public virtual DbSet<resourceinfo> ResourceInfos { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<datatypes> datatypes { get; set; }

        public virtual DbSet<searchparameters> searchparameters { get; set; }

        //protected override void OnModelCreating(DbModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<resourceinfo>()
        //        .Property(e => e.ResourceType)
        //        .IsUnicode(false);

        //    modelBuilder.Entity<resourceinfo>()
        //        .Property(e => e.Name)
        //        .IsUnicode(false);

        //    modelBuilder.Entity<resourceinfo>()
        //        .Property(e => e.Flags)
        //        .IsUnicode(false);

        //    modelBuilder.Entity<resourceinfo>()
        //        .Property(e => e.Card)
        //        .IsUnicode(false);

        //    modelBuilder.Entity<resourceinfo>()
        //        .Property(e => e.Type)
        //        .IsUnicode(false);

        //    modelBuilder.Entity<resourceinfo>()
        //        .Property(e => e.Description)
        //        .IsUnicode(false);
        //}
    }
}
