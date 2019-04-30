using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DAFT.Modules
{
    class SQLServerCredential : Module
    {    
        const string QUERY1_2 = @"
credential_id,
name as [CredentialName],
credential_identity,
create_date,
modify_date,
target_type,
target_id
FROM [master].[sys].[credentials]";

        [DataContract]
        public struct ServerCredential
        {
            [DataMember] public string ComputerName;
            [DataMember] public string Instance;
            [DataMember] public int credential_id;
            [DataMember] public string CredentialName;
            [DataMember] public string credential_identity;
            [DataMember] public DateTime create_date;
            [DataMember] public DateTime modify_date;
            [DataMember] public string target_type;
            [DataMember] public int target_id;
        }

        private List<ServerCredential> serverCredentials;

        private string credentialFilter = string.Empty;

        internal SQLServerCredential(Credentials credentials) : base(credentials)
        {
            serverCredentials = new List<ServerCredential>();
        }

        internal void SetCredentialNameFilter(string credentialFilter)
        {
            this.credentialFilter = string.Format(" WHERE name like \'{0}\'", credentialFilter);
        }

        internal override bool Query()
        {
            bool isSysAdmin = false;
            using (SQLConnection sql = new SQLConnection(instance))
            {
                sql.BuildConnectionString(credentials);
                if (!sql.Connect())
                    return false;

                isSysAdmin = SQLSysadminCheck.Query(instance, computerName, credentials);

                StringBuilder sb = new StringBuilder();
                sb.Append(string.Format("USE master;\nSELECT  \'{0}\' as [ComputerName],\n\'{1}\' as [Instance],", computerName, instance));
                sb.Append(QUERY1_2);
                if (!string.IsNullOrEmpty(credentialFilter)) { sb.Append(credentialFilter); }
#if DEBUG
                Console.WriteLine(sb.ToString());
#endif
                //table = sql.Query(sb.ToString());
                serverCredentials = sql.Query<ServerCredential>(sb.ToString(), new ServerCredential());
            }

            /*
            foreach (DataRow row in table.AsEnumerable())
            {
                try
                {
                    ServerCredential sc = new ServerCredential
                    {
                        ComputerName = (string)row["ComputerName"],
                        Instance = (string)row["Instance"],
                        credential_id = (int)row["credential_id"],
                        CredentialName = (string)row["CredentialName"],
                        credential_identity = (string)row["credential_identity"],
                        create_date = (DateTime)row["create_date"],
                        modify_date = (DateTime)row["modify_date"],
                        target_type = (string)row["target_type"],
                        target_id = (int)row["target_id"]
                    };
#if DEBUG
                    Misc.PrintStruct<ServerCredential>(sc);
#endif
                    serverCredentials.Add(sc);
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
            return false;
        }

        internal List<ServerCredential> GetResults()
        {
            return serverCredentials;
        }
    }
}
