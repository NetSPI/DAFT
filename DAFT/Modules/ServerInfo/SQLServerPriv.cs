using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DAFT.Modules
{
    class SQLServerPriv : Module
    {
        const string QUERY1_2 = @"
GRE.name as [GranteeName],
GRO.name as [GrantorName],
PER.class_desc as [PermissionClass],
PER.permission_name as [PermissionName],
PER.state_desc as [PermissionState],
COALESCE(PRC.name, EP.name, N'') as [ObjectName],
COALESCE(PRC.type_desc, EP.type_desc, N'') as [ObjectType]
FROM [sys].[server_permissions] as PER
INNER JOIN sys.server_principals as GRO
ON PER.grantor_principal_id = GRO.principal_id
INNER JOIN sys.server_principals as GRE
ON PER.grantee_principal_id = GRE.principal_id
LEFT JOIN sys.server_principals as PRC
ON PER.class = 101 AND PER.major_id = PRC.principal_id
LEFT JOIN sys.endpoints AS EP
ON PER.class = 105 AND PER.major_id = EP.endpoint_id
";

        const string QUERY1_3 = @"
ORDER BY GranteeName,PermissionName;";

        [DataContract]
        public struct ServerPrivilege
        {
            [DataMember] public string ComputerName;
            [DataMember] public string Instance;
            [DataMember] public string GranteeName;
            [DataMember] public string GrantorName;
            [DataMember] public string PermissionClass;
            [DataMember] public string PermissionName;
            [DataMember] public string PermissionState;
            [DataMember] public string ObjectName;
            [DataMember] public string ObjectType;
        }

        protected List<ServerPrivilege> serverPrivileges;

        private string permissionNameFilter = string.Empty;

        internal SQLServerPriv(Credentials credentials) : base(credentials)
        {
            serverPrivileges = new List<ServerPrivilege>();
        }

        internal void SetPermissionNameFilter(string permissionNameFilter)
        {
            this.permissionNameFilter =  string.Format(" AND PER.permission_name LIKE \'{0}\'", permissionNameFilter);
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
                if (!string.IsNullOrEmpty(permissionNameFilter)) { sb.Append(permissionNameFilter); }
                sb.Append(QUERY1_3);
#if DEBUG
                Console.WriteLine(sb.ToString());
#endif
                //table = sql.Query(sb.ToString());
                serverPrivileges = sql.Query<ServerPrivilege>(sb.ToString(), new ServerPrivilege());
            }
            /*
            foreach (DataRow row in table.AsEnumerable())
            {
                try
                {
                    ServerPrivilege dp = new ServerPrivilege
                    {
                        ComputerName = computerName,
                        Instance = instance,
                        GranteeName = (string)row["GranteeName"],
                        GrantorName = (string)row["GrantorName"],
                        PermissionClass = (string)row["PermissionClass"],
                        PermissionName = (string)row["PermissionName"],
                        PermissionState = (string)row["PermissionState"],
                        ObjectName = (string)row["ObjectName"],
                        ObjectType = (string)row["ObjectType"]
                    };
#if DEBUG
                    Misc.PrintStruct<ServerPrivilege>(dp);
#endif
                    serverPrivileges.Add(dp);
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

        internal List<ServerPrivilege> GetResults()
        {
            return serverPrivileges;
        }
    }
}
