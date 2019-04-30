using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace DAFT.Modules
{
    class SQLServerPasswordHash : Module
    {
        const string QUERY1_1 = @"
SELECT name as [PrincipalName],
principal_id as [PrincipalId],
type_desc as [PrincipalType],
sid as [PrincipalSid],
create_date as [CreateDate],
default_database_name as [DefaultDatabaseName],
[sys].fn_varbintohexstr(password_hash) as [PasswordHash]
FROM [sys].[sql_logins]
";

        const string QUERY2_1 = @"
SELECT name as [PrincipalName],
createdate as [CreateDate],
dbname as [DefaultDatabaseName],
password as [PasswordHash]
FROM [sysxlogins]
";
        [DataContract]
        public struct Hash
        {
            [DataMember] public string ComputerName;
            [DataMember] public string Instance;
            [DataMember] public string PrincipalName;
            [DataMember] public int PrincipalId;
            [DataMember] public string PrincipalType;
            [DataMember] public byte[] PrincipalSid;
            [DataMember] public DateTime CreateDate;
            [DataMember] public string DefaultDatabaseName;
            [DataMember] public string PasswordHash;
        }
        private List<Hash> hashes;

        internal SQLServerPasswordHash(Credentials credentials) : base(credentials)
        {
            hashes = new List<Hash>();
        }
        
        internal override bool Query()
        {
            using (SQLConnection sql = new SQLConnection(instance))
            {
                sql.BuildConnectionString(credentials);
                if (!sql.Connect())
                    return false;

                if (!SQLSysadminCheck.Query(instance, computerName, credentials))
                {
                    Console.WriteLine("[-] User is not Sysadmin");
                    return false;
                }

                SQLServerInfo i = new SQLServerInfo(credentials);
                i.SetInstance(instance);
                i.Query();
                SQLServerInfo.Details d = i.GetResults();

                int versionShort;
                if (!int.TryParse(d.SQLServerMajorVersion.Split('.').First(), out versionShort))
                {
                    Console.WriteLine("[-] Unable to ascertain SQL Version");
                    Console.WriteLine("[*] It is possible to override this with the --version flag");
                    return false;
                }

                string query = string.Empty;
                if (8 < versionShort)
                    query = QUERY1_1;
                else
                    query = QUERY2_1;

                //table = sql.Query(query);
                hashes = sql.Query<Hash>(query, new Hash());
            }
            return false;
        }

        internal List<Hash> GetResults()
        {
            return hashes;
        }
    }
}
