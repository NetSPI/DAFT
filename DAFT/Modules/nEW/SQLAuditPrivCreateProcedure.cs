using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DAFT.Modules
{
    class SQLAuditPrivCreateProcedure : SQLServerRoleMember
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

        internal SQLAuditPrivCreateProcedure(Credentials credentials) : base(credentials)
        {
            spExecuteAs = new List<XpDirTree>();
        }

        internal override bool Query()
        {
            SQLServerInfo i = new SQLServerInfo(credentials);
            i.SetInstance(instance);
            i.Query();
            var info = i.GetResults();

            SQLDatabase db = new SQLDatabase(credentials);
            db.EnableHasAccessFilter();
            db.Query();
            SQLDatabasePriv priv = new SQLDatabasePriv(credentials);
            priv.SetInstance(instance);
            priv.SetPermissionNameFilter("CREATE PROCEDURE");
            var dbPrivs = new List<SQLDatabasePriv.DatabasePrivilege>();
            foreach (var d in db.GetResults())
            {
                priv.SetDatabase(d.DatabaseName);
                priv.Query();
                foreach (var pr in priv.GetResults())
                {
                    dbPrivs.Add(pr);
                }
            }     

            List<string> principals = new List<string>();
            SetPrincipalNameFilter(info.Currentlogin);
            base.Query();
            foreach (var s in serverRoles)
            {
                principals.Add(s.PrincipalName);
            }
            principals.Add(info.Currentlogin);
            principals.Add("Public");


            priv.SetPermissionNameFilter("ALTER");
            priv.SetPermissionTypeFilter("SCHEMA");
            foreach (string principal in principals)
            {
                priv.SetPrincipalNameFilter(principal);
                foreach (var dbp in dbPrivs)
                {
                    priv.SetDatabase(dbp.DatabaseName);
                    priv.Query();
                    foreach (var asPriv in priv.GetResults())
                    {
                        if (dbp.PrincipalName.Contains(principal))
                        {
                            var s = new XpDirTree
                            {
                                ComputerName = computerName,
                                Instance = instance,
                                Vulnerability = "Permission - CREATE PROCEDURE",
                                Description = "The login has privileges to create stored procedures in one or more databases.  This may allow the login to escalate privileges within the database.",
                                Remediation = "If the permission is not required remove it.  Permissions are granted with a command like: GRANT CREATE PROCEDURE TO user, and can be removed with a command like: REVOKE CREATE PROCEDURE TO user",
                                Severity = "Medium",
                                IsVulnerable = "Yes",
                                IsExploitable = "Unknown",
                                Exploited = "No",
                                ExploitCmd = "No exploit is currently available that will allow the current user to become a sysadmin.",
                                Reference = @"https://msdn.microsoft.com/en-us/library/ms187926.aspx?f=255&MSPPError=-2147217396",
                                Details = string.Format("The {0} principal has EXECUTE privileges on the {1} procedure in the master database.", principal, xp)
                            };
                            spExecuteAs.Add(s);
                        }
                    }
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
