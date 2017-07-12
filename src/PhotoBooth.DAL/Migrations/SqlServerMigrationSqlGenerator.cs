namespace PhotoBooth.WebApp.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.SqlServer;
    using System.Globalization;

    internal class SqlServerMigrationSqlGeneratorFixed : SqlServerMigrationSqlGenerator
    {
        protected override string Generate(DateTime defaultValue)
        {
            return string.Format("'{0}'", defaultValue.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
        }

        protected override void Generate(AddColumnOperation addColumnOperation)
        {
            SetCreatedColumn(addColumnOperation.Column);

            base.Generate(addColumnOperation);
        }

        protected override void Generate(CreateTableOperation createTableOperation)
        {
            SetCreatedColumn(createTableOperation.Columns);

            base.Generate(createTableOperation);
        }

        private static void SetCreatedColumn(IEnumerable<ColumnModel> columns)
        {
            foreach (var columnModel in columns)
            {
                SetCreatedColumn(columnModel);
            }
        }

        private static void SetCreatedColumn(PropertyModel column)
        {
            if (column.Name == "Created")
            {
                column.DefaultValueSql = "GETDATE()";
            }
        }
    }
}
