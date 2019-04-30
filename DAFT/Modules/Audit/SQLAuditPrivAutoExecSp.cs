using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DAFT.Modules
{
    class SQLAuditPrivAutoExecSp : SQLStoredProcedureAutoExec
    {
        [DataContract]
        public struct Trustworthy
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

        private List<Trustworthy> trustworthies;

        internal SQLAuditPrivAutoExecSp(Credentials credentials) : base(credentials)
        {
            trustworthies = new List<Trustworthy>();
        }

        internal override bool Query()
        {
            base.Query();
            SQLDatabasePriv priv = new SQLDatabasePriv(credentials);
            foreach (var p in procedures)
            {
                priv.SetInstance(instance);
                priv.SetDatabase(p.DatabaseName);
                priv.Query();
                foreach (var d in priv.GetResults())
                { 
                    var s = new Trustworthy
                    {
                        ComputerName = computerName,
                        Instance = instance,
                        Vulnerability = "Excessive Privilege - Auto Execute Stored Procedure",
                        Description = "A stored procedured is configured for automatic execution and has explicit permissions assigned.  This may allow non sysadmin logins to execute queries as sa when the SQL Server service is restarted.",
                        Remediation = "Ensure that non sysadmin logins do not have privileges to ALTER stored procedures configured with the is_auto_executed settting set to 1.",
                        Severity = "Low",
                        IsVulnerable = "Yes",
                        IsExploitable = "No",
                        Exploited = "No",
                        ExploitCmd = "There is not exploit available at this time.",
                        Reference = @"https://msdn.microsoft.com/en-us/library/ms187861.aspx",
                        Details = string.Format("{0} has {1} {2} on {3}", d.PrincipalName, d.StateDescription, d.PermissionName, d.DatabaseName + "." + p.SchemaName + "." + p.ProcedureName)
                    };
                    trustworthies.Add(s);
                }
            }
            return true;
        }

        internal new List<Trustworthy> GetResults()
        {
            return trustworthies;
        }
    }
}
