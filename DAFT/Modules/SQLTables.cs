using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DAFT.Modules
{
    sealed class SQLTables : Module
    {
        private const string QUERY1_3 = @"
TABLE_CATALOG as [DatabaseName],
TABLE_SCHEMA as [SchemaName],
TABLE_NAME as [TableName],
TABLE_TYPE as [TableType]
";

        private const string QUERY1_5 = @"
ORDER BY TABLE_CATALOG, TABLE_SCHEMA, TABLE_NAME";      

        [DataContract]
        public struct Table
        {
            [DataMember] public string ComputerName;
            [DataMember] public string Instance;
            [DataMember] public string DatabaseName;
            [DataMember] public string SchemaName;
            [DataMember] public string TableName;
            [DataMember] public string TableType;
        }

        private List<Table> tables;

        private string tableFilter = string.Empty;

        internal SQLTables(Credentials credentials) : base(credentials)
        {
            tables = new List<Table>();
        }

        internal void SetTable(string tableFilter)
        {
            this.tableFilter = string.Format(" where table_name like \'%{0}%\'", tableFilter);
        }

        internal override bool Query()
        {
            using (SQLConnection sql = new SQLConnection(instance))
            {
                sql.BuildConnectionString(credentials);
                if (!sql.Connect())
                    return false;

                StringBuilder sb = new StringBuilder();
                sb.Append(string.Format("USE {0};\n", database));
                sb.Append(string.Format("SELECT \'{0}\' as [ComputerName],\n", computerName));
                sb.Append(string.Format("\'{0}\' as [Instance],", instance));
                sb.Append(QUERY1_3);
                sb.Append(string.Format("FROM[{0}].[INFORMATION_SCHEMA].[TABLES]", database));
                if (!string.IsNullOrEmpty(tableFilter)) { sb.Append(tableFilter); }
                sb.Append(QUERY1_5);
#if DEBUG
                Console.WriteLine(sb.ToString());
#endif
                //table = sql.Query(sb.ToString());
                tables = sql.Query<Table>(sb.ToString(), new Table());
            }

            /*
            foreach (DataRow row in table.AsEnumerable())
            {
                try
                {
                    Table t = new Table
                    {
                        ComputerName = (string)row["ComputerName"],
                        Instance = (string)row["Instance"],
                        DatabaseName = (string)row["DatabaseName"],
                        SchemaName = (string)row["SchemaName"],
                        TableName = (string)row["TableName"],
                        TableType = (string)row["TableType"]
                    };
#if DEBUG
                    Misc.PrintStruct<Table>(t);
#endif
                    tables.Add(t);
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

        internal List<Table> GetResults()
        {
            return tables;
        }
    }
}
