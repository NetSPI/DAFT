using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DAFT.Modules
{
    class SQLDatabaseUser : Module
    {
        const string QUERY1_2 = @"
a.principal_id as [DatabaseUserId],
a.name as [DatabaseUser],
a.sid as [PrincipalSid],
b.name as [PrincipalName],
a.type_desc as [PrincipalType],
default_schema_name,
a.create_date,
a.is_fixed_role
FROM [sys].[database_principals] a
LEFT JOIN [sys].[server_principals] b
ON a.sid = b.sid WHERE 1=1
";
        [DataContract]
        public struct DatabaseUsers
        {
            [DataMember] public string ComputerName;
            [DataMember] public string Instance;
            [DataMember] public string DatabaseName;
            [DataMember] public int DatabaseUserId;
            [DataMember] public string DatabaseUser;
            [DataMember] public byte[] PrincipalSid;
            [DataMember] public object PrincipalName;
            [DataMember] public string PrincipalType;
            [DataMember] public object default_schema_name;
            [DataMember] public DateTime create_date;
            [DataMember] public bool is_fixed_role;
        }

        private List<DatabaseUsers> databaseUsers;

        private string principalNameFilter = string.Empty;
        private string databaseUserFilter = string.Empty;

        internal SQLDatabaseUser(Credentials credentials) : base(credentials)
        {
            databaseUsers = new List<DatabaseUsers>();
        }

        internal void SetPrincipalNameFilter(string principalNameFilter)
        {
            this.principalNameFilter =  string.Format(" AND b.name LIKE \'{0}\'", principalNameFilter);
        }

        internal void SetDatabaseUserFilter(string databaseUserFilter)
        {
            this.databaseUserFilter = string.Format(" AND a.name LIKE \'{0}\'", databaseUserFilter);
        }

        internal override bool Query()
        {
            using (SQLConnection sql = new SQLConnection(instance))
            {
                sql.BuildConnectionString(credentials);
                if (!sql.Connect())
                    return false;

                string query1_1 = string.Format(
                    "USE {0};\n" +
                    "SELECT \'{1}\' as [ComputerName],\n" +
                    "\'{2}\' as [Instance],\n" +
                    "\'{0}\' as [DatabaseName],",
                    database, computerName, instance);

                StringBuilder sb = new StringBuilder();
                sb.Append(query1_1);
                sb.Append(QUERY1_2);
                if (!string.IsNullOrEmpty(principalNameFilter)) { sb.Append(principalNameFilter); }
                if (!string.IsNullOrEmpty(databaseUserFilter)) { sb.Append(databaseUserFilter); }
#if DEBUG
                Console.WriteLine(sb.ToString());
#endif
                databaseUsers = sql.Query<DatabaseUsers>(sb.ToString(), new DatabaseUsers());
            }

            /*
            foreach (DataRow row in table.AsEnumerable())
            {
                try
                {
                    DatabaseUsers du = new DatabaseUsers
                    {
                        ComputerName = computerName,
                        Instance = instance,
                        DatabaseName = database,
                        DatabaseUserId = (int)row["DatabaseUserId"],
                        DatabaseUser = (string)row["DatabaseUser"],
                        PrincipalSid = (byte[])row["PrincipalSid"],
                        PrincipalName = (object)row["PrincipalName"],
                        PrincipalType = (string)row["PrincipalType"],
                        default_schema_name = (object)row["default_schema_name"],
                        create_date = (DateTime)row["create_date"],
                        is_fixed_role = (bool)row["is_fixed_role"]
                        
                    };
#if DEBUG
                    Misc.PrintStruct<DatabaseUsers>(du);
#endif
                    databaseUsers.Add(du);
                }
                catch (Exception ex)
                {
                    if (ex is ArgumentNullException)
                        Console.WriteLine("Empty Response");
                    else
                        Console.WriteLine(ex.Message);
                    return false;
                }
            }
            */
            return true;
        }

        internal List<DatabaseUsers> GetResults()
        {
            return databaseUsers;
        }
    }
}