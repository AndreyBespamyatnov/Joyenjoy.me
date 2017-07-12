namespace PhotoBooth.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PhotoBoothEntity",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(nullable: false),
                        PathToDSLRSettings = c.String(nullable: false),
                        LastAvailableDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PhotoEvent",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(nullable: false),
                        Description = c.String(),
                        HashTag = c.String(),
                        StartDateTime = c.DateTime(nullable: false),
                        EndDateTime = c.DateTime(nullable: false),
                        ShowOnGallery = c.Boolean(nullable: false),
                        IsPublic = c.Boolean(nullable: false),
                        Password = c.String(),
                        LinkToLastZip = c.String(),
                        LinkToGalleryPreviewImage = c.String(),
                        PhotoBoothEntityId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PhotoBoothEntity", t => t.PhotoBoothEntityId, cascadeDelete: true)
                .Index(t => t.PhotoBoothEntityId);
            
            CreateTable(
                "dbo.Photo",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        LocalPathToImage = c.String(),
                        BlobPathToImage = c.String(),
                        BlobPathToPreviewImage = c.String(),
                        Md5Hash = c.String(),
                        PhotoEventId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PhotoEvent", t => t.PhotoEventId, cascadeDelete: true)
                .Index(t => t.PhotoEventId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PhotoEvent", "PhotoBoothEntityId", "dbo.PhotoBoothEntity");
            DropForeignKey("dbo.Photo", "PhotoEventId", "dbo.PhotoEvent");
            DropIndex("dbo.Photo", new[] { "PhotoEventId" });
            DropIndex("dbo.PhotoEvent", new[] { "PhotoBoothEntityId" });
            DropTable("dbo.Photo");
            DropTable("dbo.PhotoEvent");
            DropTable("dbo.PhotoBoothEntity");
        }
    }
}
