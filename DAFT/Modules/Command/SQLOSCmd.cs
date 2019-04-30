using System;
using System.Data;
using System.Linq;
using System.Text;

namespace DAFT.Modules
{
    sealed class SQLOSCmd
    {
        private readonly Credentials credentials;
        private bool restoreState = false;
        private string instance = string.Empty;
        private string computerName = string.Empty;

        internal SQLOSCmd(Credentials credentials)
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
                {
                    Console.WriteLine("{0} : Show Advanced Options is disabled, enabling.", instance);
                    _Query(sql, @"sp_configure 'Show Advanced Options',1;RECONFIGURE", string.Empty);
                }
                else
                {
                    Console.WriteLine("{0} : Show Advanced Options is enabled.", instance);
                }

                int xcs_value = (int)_Query(sql, @"sp_configure 'xp_cmdshell'", "config_value");
                if (0 == xcs_value)
                {
                    Console.WriteLine("{0} : xp_cmdshell is disabled, enabling.", instance);
                    _Query(sql, @"sp_configure 'xp_cmdshell',1;RECONFIGURE", string.Empty);
                }
                else
                {
                    Console.WriteLine("{0} : xp_cmdshell is enabled.", instance);
                }

                Console.WriteLine(App.DELIMITER);
                Console.WriteLine((string)_Query(sql, string.Format("EXEC master..xp_cmdshell \'{0}\'", query), "output"));
                Console.WriteLine(App.DELIMITER);

                if (0 == xcs_value && restoreState)
                {
                    Console.WriteLine("{0} : Disabling xp_cmdshell.", instance);
                    _Query(sql, @"sp_configure 'xp_cmdshell',0;RECONFIGURE", string.Empty);
                }

                if (0 == sao_value && restoreState)
                {
                    Console.WriteLine("{0} : Disabling Show Advanced Options.", instance);
                    _Query(sql, @"sp_configure 'Show Advanced Options',0;RECONFIGURE", string.Empty);
                }
            }
        }

        private static object _Query(SQLConnection sql, string query, string value)
        {
#if DEBUG
            Console.WriteLine(query);
#endif
            DataTable table = sql.Query(query);
            StringBuilder sb = new StringBuilder();
            try
            {
                if (!string.IsNullOrEmpty(value))
                {
                    foreach (DataRow row in table.AsEnumerable())
                    {
                        if (row[value] is DBNull)
                            sb.Append("\n");
                        else if (row[value] is string)
                            sb.Append(row[value]);
                        else
                            return row[value];
                    }
                }
                return sb.ToString();
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
