using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DAFT.Modules
{
    class SQLOleDbProvider : Module
    {
        const string QUERY1_1 = @"
-- Name: Get-SQLOleDbProvider.sql
-- Description: Get a list of OLE provider along with their current settings.
-- Author: Scott Sutherland, NetSPI 2017

-- Get a list of providers
CREATE TABLE #Providers ([ProviderName] varchar(8000), 
[ParseName] varchar(8000),
[ProviderDescription] varchar(8000))

INSERT INTO #Providers
EXEC xp_enum_oledb_providers

-- Create temp table for provider information
CREATE TABLE #ProviderInformation ([ProviderName] varchar(8000), 
[ProviderDescription] varchar(8000),
[ProviderParseName] varchar(8000),
[AllowInProcess] int, 
[DisallowAdHocAccess] int, 
[DynamicParameters] int,  
[IndexAsAccessPath] int,  
[LevelZeroOnly] int,  
[NestedQueries] int,  
[NonTransactedUpdates] int,  
[SqlServerLIKE] int)

-- Setup required variables for cursor
DECLARE @Provider_name varchar(8000);
DECLARE @Provider_parse_name varchar(8000);
DECLARE @Provider_description varchar(8000);
DECLARE @property_name varchar(8000)
DECLARE @regpath nvarchar(512)  

-- Start cursor
DECLARE MY_CURSOR1 CURSOR
FOR
SELECT * FROM #Providers
OPEN MY_CURSOR1
FETCH NEXT FROM MY_CURSOR1 INTO @Provider_name,@Provider_parse_name,@Provider_description
WHILE @@FETCH_STATUS = 0 
  
	BEGIN  
		
	-- Set the registry path
	SET @regpath = N'SOFTWARE\Microsoft\MSSQLServer\Providers\' + @provider_name  

	-- AllowInProcess	
	    DECLARE @AllowInProcess int 
	    SET @AllowInProcess = 0 
	    exec sys.xp_instance_regread N'HKEY_LOCAL_MACHINE',@regpath,'AllowInProcess',	@AllowInProcess OUTPUT		 
	    IF @AllowInProcess IS NULL 
	    SET @AllowInProcess = 0

	-- DisallowAdHocAccess 
	    DECLARE @DisallowAdHocAccess int  
	    SET @DisallowAdHocAccess = 0
	    exec sys.xp_instance_regread N'HKEY_LOCAL_MACHINE',@regpath,'DisallowAdHocAccess',	@DisallowAdHocAccess OUTPUT	 
	    IF @DisallowAdHocAccess IS NULL 
	    SET @DisallowAdHocAccess = 0

	-- DynamicParameters 
	    DECLARE @DynamicParameters  int  
	    SET @DynamicParameters  = 0
	    exec sys.xp_instance_regread N'HKEY_LOCAL_MACHINE',@regpath,'DynamicParameters',	@DynamicParameters OUTPUT	 
	    IF @DynamicParameters  IS NULL 
	    SET @DynamicParameters  = 0

	-- IndexAsAccessPath 
	    DECLARE @IndexAsAccessPath int 
	    SET @IndexAsAccessPath = 0 
	    exec sys.xp_instance_regread N'HKEY_LOCAL_MACHINE',@regpath,'IndexAsAccessPath',	@IndexAsAccessPath OUTPUT	 
	    IF @IndexAsAccessPath IS NULL 
	    SET @IndexAsAccessPath  = 0

	-- LevelZeroOnly 
	    DECLARE @LevelZeroOnly int
	    SET @LevelZeroOnly  = 0
	    exec sys.xp_instance_regread N'HKEY_LOCAL_MACHINE',@regpath,'LevelZeroOnly',	@LevelZeroOnly OUTPUT	
	    IF  @LevelZeroOnly IS NULL 
	    SET  @LevelZeroOnly  = 0	  

	-- NestedQueries 
	    DECLARE @NestedQueries int  
	    SET @NestedQueries = 0
	    exec sys.xp_instance_regread N'HKEY_LOCAL_MACHINE',@regpath,'NestedQueries',	@NestedQueries OUTPUT
	    IF   @NestedQueries IS NULL 
	    SET  @NestedQueries = 0		 	 

	-- NonTransactedUpdates 
	    DECLARE @NonTransactedUpdates int  
	    SET @NonTransactedUpdates = 0
	    exec sys.xp_instance_regread N'HKEY_LOCAL_MACHINE',@regpath,'NonTransactedUpdates',	@NonTransactedUpdates  OUTPUT	 
	    IF  @NonTransactedUpdates IS NULL 
	    SET @NonTransactedUpdates = 0

	-- SqlServerLIKE
	    DECLARE @SqlServerLIKE int  
	    SET @SqlServerLIKE  = 0
	    exec sys.xp_instance_regread N'HKEY_LOCAL_MACHINE',@regpath,'SqlServerLIKE',	@SqlServerLIKE OUTPUT	
	    IF  @SqlServerLIKE IS NULL 
	    SET @SqlServerLIKE = 0 

	-- Add the full provider record to the temp table
	INSERT INTO #ProviderInformation
	VALUES (@Provider_name,@Provider_description,@Provider_parse_name,@AllowInProcess,@DisallowAdHocAccess,@DynamicParameters,@IndexAsAccessPath,@LevelZeroOnly,@NestedQueries,@NonTransactedUpdates,@SqlServerLIKE);

	FETCH NEXT FROM MY_CURSOR1 INTO  @Provider_name,@Provider_parse_name,@Provider_description

	END   

-- Return records
SELECT * FROM #ProviderInformation

-- Clean up
CLOSE MY_CURSOR1
DEALLOCATE MY_CURSOR1
DROP TABLE #Providers
DROP TABLE #ProviderInformation
";
        [DataContract]
        public struct OleProvider
        {
            [DataMember] public string ComputerName;
            [DataMember] public string Instance;
            [DataMember] public string ProviderName;
            [DataMember] public string ProviderDescription;
            [DataMember] public string ProviderParseName;
            [DataMember] public int AllowInProcess;
            [DataMember] public int DisallowAdHocAccess;
            [DataMember] public int DynamicParameters;
            [DataMember] public int IndexAsAccessPath;
            [DataMember] public int LevelZeroOnly;
            [DataMember] public int NestedQueries;
            [DataMember] public int NonTransactedUpdates;
            [DataMember] public int SqlServerLIKE;
        }

        private List<OleProvider> oleProviders;

        internal SQLOleDbProvider(Credentials credentials) : base(credentials)
        {
            oleProviders = new List<OleProvider>();
        }

        internal override bool Query()
        {
            using (SQLConnection sql = new SQLConnection(instance))
            {
                sql.BuildConnectionString(credentials);
                if (!sql.Connect())
                    return false;

                oleProviders = sql.Query<OleProvider>(QUERY1_1, new OleProvider());
            }
            return true;
        }

        internal List<OleProvider> GetResults()
        {
            return oleProviders;
        }
    }
}