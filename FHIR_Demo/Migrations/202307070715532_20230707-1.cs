namespace FHIR_Demo.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _202307071 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.User", "RememberMe", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.User", "RememberMe");
        }
    }
}
