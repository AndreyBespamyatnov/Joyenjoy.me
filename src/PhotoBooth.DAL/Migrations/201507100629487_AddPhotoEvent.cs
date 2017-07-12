namespace PhotoBooth.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPhotoEvent : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PhotoEvent", "Created", c => c.DateTime(nullable: false, defaultValueSql: "GETUTCDATE()"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PhotoEvent", "Created");
        }
    }
}
