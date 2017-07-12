namespace PhotoBooth.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPrintQueueTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PrintQueue",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        BlobPathToImage = c.String(nullable: false),
                        PhotoBoothEntityId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PhotoBoothEntity", t => t.PhotoBoothEntityId, cascadeDelete: true)
                .Index(t => t.PhotoBoothEntityId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PrintQueue", "PhotoBoothEntityId", "dbo.PhotoBoothEntity");
            DropIndex("dbo.PrintQueue", new[] { "PhotoBoothEntityId" });
            DropTable("dbo.PrintQueue");
        }
    }
}
