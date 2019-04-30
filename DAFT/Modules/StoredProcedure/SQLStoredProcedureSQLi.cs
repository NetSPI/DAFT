using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DAFT.Modules
{
    class SQLStoredProcedureSQLi : Module
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
WHERE 1=1 AND               
(ROUTINE_DEFINITION like '%sp_executesql%' OR
ROUTINE_DEFINITION like '%sp_sqlexec%' OR
ROUTINE_DEFINITION like '%exec @%' OR
ROUTINE_DEFINITION like '%execute @%' OR
ROUTINE_DEFINITION like '%exec (%' OR
ROUTINE_DEFINITION like '%exec(%' OR
ROUTINE_DEFINITION like '%execute (%' OR
ROUTINE_DEFINITION like '%execute(%' OR
ROUTINE_DEFINITION like '%''''''+%' OR
ROUTINE_DEFINITION like '%'''''' +%') 
AND ROUTINE_DEFINITION like '%+%'
AND ROUTINE_CATALOG not like 'msdb' 
";

        const string QUERY2_2 = @"
spr.ROUTINE_CATALOG as DB_NAME,
spr.SPECIFIC_SCHEMA as SCHEMA_NAME,
spr.ROUTINE_NAME as SP_NAME,
spr.ROUTINE_DEFINITION as SP_CODE,
CASE cp.crypt_type
when 'SPVC' then cer.name
when 'CPVC' then Cer.name
when 'SPVA' then ak.name
when 'CPVA' then ak.name
END as CERT_NAME,
sp.name as CERT_LOGIN,
sp.sid as CERT_SID
FROM sys.crypt_properties cp
JOIN sys.objects o ON cp.major_id = o.object_id
LEFT JOIN sys.certificates cer ON cp.thumbprint = cer.thumbprint
LEFT JOIN sys.asymmetric_keys ak ON cp.thumbprint = ak.thumbprint
LEFT JOIN INFORMATION_SCHEMA.ROUTINES spr on spr.ROUTINE_NAME = o.name
LEFT JOIN sys.server_principals sp on sp.sid = cer.sid
WHERE o.type_desc = 'SQL_STORED_PROCEDURE'AND
(ROUTINE_DEFINITION like '%sp_executesql%' OR
ROUTINE_DEFINITION like '%sp_sqlexec%' OR
ROUTINE_DEFINITION like '%exec @%' OR
ROUTINE_DEFINITION like '%exec (%' OR
ROUTINE_DEFINITION like '%exec(%' OR
ROUTINE_DEFINITION like '%execute @%' OR
ROUTINE_DEFINITION like '%execute (%' OR
ROUTINE_DEFINITION like '%execute(%' OR
ROUTINE_DEFINITION like '%''''''+%' OR
ROUTINE_DEFINITION like '%'''''' +%') AND
ROUTINE_CATALOG not like 'msdb' AND 
ROUTINE_DEFINITION like '%+%'
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

            [DataMember] public string DB_NAME;
            [DataMember] public string SCHEMA_NAME;
            [DataMember] public string SP_NAME;
            [DataMember] public string SP_CODE;
            [DataMember] public string CERT_NAME;
            [DataMember] public string CERT_LOGIN;
            [DataMember] public string CERT_SID;
        }

        protected List<Procedure> procedures;

        private string procedureNameFilter = string.Empty;
        private string keywordFilter = string.Empty;
        private string autoExecFilter = string.Empty;
        private bool onlySigned = false;
        private bool showAll = false;

        internal SQLStoredProcedureSQLi(Credentials credentials) : base(credentials)
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

        internal void SetAutoExecFilter()
        {
            autoExecFilter = " AND is_auto_executed = 1";
        }

        internal void EnableOnlySigned()
        {
            onlySigned = true;
        }

        internal void EnableShowAll()
        {
            showAll = true;
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
                sb.Append(onlySigned ? QUERY2_2 : QUERY1_2);
                if (!string.IsNullOrEmpty(procedureNameFilter)) { sb.Append(procedureNameFilter); }
                if (!string.IsNullOrEmpty(keywordFilter)) { sb.Append(keywordFilter); }
                if (!string.IsNullOrEmpty(autoExecFilter)) { sb.Append(autoExecFilter); }
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