using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DAFT.Modules
{
    class SQLDatabaseRoleMember : Module
    {       
        const string QUERY1_2 = @"
role_principal_id as [RolePrincipalId],
USER_NAME(role_principal_id) as [RolePrincipalName],
member_principal_id as [PrincipalId],
USER_NAME(member_principal_id) as [PrincipalName]
";
        [DataContract]
        public struct UserRole
        {
            [DataMember] public string ComputerName;
            [DataMember] public string Instance;
            [DataMember] public string DatabaseName;
            [DataMember] public int RolePrincipalId;
            [DataMember] public string RolePrincipalName;
            [DataMember] public int PrincipalId;
            [DataMember] public object PrincipalName;
        }

        private List<UserRole> userRoles;

        private string principalNameFilter = string.Empty;
        private string rolePrincipalNameFilter = string.Empty;

        internal SQLDatabaseRoleMember(Credentials credentials) : base(credentials)
        {
            userRoles = new List<UserRole>();
        }

        internal void SetPrincipalNameFilter(string principalNameFilter)
        {
            this.principalNameFilter =  string.Format(" AND USER_NAME(member_principal_id) LIKE \'{0}\'", principalNameFilter);
        }

        internal void SetRolePrincipalNameFilter(string rolePrincipalNameFilter)
        {
            this.rolePrincipalNameFilter = string.Format(" AND USER_NAME(role_principal_id) LIKE \'{0}\'", rolePrincipalNameFilter);
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

                string query1_3 = string.Format(
                    "FROM [{0}].[sys].[database_role_members]\n" +
                    "WHERE 1 = 1",
                    database);

                StringBuilder sb = new StringBuilder();
                sb.Append(query1_1);
                sb.Append(QUERY1_2);
                sb.Append(query1_3);
                if (!string.IsNullOrEmpty(rolePrincipalNameFilter)) { sb.Append(rolePrincipalNameFilter); }
                if (!string.IsNullOrEmpty(principalNameFilter)) { sb.Append(principalNameFilter); }
#if DEBUG
                Console.WriteLine(sb.ToString());
#endif
                //table = sql.Query(sb.ToString());
                userRoles = sql.Query<UserRole>(sb.ToString(), new UserRole());
            }
            /*
            foreach (DataRow row in table.AsEnumerable())
            {
                try
                {
                    UserRole ur = new UserRole
                    {
                        ComputerName = computerName,
                        Instance = instance,
                        DatabaseName = database,
                        RolePrincipalId = (int)row["RolePrincipalId"],
                        RolePrincipalName = (string)row["RolePrincipalName"],
                        PrincipalId = (int)row["PrincipalId"],
                        PrincipalName = (string)row["PrincipalName"]
                    };
#if DEBUG
                    Misc.PrintStruct<UserRole>(ur);
#endif
                    userRoles.Add(ur);
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

        internal List<UserRole> GetResults()
        {
            return userRoles;
        }
    }
}
