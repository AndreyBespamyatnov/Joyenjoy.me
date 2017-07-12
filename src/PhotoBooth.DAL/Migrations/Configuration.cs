namespace PhotoBooth.DAL.Migrations
{
    using System.Data.Entity.Migrations;

    using PhotoBooth.WebApp.Migrations;

    internal sealed class Configuration : DbMigrationsConfiguration<PhotoBoothContext>
    {
        public Configuration()
        {
            this.AutomaticMigrationsEnabled = false;
            this.ContextKey = "PhotoBooth.DAL.PhotoBoothContext";
            this.SetSqlGenerator("System.Data.SqlClient", new SqlServerMigrationSqlGeneratorFixed());
        }
    }
}
