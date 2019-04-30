using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DAFT.Modules
{
    class SQLDatabasePriv : Module
    {
        const string QUERY1_2 = @"
rp.name as [PrincipalName],
rp.type_desc as [PrincipalType],
pm.class_desc as [PermissionType],
pm.permission_name as [PermissionName],
pm.state_desc as [StateDescription],
ObjectType = CASE
WHEN obj.type_desc IS NULL
OR obj.type_desc = 'SYSTEM_TABLE' THEN
pm.class_desc
ELSE
obj.type_desc
END,
[ObjectName] = Isnull(ss.name, Object_name(pm.major_id))
";
        [DataContract]
        public struct DatabasePrivilege
        {
            [DataMember] public string ComputerName;
            [DataMember] public string Instance;
            [DataMember] public string DatabaseName;
            [DataMember] public string PrincipalName;
            [DataMember] public string PrincipalType;
            [DataMember] public string PermissionType;
            [DataMember] public string PermissionName;
            [DataMember] public string StateDescription;
            [DataMember] public string ObjectType;
            [DataMember] public string ObjectName;
        }

        protected List<DatabasePrivilege> databasePrivileges;

        private string permissionNameFilter = string.Empty;
        private string principalNameFilter = string.Empty;
        private string permissionTypeFilter = string.Empty;

        internal SQLDatabasePriv(Credentials credentials) : base(credentials)
        {
            databasePrivileges = new List<DatabasePrivilege>();
        }

        internal void SetPermissionNameFilter(string permissionNameFilter)
        {
            this.permissionNameFilter =  string.Format(" AND pm.permission_name LIKE \'%{0}%\'", permissionNameFilter);
        }

        internal void SetPrincipalNameFilter(string principalNameFilter)
        {
            this.principalNameFilter = string.Format(" AND rp.name LIKE \'{0}\'", principalNameFilter);
        }

        internal void SetPermissionTypeFilter(string permissionTypeFilter)
        {
            this.permissionTypeFilter = string.Format(" AND pm.class_desc LIKE \'{0}\'", permissionTypeFilter);
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
                    "FROM {0}.sys.database_principals rp \n" +
                    "INNER JOIN {0}.sys.database_permissions pm \n" +
                    "ON pm.grantee_principal_id = rp.principal_id \n" +
                    "LEFT JOIN {0}.sys.schemas ss \n" +
                    "ON pm.major_id = ss.schema_id \n" +
                    "LEFT JOIN {0}.sys.objects obj \n" +
                    "ON pm.[major_id] = obj.[object_id] WHERE 1 = 1",
                    database);

                StringBuilder sb = new StringBuilder();
                sb.Append(query1_1);
                sb.Append(QUERY1_2);
                sb.Append(query1_3);
                if (!string.IsNullOrEmpty(permissionNameFilter)) { sb.Append(permissionNameFilter); }
                if (!string.IsNullOrEmpty(principalNameFilter)) { sb.Append(principalNameFilter); }
                if (!string.IsNullOrEmpty(permissionTypeFilter)) { sb.Append(permissionTypeFilter); }
#if DEBUG
                Console.WriteLine(sb.ToString());
#endif
                databasePrivileges = sql.Query<DatabasePrivilege>(sb.ToString(), new DatabasePrivilege());
            }
            return true;
        }

        internal List<DatabasePrivilege> GetResults()
        {
            return databasePrivileges;
        }
    }
}