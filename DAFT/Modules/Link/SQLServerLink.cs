using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DAFT.Modules
{
    class SQLServerLink : Module
    {
        private const string QUERY1_2 = @"
a.server_id as [DatabaseLinkId],
a.name AS [DatabaseLinkName],
CASE a.Server_id
WHEN 0
THEN 'Local'
ELSE 'Remote'
END AS [DatabaseLinkLocation],
a.product as [Product],
a.provider as [Provider],
a.catalog as [Catalog],
'LocalLogin' = CASE b.uses_self_credential
WHEN 1 THEN 'Uses Self Credentials'
ELSE c.name
END,
b.remote_name AS [RemoteLoginName],
a.is_rpc_out_enabled,
a.is_data_access_enabled,
a.modify_date
FROM [Master].[sys].[Servers] a
LEFT JOIN [Master].[sys].[linked_logins] b
ON a.server_id = b.server_id
LEFT JOIN [Master].[sys].[server_principals] c
ON c.principal_id = b.local_principal_id";

        [DataContract]
        public struct ServerLink
        {
            [DataMember] public string ComputerName;
            [DataMember] public string Instance;
            [DataMember] public int DatabaseLinkId;
            [DataMember] public string DatabaseLinkName;
            [DataMember] public string DatabaseLinkLocation;
            [DataMember] public string Product;
            [DataMember] public string Provider;
            [DataMember] public string Catalog;
            [DataMember] public string LocalLogin;
            [DataMember] public string RemoteLoginName;
            [DataMember] public bool is_rpc_out_enabled;
            [DataMember] public bool is_data_access_enabled;
            [DataMember] public DateTime modify_date;
        }

        protected List<ServerLink> serverLinks;

        private string linkNameFilter = string.Empty;

        internal SQLServerLink(Credentials credentials) : base(credentials)
        {
            serverLinks = new List<ServerLink>();
        }

        internal void SetDatabaseLinkName(string linkNameFilter)
        {
            this.linkNameFilter = string.Format(" WHERE a.name LIKE \'%{0}%\'", linkNameFilter);
        }

        internal override bool Query()
        {
            string query1_1 = string.Format(
                "SELECT  \'{0}\' as [ComputerName],\n" +
                "\'{1}\' as [Instance],",
                computerName, instance
            );

            using (SQLConnection sql = new SQLConnection(instance))
            {
                sql.BuildConnectionString(credentials);
                if (!sql.Connect())
                    return false;

                StringBuilder sb = new StringBuilder();
                sb.Append(query1_1);
                sb.Append(QUERY1_2);
                if (!string.IsNullOrEmpty(linkNameFilter)) { sb.Append(linkNameFilter); }
#if DEBUG
                Console.WriteLine(sb.ToString());
#endif
                serverLinks = sql.Query<ServerLink>(sb.ToString(), new ServerLink());
                Console.WriteLine("Test");
            }
            return true;
        }

        internal List<ServerLink> GetResults()
        {
            return serverLinks;
        }
    }
}
