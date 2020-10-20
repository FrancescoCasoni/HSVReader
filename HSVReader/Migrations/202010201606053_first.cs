namespace HSVReader.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class first : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.HSVs", "H", c => c.Double(nullable: false));
            AlterColumn("dbo.HSVs", "S", c => c.Double(nullable: false));
            AlterColumn("dbo.HSVs", "V", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.HSVs", "V", c => c.String());
            AlterColumn("dbo.HSVs", "S", c => c.String());
            AlterColumn("dbo.HSVs", "H", c => c.String());
        }
    }
}
