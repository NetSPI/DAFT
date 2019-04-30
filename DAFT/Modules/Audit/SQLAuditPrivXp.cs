using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DAFT.Modules
{
    class SQLAuditPrivXp : SQLServerRoleMember
    {
        [DataContract]
        public struct XpDirTree
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

        private List<XpDirTree> spExecuteAs;
        private string xp = string.Empty;

        internal SQLAuditPrivXp(Credentials credentials) : base(credentials)
        {
            spExecuteAs = new List<XpDirTree>();
        }

        internal void SetExtendedProcedure(string xp)
        {
            this.xp = xp;
        }

        internal override bool Query()
        {
            SQLServerInfo i = new SQLServerInfo(credentials);
            i.SetInstance(instance);
            i.Query();
            var info = i.GetResults();

            List<string> principals = new List<string>();
            SetPrincipalNameFilter(info.Currentlogin);
            base.Query();
            foreach (var s in serverRoles)
            {
                principals.Add(s.PrincipalName);
            }
            principals.Add(info.Currentlogin);
            principals.Add("Public");

            SQLDatabasePriv p = new SQLDatabasePriv(credentials);
            p.SetInstance(instance);
            p.SetDatabase("master");
            p.SetPermissionNameFilter("EXECUTE");
            p.Query();

            var dirTree = new List<SQLDatabasePriv.DatabasePrivilege>();
            foreach (var priv in p.GetResults())
            {
                if (!string.IsNullOrEmpty(priv.ObjectName) && priv.ObjectName.Contains(xp) && priv.StateDescription.Contains("grant"))
                {
                    dirTree.Add(priv);
                }
            }

            foreach (var r in dirTree)
            {
                if (r.PrincipalName.Contains("public") || principals.Contains(r.PrincipalName))
                {
                    var s = new XpDirTree
                    {
                        ComputerName = computerName,
                        Instance = instance,
                        Vulnerability = string.Format("Excessive Privilege - Execute {0}", xp),
                        Description = string.Format("{0} is a native extended stored procedure that can be executed by members of the Public role by default in SQL Server 2000-2014. {0} can be used to force the SQL Server service account to authenticate to a remote attacker.  The service account password hash can then be captured + cracked or relayed to gain unauthorized access to systems. This also means {0} can be used to escalate a lower privileged user to sysadmin when a machine or managed account isnt being used.  Thats because the SQL Server service account is a member of the sysadmin role in SQL Server 2000-2014, by default.", xp),
                        Remediation = string.Format("Remove EXECUTE privileges on the {0} procedure for non administrative logins and roles.  Example command: REVOKE EXECUTE ON {0} to Public.", xp),
                        Severity = "Medium",
                        IsVulnerable = "Yes",
                        IsExploitable = "Unknown",
                        Exploited = "No",
                        ExploitCmd = "Crack the password hash offline or relay it to another system.",
                        Reference = @"https://blog.netspi.com/executing-smb-relay-attacks-via-sql-server-using-metasploit/",
                        Details = string.Format("The {0} principal has EXECUTE privileges on the {1} procedure in the master database.", r.PrincipalName, xp)
                    };
                    spExecuteAs.Add(s);
                }
            }

            return true;
        }

        internal new List<XpDirTree> GetResults()
        {
            return spExecuteAs;
        }
    }
}
