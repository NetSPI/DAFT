using System;
using System.Data;
using System.Linq;

namespace DAFT.Modules
{
    static class SQLSysadminCheck
    {
        internal static bool Query(string instance, string computerName, Credentials credentials)
        {
            string query = string.Format(
                "SELECT \'{0}\' as [ComputerName],\n" +
                "\'{1}\' as [Instance],\n" +
                "CASE\n" +
                "WHEN IS_SRVROLEMEMBER(\'sysadmin\') = 0 THEN \'No\'\n" +
                "ELSE \'Yes\'\n" +
                "END as IsSysadmin"
                , computerName, instance
            );
            
            using (SQLConnection sql = new SQLConnection(instance))
            {
                sql.BuildConnectionString(credentials);
                if (!sql.Connect())
                    return false;

                DataTable table = sql.Query(query);
                try
                {
                    foreach (DataRow row in table.AsEnumerable())
                    {
#if DEBUG
                        Console.WriteLine("{0}\t{1}\t{2}", row["Instance"].ToString(), row["Instance"].ToString(), row["IsSysadmin"].ToString());
#endif
                        return "Yes" == row["IsSysadmin"].ToString() ? true : false;
                    }
                }
                catch (Exception ex)
                {
                    if (ex is ArgumentNullException)
                        Console.WriteLine("Empty Response");
                    else
                        Console.WriteLine(ex.Message);
                    return false;
                }
                return false;
            }
        }
    }
}
