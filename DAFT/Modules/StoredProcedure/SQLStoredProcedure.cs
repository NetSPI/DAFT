using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DAFT.Modules
{
    class SQLStoredProcedure : Module
    {
        const string QUERY1_2 = @"
ROUTINE_CATALOG AS [DatabaseName],
ROUTINE_SCHEMA AS [SchemaName],
ROUTINE_NAME as [ProcedureName],
ROUTINE_TYPE as [ProcedureType],
ROUTINE_DEFINITION as [ProcedureDefinition],
SQL_DATA_ACCESS,
ROUTINE_BODY,
CREATED,
LAST_ALTERED,
b.is_ms_shipped,
b.is_auto_executed
FROM [INFORMATION_SCHEMA].[ROUTINES] a
JOIN [sys].[procedures]  b
ON a.ROUTINE_NAME = b.name
WHERE 1=1
";
        [DataContract]
        public struct AssemblyFiles
        {
            [DataMember] public string ComputerName;
            [DataMember] public string Instance;
            [DataMember] public string DatabaseName;
            [DataMember] public string schema_name;
            [DataMember] public int file_id;
            [DataMember] public string file_name;
            [DataMember] public string clr_name;
            [DataMember] public int assembly_id;
            [DataMember] public string assembly_name;
            [DataMember] public string assembly_class;
            [DataMember] public string assembly_method;
            [DataMember] public int sp_object_id;
            [DataMember] public string sp_name;
            [DataMember] public string sp_type;
            [DataMember] public string permission_set_desc;
            [DataMember] public DateTime create_date;
            [DataMember] public DateTime modify_date;
            [DataMember] public string content;
        }

        private List<AssemblyFiles> files;

        private string procedureNameFilter = string.Empty;
        private string keywordFilter = string.Empty;
        private string autoExecFilter = string.Empty;

        internal SQLStoredProcedure(Credentials credentials) : base(credentials)
        {
            files = new List<AssemblyFiles>();
        }

        internal void SetProcedureNameFilter(string procedureNameFilter)
        {
            this.procedureNameFilter = string.Format(" AND ROUTINE_NAME LIKE \'{0}\'", procedureNameFilter);
        }

        internal void SetKeywordFilter(string keywordFilter)
        {
            this.keywordFilter = string.Format(" AND ROUTINE_DEFINITION LIKE \'%{0}%\'", keywordFilter);
        }

        internal void SetAutoExecFilter()
        {
            this.autoExecFilter = " AND is_auto_executed = 1";
        }

        internal override bool Query()
        {
            using (SQLConnection sql = new SQLConnection(instance))
            {
                sql.BuildConnectionString(credentials);
                if (!sql.Connect())
                    return false;

                string query1_1 = string.Format(
                    "USE [{0}];" +
                    "SELECT  \'{0}\' as [ComputerName]," +
                    "\'{1}\' as [Instance],",
                    database, computerName, instance);

                StringBuilder sb = new StringBuilder();
                sb.Append(query1_1);
                sb.Append(QUERY1_2);
                sb.Append(autoExecFilter);
                sb.Append(procedureNameFilter);
                sb.Append(keywordFilter);
#if DEBUG
                Console.WriteLine(sb.ToString());
#endif
                files = sql.Query<AssemblyFiles>(sb.ToString(), new AssemblyFiles());
            }
            return true;
        }

        internal List<AssemblyFiles> GetResults()
        {
            return files;
        }
    }
}