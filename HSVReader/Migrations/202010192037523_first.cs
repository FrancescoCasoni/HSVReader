namespace HSVReader.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class first : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.HSVs",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        H = c.String(),
                        S = c.String(),
                        V = c.String(),
                    })
                .PrimaryKey(t => t.id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.HSVs");
        }
    }
}
