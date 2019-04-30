using System;
using System.Data;
using System.Linq;
using System.Text;

namespace DAFT.Modules
{
    sealed class SQLQuery
    {
        private readonly Credentials credentials;
        private string instance = string.Empty;

        internal SQLQuery(Credentials credentials)
        {
            this.credentials = credentials;
        }

        internal void SetInstance(string instance)
        {
            this.instance = instance;
        }

        internal bool Query(string query)
        {
            using (SQLConnection sql = new SQLConnection(instance))
            {
                sql.BuildConnectionString(credentials);
                if (sql.Connect())
                {
                    DataTable table = sql.Query(query);
                    StringBuilder output = new StringBuilder();
                    try
                    {
                        foreach (DataRow row in table.AsEnumerable())
                        {
                            foreach (var col in row.ItemArray)
                            {
                                try
                                {
                                    if (col is byte[])
                                        output.AppendFormat("{0}\n", BitConverter.ToString((byte[])col));
                                    else
                                        output.AppendFormat("{0}\n", col);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                    continue;
                                }
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
                        return false;
                    }
                }
            }
            return true;
        }
    }
}