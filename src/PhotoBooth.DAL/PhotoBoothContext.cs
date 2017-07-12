using PhotoBooth.Models;

namespace PhotoBooth.DAL
{
    using System.Data.Entity;
    using System.Data.Entity.ModelConfiguration.Conventions;

    // add-migration -ProjectName "PhotoBooth.DAL"
    // update-database -ProjectName "PhotoBooth.DAL"
    
    public class PhotoBoothContext : DbContext
    {
        public PhotoBoothContext()
            : base("DefaultConnection")
        {
            Database.SetInitializer<PhotoBoothContext>(null);
        }

        public DbSet<PhotoBoothEntity> PhotoBooths { get; set; }
        public DbSet<PhotoEvent> PhotoEvents { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<PrintQueue> PrintQueue { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            modelBuilder.Entity<Photo>().HasKey(photo => photo.Id);
            modelBuilder.Entity<PhotoEvent>().HasKey(photo => photo.Id);
            modelBuilder.Entity<PhotoBoothEntity>().HasKey(photo => photo.Id);
            modelBuilder.Entity<PrintQueue>().HasKey(photo => photo.Id);

            modelBuilder.Entity<PhotoBoothEntity>()
                        .HasMany<PhotoEvent>(s => s.Events)
                        .WithRequired(s => s.PhotoBoothEntity)
                        .HasForeignKey(s => s.PhotoBoothEntityId)
                        .WillCascadeOnDelete(true);

            modelBuilder.Entity<PhotoEvent>()
                        .HasMany<Photo>(s => s.Photos)
                        .WithRequired(s => s.PhotoEvent)
                        .HasForeignKey(s => s.PhotoEventId)
                        .WillCascadeOnDelete(true);


            modelBuilder.Entity<PhotoEvent>()
                .Property(p => p.StartDateTime)
                .HasColumnType("datetime")
                .HasPrecision(0)
                .IsRequired();

            modelBuilder.Entity<PhotoEvent>()
                .Property(p => p.EndDateTime)
                .HasColumnType("datetime")
                .HasPrecision(0)
                .IsRequired();
        }
    }
}
