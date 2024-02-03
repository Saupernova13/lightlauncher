namespace lightlauncher.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class emulators1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Games", "needsEmulator", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Games", "needsEmulator");
        }
    }
}
