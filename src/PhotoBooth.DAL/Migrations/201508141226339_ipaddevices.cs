namespace PhotoBooth.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ipaddevices : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PhotoBoothEntity", "IPadDeviceId", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PhotoBoothEntity", "IPadDeviceId");
        }
    }
}
