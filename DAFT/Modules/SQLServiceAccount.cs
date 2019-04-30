using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DAFT.Modules
{
    class SQLServiceAccount : Module
    {
        const string QUERY1_1 = @"
-- Setup variables
DECLARE		@SQLServerInstance	VARCHAR(250)
DECLARE		@MSOLAPInstance		VARCHAR(250)
DECLARE		@ReportInstance 	VARCHAR(250)
DECLARE		@AgentInstance	 	VARCHAR(250)
DECLARE		@IntegrationVersion	VARCHAR(250)
DECLARE		@DBEngineLogin		VARCHAR(100)
DECLARE		@AgentLogin		VARCHAR(100)
DECLARE		@BrowserLogin		VARCHAR(100)
DECLARE     	@WriterLogin		VARCHAR(100)
DECLARE		@AnalysisLogin		VARCHAR(100)
DECLARE		@ReportLogin		VARCHAR(100)
DECLARE		@IntegrationDtsLogin	VARCHAR(100)

-- Get Service Paths for default and name instance
if @@SERVICENAME = 'MSSQLSERVER' or @@SERVICENAME = HOST_NAME()
BEGIN
-- Default instance paths
set @SQLServerInstance = 'SYSTEM\CurrentControlSet\Services\MSSQLSERVER'
set @MSOLAPInstance = 'SYSTEM\CurrentControlSet\Services\MSSQLServerOLAPService'
set @ReportInstance = 'SYSTEM\CurrentControlSet\Services\ReportServer'
set @AgentInstance = 'SYSTEM\CurrentControlSet\Services\SQLSERVERAGENT'
set @IntegrationVersion  = 'SYSTEM\CurrentControlSet\Services\MsDtsServer'+ SUBSTRING(CAST(SERVERPROPERTY('productversion') AS VARCHAR(255)),0, 3) + '0'
END
ELSE
BEGIN
-- Named instance paths
set @SQLServerInstance = 'SYSTEM\CurrentControlSet\Services\MSSQL$' + cast(@@SERVICENAME as varchar(250))
set @MSOLAPInstance = 'SYSTEM\CurrentControlSet\Services\MSOLAP$' + cast(@@SERVICENAME as varchar(250))
set @ReportInstance = 'SYSTEM\CurrentControlSet\Services\ReportServer$' + cast(@@SERVICENAME as varchar(250))
set @AgentInstance = 'SYSTEM\CurrentControlSet\Services\SQLAgent$' + cast(@@SERVICENAME as varchar(250))
set @IntegrationVersion  = 'SYSTEM\CurrentControlSet\Services\MsDtsServer'+ SUBSTRING(CAST(SERVERPROPERTY('productversion') AS VARCHAR(255)),0, 3) + '0'
END

-- Get SQL Server - Calculated
EXECUTE		master.dbo.xp_instance_regread
N'HKEY_LOCAL_MACHINE', @SQLServerInstance,
N'ObjectName',@DBEngineLogin OUTPUT

-- Get SQL Server Agent - Calculated
EXECUTE	master.dbo.xp_instance_regread
N'HKEY_LOCAL_MACHINE', @AgentInstance,
N'ObjectName',@AgentLogin OUTPUT
";

        const string QUERY1_2 = @"
-- Get SQL Server Browser - Static Location
EXECUTE       master.dbo.xp_instance_regread
@rootkey      = N'HKEY_LOCAL_MACHINE',
@key          = N'SYSTEM\CurrentControlSet\Services\SQLBrowser',
@value_name   = N'ObjectName',
@value        = @BrowserLogin OUTPUT

-- Get SQL Server Writer - Static Location
EXECUTE       master.dbo.xp_instance_regread
@rootkey      = N'HKEY_LOCAL_MACHINE',
@key          = N'SYSTEM\CurrentControlSet\Services\SQLWriter',
@value_name   = N'ObjectName',
@value        = @WriterLogin OUTPUT

-- Get MSOLAP - Calculated
EXECUTE		master.dbo.xp_instance_regread
N'HKEY_LOCAL_MACHINE', @MSOLAPInstance,
N'ObjectName',@AnalysisLogin OUTPUT

-- Get Reporting - Calculated
EXECUTE		master.dbo.xp_instance_regread
N'HKEY_LOCAL_MACHINE', @ReportInstance,
N'ObjectName',@ReportLogin OUTPUT

-- Get SQL Server DTS Server / Analysis - Calulated
EXECUTE		master.dbo.xp_instance_regread
N'HKEY_LOCAL_MACHINE', @IntegrationVersion,
N'ObjectName',@IntegrationDtsLogin OUTPUT
";

        const string QUERY1_3 = @"
SELECT [DBEngineLogin] = @DBEngineLogin,
[AgentLogin] = @AgentLogin";

        const string QUERY1_4 = @"
,[BrowserLogin] = @BrowserLogin,
[WriterLogin] = @WriterLogin,
[AnalysisLogin] = @AnalysisLogin,
[ReportLogin] = @ReportLogin,
[IntegrationLogin] = @IntegrationDtsLogin
";
        [DataContract]
        public struct ServerLogin
        {
            [DataMember] public string ComputerName;
            [DataMember] public string Instance;
            [DataMember] public string DBEngineLogin;
            [DataMember] public string AgentLogin;
            [DataMember] public string BrowserLogin;
            [DataMember] public string WriterLogin;
            [DataMember] public object AnalysisLogin;
            [DataMember] public object ReportLogin;
            [DataMember] public object IntegrationLogin;
        }

        private List<ServerLogin> serverLogins;

        internal SQLServiceAccount(Credentials credentials) : base(credentials)
        {
            serverLogins = new List<ServerLogin>();
        }

        internal override bool Query()
        {
            bool isSysAdmin = false;
            using (SQLConnection sql = new SQLConnection(instance))
            {
                sql.BuildConnectionString(credentials);
                if (!sql.Connect())
                    return false;

                isSysAdmin = SQLSysadminCheck.Query(instance, computerName, credentials);

                StringBuilder sb = new StringBuilder();
                sb.Append(QUERY1_1);
                if (isSysAdmin) { sb.Append(QUERY1_2); }
                sb.Append(QUERY1_3);
                if (isSysAdmin) { sb.Append(QUERY1_4); }
#if DEBUG
                Console.WriteLine(sb.ToString());
#endif
                //table = sql.Query(sb.ToString());
                serverLogins = sql.Query<ServerLogin>(sb.ToString(), new ServerLogin());
            }

            /*
            foreach (DataRow row in table.AsEnumerable())
            {
                try
                {
                    ServerLogin sl = new ServerLogin
                    {
                        ComputerName = computerName,
                        Instance = instance,
                        DBEngineLogin = (string)row["DBEngineLogin"],
                        AgentLogin = (string)row["AgentLogin"],
                        BrowserLogin = (string)row["BrowserLogin"],
                        WriterLogin = (string)row["WriterLogin"],
                        AnalysisLogin = (object)row["AnalysisLogin"],
                        ReportLogin = (object)row["ReportLogin"],
                        IntegrationLogin = (object)row["IntegrationLogin"]
                    };
#if DEBUG
                    Misc.PrintStruct<ServerLogin>(sl);
#endif
                    serverLogins.Add(sl);
                    return true;
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
            */
            return true;
        }

        internal List<ServerLogin> GetResults()
        {
            return serverLogins;
        }
    }
}
