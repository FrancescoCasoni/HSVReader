namespace HSVReader.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class second : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.HSVs", "X", c => c.Int(nullable: false));
            AddColumn("dbo.HSVs", "Y", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.HSVs", "Y");
            DropColumn("dbo.HSVs", "X");
        }
    }
}
