using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;

namespace DAFT.Modules
{
    class SQLFuzzDomainAccount : Module
    {
        [DataContract]
        public struct Fuzz
        {
            [DataMember] public string ComputerName;
            [DataMember] public string Instance;
            [DataMember] public string SID;
            [DataMember] public int RID;
            [DataMember] public string DomainAccount;
        }

        private List<Fuzz> fuzzed;

        private string domainName = string.Empty;
        private string domainGroup = "Domain Users";

        private int start = 0;
        private int end = 0;

        internal SQLFuzzDomainAccount(Credentials credentials) : base(credentials)
        {
            fuzzed = new List<Fuzz>();
        }

        internal void SetStartId(int start)
        {
            this.start = start;
        }

        internal void SetEndId(int end)
        {
            this.end = end;
        }

        internal override bool Query()
        {
            using (SQLConnection sql = new SQLConnection(instance))
            {
                sql.BuildConnectionString(credentials);
                if (!sql.Connect())
                    return false;

                SQLServerInfo info = new SQLServerInfo(credentials);
                info.SetInstance(instance);
                info.Query();
                SQLServerInfo.Details d = info.GetResults();

                domainName = d.DomainName;

                string query1_1 = string.Format("SELECT SUSER_SID(\'{0}\\{1}\') as DomainGroupSid", domainName, domainGroup);
#if DEBUG
                Console.WriteLine(query1_1);
#endif
                DataTable table = sql.Query(query1_1);
                byte[] sidBytes = (byte[])table.AsEnumerable().First()["DomainGroupSid"];
                string strSid = BitConverter.ToString(sidBytes).Replace("-", "").Substring(0, 48);

                Console.WriteLine("Base SID: {0}", strSid);

                for (int i = start; i <= end; i++)
                {
                    string strHexI = i.ToString("x");
                    int nStrHexI = strHexI.Length;
                    string rid = strHexI;
                    if (0 != nStrHexI % 2)
                    {
                        rid = "0" + strHexI;
                    }

                    string[] arrSplit = Split(rid, 2).ToArray();
                    Array.Reverse(arrSplit);
                    rid = string.Join("", arrSplit);
                    rid = rid.PadRight(8, '0');
                    rid = "0x" + strSid + rid;

                    string query2_1 = string.Format("SELECT SUSER_SNAME({0}) as [DomainAccount]", rid);
#if DEBUG
                    Console.WriteLine(query2_1);
#endif
                    table = sql.Query(query2_1);

                    foreach (DataRow row in table.AsEnumerable())
                    {
                        try
                        {
                            if (row["DomainAccount"] is DBNull)
                                continue;

                            Fuzz f = new Fuzz
                            {
                                ComputerName = computerName,
                                Instance = instance,
                                SID = rid,
                                RID = i,
                                DomainAccount = (string)row["DomainAccount"],
                            };
                            fuzzed.Add(f);
                        }
                        catch (Exception ex)
                        {
                            if (ex is ArgumentNullException)
                                continue;
                            else
                                Console.WriteLine(ex.Message);
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        //https://stackoverflow.com/questions/1450774/splitting-a-string-into-chunks-of-a-certain-size
        static IEnumerable<string> Split(string str, int chunkSize)
        {
            return Enumerable.Range(0, str.Length / chunkSize)
                .Select(i => str.Substring(i * chunkSize, chunkSize));
        }

        internal List<Fuzz> GetResults()
        {
            return fuzzed;
        }
    }
}