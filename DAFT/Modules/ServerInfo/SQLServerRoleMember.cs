using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DAFT.Modules
{
    class SQLServerRoleMember : Module
    {
        const string QUERY1_2 = @"
role_principal_id as [RolePrincipalId],
SUSER_NAME(role_principal_id) as [RolePrincipalName],
member_principal_id as [PrincipalId],
SUSER_NAME(member_principal_id) as [PrincipalName]
FROM sys.server_role_members WHERE 1=1
";
        [DataContract]
        public struct ServerRoleMember
        {
            [DataMember] public string ComputerName;
            [DataMember] public string Instance;
            [DataMember] public int RolePrincipalId;
            [DataMember] public string RolePrincipalName;
            [DataMember] public int PrincipalId;
            [DataMember] public string PrincipalName;
        }

        protected List<ServerRoleMember> serverRoles;

        private string roleOwnerFilter = string.Empty;
        private string principalNameFilter = string.Empty;

        internal SQLServerRoleMember(Credentials credentials) : base(credentials)
        {
            serverRoles = new List<ServerRoleMember>();
        }

        internal void SetRoleOwnerFilter(string roleOwnerFilter)
        {
            this.roleOwnerFilter =  string.Format(" AND suser_name(role_principal_id) LIKE \'{0}\'", roleOwnerFilter);
        }

        internal void SetPrincipalNameFilter(string principalNameFilter)
        {
            this.principalNameFilter = string.Format(" AND suser_name(member_principal_id) LIKE \'{0}\'", principalNameFilter);
        }

        internal override bool Query()
        {
            using (SQLConnection sql = new SQLConnection(instance))
            {
                sql.BuildConnectionString(credentials);
                if (!sql.Connect())
                    return false;

                string query1_1 = string.Format(
                    "SELECT  \'{0}\' as [ComputerName],\n" +
                    "\'{1}\' as [Instance],\n",
                    computerName, instance);

                StringBuilder sb = new StringBuilder();
                sb.Append(query1_1);
                sb.Append(QUERY1_2);
                if (!string.IsNullOrEmpty(principalNameFilter)) { sb.Append(principalNameFilter); }
                if (!string.IsNullOrEmpty(roleOwnerFilter)) { sb.Append(roleOwnerFilter); }
#if DEBUG
                Console.WriteLine(sb.ToString());
#endif
                //table = sql.Query(sb.ToString());
                serverRoles = sql.Query<ServerRoleMember>(sb.ToString(), new ServerRoleMember());
            }

            /*
            foreach (DataRow row in table.AsEnumerable())
            {
                try
                {
                    ServerRoleMember sr = new ServerRoleMember
                    {
                        ComputerName = computerName,
                        Instance = instance,
                        RolePrincipalId = (int)row["RolePrincipalId"],
                        RolePrincipalName = (string)row["RolePrincipalName"],
                        PrincipalId = (int)row["PrincipalId"],
                        PrincipalName = (string)row["PrincipalName"]
                    };
#if DEBUG
                    Misc.PrintStruct<ServerRoleMember>(sr);
#endif
                    serverRoles.Add(sr);
                    return true;
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

        internal List<ServerRoleMember> GetResults()
        {
            return serverRoles;
        }
    }
}