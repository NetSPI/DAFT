using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;
using System.Text;

namespace DAFT.Modules
{
    class SQLAgentJob : Module
    {
        private const string QUERY1_1 = @"
SELECT loginame FROM sysprocesses WHERE LEFT(program_name, 8) = 'SQLAgent'
";

        private const string QUERY2_1 = @"
SELECT steps.database_name,
	job.job_id as [JOB_ID],
	job.name as [JOB_NAME],
	job.description as [JOB_DESCRIPTION],
	SUSER_SNAME(job.owner_sid) as [JOB_OWNER],
	steps.proxy_id,
	proxies.name as [proxy_account],
	job.enabled,
	steps.server,
	job.date_created,   
    steps.last_run_date,								                             
	steps.step_name,
	steps.subsystem,
	steps.command
FROM [msdb].[dbo].[sysjobs] job
INNER JOIN [msdb].[dbo].[sysjobsteps] steps        
	ON job.job_id = steps.job_id
left join [msdb].[dbo].[sysproxies] proxies
	on steps.proxy_id = proxies.proxy_id
WHERE 1=1";

        private static readonly List<string> roles = new List<string>
        {
            "SQLAgentUserRole",
            "SQLAgentReaderRole",
            "SQLAgentOperatorRole"
        };

        [DataContract]
        public struct ServerJob
        {
            [DataMember] public string ComputerName;
            [DataMember] public string Instance;
            [DataMember] public string DatabaseName;
            [DataMember] public int Job_Id;
            [DataMember] public string Job_Name;
            [DataMember] public string Job_Description;
            [DataMember] public object Job_Owner;
            [DataMember] public object Proxy_Id;
            [DataMember] public object Proxy_Credential;
            [DataMember] public string Date_Created;
            [DataMember] public DateTime Last_Run_Date;
            [DataMember] public bool Enabled;
            [DataMember] public object Server;
            [DataMember] public object Step_Name;
            [DataMember] public object SubSystem;
            [DataMember] public object Command;
        }

        private List<ServerJob> serverJobs;

        private string subsystemFilter = string.Empty;
        private string keywordFilter = string.Empty;
        private string usingProxyCredFilter = string.Empty;
        private string proxyCredFilter = string.Empty;

        internal SQLAgentJob(Credentials credentials) : base(credentials)
        {
            serverJobs = new List<ServerJob>();
        }

        internal void SetSubsystemFilter(string subsystemFilter)
        {
            this.subsystemFilter =  string.Format(" and steps.subsystem like \'{0}\'", subsystemFilter);
        }

        internal void SetKeywordFilter(string keywordFilter)
        {
            this.keywordFilter = string.Format(" and steps.command like \'%{0}%\'", keywordFilter);
        }

        internal void SetUsingProxyCredFilter()
        {
            this.usingProxyCredFilter = " and steps.proxy_id > 0";
        }

        internal void SetProxyCredentialFilter(string proxyCredFilter)
        {
            this.proxyCredFilter = string.Format(" and proxies.name like \'{0}\'", proxyCredFilter);
        }

        internal override bool Query()
        {
            using (SQLConnection sql = new SQLConnection(instance))
            {
                sql.BuildConnectionString(credentials);
                if (!sql.Connect())
                    return false;

                if (!_Check(sql))
                    return false;

                StringBuilder sb = new StringBuilder();
                sb.Append(QUERY2_1);
                if (!string.IsNullOrEmpty(keywordFilter)) { sb.Append(keywordFilter); }
                if (!string.IsNullOrEmpty(subsystemFilter)) { sb.Append(subsystemFilter); }
                if (!string.IsNullOrEmpty(proxyCredFilter)) { sb.Append(proxyCredFilter); }
                if (!string.IsNullOrEmpty(usingProxyCredFilter)) { sb.Append(usingProxyCredFilter); }
                table = sql.Query(sb.ToString());
            }

            foreach (DataRow row in table.AsEnumerable())
            {
                try
                {
                    ServerJob sj = new ServerJob
                    {
                        ComputerName = computerName,
                        Instance = instance,
                        DatabaseName = database,
                        Job_Id = (int)row["Job_Id"],
                        Job_Name = (string)row["Job_Name"],
                        Job_Description = (string)row["Job_Description"],
                        Job_Owner = (string)row["Job_Owner"],
                        Proxy_Id = (int)row["Proxy_Id"],
                        Proxy_Credential = (string)row["Proxy_Credential"],
                        Date_Created = (string)row["Date_Created"],
                        Last_Run_Date = (DateTime)row["Last_Run_Date"],
                        Enabled = (bool)row["Enabled"],
                        Server = (string)row["Server"],
                        Step_Name = (string)row["Step_Name"],
                        SubSystem = (string)row["SubSystem"],
                        Command = (string)row["Command"]
                    };
#if DEBUG
                    Misc.PrintStruct<ServerJob>(sj);
#endif
                    serverJobs.Add(sj);
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
            return true;
        }

        protected bool _Check(SQLConnection sql)
        {
#if DEBUG
            Console.WriteLine(QUERY1_1);
#endif
            DataTable table = sql.Query(QUERY1_1);
            if (!_CheckAgent(table))
            {
                Console.WriteLine("{0} : SQL Server Agent not currently running", instance);
                return false;
            }

            if (!_CheckPrivilege())
            {
                Console.WriteLine("{0} : Insufficient privileges to start job", instance);
                return false;
            }

            return true;
        }

        protected static bool _CheckAgent(DataTable table)
        {
            foreach (var row in table.AsEnumerable())
            {
                if (!(row["loginame"] is DBNull))
                {
                    return true;
                }
            }
            return false;
        }

        protected bool _CheckPrivilege()
        {
            if (!SQLSysadminCheck.Query(instance, computerName, credentials))
            {
                SQLDatabaseRoleMember sDRM = new SQLDatabaseRoleMember(credentials);
                sDRM.SetComputerName(computerName);
                sDRM.SetInstance(instance);
                sDRM.SetDatabase("msdb");
                sDRM.Query();
                foreach (var row in sDRM.GetResults())
                {
#if DEBUG
                    Console.WriteLine(row.RolePrincipalName);
#endif
                    if (roles.Contains(row.RolePrincipalName))
                    {
#if DEBUG
                        Console.WriteLine(row.PrincipalName + "\t" + Environment.UserDomainName + "\\" + Environment.UserName);
#endif
                        if (row.PrincipalName.ToString().ToUpper() == Environment.UserDomainName + "\\" + Environment.UserName)
                        {
                            return true;
                        }
                    }
                }
            }
            else
            {
                return true;
            }

            return false;
        }

        internal List<ServerJob> GetResults()
        {
            return serverJobs;
        }
    }
}