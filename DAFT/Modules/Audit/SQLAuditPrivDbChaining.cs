using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DAFT.Modules
{
    class SQLAuditPrivDbChaining : SQLDatabase
    {
        [DataContract]
        public struct Ownership
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

        private List<Ownership> ownerships;

        internal SQLAuditPrivDbChaining(Credentials credentials) : base(credentials)
        {
            ownerships = new List<Ownership>();
        }

        internal override bool Query()
        {
            SQLServerConfiguration config = new SQLServerConfiguration(credentials);
            config.SetInstance(instance);
            config.Query();
            foreach (var c in config.GetResults())
            {
                if (c.Name.Contains("chain"))
                {
                    var s = new Ownership
                    {
                        ComputerName = computerName,
                        Instance = instance,
                        Vulnerability = "Excessive Privilege - Database Ownership Chaining - Server Instance",
                        Description = "Ownership chaining was found enabled at the server level.  Enabling ownership chaining can lead to unauthorized access to database resources.",
                        Remediation = "Configured the affected database so the 'is_db_chaining_on' flag is set to 'false'.  A query similar to 'ALTER DATABASE Database1 SET DB_CHAINING ON' is used enable chaining.  A query similar to 'ALTER DATABASE Database1 SET DB_CHAINING OFF;' can be used to disable chaining.",
                        Severity = "Low",
                        IsVulnerable = "Yes",
                        IsExploitable = "No",
                        Exploited = "No",
                        ExploitCmd = "There is not exploit available at this time.",
                        Reference = @"https://technet.microsoft.com/en-us/library/ms188676(v=sql.105).aspx, https://msdn.microsoft.com/en-us/library/bb669059(v=vs.110).aspx",
                        Details = string.Format("The server instance was found configured with ownership chaining enabled.", instance)
                    };
                    ownerships.Add(s);
                }
            }
        
            base.Query();

            foreach (var d in databases)
            {
                if (d.is_db_chaining_on)
                {
                    var s = new Ownership
                    {
                        ComputerName = computerName,
                        Instance = instance,
                        Vulnerability = "Excessive Privilege - Database Ownership Chaining - Database",
                        Description = "Ownership chaining was found enabled at the database level.  Enabling ownership chaining can lead to unauthorized access to database resources.",
                        Remediation = "Configured the affected database so the 'is_db_chaining_on' flag is set to 'false'.  A query similar to 'ALTER DATABASE Database1 SET DB_CHAINING ON' is used enable chaining.  A query similar to 'ALTER DATABASE Database1 SET DB_CHAINING OFF;' can be used to disable chaining.",
                        Severity = "Low",
                        IsVulnerable = "Yes",
                        IsExploitable = "No",
                        Exploited = "No",
                        ExploitCmd = "There is not exploit available at this time.",
                        Reference = @"https://technet.microsoft.com/en-us/library/ms188676(v=sql.105).aspx, https://msdn.microsoft.com/en-us/library/bb669059(v=vs.110).aspx",
                        Details = string.Format("The database {0} was found configured with ownership chaining enabled.", d.DatabaseName)
                    };
                    ownerships.Add(s);
                }
            }
            return true;
        }

        internal new List<Ownership> GetResults()
        {
            return ownerships;
        }
    }
}
