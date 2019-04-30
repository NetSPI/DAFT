using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace DAFT.Modules
{
    class SQLServerLinkCrawl : Module
    {
        private const string QUERY1_1 = @"SELECT @@servername as servername, @@version as version, system_user as linkuser, is_srvrolemember('sysadmin') as role";

        private const string QUERY2_1 = @"SELECT srvname FROM master..sysservers WHERE dataaccess=1";

        const string QUERY_XP = @" WITH RESULT SETS ((output VARCHAR(8000)))";
        const string QUERY = @" WITH RESULT SETS ((output VARCHAR(8000), depth int))";

        private string sourceInstance;
        private string custom_query = string.Empty;

        [DataContract]
        public struct ServerLink
        {
            [DataMember] public string servername;
            [DataMember] public string sourceInstance;
            [DataMember] public string version;
            [DataMember] public string linkuser;
            [DataMember] public int role;
            [DataMember] public string result;
        }

        private readonly Dictionary<string, ServerLink> serverLinks;

        internal SQLServerLinkCrawl(Credentials credentials) : base(credentials)
        {
            serverLinks = new Dictionary<string, ServerLink>();
        }

        internal void SetQuery(string custom_query)
        {
            this.custom_query = custom_query;
        }

        internal override bool Query()
        {
            using (SQLConnection sql = new SQLConnection(instance))
            {
                sql.BuildConnectionString(credentials);
                if (!sql.Connect())
                    return false;
#if DEBUG
                Console.WriteLine(QUERY1_1);
#endif
                foreach (DataRow r in sql.Query(QUERY1_1).AsEnumerable())
                {
                    _AddLink(r);
                }
#if DEBUG
                Console.WriteLine(QUERY2_1);
#endif
                sourceInstance = instance;
                foreach (DataRow r in sql.Query(QUERY2_1).AsEnumerable())
                {
#if DEBUG
                    Console.WriteLine((string)r["srvname"]);
#endif
                    _Query(sql, string.Empty, (string)r["srvname"], 0);
                }
            }
            return true;
        }

        private void _Query(SQLConnection sql, string query, string instance, int depth)
        {
            Console.WriteLine("{0} -> {1}", sourceInstance, instance);
            ////////////////////////////////////////////////////////////////////////////////
            // Parse Server
            ////////////////////////////////////////////////////////////////////////////////
            string query1_1 = _GetInfoQuery(depth);
            string open_query1 = _GetOpenQuery1(instance, query, depth);
            string open_query2 = _GetOpenQuery2(depth);
            string query1_2 = open_query1 + query1_1 + open_query2;
#if DEBUG
            Console.WriteLine(depth);
            Console.WriteLine(query1_2);
#endif
            foreach (DataRow r in sql.Query(query1_2).AsEnumerable())
            {
                _AddLink(r);
            }

            ////////////////////////////////////////////////////////////////////////////////
            // Run a custom query
            ////////////////////////////////////////////////////////////////////////////////
            if (!string.IsNullOrEmpty(custom_query))
            {
                string query3_1 = open_query1 + _ConvertCustomQuery(custom_query, depth) + open_query2;
#if DEBUG
                Console.WriteLine(query3_1);
#endif
                StringBuilder output = new StringBuilder();
                try
                {
                    foreach (DataRow r in sql.Query(query3_1).AsEnumerable())
                    {
                        foreach (var col in r.ItemArray)
                        {
                            output.AppendFormat("{0} ", col);
                        }
                        Console.WriteLine("Query Output: {0}", output.ToString());
                    }
                }
                catch (Exception ex)
                {
                    if (ex is ArgumentNullException)
                        Console.WriteLine("Empty Response");
                    else
                        Console.WriteLine(ex.Message);
                }
            }

            ////////////////////////////////////////////////////////////////////////////////
            // Get Links
            ////////////////////////////////////////////////////////////////////////////////
            string query2_2 = open_query1 + QUERY2_1 + open_query2;
#if DEBUG
            Console.WriteLine(query2_2);
#endif
            lock (sourceInstance)
            {
                sourceInstance = instance;
            }

            foreach (DataRow r in sql.Query(query2_2).AsEnumerable())
            {             
                _Query(sql, open_query1, (string)r["srvname"], ++depth);
            }

            
        }

        private static string _GetOpenQuery1(string instance, string query, int ticks)
        {
            string strTicks = new string('\'', (int)Math.Pow(2, ticks));
            return string.Format("{0}SELECT * FROM openquery(\"{1}\", {2}", query, instance, strTicks);
        }

        private static string _GetOpenQuery2(int ticks)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = ticks; i >= 0; i--)
                sb.AppendFormat("{0})", new string('\'', (int)Math.Pow(2, i)));
            return sb.ToString();
        }

        private static string _GetInfoQuery(int ticks)
        {
            string strTicks = new string('\'', (int)Math.Pow(2, ticks + 1));
            return string.Format(
                "SELECT\n" +
                "@@servername as servername,\n" +
                "@@version as version,\n" +
                "system_user as linkuser,\n" +
                "is_srvrolemember({0}sysadmin{0}) as role",
                strTicks);
        }

        private static string _ConvertCustomQuery(string query, int ticks)
        {
            string strTicks = new string('\'', (int)Math.Pow(2, ticks));
            return query.Replace("\'", strTicks);
        }

        private bool _AddLink(DataRow r)
        {
            ServerLink s = new ServerLink
            {
                servername = (string)r["servername"],
                sourceInstance = sourceInstance,
                version = (string)r["version"],
                linkuser = (string)r["linkuser"],
                role = (int)r["role"]
            };

            try
            {
                lock (serverLinks)
                {
                    if (!serverLinks.ContainsKey(s.servername))
                        serverLinks.Add(s.servername, s);
                    else
                        Console.WriteLine("[*] Duplicate Key: {0}", s.servername);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return true;
        }

        internal Dictionary<string, ServerLink> GetResults()
        {
            return serverLinks;
        }  
    }
}
