using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DAFT.Modules
{
    class SQLServerLogin : Module
    {
        const string QUERY1_2 = @"
principal_id as [PrincipalId],
name as [PrincipalName],
sid as [PrincipalSid],
type_desc as [PrincipalType],
create_date as [CreateDate],
LOGINPROPERTY ( name , 'IsLocked' ) as [IsLocked]
FROM [sys].[server_principals]
WHERE type = 'S' or type = 'U' or type = 'C'
";
        [DataContract]
        public struct ServerLogin
        {
            [DataMember] public string ComputerName;
            [DataMember] public string Instance;
            [DataMember] public int PrincipalId;
            [DataMember] public string PrincipalName;
            [DataMember] public byte[] PrincipalSid;
            [DataMember] public object PrincipalType;
            [DataMember] public DateTime CreateDate;
            [DataMember] public object IsLocked;
        }

        private List<ServerLogin> serverLogins;

        private string nameFilter = string.Empty;

        internal SQLServerLogin(Credentials credentials) : base(credentials)
        {
            serverLogins = new List<ServerLogin>();
        }

        internal void SetPrincipalNameFilter(string nameFilter)
        {
            this.nameFilter = string.Format(" and name like \'{0}\'", nameFilter);
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
                if (!string.IsNullOrEmpty(nameFilter)) { sb.Append(nameFilter); }
#if DEBUG
                Console.WriteLine(sb.ToString());
#endif
                //table = sql.Query(sb.ToString());
                serverLogins = sql.Query<ServerLogin>(sb.ToString(), new ServerLogin());
            }
            /*
            foreach (DataRow row in table.AsEnumerable())
            {
                try
                {
                    ServerLogin sl = new ServerLogin
                    {
                        ComputerName = computerName,
                        Instance = instance,
                        PrincipalId = (int)row["PrincipalId"],
                        PrincipalName = (string)row["PrincipalName"],
                        PrincipalSid = (byte[])row["PrincipalSid"],
                        PrincipalType = (object)row["PrincipalType"],
                        CreateDate = (DateTime)row["CreateDate"],
                        IsLocked = (object)row["IsLocked"]
                    };
#if DEBUG
                    Misc.PrintStruct<ServerLogin>(sl);
#endif
                    serverLogins.Add(sl);
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

        internal List<ServerLogin> GetResults()
        {
            return serverLogins;
        }
    }
}
