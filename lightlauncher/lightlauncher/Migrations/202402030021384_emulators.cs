namespace lightlauncher.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class emulators : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Emulators",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        name = c.String(),
                        executablePath = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Emulators");
        }
    }
}
