using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DAFT.Modules
{
    class SQLView : Module
    {
        private const string QUERY1_2 = @"
TABLE_CATALOG as [DatabaseName],
TABLE_SCHEMA as [SchemaName],
TABLE_NAME as [ViewName],
VIEW_DEFINITION as [ViewDefinition],
IS_UPDATABLE as [IsUpdatable],
CHECK_OPTION as [CheckOption]
FROM [INFORMATION_SCHEMA].[VIEWS]
";

        private const string QUERY1_3 = @"
ORDER BY TABLE_CATALOG, TABLE_SCHEMA, TABLE_NAME
";
        [DataContract]
        public struct View
        {
            [DataMember] public string ComputerName;
            [DataMember] public string Instance;
            [DataMember] public string DatabaseName;
            [DataMember] public string SchemaName;
            [DataMember] public string ViewName;
            [DataMember] public string ViewDefinition;
            [DataMember] public string IsUpdatable;
            [DataMember] public string CheckOption;
        }

        private List<View> views;

        private string viewFilter = string.Empty;

        internal SQLView(Credentials credentials) : base(credentials)
        {
            views = new List<View>();
        }

        internal void SetTableNameFilter(string viewFilter)
        {
            this.viewFilter = string.Format("WHERE table_name LIKE \'%{0}%\'", viewFilter);
        }

        internal override bool Query()
        {
            string query1_1 = string.Format(
                "USE {0};\n" +
                "SELECT \'{1}\' as [ComputerName],\n" +
                "\'{2}\' as [Instance],",
                database, computerName, instance
            );

            using (SQLConnection sql = new SQLConnection(instance))
            {
                sql.BuildConnectionString(credentials);
                if (!sql.Connect())
                    return false;

                StringBuilder sb = new StringBuilder();
                sb.Append(query1_1);
                sb.Append(QUERY1_2);
                if (!string.IsNullOrEmpty(viewFilter)) { sb.Append(viewFilter); }
                sb.Append(QUERY1_3);
#if DEBUG
                Console.WriteLine(sb.ToString());
#endif
                //table = sql.Query(sb.ToString());
                views = sql.Query<View>(sb.ToString(), new View());
            }

            /*
            foreach (DataRow row in table.AsEnumerable())
            {
                try
                {
                    View v = new View
                    {
                        ComputerName = (string)row["ComputerName"],
                        Instance = (string)row["Instance"],
                        DatabaseName = (string)row["DatabaseName"],
                        SchemaName = (string)row["SchemaName"],
                        ViewName = (string)row["ViewName"],
                        ViewDefinition = (string)row["ViewDefinition"],
                        IsUpdatable = (string)row["IsUpdatable"],
                        CheckOption = (string)row["CheckOption"]
                    };
#if DEBUG
                    Misc.PrintStruct<View>(v);
#endif
                    views.Add(v);
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

        internal List<View> GetResults()
        {
            return views;
        }
    }
}
