using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Runtime.InteropServices;
using System.IO;

using Mono.Options;
using DAFT.Modules;

namespace DAFT
{
    class Program
    {
        [DllImport("shell32.dll", SetLastError = true)]
        static extern IntPtr CommandLineToArgvW([MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);

        public static string[] CommandLineToArgs(string commandLine)
        {
            int argc;
            var argv = CommandLineToArgvW(commandLine, out argc);
            if (argv == IntPtr.Zero)
                throw new System.ComponentModel.Win32Exception();
            try
            {
                var args = new string[argc];
                for (var i = 0; i < args.Length; i++)
                {
                    var p = Marshal.ReadIntPtr(argv, i * IntPtr.Size);
                    args[i] = Marshal.PtrToStringUni(p);
                }

                return args;
            }
            finally
            {
                Marshal.FreeHGlobal(argv);
            }
        }

        public static string Execute(string commandLine)
        {
            var sw = new StringWriter();
            Console.SetOut(sw);
            Console.SetError(sw);

            try
            {
                Main(CommandLineToArgs(commandLine));
            }
            catch (Exception e)
            {
                return e.ToString();
            }
            
            return sw.ToString();
        }

        static void Main(string[] args)
        {
            try
            {
                App app = new App();
                app.Run(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }

    sealed partial class App
    {
        private const string ORIGINAL_BANNER = @"
 _____ _                      _   _       _____  _____ _     
/  ___| |                    | | | |     /  ___||  _  | |    
\ `--.| |__   __ _ _ __ _ __ | | | |_ __ \ `--. | | | | |    
 `--. \ '_ \ / _` | '__| '_ \| | | | '_ \ `--. \| | | | |    
/\__/ / | | | (_| | |  | |_) | |_| | |_) /\__/ /\ \/' / |____
\____/|_| |_|\__,_|_|  | .__/ \___/| .__/\____/  \_/\_\_____/
                       | |         | |                       
                       |_|         |_|                       
";

        internal const string DELIMITER = @"
=============================================================
";

        private const string BANNER2 = @"
  _____              ______ _______ 
 |  __ \     /\     |  ____|__   __|
 | |  | |   /  \    | |__     | |   
 | |  | |  / /\ \   |  __|    | |   
 | |__| | / ____ \ _| |_      | |_  
 |_____(_)_/    \_(_)_(_)     |_(_)  
 Database Audit Framework & Toolkit
 
 A NetSPI Open Source Project
 @_nullbind, @0xbadjuju
";

        private Credentials credentials = null;

        private static string domainController = string.Empty;
        private static bool csv = false;
        private static string database = string.Empty;
        private static string excreds = string.Empty;
        private static string filters = string.Empty;
        private static bool hasAccess = false;
        private static string instance = string.Empty;
        private static bool json = false;
        private static string list = string.Empty;
        private static string module = string.Empty;
        private static bool nodefaults = false;
        private static bool output = false;
        private static string outputFileName = string.Empty;
        private static string query = string.Empty;
        private static bool restoreState = false;
        private static bool sysadmin = false;
        private static string creds = string.Empty;
        private static string version = string.Empty;
        private static bool xml = false;
        private static bool help = false;

        private static string subsystemFilter = string.Empty;
        private static string keywordFilter = string.Empty;
        private static bool usingProxyCredentials = false;
        private static string proxyCredentials = string.Empty;

        private static string assemblyNameFilter = string.Empty;
        private static bool exportAssembly = false;

        private static string ColumnFilter = string.Empty;
        private static string ColumnSearchFilter = string.Empty;
        private static string TableNameFilter = string.Empty;

        private static string SearchKeywords = string.Empty;
        private static bool ValidateCC = false;
        private static string SampleSize = string.Empty;

        private static string PermissionNameFilter = string.Empty;
        private static string PrincipalNameFilter = string.Empty;
        private static string PermissionTypeFilter = string.Empty;

        private static string RoleOwnerFilter = string.Empty;
        private static string RolePrincipalNameFilter = string.Empty;

        private static string SchemaFilter = string.Empty;

        private static string DatabaseUserFilter = string.Empty;
        
        private static string DatabaseLinkName = string.Empty;

        private static string StartId = string.Empty;
        private static string EndId = string.Empty;

        private static string CredentialNameFilter = string.Empty;

        private static string ProcedureNameFilter = string.Empty;
        private static bool AutoExecFilter = false;

        private static bool ShowAllAssemblyFiles = false;

        private static string TriggerNameFilter = string.Empty;

        private static string UNCPath = string.Empty;

        private static string AuditNameFilter = string.Empty;
        private static string AuditSpecificationFilter = string.Empty;
        private static string AuditActionNameFilter = string.Empty;

        private readonly OptionSet options = new OptionSet() {
            { "a|domaincontroller=", "Domain Controller for LDAP Queries.", v => domainController = v },
            { "c|csv", "CSV Output", v => csv = v != null },
            { "d|database=", "Database Name", v => database = v },
            { "e|dbcredentials=", "Explict database credentials.", v => excreds = v },
            { "f|filters=", "Explict database credentials.", v => filters = v },
            { "h|hasaccess", "Filter Database that are Accessible", v => hasAccess = v != null },
            { "i|instance=", "Instance Name", v => instance = v },
            { "j|json", "JSON Output", v => json = v != null },
            { "l|inputlist=", "Input Instance List", v => list = v },
            { "m|module=", "Module to Execute", v => module = v },
            { "n|nodefaults", "Filter Out Default Databases", v => nodefaults = v != null },
            { "o|output=", "Output CSV File.", v => outputFileName = v },
            { "q|query=", "Query/Command to Execute", v => query = v },
            { "r|restorestate=", "If server config is altered, return it to it's original state", v => restoreState = v != null},
            { "s|sysadmin", "Filter Database where SysAdmin Privileges", v => sysadmin = v != null },
            { "u|credentials=", "Credentials to Login With", v => creds = v },
            { "v|version=", "Override version detection", v => version = v },
            { "x|xml", "XML Output", v => xml = v != null},
            { "?|help",  "Display this message and exit", v => help = v != null },
            //Begin Keyword Hell
            //AgentJob
            { "SubsystemFilter=", "Agent Job Subsystem Filter", v => subsystemFilter = v },
            { "KeywordFilter=", "Agent Job and Stored Procedure Keyword Filter", v => keywordFilter = v },//Stored Procedure
            { "UsingProxyCredFilter", "Agent Jobs using Proxy Credentials ", v => usingProxyCredentials = v != null },
            { "ProxyCredentialFilter=", "Agent Job using Specific Proxy", v => proxyCredentials = v },
            //AssemblyFile
            { "AssemblyNameFilter=", "Assembly Name", v => assemblyNameFilter = v },
            { "ExportAssembly", "Export Assemblies", v => exportAssembly = v != null },
            //Column
            { "ColumnFilter=", "Exact Column Name Search Filter", v => ColumnFilter = v },
            { "ColumnSearchFilter=", "Column Name Wildcard Search Filter", v => ColumnSearchFilter = v },
            { "TableNameFilter=", "Table Name to Retrieve Columns From", v => TableNameFilter = v }, //View
            //ColumnSampleData
            { "SearchKeywords=", "Column Name Search Keyword", v => SearchKeywords = v },
            { "ValidateCC", "Validate Data Against Luhn Algorithm", v => ValidateCC = v != null },
            { "SampleSize=", "Number of Rows to Retrieve", v => SampleSize = v },
            //DatabasePriv 
            { "PermissionNameFilter=", "Permission Name Filter", v => PermissionNameFilter = v }, //ServerPriv,
            { "PrincipalNameFilter=", "Principal Name Filter", v => PrincipalNameFilter = v }, //DatabaseRoleMember,DatabaseUser,ServerLogin,Session,ServerRoleMember
            { "PermissionTypeFilter=", "Database Permission Type Filter", v => PermissionTypeFilter = v },
            //DatabasePriv
            { "RoleOwnerFilter=", "Role Owner Filter", v => RoleOwnerFilter = v }, //DatabaseRole,ServerRole,ServerRoleMember
            { "RolePrincipalNameFilter=", "Role Principal Name Filter", v => RolePrincipalNameFilter = v }, //DatabaseRole,DatabaseRoleMember,ServerRole
            //DatabaseSchema
            { "SchemaFilter=", "Database Schema Name Filter", v => SchemaFilter = v },
            //DatabaseUser
            { "DatabaseUserFilter=", "Database UserName Filter", v => DatabaseUserFilter = v },
            //DatabaseLink
            { "DatabaseLinkName=", "Database Link Name Filter", v => DatabaseLinkName = v },
            //Fuzz
            { "StartId=", "Fuzzing Start ID, Defaults to Zero", v => StartId = v },
            { "EndId=", "Fuzzing End ID, Defaults to Five", v => EndId = v },
            //ServerCredential
            { "CredentialNameFilter=", "Database Link Name Filter", v => CredentialNameFilter = v },
            //StoredProcedure
            { "ProcedureNameFilter=", "Database Link Name Filter", v => ProcedureNameFilter = v },
            { "AutoExecFilter", "Database Link Name Filter", v => AutoExecFilter = v != null},
            //StoredProcedureCLR
            { "ShowAllAssemblyFiles", "Database Link Name Filter", v => ShowAllAssemblyFiles = v != null},
            //TriggerDdl
            { "TriggerNameFilter=", "Trigger Name Filter", v => TriggerNameFilter = v },
            //UNCPathInjection
            { "CaptureUNCPath=", "UNC Path to Capture Hashes", v => UNCPath = v },
            //AuditDatabaseSpec,AuditServerSpec
            { "AuditNameFilter=", "", v => AuditNameFilter = v },
            { "AuditSpecificationFilter=", "Agent Jobs using Proxy Credentials ", v => AuditSpecificationFilter = v },
            { "AuditActionNameFilter=", "Agent Job using Specific Proxy", v => AuditActionNameFilter = v },
        };

        public struct SqlInstances
        {
            public string Server;
            public string ServerInstance;
            public object UserSid;
            public string User;
            public string UserCN;
            public string Service;
            public object SPN;
            public DateTime LastLogon;
            public string Description;
        }
        public List<SqlInstances> instances;
        
        public SQLDatabase sD;
        public List<SQLDatabase.Database> databases;

        public List<SQLConnection.Connection> connections;

        private FileStream inputFileStream = null;
        private FileStream outputFileStream = null;

        internal App()
        {
            instances = new List<SqlInstances>();
            databases = new List<SQLDatabase.Database>();
            connections = new List<SQLConnection.Connection>();
        }

        internal void Run(string[] args)
        {
            Console.WriteLine(BANNER2);
            Console.WriteLine(DELIMITER);

            try
            {
                options.Parse(args);
            }
            catch (OptionException ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            if (help)
            {
                Console.WriteLine(DELIMITER);
                options.WriteOptionDescriptions(Console.Out);

                Console.WriteLine(DELIMITER);
                Console.WriteLine("Options per Method: ");
                Console.WriteLine(DELIMITER);
                Console.WriteLine("AgentJob:");
                Console.WriteLine("\t-i InstanceName\n\t--SubsystemFilter=SUBSYSTEM\n\t--KeywordFilter=KEYWORD\n\t--UsingProxyCredentials <Filter for Proxy Credentials>\n\t--ProxyCredentials=CREDENTIALS\n");
                Console.WriteLine("AssemblyFile:");
                Console.WriteLine("\t-i InstanceName\n\t--AssemblyNameFilter=ASSEMBLY\n\t--ExportAssembly <Export Assemblies>\n");
                Console.WriteLine("AuditDatabaseSpec:");
                Console.WriteLine("\t-i InstanceName\n");
                Console.WriteLine("AuditPrivCreateProcedure:");
                Console.WriteLine("\t-i InstanceName\n");
                Console.WriteLine("AuditPrivDbChaining:");
                Console.WriteLine("\t-i InstanceName\n");
                Console.WriteLine("AuditPrivServerLink:");
                Console.WriteLine("\t-i InstanceName\n");
                Console.WriteLine("AuditPrivTrustworthy:");
                Console.WriteLine("\t-i InstanceName\n");
                Console.WriteLine("AuditPrivXpDirTree:");
                Console.WriteLine("\t-i InstanceName\n");
                Console.WriteLine("AuditPrivXpFileExists:");
                Console.WriteLine("\t-i InstanceName\n");
                Console.WriteLine("AuditRoleDbOwner:");
                Console.WriteLine("\t-i InstanceName\n");
                Console.WriteLine("AuditServerSpec:");
                Console.WriteLine("\t-i InstanceName\n\t--AuditNameFilter=NAME\n\t--AuditSpecificationFilter=SPECIFICATION\n\t--AuditActionNameFilter=ACTION\n");
                Console.WriteLine("AuditSQLiSpExecuteAs:");
                Console.WriteLine("\t-i InstanceName\n");
                Console.WriteLine("AuditSQLiSpSigned:");
                Console.WriteLine("\t-i InstanceName\n");
                Console.WriteLine("Column:");
                Console.WriteLine("\t-i InstanceName\t-d DatabaseName\n\t-n <No Defaults> \n\t-h <Has Access>\n\t-s <Is SysAdmin>\n\t--ColumnFilter=FILTER\n\t--ColumnSearchFilter=WILDCARD_FILTER\n");
                Console.WriteLine("ColumnSampleData:");
                Console.WriteLine("\t-i InstanceName\t-d DatabaseName\n\t-n <No Defaults> \n\t-h <Has Access>\n\t-s <Is SysAdmin>\n\t--SearchKeywords=KEYWORDS\n\t--SampleSize=SIZE\n\t--ValidateCC <Run Luhn Algorithm on Results>\n");
                Console.WriteLine("Connection:");
                Console.WriteLine("\t-i InstanceName\n");
                Console.WriteLine("Database:");
                Console.WriteLine("\t-i InstanceName\t-d DatabaseName\n\t-n <No Defaults> \n\t-h <Has Access>\n\t-s <Is SysAdmin>\n");
                Console.WriteLine("DatabasePriv:");
                Console.WriteLine("\t-i InstanceName\t-d DatabaseName\n\t-n <No Defaults> \n\t--PermissionNameFilter=PERMISSION\n\t--PrincipalNameFilter=PRINCIPAL\n\t--PermissionTypeFilter=PERMISSION\n");
                Console.WriteLine("DatabaseRole:");
                Console.WriteLine("\t-i InstanceName\t-d DatabaseName\n\t-n <No Defaults> \n\t--RoleOwnerFilter=OWNER\n\t--RolePrincipalNameFilter=PRINCIPAL\n");
                Console.WriteLine("DatabaseSchema:");
                Console.WriteLine("\t-i InstanceName\t-d DatabaseName\n\t-n <No Defaults> \n\t--SchemaFilter=SCHEMA\n");
                Console.WriteLine("DatabaseUser:");
                Console.WriteLine("\t-i InstanceName\t-d DatabaseName\n\t-n <No Defaults> \n\t--DatabaseUserFilter=USER\n\t--PrincipalNameFilter=NAME\n");
                Console.WriteLine("FuzzDatabaseName:");
                Console.WriteLine("\t-i InstanceName\n\t-StartId=0 \n\t--EndId=5\n");
                Console.WriteLine("FuzzDomainAccount:");
                Console.WriteLine("\t-i InstanceName\n\t-StartId=0 \n\t--EndId=5\n");
                Console.WriteLine("FuzzObjectName:");
                Console.WriteLine("\t-i InstanceName\n\t-StartId=0 \n\t--EndId=5\n");
                Console.WriteLine("FuzzServerLogin:");
                Console.WriteLine("\t-i InstanceName\n\t--EndId=5\n");
                Console.WriteLine("OleDbProvider:");
                Console.WriteLine("\t-i InstanceName\n");
                Console.WriteLine("OSCmd:");
                Console.WriteLine("\t-i InstanceName -q COMMAND --RestoreState <Undo any changes made to run command>\n");
                Console.WriteLine("OSCmdAgentJob:");
                Console.WriteLine("\t-i InstanceName -q COMMAND\n");
                Console.WriteLine("OSCmdOle:");
                Console.WriteLine("\t-i InstanceName -q COMMAND --RestoreState <Undo any changes made to run command>\n");
                Console.WriteLine("OSCmdPython:");
                Console.WriteLine("\t-i InstanceName -q COMMAND --RestoreState <Undo any changes made to run command>\n");
                Console.WriteLine("OSCmdR:");
                Console.WriteLine("\t-i InstanceName -q COMMAND --RestoreState <Undo any changes made to run command>\n");
                Console.WriteLine("Query:");
                Console.WriteLine("\t-i InstanceName -q QUERY\n");
                Console.WriteLine("ServerConfiguration:");
                Console.WriteLine("\t-i InstanceName\n");
                Console.WriteLine("ServerCredential:");
                Console.WriteLine("\t-i InstanceName \n\t--CredentialNameFilter=CREDENTIAL\n");
                Console.WriteLine("ServerInfo:");
                Console.WriteLine("\t-i InstanceName\n");
                Console.WriteLine("ServerLink:");
                Console.WriteLine("\t-i InstanceName \n\t--DatabaseLinkName=LINK\n");
                Console.WriteLine("ServerLinkCrawl:");
                Console.WriteLine("\t-i InstanceName -q QUERY\n");
                Console.WriteLine("ServerLogin:");
                Console.WriteLine("\t-i InstanceName \n\t--PrincipalNameFilter=NAME\n");
                Console.WriteLine("ServerLoginDefaultPw:");
                Console.WriteLine("\t-i InstanceName\n");
                Console.WriteLine("ServerPasswordHash:");
                Console.WriteLine("\t-i InstanceName\n");
                Console.WriteLine("ServerPriv:");
                Console.WriteLine("\t-i InstanceName \n\t--PermissionNameFilter=PERMISSION\n");
                Console.WriteLine("ServerRole:");
                Console.WriteLine("\t-i InstanceName \n\t--RoleOwnerFilter=ROLE \n\t--RolePrincipalNameFilter=NAME\n");
                Console.WriteLine("ServerRoleMember:");
                Console.WriteLine("\t-i InstanceName \n\t--PrincipalNameFilter=NAME\n");
                Console.WriteLine("ServiceAccount:");
                Console.WriteLine("\t-i InstanceName\n");
                Console.WriteLine("Session:");
                Console.WriteLine("\t-i InstanceName \n\t--PrincipalNameFilter=NAME\n");
                Console.WriteLine("StoredProcedure:");
                Console.WriteLine("\t-i InstanceName \n\t--ProcedureNameFilter=NAME \n\t--KeywordFilter=KEYWORD \n\t--AutoExecFilter <Filter fore Auto Exec Stored Procedures>\n");
                Console.WriteLine("StoredProcedureAutoExec:");
                Console.WriteLine("\t-i InstanceName \n\t--ProcedureNameFilter=NAME \n\t--KeywordFilter=KEYWORD\n");
                Console.WriteLine("StoredProcedureCLR:");
                Console.WriteLine("\t-i InstanceName \t-d DatabaseName \n\t-n <No Defaults> \n\t-h <Has Access>\n\t-s <Is SysAdmin> \n\t--ShowAllAssemblyFiles <Display all Assemblies>\n");
                Console.WriteLine("StoredProcedureXP:");
                Console.WriteLine("\t-i InstanceName \t-d DatabaseName \n\t-n <No Defaults> \n\t-h <Has Access>\n\t-s <Is SysAdmin> \n\t--ProcedureNameFilter=NAME\n");
                Console.WriteLine("SysAdminCheck:");
                Console.WriteLine("\t-i InstanceName\n");
                Console.WriteLine("Tables:");
                Console.WriteLine("\t-i InstanceName \t-d DatabaseName \n\t-n <No Defaults> \n\t-h <Has Access>\n\t-s <Is SysAdmin>\n");
                Console.WriteLine("TriggerDdl:");
                Console.WriteLine("\t-i InstanceName \t-d DatabaseName \n\t-n <No Defaults> \n\t-h <Has Access>\n\t-s <Is SysAdmin> \n\t--TriggerNameFilter=TRIGGER\n");
                Console.WriteLine("TriggerDml:");
                Console.WriteLine("\t-i InstanceName \t-d DatabaseName \n\t-n <No Defaults> \n\t-h <Has Access>\n\t-s <Is SysAdmin> \n\t--TriggerNameFilter=TRIGGER\n");
                Console.WriteLine("UncPathInjection:");
                Console.WriteLine("\t-i InstanceName \t--UNCPath=\\\\IP\\PATH\n");
                Console.WriteLine("View:");
                Console.WriteLine("\t-i InstanceName \t-d DatabaseName \n\t-n <No Defaults> \n\t-h <Has Access> \n\t--TableNameFilter=TABLE\n");
                return;
            }

            if (string.IsNullOrWhiteSpace(domainController) && string.IsNullOrEmpty(instance))
            {
                domainController = Environment.GetEnvironmentVariable("LogonServer").Replace("\\\\", "");
            }

            if (string.IsNullOrEmpty(module))
            {
                Console.WriteLine("[-] No module selected (-m || --module)");
                return;
            }

            Console.WriteLine("{0,-40}{1}", "Module", module);
            if (!string.IsNullOrEmpty(domainController)) { Console.WriteLine("{0,-40}{1}", "Domain Controller" + new string('.', 40-17), domainController); }
            if (csv) { Console.WriteLine("{0,-40}{1}", "CSV Output" + new string('.', 40-10), csv); }
            if (!string.IsNullOrEmpty(database)) { Console.WriteLine("{0,-40}{1}", "Database" + new string('.', 40-8), database); }
            if (!string.IsNullOrEmpty(excreds)) { Console.WriteLine("{0,-40}{1}", "Explicit DB Credentials" + new string('.', 40-33), excreds); }
            if (json) { Console.WriteLine("{0,-40}{1}", "JSON Output" + new string('.', 40 - 11), json); }
            if (!string.IsNullOrEmpty(excreds)) { Console.WriteLine("{0,-40}{1}", "Search Filters" + new string('.', 40-14), filters); }
            if (!string.IsNullOrEmpty(instance)) { Console.WriteLine("{0,-40}{1}", "Server Instance" + new string('.', 40-15), instance); }
            if (!string.IsNullOrEmpty(list)) { Console.WriteLine("{0,-40}{1}", "DB Instance Input List" + new string('.', 40-30), list); }
            if (nodefaults) { Console.WriteLine("{0,-40}{1}", "Skipping Default Databases" + new string('.', 40-34), nodefaults); }
            if (!string.IsNullOrEmpty(outputFileName)) { Console.WriteLine("{0,-40}{1}", "Output file" + new string('.', 40-11), outputFileName); }
            if (!string.IsNullOrEmpty(query)) { Console.WriteLine("{0,-40}{1}", "Query/Command to Execute" + new string('.', 40-25), query); }
            if (!string.IsNullOrEmpty(creds)) { Console.WriteLine("{0,-40}{1}", "LDAP/DB Credentials" + new string('.', 40-19), creds); }
            Console.WriteLine(DELIMITER);
            
            if (!string.IsNullOrEmpty(creds))
            {
                string[] c = creds.Split(':');
                string username = c.First();
                string password = string.Join("", c.Skip(1).Take(c.Length - 1).ToArray());
                Console.WriteLine("Username: {0}", username);
                Console.WriteLine("Password: {0}", password);
                credentials = new Credentials(username, password);
                creds = string.Empty;
                username = string.Empty;
                password = string.Empty;
            }

            if (!string.IsNullOrEmpty(database))
            {
                databases.Add(
                    new SQLDatabase.Database
                    {
                        DatabaseName = database,
                        Instance = instance,
                    }
               );
            }           

            if (!string.IsNullOrEmpty(instance))
            {
                SqlInstances i = new SqlInstances
                {
                    ServerInstance = instance,
                    Server = Misc.ComputerNameFromInstance(instance)
                };
                instances.Add(i);
            }
            else if (!string.IsNullOrEmpty(domainController))
            {
                SQLServers servers = new SQLServers();
                servers.SetDomainController(domainController);
                if (null == credentials || credentials.IsSqlAccount())
                    servers.Connect(null);
                else
                    servers.Connect(credentials);
                if (!servers.Search())
                    return;

                servers.ParseCollection(true, ref instances);
            }
            else if (!string.IsNullOrEmpty(list))
            {
                string path = string.Empty;
                try
                {
                    path = Path.GetFullPath(list);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unable to open file");
                    Console.WriteLine(ex);
                    return;
                }

                using (StreamReader sr = new StreamReader(path))
                {
                    string line = string.Empty;
                    while (null != (line = sr.ReadLine()))
                    {
                        if (string.IsNullOrEmpty(line))
                            continue;

                        instances.Add(
                            new SqlInstances
                            {
                                ServerInstance = line,
                                Server = Misc.ComputerNameFromInstance(line),
                                User = string.Empty
                            }
                        );
                    }
                }
            }
            else
            {
                Console.WriteLine("[-] No instances to target");
                return;
            }

            if (!string.IsNullOrEmpty(excreds))
            {
                string[] c = excreds.Split(':');
                string username = c.First();
                string password = string.Join("", c.Skip(1).Take(c.Length - 1).ToArray());
                Console.WriteLine("Username: {0}", username);
                Console.WriteLine("Password: {0}", password);
                credentials = new Credentials(username, password);
                creds = string.Empty;
                username = string.Empty;
                password = string.Empty;
            }

            if (!string.IsNullOrEmpty(outputFileName))
            {
                output = true;
                string path = string.Empty;
                try
                {
                    path = Path.GetFullPath(outputFileName);
#if DEBUG
                    Console.WriteLine(path);
#endif
                    outputFileStream = new FileStream(path, FileMode.OpenOrCreate);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unable to create file");
                    Console.WriteLine(ex);
                    output = false;
                }
            }

            switch (module.ToLower())
            {
                case "instancedomain":
                    Console.WriteLine("{0,-30} {1,-40} {2,-10}", "Server", "Instance", "User");
                    Console.WriteLine("{0,-30} {1,-40} {2,-10}", "======", "========", "====");
                    break;
                default:
                    break;
            }

            foreach (var i in instances)
            {
                switch (module.ToLower())
                {
                    case "agentjob":
                        _SQLAgentJob(i);
                        break;
                    case "assemblyfile":
                        _SQLAssemblyFile(i);
                        break;
                    case "auditdatabasespec":
                        _SQLAuditDatabaseSpec(i);
                        break;
                    case "auditprivautoexecsp":
                        _SQLAuditPrivAutoExecSp(i);
                        break;
                    case "auditprivcreateprocedure":
                        _SQLAuditPrivCreateProcedure(i);
                        break;
                    case "auditprivdbchaining":
                        _SQLAuditPrivDbChaining(i);
                        break;
                    case "auditprivimpersonatelogin":
                        _SQLAuditPrivImpersonateLogin(i);
                        break;
                    case "auditprivserverlink":
                        _SQLAuditPrivServerLink(i);
                        break;
                    case "auditprivtrustworthy":
                        _SQLAuditPrivTrustworthy(i);
                        break;
                    case "auditprivxpdirtree":
                        _SQLAuditPrivXpDirTree(i);
                        break;
                    case "auditprivxpfileexists":
                        _SQLAuditPrivXpFileExists(i);
                        break;
                    case "auditroledbowner":
                        _SQLAuditRoleDbOwner(i);
                        break;
                    case "auditroledbddladmin":
                        _SQLAuditRoleDBDDLADMIN(i);
                        break;
                    case "auditserverspec":
                        _SQLAuditServerSpec(i);
                        break;
                    case "auditispexecuteas":
                        _SQLAuditSQLiSpExecuteAs(i);
                        break;
                    case "auditispsigned":
                        _SQLAuditSQLiSpSigned(i);
                        break;
                    case "column":
                        _SQLColumn(i);
                        break;
                    case "columnsampledata":
                        _SQLColumnSampleData(i);
                        break;
                    case "connection":
                        _SQLConnection(i);
                        break;
                    case "database":
                        _SQLDatabase(i);
                        break;
                    case "databasepriv":
                        _SQLDatabasePriv(i);
                        break;
                    case "databaserole":
                        _SQLDatabaseRole(i);
                        break;
                    case "databaserolemember":
                        _SQLDatabaseRoleMember(i);
                        break;
                    case "databaseschema":
                        _SQLDatabaseSchema(i);
                        break;
                    case "databaseuser":
                        _SQLDatabaseUser(i);
                        break;
                    case "fuzzdatabasename":
                        _SQLFuzzDatabaseName(i);
                        break;
                    case "fuzzdomainaccount":
                        _SQLFuzzDomainAccount(i);
                        break;
                    case "fuzzobjectname":
                        _SQLFuzzObjectName(i);
                        break;
                    case "fuzzserverlogin":
                        _SQLFuzzServerLogin(i);
                        break;                   
                    case "oledbprovider":
                        _SQLOleDbProvider(i);
                        break;
                    case "oscmd":
                        _SQLOSCmd(i);
                        break;
                    case "oscmdagentjob":
                        _SQLOSCmdAgentJob(i);
                        break;
                    case "oscmdole":
                        _SQLOSCmdOle(i);
                        break;
                    case "oscmdpython":
                        _SQLOSCmdPython(i);
                        break;
                    case "oscmdr":
                        _SQLOSCmdR(i);
                        break;
                    case "query":
                        _SQLQuery(i);
                        break;
                    case "serverconfiguration":
                        _SQLServerConfiguration(i);
                        break;
                    case "servercredential":
                        _SQLServerCredential(i);
                        break;
                    case "serverinfo":
                        _SQLServerInfo(i);
                        break;
                    case "serverlink":
                        _SQLServerLink(i);
                        break;
                    case "serverlinkcrawl":
                        _SQLServerLinkCrawl(i);
                        break;
                    case "serverlogin":
                        _SQLServerLogin(i);
                        break;
                    case "serverdefaultloginpw":
                        _SQLServerLoginDefaultPw(i);
                        break;
                    case "serverpasswordhash":
                        _SQLServerPasswordHash(i);
                        break;
                    case "serverpriv":
                        _SQLServerPriv(i);
                        break;
                    case "serverrole":
                        _SQLServerRole(i);
                        break;
                    case "serverrolemember":
                        _SQLServerRoleMember(i);
                        break;
                    case "serviceaccount":
                        _SQLServiceAccount(i);
                        break;
                    case "session":
                        _SQLSession(i);
                        break;
                    case "storedprocedure":
                        _SQLStoredProcedure(i);
                        break;
                    case "storedprocedureautoexec":
                        _SQLStoredProcedureAutoExec(i);
                        break;
                    case "storedprocedureclr":
                        _SQLStoredProcedureCLR(i);
                        break;
                    case "storedproceduresqli":
                        _SQLStoredProcedureSQLi(i);
                        break;
                    case "storedprocedurexp":
                        _SQLStoredProcedureXP(i);
                        break;
                    case "sysadmincheck":
                        _SQLSysAdminCheck(i);
                        break;
                    case "tables":
                        _SQLTables(i);
                        break;
                    case "triggerddl":
                        _SQLTriggerDdl(i);
                        break;
                    case "triggerdml":
                        _SQLTriggerDml(i);
                        break;
                    case "uncpathinjection":
                        _SQLUncPathInjection(i);
                        break;
                    case "view":
                        _SQLView(i);
                        break;
                    case "instancedomain":
                        Console.WriteLine("{0,-30} {1,-40} {2,-10}", i.Server, i.ServerInstance, i.User);
                        break;
                    default:
                        Console.WriteLine("[-] Invalid Module");
                        break;
                }
            }

            switch (module.ToLower())
            {
                case "connection":
                    _WriteJSONOutput(connections.ToArray());
                    break;
                default:
                    break;
            }
        }

        private void _PrintOutput<T>(List<T> data)
        {
            foreach (var a in data)
                Misc.PrintStruct(a);
            if (output)
                _WriteOutput(data);
        }

        private void _PrintOutput<T>(T data)
        {
            Misc.PrintStruct(data);
            if (output)
                _WriteOutput(data);
        }

        private void _WriteOutput<T>(List<T> data)
        {
            if (json)
            {
#if DEBUG
                Console.WriteLine("Writing JSON Output");
#endif
                _WriteJSONOutput(data);
            }
            else if (xml)
            {
#if DEBUG
                Console.WriteLine("Writing XML Output");
#endif
                _WriteXMLOutput(data);
            }
            else if (csv)
            {
#if DEBUG
                Console.WriteLine("Writing CSV Output");
#endif
                _WriteCSVOutput(data);
            }
        }

        private void _WriteOutput<T>(T data)
        {
            if (json)
            {
#if DEBUG
                Console.WriteLine("Writing JSON Output");
#endif
                _WriteJSONOutput(data);
            }
            else if (xml)
            {
#if DEBUG
                Console.WriteLine("Writing XML Output");
#endif
                _WriteXMLOutput(data);
            }
            else if (csv)
            {
#if DEBUG
                Console.WriteLine("Writing CSV Output");
#endif
                _WriteCSVOutput(data);
            }
        }

        private T _ReadJSONOutput<T>()
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            return (T)ser.ReadObject(inputFileStream);
        }

        private void _WriteJSONOutput<T>(T outputObject)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            ser.WriteObject(outputFileStream, outputObject);
        }

        private T _ReadXMLOutput<T>(T outputObject)
        {
            DataContractSerializer ser = new DataContractSerializer(typeof(T));
            return (T)ser.ReadObject(inputFileStream);
        }

        private void _WriteXMLOutput<T>(T outputObject)
        {
            DataContractSerializer ser = new DataContractSerializer(typeof(T));
            ser.WriteObject(outputFileStream, outputObject);
        }

        private void _WriteCSVOutput<T>(T outputObject)
        {
            CSVSerializer ser = new CSVSerializer(outputFileStream);
            ser.WriteObject(outputObject);
        }

        private void _WriteCSVOutput<T>(List<T> outputObject)
        {
            CSVSerializer ser = new CSVSerializer(outputFileStream);
            ser.WriteObject(outputObject);
        }

        ~App()
        {
            if (null != outputFileStream)
                outputFileStream.Dispose();
        }
    } 
}
