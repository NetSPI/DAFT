using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DAFT.Modules
{
    class SQLDatabaseRole : Module
    {
        const string QUERY1_2 = @"
principal_id as [RolePrincipalId],
sid as [RolePrincipalSid],
name as [RolePrincipalName],
type_desc as [RolePrincipalType],
owning_principal_id as [OwnerPrincipalId],
suser_name(owning_principal_id) as [OwnerPrincipalName],
is_fixed_role,
create_date,
modify_Date,
default_schema_name
";
        [DataContract]
        public struct DatabaseRole
        {
            [DataMember] public string ComputerName;
            [DataMember] public string Instance;
            [DataMember] public string DatabaseName;
            [DataMember] public int RolePrincipalId;
            [DataMember] public byte[] RolePrincipalSid;
            [DataMember] public string RolePrincipalName;
            [DataMember] public string RolePrincipalType;
            [DataMember] public int OwnerPrincipalId;
            [DataMember] public string OwnerPrincipalName;
            [DataMember] public bool is_fixed_role;
            [DataMember] public DateTime create_date;
            [DataMember] public DateTime modify_Date;
            [DataMember] public object default_schema_name;
        }

        private List<DatabaseRole> databaseRoles;

        private string roleOwnerFilter = string.Empty;
        private string rolePrincipalNameFilter = string.Empty;

        internal SQLDatabaseRole(Credentials credentials) : base(credentials)
        {
            databaseRoles = new List<DatabaseRole>();
        }

        internal void SetRoleOwnerFilter(string roleOwnerFilter)
        {
            this.roleOwnerFilter =  string.Format(" AND suser_name(owning_principal_id) LIKE \'{0}\'", roleOwnerFilter);
        }

        internal void SetRolePrincipalNameFilter(string rolePrincipalNameFilter)
        {
            this.rolePrincipalNameFilter = string.Format(" AND name LIKE \'{0}\'", rolePrincipalNameFilter);
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
                    "SELECT  \'{1}\' as [ComputerName],\n" +
                    "\'{2}\' as [Instance],\n" +
                    "\'{0}\' as [DatabaseName],",
                    database, computerName, instance);

                string query1_3 = string.Format(
                    "FROM [{0}].[sys].[database_principals]\n" +
                    "WHERE type like \'R\'",
                    database);

                StringBuilder sb = new StringBuilder();
                sb.Append(query1_1);
                sb.Append(QUERY1_2);
                sb.Append(query1_3);
                if (!string.IsNullOrEmpty(rolePrincipalNameFilter)) { sb.Append(rolePrincipalNameFilter); }
                if (!string.IsNullOrEmpty(roleOwnerFilter)) { sb.Append(roleOwnerFilter); }
#if DEBUG
                Console.WriteLine(sb.ToString());
#endif
                //table = sql.Query(sb.ToString());
                databaseRoles = sql.Query<DatabaseRole>(sb.ToString(), new DatabaseRole());
            }
            /*
            foreach (DataRow row in table.AsEnumerable())
            {
                try
                {
                    DatabaseRole dr = new DatabaseRole
                    {
                        ComputerName = computerName,
                        Instance = instance,
                        DatabaseName = database,
                        RolePrincipalId = (int)row["RolePrincipalId"],
                        RolePrincipalSid = (byte[])row["RolePrincipalSid"],
                        RolePrincipalName = (string)row["RolePrincipalName"],
                        RolePrincipalType = (string)row["RolePrincipalType"],
                        OwnerPrincipalId = (int)row["OwnerPrincipalId"],
                        OwnerPrincipalName = (string)row["OwnerPrincipalName"],
                        is_fixed_role = (bool)row["is_fixed_role"],
                        create_date = (DateTime)row["create_date"],
                        modify_Date = (DateTime)row["modify_Date"],
                        default_schema_name = (object)row["default_schema_name"]
                    };
#if DEBUG
                    Misc.PrintStruct<DatabaseRole>(dr);
#endif
                    databaseRoles.Add(dr);
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

        internal List<DatabaseRole> GetResults()
        {
            return databaseRoles;
        }
    }
}