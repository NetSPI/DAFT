using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DAFT.Modules
{
    class SQLAuditSQLiSpSigned : SQLStoredProcedureSQLi
    {
        [DataContract]
        public struct SpSigned
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

        private List<SpSigned> spSigned;

        internal SQLAuditSQLiSpSigned(Credentials credentials) : base(credentials)
        {
            spSigned = new List<SpSigned>();
        }

        internal override bool Query()
        {
            SetKeywordFilter("EXECUTE AS OWNER");
            base.Query();
            
            foreach (var r in procedures)
            {
                var s = new SpSigned
                {
                    ComputerName = computerName,
                    Instance = instance,
                    Vulnerability = "Potential SQL Injection - Signed by Certificate Login",
                    Description = "The affected procedure is using dynamic SQL and has been signed by a certificate login.  As a result, it may be possible to impersonate signer if SQL injection is possible.",
                    Remediation = "Consider using parameterized queries instead of concatenated strings.",
                    Severity = "High",
                    IsVulnerable = "Yes",
                    IsExploitable = "Unknown",
                    Exploited = "No",
                    ExploitCmd = "Unknown",
                    Reference = @"https://blog.netspi.com/hacking-sql-server-stored-procedures-part-3-sqli-and-user-impersonation",
                    Details = string.Format("{0}.{1}.{2}", r.DatabaseName, r.SCHEMA_NAME, r.ProcedureName)
                };
                spSigned.Add(s);
            }

            return true;
        }

        internal new List<SpSigned> GetResults()
        {
            return spSigned;
        }
    }
}
