namespace PhotoBooth.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddImagesSizes : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Photo", "ImageWidth", c => c.Int(nullable: false));
            AddColumn("dbo.Photo", "ImageHeight", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Photo", "ImageHeight");
            DropColumn("dbo.Photo", "ImageWidth");
        }
    }
}
