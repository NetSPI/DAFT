using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DAFT.Modules
{
    class SQLStoredProcedureAutoExec : Module
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
AND is_auto_executed = 1
";

        const string QUERY1_3 = @"ORDER BY ROUTINE_NAME";

        [DataContract]
        public struct Procedure
        {
            [DataMember] public string ComputerName;
            [DataMember] public string Instance;
            [DataMember] public string DatabaseName;
            [DataMember] public string SchemaName;
            [DataMember] public string ProcedureName;
            [DataMember] public string ProcedureType;
            [DataMember] public string ProcedureDefinition;
            [DataMember] public string SQL_DATA_ACCESS;
            [DataMember] public string ROUTINE_BODY;
            [DataMember] public DateTime CREATED;
            [DataMember] public DateTime LAST_ALTERED;
            [DataMember] public bool is_ms_shipped;
            [DataMember] public bool is_auto_executed;
        }

        protected List<Procedure> procedures;

        private string procedureNameFilter = string.Empty;
        private string keywordFilter = string.Empty;

        internal SQLStoredProcedureAutoExec(Credentials credentials) : base(credentials)
        {
            procedures = new List<Procedure>();
        }

        internal void SetProcedureNameFilter(string procedureNameFilter)
        {
            this.procedureNameFilter = string.Format(" AND ROUTINE_NAME LIKE \'{0}\'", procedureNameFilter);
        }

        internal void SetKeywordFilter(string keywordFilter)
        {
            this.keywordFilter = string.Format(" AND ROUTINE_DEFINITION LIKE \'%{0}%\'", keywordFilter);
        }

        internal override bool Query()
        {
            using (SQLConnection sql = new SQLConnection(instance))
            {
                sql.BuildConnectionString(credentials);
                if (!sql.Connect())
                    return false;

                string query1_1 = string.Format("" +
                    "SELECT \'{0}\' as [ComputerName],\n" +
                    "\'{1}\' as [Instance],\n" +
                    "\'{2}\' as [DatabaseName],\n",
                    computerName, instance, database);

                StringBuilder sb = new StringBuilder();
                sb.Append(query1_1);
                sb.Append(QUERY1_2);
                if (!string.IsNullOrEmpty(procedureNameFilter)) { sb.Append(procedureNameFilter); }
                if (!string.IsNullOrEmpty(keywordFilter)) { sb.Append(keywordFilter); }
                sb.Append(QUERY1_3);

#if DEBUG
                Console.WriteLine(sb.ToString());
#endif
                procedures = sql.Query<Procedure>(sb.ToString(), new Procedure());
            }
            return true;
        }

        internal List<Procedure> GetResults()
        {
            return procedures;
        }
    }
}