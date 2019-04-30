using System;
using System.Data;
using System.Runtime.Serialization;

namespace DAFT.Modules
{
    class SQLServerInfo : Module
    {
        const string query1_1 = @"
-- Get SQL Server Information

-- Get SQL Server Service Name and Path
DECLARE @SQLServerInstance varchar(250)
DECLARE @SQLServerServiceName varchar(250)
if @@SERVICENAME = 'MSSQLSERVER'
BEGIN
set @SQLServerInstance = 'SYSTEM\CurrentControlSet\Services\MSSQLSERVER'
set @SQLServerServiceName = 'MSSQLSERVER'
END
ELSE
BEGIN
set @SQLServerInstance = 'SYSTEM\CurrentControlSet\Services\MSSQL$' + cast(@@SERVICENAME as varchar(250))
set @SQLServerServiceName = 'MSSQL$' + cast(@@SERVICENAME as varchar(250))
END

-- Get SQL Server Service Account
DECLARE @ServiceaccountName varchar(250)
EXECUTE master.dbo.xp_instance_regread
N'HKEY_LOCAL_MACHINE', @SQLServerInstance,
N'ObjectName',@ServiceAccountName OUTPUT, N'no_output'

-- Get authentication mode
DECLARE @AuthenticationMode INT
EXEC master.dbo.xp_instance_regread N'HKEY_LOCAL_MACHINE',
N'Software\Microsoft\MSSQLServer\MSSQLServer',
N'LoginMode', @AuthenticationMode OUTPUT

--Get the forced encryption flag
BEGIN TRY
    DECLARE @ForcedEncryption INT
    EXEC master.dbo.xp_instance_regread N'HKEY_LOCAL_MACHINE',
	N'SOFTWARE\MICROSOFT\Microsoft SQL Server\MSSQLServer\SuperSocketNetLib',
	N'ForceEncryption', @ForcedEncryption OUTPUT
END TRY
BEGIN CATCH
END CATCH
";

        const string query1_2 = @"
--Grab additional information as sysadmin
-- Get machine type
DECLARE @MachineType SYSNAME
EXECUTE master.dbo.xp_regread
@rootkey = N'HKEY_LOCAL_MACHINE',
@key = N'SYSTEM\CurrentControlSet\Control\ProductOptions',
@value_name = N'ProductType',
@value = @MachineType output

-- Get OS version
DECLARE @ProductName SYSNAME
EXECUTE master.dbo.xp_regread
@rootkey = N'HKEY_LOCAL_MACHINE',
@key = N'SOFTWARE\Microsoft\Windows NT\CurrentVersion',
@value_name = N'ProductName',
@value = @ProductName output

-- Return server and version information
";

        const string query1_4 = @"
@@servername as [Instance],
DEFAULT_DOMAIN() as [DomainName],
SERVERPROPERTY('processid') as ServiceProcessID,
@SQLServerServiceName as [ServiceName],
@ServiceAccountName as [ServiceAccount],
(SELECT CASE @AuthenticationMode
WHEN 1 THEN 'Windows Authentication'
WHEN 2 THEN 'Windows and SQL Server Authentication'
ELSE 'Unknown'
END) as [AuthenticationMode],
@ForcedEncryption as ForcedEncryption,
CASE SERVERPROPERTY('IsClustered')
WHEN 0
THEN 'No'
ELSE 'Yes'
END as [Clustered],
SERVERPROPERTY('productversion') as [SQLServerVersionNumber],
SUBSTRING(@@VERSION, CHARINDEX('2', @@VERSION), 4) as [SQLServerMajorVersion],
serverproperty('Edition') as [SQLServerEdition],
SERVERPROPERTY('ProductLevel') AS[SQLServerServicePack],
SUBSTRING(@@VERSION, CHARINDEX('x', @@VERSION), 3) as [OSArchitecture],
";

        const string query1_5 = @"
@MachineType as [OsMachineType],
@ProductName as [OSVersionName],
";

        const string query1_6 = @"
RIGHT(SUBSTRING(@@VERSION, CHARINDEX('Windows NT', @@VERSION), 14), 3) as [OsVersionNumber],
SYSTEM_USER as [Currentlogin]
";
        [DataContract]
        public struct Details
        {
            [DataMember] public string ComputerName;
            [DataMember] public string Instance;
            [DataMember] public string DomainName;
            [DataMember] public int ServiceProcessID;
            [DataMember] public string ServiceName;
            [DataMember] public string ServiceAccount;
            [DataMember] public string AuthenticationMode;
            [DataMember] public int ForcedEncryption;
            [DataMember] public string Clustered;
            [DataMember] public string SQLServerVersionNumber;
            [DataMember] public string SQLServerMajorVersion;
            [DataMember] public string SQLServerEdition;
            [DataMember] public string SQLServerServicePack;
            [DataMember] public string OSArchitecture;
            [DataMember] public string OsMachineType;
            [DataMember] public string OSVersionName;
            [DataMember] public string OsVersionNumber;
            [DataMember] public string Currentlogin;
        }

        private Details details;

        internal SQLServerInfo(Credentials credentials) : base(credentials)
        {
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

                string query = query1_1;
                if (isSysAdmin) { query += query1_2; }
                query += string.Format("SELECT  \'{0}\' as [ComputerName],\n", computerName); ;
                query += query1_4;
                if (isSysAdmin) { query += query1_5; }
                query += query1_6;

                table = sql.Query(query);
            }

            foreach (DataRow row in table.AsEnumerable())
            {
                try
                {
                    details = new Details
                    {
                        ComputerName = (string)row["ComputerName"],
                        Instance = (string)row["Instance"],
                        DomainName = (string)row["DomainName"],
                        ServiceProcessID = (int)row["ServiceProcessID"],
                        ServiceName = (string)row["ServiceName"],
                        ServiceAccount = (string)row["ServiceAccount"],
                        AuthenticationMode = (string)row["AuthenticationMode"],
                        ForcedEncryption = (int)row["ForcedEncryption"],
                        Clustered = (string)row["Clustered"],
                        SQLServerVersionNumber = (string)row["SQLServerVersionNumber"],
                        SQLServerMajorVersion = (string)row["SQLServerMajorVersion"],
                        SQLServerEdition = (string)row["SQLServerEdition"],
                        SQLServerServicePack = (string)row["SQLServerServicePack"],
                        OSArchitecture = (string)row["OSArchitecture"],
                        OsVersionNumber = (string)row["OsVersionNumber"],
                        Currentlogin = (string)row["Currentlogin"]
                    };

                    if (isSysAdmin)
                    {
                        details.OsMachineType = (string)row["OsMachineType"];
                        details.OSVersionName = (string)row["OSVersionName"];
                    }
#if DEBUG
                    Misc.PrintStruct<Details>(details);
#endif
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
            return false;
        }

        internal Details GetResults()
        {
            return details;
        }
    }
}
