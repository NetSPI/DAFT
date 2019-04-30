using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DAFT.Modules
{
    class SQLAuditPrivServerLink : SQLServerLink
    {
        [DataContract]
        public struct ServerLink
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

        private List<ServerLink> links;

        internal SQLAuditPrivServerLink(Credentials credentials) : base(credentials)
        {
            links = new List<ServerLink>();
        }

        internal override bool Query()
        {
            base.Query();

            foreach (var l in serverLinks)
            {
                if (!string.IsNullOrEmpty(l.LocalLogin) && l.LocalLogin.Contains("Credentials"))
                {
                    var s = new ServerLink
                    {
                        ComputerName = computerName,
                        Instance = instance,
                        Vulnerability = "Excessive Privilege - Linked Server",
                        Description = "One or more linked servers is preconfigured with alternative credentials which could allow a least privilege login to escalate their privileges on a remote server.",
                        Remediation = "Configure SQL Server links to connect to remote servers using the login's current security context.",
                        Severity = "Medium",
                        IsVulnerable = "Yes",
                        IsExploitable = "Unknown",
                        Exploited = "No",
                        ExploitCmd = string.Format("e.g. SELECT * FROM OPENQUERY([{0}], \'SELECT system_user\')", instance),
                        Reference = @"https://msdn.microsoft.com/en-us/library/ms190479.aspx",
                        Details = string.Format("The SQL Server link {0} was found configured with the {1} login.", instance, l.LocalLogin)
                    };
                    links.Add(s);
                }
            }
            return true;
        }

        internal new List<ServerLink> GetResults()
        {
            return links;
        }
    }
}
