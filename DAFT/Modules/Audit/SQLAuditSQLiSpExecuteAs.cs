using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DAFT.Modules
{
    class SQLAuditSQLiSpExecuteAs : SQLStoredProcedureSQLi
    {
        [DataContract]
        public struct SpExecuteAs
        {
            [DataMember] public string ComputerName;
            [DataMember] public string Instance;
            [DataMember] public string Vulnerability;
            [DataMember] public string Description;
            [DataMember] public string Remediation;
            [DataMember] public string Severity;
            [DataMember] public string IsVulnerable;
            [DataMember] public string IsExploitable;
            [DataMember] public string Exploited;
            [DataMember] public string ExploitCmd;
            [DataMember] public string Details;
            [DataMember] public string Reference;
        }

        private List<SpExecuteAs> spExecuteAs;

        internal SQLAuditSQLiSpExecuteAs(Credentials credentials) : base(credentials)
        {
            spExecuteAs = new List<SpExecuteAs>();
        }

        internal override bool Query()
        {
            SetKeywordFilter("EXECUTE AS OWNER");
            base.Query();
            
            foreach (var r in procedures)
            {
                var s = new SpExecuteAs
                {
                    ComputerName = computerName,
                    Instance = instance,
                    Vulnerability = "Potential SQL Injection - EXECUTE AS OWNER",
                    Description = "The affected procedure is using dynamic SQL and the EXECUTE AS OWNER clause.  As a result, it may be possible to impersonate the procedure owner if SQL injection is possible.",
                    Remediation = "Consider using parameterized queries instead of concatenated strings, and use signed procedures instead of the EXECUTE AS OWNER clause.",
                    Severity = "High",
                    IsVulnerable = "Yes",
                    IsExploitable = "Unknown",
                    Exploited = "No",
                    ExploitCmd = "Unknown",
                    Reference = @"https://blog.netspi.com/hacking-sql-server-stored-procedures-part-3-sqli-and-user-impersonation",
                    Details = string.Format("{0}.{1}.{2}", r.DatabaseName, r.SCHEMA_NAME, r.ProcedureName)
                };
                spExecuteAs.Add(s);
            }

            return true;
        }

        internal new List<SpExecuteAs> GetResults()
        {
            return spExecuteAs;
        }
    }
}
