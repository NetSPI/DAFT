using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DAFT.Modules
{
    class SQLServerRole : Module
    {
        const string QUERY1_2 = @"
principal_id as [RolePrincipalId],
sid as [RolePrincipalSid],
name as [RolePrincipalName],
type_desc as [RolePrincipalType],
owning_principal_id as [OwnerPrincipalId],
suser_name(owning_principal_id) as [OwnerPrincipalName],
is_disabled,
is_fixed_role,
create_date,
modify_Date,
default_database_name
FROM [master].[sys].[server_principals] WHERE type like 'R'";

        [DataContract]
        public struct ServerRole
        {
            [DataMember] public string ComputerName;
            [DataMember] public string Instance;
            [DataMember] public int RolePrincipalId;
            [DataMember] public byte[] RolePrincipalSid;
            [DataMember] public string RolePrincipalName;
            [DataMember] public string RolePrincipalType;
            [DataMember] public int OwnerPrincipalId;
            [DataMember] public string OwnerPrincipalName;
            [DataMember] public bool is_disabled;
            [DataMember] public bool is_fixed_role;
            [DataMember] public DateTime create_date;
            [DataMember] public DateTime modify_Date;
            [DataMember] public object default_database_name;
        }

        private List<ServerRole> serverRoles;

        private string roleOwnerFilter = string.Empty;
        private string principalNameFilter = string.Empty;

        internal SQLServerRole(Credentials credentials) : base(credentials)
        {
            serverRoles = new List<ServerRole>();
        }

        internal void SetRoleOwnerFilter(string roleOwnerFilter)
        {
            this.roleOwnerFilter =  string.Format(" AND suser_name(owning_principal_id) LIKE \'{0}\'", roleOwnerFilter);
        }

        internal void SetRolePrincipalNameFilter(string principalNameFilter)
        {
            this.principalNameFilter = string.Format(" AND name LIKE \'{0}\'", principalNameFilter);
        }

        internal override bool Query()
        {
            using (SQLConnection sql = new SQLConnection(instance))
            {
                sql.BuildConnectionString(credentials);
                if (!sql.Connect())
                    return false;

                string query1_1 = string.Format(
                    "SELECT \'{0}\' as [ComputerName],\n" +
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
                serverRoles = sql.Query<ServerRole>(sb.ToString(), new ServerRole());
            }

            /*
            foreach (DataRow row in table.AsEnumerable())
            {
                try
                {
                    ServerRole sr = new ServerRole
                    {
                        ComputerName = computerName,
                        Instance = instance,
                        RolePrincipalId = (int)row["RolePrincipalId"],
                        RolePrincipalSid = (byte[])row["RolePrincipalSid"],
                        RolePrincipalName = (string)row["RolePrincipalName"],
                        RolePrincipalType = (string)row["RolePrincipalType"],
                        OwnerPrincipalId = (int)row["OwnerPrincipalId"],
                        OwnerPrincipalName = (string)row["OwnerPrincipalName"],
                        is_disabled = (bool)row["is_disabled"],
                        is_fixed_role = (bool)row["is_fixed_role"],
                        create_date = (DateTime)row["create_date"],
                        modify_Date = (DateTime)row["modify_Date"],
                        default_database_name = (object)row["default_database_name"]
                    };
#if DEBUG
                    Misc.PrintStruct<ServerRole>(sr);
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

        internal List<ServerRole> GetResults()
        {
            return serverRoles;
        }
    }
}
