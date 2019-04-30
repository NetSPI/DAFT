using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Serialization;

namespace DAFT.Modules
{
    class SQLFuzzServerLogin : Module
    {
        [DataContract]
        public struct Fuzz
        {
            [DataMember] public string ComputerName;
            [DataMember] public string Instance;
            [DataMember] public long PrincipalId;
            [DataMember] public string PrincipleName;
            [DataMember] public string PrincipalType;
        }

        private List<Fuzz> fuzzed;

        private int start = 0;
        private int end = 5;

        internal SQLFuzzServerLogin(Credentials credentials) : base(credentials)
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

                string query1_1 = string.Format(
                    "SELECT \'{0}\' as [ComputerName],\n" +
                    "\'{1}\' as [Instance],\n" +
                    "n as [PrincipalId],\n" + 
                    "SUSER_NAME(n) as [PrincipleName]\n" +
                    "FROM(\n" +
                    "SELECT top {2} row_number() over(order by t1.number) as N\n" +
                    "FROM master..spt_values t1\n" +
                    "       cross join master..spt_values t2\n" +
                    ") a\n" +
                    "WHERE SUSER_NAME(n) is not null",
                    computerName, instance, end
                );
#if DEBUG
                Console.WriteLine(query1_1);
#endif
                foreach (var row in sql.Query(query1_1).AsEnumerable())
                {
                    try
                    {
                        Fuzz f = new Fuzz
                        {
                            ComputerName = computerName,
                            Instance = instance,
                            PrincipalId = (long)row["PrincipalId"],
                        };

                        if (!(row["PrincipleName"] is DBNull))
                        {
                            f.PrincipleName = (string)row["PrincipleName"];
                            f.PrincipalType = string.Empty;
                            fuzzed.Add(f);
                        }
                        else
                        {
                            continue;
                        }
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

                Fuzz[] arrFuzz = fuzzed.ToArray();
                for (int i = 0; i < fuzzed.Count; i++)
                {
                    string query2_1 = string.Format("EXEC master..sp_defaultdb \'{0}\', \'NOTAREALDATABASE1234ABCD\'", arrFuzz[i].PrincipleName);
                    try
                    {
                        using (SqlDataReader reader = new SqlCommand(query2_1, sql.GetSQL()).ExecuteReader()){ }
                    }
                    catch (Exception ex)
                    {
                        if (ex is SqlException)
                        {
                            if (ex.Message.Contains("NOTAREALDATABASE") || ex.Message.Contains("alter the login"))
                            {
                                if (arrFuzz[i].PrincipleName.Contains(@"\"))
                                {
                                    arrFuzz[i].PrincipalType = "Windows Account";
                                }
                                else
                                {
                                    arrFuzz[i].PrincipalType = "SQL Login";
                                }
                            }
                            else
                            {
                                arrFuzz[i].PrincipalType = "SQL Server Role";
                            }
                        }
                        else
                        {
                            Console.WriteLine(ex);
                        }
                    }
                }
                fuzzed = arrFuzz.ToList();
            }
            return true;
        }

        internal List<Fuzz> GetResults()
        {
            return fuzzed;
        }
    }
}
