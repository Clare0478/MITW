namespace FHIR_Demo.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _20230705 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.User",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserName = c.String(nullable: false, unicode: false),
                        Password = c.String(nullable: false, maxLength: 40, storeType: "nvarchar"),
                        AuthorizeUrl = c.String(nullable: false, unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
        }
    }
}
