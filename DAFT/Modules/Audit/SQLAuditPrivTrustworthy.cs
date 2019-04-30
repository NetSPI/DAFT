using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DAFT.Modules
{
    class SQLAuditPrivTrustworthy : SQLDatabase
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

        internal SQLAuditPrivTrustworthy(Credentials credentials) : base(credentials)
        {
            trustworthies = new List<Trustworthy>();
        }

        internal override bool Query()
        {
            EnableNoDefaultsFilter();
            base.Query();

            foreach (var d in databases)
            {
                if (d.is_trustworthy_on)
                {
                    var s = new Trustworthy
                    {
                        ComputerName = computerName,
                        Instance = instance,
                        Vulnerability = "Excessive Privilege - Trustworthy Database",
                        Description = "One or more database is configured as trustworthy.  The TRUSTWORTHY database property is used to indicate whether the instance of SQL Server trusts the database and the contents within it.  Including potentially malicious assemblies with an EXTERNAL_ACCESS or UNSAFE permission setting. Also, potentially malicious modules that are defined to execute as high privileged users. Combined with other weak configurations it can lead to user impersonation and arbitrary code exection on the server.",
                        Remediation = "Configured the affected database so the 'is_trustworthy_on' flag is set to 'false'.  A query similar to 'ALTER DATABASE MyAppsDb SET TRUSTWORTHY ON' is used to set a database as trustworthy.  A query similar to 'ALTER DATABASE MyAppDb SET TRUSTWORTHY OFF' can be use to unset it.",
                        Severity = "Low",
                        IsVulnerable = "Yes",
                        IsExploitable = "No",
                        Exploited = "No",
                        ExploitCmd = "There is not exploit available at this time.",
                        Reference = @"https://msdn.microsoft.com/en-us/library/ms187861.aspx",
                        Details = string.Format("The database {0} was found configured as trustworthy.", d.DatabaseName)
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
