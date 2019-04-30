using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DAFT.Modules
{
    class SQLDatabaseSchema : Module
    {
        private const string QUERY1_2 = @"
CATALOG_NAME as [DatabaseName],
SCHEMA_NAME as [SchemaName],
SCHEMA_OWNER as [SchemaOwner]
";

        private const string QUERY1_4 = @"
ORDER BY SCHEMA_NAME";

        [DataContract]
        public struct Schema
        {
            [DataMember] public string ComputerName;
            [DataMember] public string Instance;
            [DataMember] public string DatabaseName;
            [DataMember] public string SchemaName;
            [DataMember] public string SchemaOwner;
        }

        private List<Schema> schemas;

        private string schemaFilter = string.Empty;

        internal SQLDatabaseSchema(Credentials credentials) : base(credentials)
        {
            schemas = new List<Schema>();
        }

        internal void SetSchemaFilter(string schemaFilter)
        {
            this.schemaFilter = string.Format(" WHERE schema_name LIKE \'%{0}%\'", schemaFilter);
        }

        internal override bool Query()
        {
            string query1_1 = string.Format(
                "USE {0};\n" +
                "SELECT  \'{1}\' as [ComputerName],\n" +
                "\'{2}\' as [Instance],",
                database, computerName, instance
            );

            string query1_3 = string.Format(
                "FROM [{0}].[INFORMATION_SCHEMA].[SCHEMATA]",
                database
            );

            using (SQLConnection sql = new SQLConnection(instance))
            {
                sql.BuildConnectionString(credentials);
                if (!sql.Connect())
                    return false;

                StringBuilder sb = new StringBuilder();
                sb.Append(query1_1);
                sb.Append(QUERY1_2);
                sb.Append(query1_3);
                if (!string.IsNullOrEmpty(schemaFilter)) { sb.Append(schemaFilter); }
                sb.Append(QUERY1_4);
#if DEBUG
                Console.WriteLine(sb.ToString());
#endif
                //table = sql.Query(sb.ToString());
                schemas = sql.Query<Schema>(sb.ToString(), new Schema());
            }

            /*
            foreach (DataRow row in table.AsEnumerable())
            {
                try
                {
                    Schema s = new Schema
                    {
                        ComputerName = (string)row["ComputerName"],
                        Instance = (string)row["Instance"],
                        DatabaseName = (string)row["DatabaseName"],
                        SchemaName = (string)row["SchemaName"],
                        SchemaOwner = (string)row["SchemaOwner"],
                    };
#if DEBUG
                    Misc.PrintStruct<Schema>(s);
#endif
                    schemas.Add(s);
                }
                catch (Exception ex)
                {
                    if (ex is ArgumentNullException)
                        Console.WriteLine("Empty Response");
                    else
                        Console.WriteLine(ex);
                    return false;
                }
            }
            */
            return true;
        }

        internal List<Schema> GetResults()
        {
            return schemas;
        }
    }
}