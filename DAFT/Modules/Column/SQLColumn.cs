using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DAFT.Modules
{
    class SQLColumn : Module
    {      
        private const string QUERY1_4 = @"
TABLE_CATALOG AS [DatabaseName],
TABLE_SCHEMA AS [SchemaName],
TABLE_NAME as [TableName],
COLUMN_NAME as [ColumnName],
DATA_TYPE as [ColumnDataType],
CHARACTER_MAXIMUM_LENGTH as [ColumnMaxLength]
";

        private const string QUERY1_6 = 
@"ORDER BY TABLE_CATALOG, TABLE_SCHEMA, TABLE_NAME
";

        private List<string> columnSearchFilter;
        private string columnFilter = string.Empty;
        private string tableNameFilter = string.Empty;

        [DataContract]
        public struct Column
        {
            [DataMember] public string ComputerName;
            [DataMember] public string Instance;
            [DataMember] public string DatabaseName;
            [DataMember] public string SchemaName;
            [DataMember] public string TableName;
            [DataMember] public string ColumnName;
            [DataMember] public string ColumnDataType;
            [DataMember] public object ColumnMaxLength;
        }

        protected List<Column> columns;

        internal SQLColumn(Credentials credentials) : base(credentials)
        {
            columns = new List<Column>();
            columnSearchFilter = new List<string>();
        }

        internal void SetColumnFilter(string columnFilter)
        {
            this.columnFilter = string.Format(" AND column_name LIKE \'{0}\'\n", columnFilter);
        }

        internal void AddColumnSearchFilter(string columnSearchFilter)
        {
            if (this.columnSearchFilter.Count == 0)
                this.columnSearchFilter.Add(string.Format(" AND column_name LIKE \'%{0}%\'\n", columnSearchFilter));
            else
                this.columnSearchFilter.Add(string.Format(" OR column_name LIKE \'%{0}%\'\n", columnSearchFilter));
        }

        internal void SetTableNameFilter(string tableNameFilter)
        {
            this.tableNameFilter = string.Format(" AND TABLE_NAME LIKE \'%{0}%\'\n", tableNameFilter);
        }

        internal override bool Query()
        {
            using (SQLConnection sql = new SQLConnection(instance))
            {
                sql.BuildConnectionString(credentials);
                if (!sql.Connect())
                {
                    return false;
                }

                StringBuilder sb = new StringBuilder();
                sb.Append(string.Format("USE {0};\n", database));
                sb.Append(string.Format("SELECT  \'{0}\' as [ComputerName],\n", computerName));
                sb.Append(string.Format("\'{0}\' as [Instance],", instance));
                sb.Append(QUERY1_4);
                sb.Append(string.Format("FROM [{0}].[INFORMATION_SCHEMA].[COLUMNS] WHERE 1 = 1", database));
                if (!string.IsNullOrEmpty(columnFilter)) { sb.Append(columnFilter); }
                foreach (string f in columnSearchFilter) { sb.Append(f); }
                if (!string.IsNullOrEmpty(tableNameFilter)) { sb.Append(tableNameFilter); }
                sb.Append(QUERY1_6);
#if DEBUG
                Console.WriteLine(sb.ToString());
#endif
                //table = sql.Query(sb.ToString());
                columns = sql.Query<Column>(sb.ToString(), new Column());
            }
            /*
            try
            {
                foreach (DataRow row in table.AsEnumerable())
                {
                    Column t = new Column
                    {
                        ComputerName = (string)row["ComputerName"],
                        Instance = (string)row["Instance"],
                        DatabaseName = (string)row["DatabaseName"],
                        SchemaName = (string)row["SchemaName"],
                        TableName = (string)row["TableName"],
                        ColumnName = (string)row["ColumnName"],
                        ColumnDataType = (string)row["ColumnDataType"],
                        ColumnMaxLength = (object)row["ColumnMaxLength"]
                    };
#if DEBUG
                    Misc.PrintStruct<Column>(t);
#endif
                    columns.Add(t);
                }
            }
            catch (Exception ex)
            {
                if (ex is ArgumentNullException)
                    Console.WriteLine("Empty Response");
                else
                    Console.WriteLine(ex);
                return false;
            }
            */
            return true;
        }

        internal List<Column> GetResults()
        {
            return columns;
        }
    }
}