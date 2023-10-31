namespace FHIR_Demo.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _20230707 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.User", "Role", c => c.String(unicode: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.User", "Role");
        }
    }
}
