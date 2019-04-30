using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DAFT.Modules
{
    class SQLAuditRole : SQLServerRoleMember
    {
        [DataContract]
        public struct DbOwner
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

        private List<DbOwner> spExecuteAs;
        private string role = string.Empty;

        internal SQLAuditRole(Credentials credentials) : base(credentials)
        {
            spExecuteAs = new List<DbOwner>();
        }

        internal void SetRole(string role)
        {
            this.role = role;
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

            SQLDatabaseRoleMember roleMember = new SQLDatabaseRoleMember(credentials);
            roleMember.SetRolePrincipalNameFilter(role);
            roleMember.SetInstance(instance);
            SQLDatabase database = new SQLDatabase(credentials);
            database.SetInstance(instance);
       
            foreach (var principal in principals)
            {
                roleMember.SetPrincipalNameFilter(principal);
                foreach (var db in database.GetResults())
                {
                    if (db.is_trustworthy_on && (bool)db.OwnerIsSysadmin)
                    {
                        roleMember.SetDatabase(db.DatabaseName);
                        roleMember.Query();
                        foreach (var r in roleMember.GetResults())
                        {
                            var s = new DbOwner
                            {
                                ComputerName = computerName,
                                Instance = instance,
                                Vulnerability = string.Format("Database Role - {0}", role),
                                Description = string.Format("The login has the {0} role in one or more databases.  This may allow the login to escalate privileges to sysadmin if the affected databases are trusted and owned by a sysadmin.", role),
                                Remediation = string.Format("If the permission is not required remove it.  Permissions are granted with a command like: EXEC sp_addrolemember \'{0}\', \'MyDbUser\', and can be removed with a command like:  EXEC sp_droprolemember \'{0}\', \'MyDbUser\'", role),
                                Severity = "Medium",
                                IsVulnerable = "Yes",
                                IsExploitable = "Unknown",
                                Exploited = "No",
                                ExploitCmd = "",
                                Reference = @"https://msdn.microsoft.com/en-us/library/ms189121.aspx, https://msdn.microsoft.com/en-us/library/ms187861.aspx",
                                Details = string.Format("The {0} database is set as trustworthy and is owned by a sysadmin. This is exploitable.", database)
                            };
                            spExecuteAs.Add(s);
                        }
                    }
                }
            }
            return true;
        }

        internal new List<DbOwner> GetResults()
        {
            return spExecuteAs;
        }
    }
}
