using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace DAFT.Modules
{
    class SQLDatabase : Module
    {
        const string query1_2 = @"
a.database_id as [DatabaseId],
a.name as [DatabaseName],
SUSER_SNAME(a.owner_sid) as [DatabaseOwner],
IS_SRVROLEMEMBER('sysadmin', SUSER_SNAME(a.owner_sid)) as [OwnerIsSysadmin],
a.is_trustworthy_on,
a.is_db_chaining_on,";

        const string query1_3 = @"
a.is_broker_enabled,
a.is_encrypted,
a.is_read_only,";

        const string query1_4 = @"
a.create_date,
a.recovery_model_desc,
b.filename as [FileName],
(SELECT CAST(SUM(size) * 8. / 1024 AS DECIMAL(8,2))
from sys.master_files where name like a.name) as [DbSizeMb],
HAS_DBACCESS(a.name) as [has_dbaccess]
FROM [sys].[databases] a
INNER JOIN [sys].[sysdatabases] b ON a.database_id = b.dbid WHERE 1=1";

        [DataContract]
        public struct Database
        {
            [DataMember] public string ComputerName;
            [DataMember] public string Instance;
            [DataMember] public int DatabaseId;
            [DataMember] public string DatabaseName;
            [DataMember] public string DatabaseOwner;
            [DataMember] public object OwnerIsSysadmin;
            [DataMember] public bool is_trustworthy_on;
            [DataMember] public bool is_db_chaining_on;
            [DataMember] public bool is_broker_enabled;
            [DataMember] public bool is_encrypted;
            [DataMember] public bool is_read_only;
            [DataMember] public DateTime create_date;
            [DataMember] public string recovery_model_desc;
            [DataMember] public string FileName;
            [DataMember] public decimal DbSizeMb;
            [DataMember] public int has_dbaccess;
        }

        protected List<Database> databases;

        private string databaseFilter = string.Empty;
        private string noDefaultsFilter = string.Empty;
        private string hasAccessFilter = string.Empty;
        private string sysAdminFilter = string.Empty;

        internal SQLDatabase(Credentials credentials) : base(credentials)
        {
            databases = new List<Database>();
        }

        internal void SetDatabaseFilter(string databaseFilter)
        {
            this.databaseFilter = string.Format(" and a.name like \'{0}\'", databaseFilter);
        }

        internal void EnableNoDefaultsFilter()
        {
            noDefaultsFilter = @" AND a.name NOT in ('master','tempdb','msdb','model')";
        }

        internal void DisableNoDefaultsFilter()
        {
            noDefaultsFilter = string.Empty;
        }

        internal void EnableHasAccessFilter()
        {
            hasAccessFilter = @" AND HAS_DBACCESS(a.name)=1";
        }

        internal void DisableHasAccessFilter()
        {
            hasAccessFilter = string.Empty;
        }

        internal void EnableSysAdminFilter()
        {
            sysAdminFilter = @" AND IS_SRVROLEMEMBER('sysadmin',SUSER_SNAME(a.owner_sid))=1";
        }

        internal void DisableSysAdminFilter()
        {
            sysAdminFilter = string.Empty;
        }

        internal override bool Query()
        {
            string query1_1 = string.Format(
                "SELECT  \'{0}\' as [ComputerName],\n" +
                "\'{1}\' as [Instance],", 
                computerName, instance);

            int versionShort = 0;
            using (SQLConnection sql = new SQLConnection(instance))
            {
                sql.BuildConnectionString(credentials);
                if (!sql.Connect())
                    return false;

                SQLServerInfo serverInfo = new SQLServerInfo(credentials);
                serverInfo.SetInstance(instance);
                if (!serverInfo.Query())
                    return false;
                SQLServerInfo.Details details = serverInfo.GetResults();
               
                int.TryParse(details.SQLServerMajorVersion.Split('.').First(), out versionShort);

                StringBuilder sb = new StringBuilder();
                sb.Append(query1_1);
                sb.Append(query1_2);
                if (versionShort > 10) { sb.Append(query1_3); }
                sb.Append(query1_4);

                if (!string.IsNullOrEmpty(databaseFilter))
                    sb.Append(databaseFilter);
                if (!string.IsNullOrEmpty(noDefaultsFilter))
                    sb.Append(noDefaultsFilter);
                if (!string.IsNullOrEmpty(hasAccessFilter))
                    sb.Append(hasAccessFilter);
                if (!string.IsNullOrEmpty(sysAdminFilter))
                    sb.Append(sysAdminFilter);
#if DEBUG
                Console.WriteLine(sb.ToString());
#endif
                databases = sql.Query<Database>(sb.ToString(), new Database());
            }
            return true;
        }

        internal List<Database> GetResults()
        {
            return databases;
        }
    }
}
