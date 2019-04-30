using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DAFT.Modules
{
    sealed class SQLSession : Module
    {
        private string principalName = string.Empty;

        [DataContract]
        public struct Sessions
        {
            [DataMember] public string ComputerName;
            [DataMember] public string Instance;
            [DataMember] public byte[] PrincipalSid;
            [DataMember] public string PrincipalName;
            [DataMember] public string OriginalPrincipalName;
            [DataMember] public object SessionId;
            [DataMember] public DateTime SessionStartTime;
            [DataMember] public DateTime SessionLoginTime;
            [DataMember] public object SessionStatus;
        }

        private List<Sessions> sessions;

        internal SQLSession(Credentials credentials) : base(credentials)
        {
            sessions = new List<Sessions>();
        }

        internal void SetPrincipalNameFilter(string principalName)
        {
            this.principalName = string.Format(" WHERE login_name like \'{0}\'", principalName);
        }

        internal override bool Query()
        {
            string query1_1 = string.Format(
                "USE master;\n" +
                "SELECT  \'{0}\' as [ComputerName],\n" +
                "\'{1}\' as [Instance],\n" +
                "security_id as [PrincipalSid],\n" +
                "login_name as [PrincipalName],\n" +
                "original_login_name as [OriginalPrincipalName],\n" +
                "session_id as [SessionId],\n" +
                "last_request_start_time as [SessionStartTime],\n" +
                "login_time as [SessionLoginTime],\n" +
                "status as [SessionStatus]\n" +
                "FROM[sys].[dm_exec_sessions]\n" +
                "{2}\n"+
                "ORDER BY status\n"
                , computerName, instance, principalName);

            using (SQLConnection sql = new SQLConnection(instance))
            {
                sql.BuildConnectionString(credentials);
                if (!sql.Connect())
                {
                    return false;
                }
#if DEBUG
                Console.WriteLine(query1_1);
#endif
                //table = sql.Query(query1_1);
                sessions = sql.Query<Sessions>(query1_1, new Sessions());
            }

            /*
            try
            {
                foreach (var row in table.AsEnumerable())
                {
                    Sessions s = new Sessions
                    {
                        ComputerName = (string)row["ComputerName"],
                        Instance = (string)row["Instance"],
                        PrincipalSid = (byte[])row["PrincipalSid"],
                        PrincipalName = (string)row["PrincipalName"],
                        OriginalPrincipalName = (string)row["OriginalPrincipalName"],
                        SessionId = row["SessionId"],
                        SessionStartTime = (DateTime)row["SessionStartTime"],
                        SessionLoginTime = (DateTime)row["SessionLoginTime"],
                        SessionStatus = row["SessionStatus"],
                    };
#if DEBUG
                    Misc.PrintStruct<Sessions>(s);
#endif
                    sessions.Add(s);
                }
            }
            catch (Exception ex)
            {
                if (ex is ArgumentNullException)
                    Console.WriteLine("Empty Response");
                else
                    Console.WriteLine(ex);
                return false;
            }
            */
            return true;
        }

        internal List<Sessions> GetResults()
        {
            return sessions;
        }
    }
}
