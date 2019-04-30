using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;

namespace DAFT.Modules
{
    class SQLAuditPrivImpersonateLogin : SQLServerPriv
    {
        [DataContract]
        public struct Impersonate
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

        private List<Impersonate> impersonates;
        private string role = string.Empty;

        internal SQLAuditPrivImpersonateLogin(Credentials credentials) : base(credentials)
        {
            impersonates = new List<Impersonate>();
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

            SetPermissionNameFilter("IMPERSONATE");
            base.Query();

            using (SQLConnection sql = new SQLConnection())
            {
                sql.BuildConnectionString(credentials);
                sql.Connect();
                foreach (var j in serverPrivileges)
                {
                    string query = string.Format("SELECT IS_SRVROLEMEMBER(\'sysadmin\', \'{0}\') as Status", j.ObjectName);
                    foreach (var r in sql.Query(query).AsEnumerable())
                    {
                        if (!(r["Status"] is DBNull) && 1 == (int)r["Status"])
                        {
                            var s = new Impersonate
                            {
                                ComputerName = computerName,
                                Instance = instance,
                                Vulnerability = "Excessive Privilege - Impersonate Login",
                                Description = "The current SQL Server login can impersonate other logins.  This may allow an authenticated login to gain additional privileges.",
                                Remediation = "Consider using an alterative to impersonation such as signed stored procedures. Impersonation is enabled using a command like: GRANT IMPERSONATE ON Login::sa to [user]. It can be removed using a command like: REVOKE IMPERSONATE ON Login::sa to [user]",
                                Severity = "High",
                                IsVulnerable = "Yes",
                                IsExploitable = "Unknown",
                                Exploited = "No",
                                ExploitCmd = "",
                                Reference = @"https://msdn.microsoft.com/en-us/library/ms181362.aspx",
                                Details = string.Format("{0} can impersonate the {1} SYSADMIN login. This test was ran with the {2} login.", j.GranteeName, j.ObjectName, info.Currentlogin)
                            };
                            impersonates.Add(s);
                        }
                    }
                }
            }
            return true;
        }

        internal new List<Impersonate> GetResults()
        {
            return impersonates;
        }
    }
}
