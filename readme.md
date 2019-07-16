## DAFT: Database Audit Framework & Toolkit 
This is a database auditing and assessment toolkit written in C# and inspired by <a href="https://github.com/NetSPI/PowerUpSQL/wiki">PowerUpSQL</a>.  Feel free to compile it yourself or download the release from <a href="https://github.com/NetSPI/DAFT/releases/tag/0.9.0">here</a>.

### DAFT: Common Command Examples
Below are a few common command examples to get you started.

#### List non-default databases
<pre>DAFT.exe -i "TEST-SYSTEM\SQLEXPRESS" -m "database" -n</pre>

#### List table for a database
<pre>DAFT.exe -i "TEST-SYSTEM\SQLEXPRESS" -d "database" -m "tables"</pre>

#### Search for senstive data by keyword
<pre>DAFT.exe -i "TEST-SYSTEM\SQLEXPRESS" -m "ColumnSampleData" --SearchKeywords="password,licence,ssn" --SampleSize=5</pre>

#### Search for senstive data by keyword and export results to json
<pre>DAFT.exe -i "TEST-SYSTEM\SQLEXPRESS" -m "ColumnSampleData" --SearchKeywords="password,licence,ssn" --SampleSize=5 -j -o "sensative_data_discovered.json"</pre>

#### Check for default or weak password
<pre>DAFT.exe -i "TEST-SYSTEM\SQLEXPRESS" -m "ServerLoginDefaultPw" -c -o "default_passwords_found.csv"</pre>

#### Execute command through SQL Server
<pre>DAFT.exe -i "Target\Instance" -m "OSCmd" -q "whoami"</pre>

### DAFT: Help
Since we lack a proper wiki at the moment below is help output for the tool.

<pre>
DAFT.exe -?

  _____              ______ _______
 |  __ \     /\     |  ____|__   __|
 | |  | |   /  \    | |__     | |
 | |  | |  / /\ \   |  __|    | |
 | |__| | / ____ \ _| |_      | |_
 |_____(_)_/    \_(_)_(_)     |_(_)
 Database Audit Framework & Toolkit

 A NetSPI Open Source Project
 @_nullbind, @0xbadjuju


=============================================================

=============================================================

  -a, --domaincontroller=VALUE
                             Domain Controller for LDAP Queries.
  -c, --csv                  CSV Output
  -d, --database=VALUE       Database Name
  -e, --dbcredentials=VALUE  Explict database credentials.
  -f, --filters=VALUE        Explict database credentials.
  -h, --hasaccess            Filter Database that are Accessible
  -i, --instance=VALUE       Instance Name
  -j, --json                 JSON Output
  -l, --inputlist=VALUE      Input Instance List
  -m, --module=VALUE         Module to Execute
  -n, --nodefaults           Filter Out Default Databases
  -o, --output=VALUE         Output CSV File.
  -q, --query=VALUE          Query/Command to Execute
  -r, --restorestate=VALUE   If server config is altered, return it to it's
                               original state
  -s, --sysadmin             Filter Database where SysAdmin Privileges
  -u, --credentials=VALUE    Credentials to Login With
  -v, --version=VALUE        Override version detection
  -x, --xml                  XML Output
  -?, --help                 Display this message and exit
      --SubsystemFilter=VALUE
                             Agent Job Subsystem Filter
      --KeywordFilter=VALUE  Agent Job and Stored Procedure Keyword Filter
      --UsingProxyCredFilter Agent Jobs using Proxy Credentials
      --ProxyCredentialFilter=VALUE
                             Agent Job using Specific Proxy
      --AssemblyNameFilter=VALUE
                             Assembly Name
      --ExportAssembly       Export Assemblies
      --ColumnFilter=VALUE   Exact Column Name Search Filter
      --ColumnSearchFilter=VALUE
                             Column Name Wildcard Search Filter
      --TableNameFilter=VALUE
                             Table Name to Retrieve Columns From
      --SearchKeywords=VALUE Column Name Search Keyword
      --ValidateCC           Validate Data Against Luhn Algorithm
      --SampleSize=VALUE     Number of Rows to Retrieve
      --PermissionNameFilter=VALUE
                             Permission Name Filter
      --PrincipalNameFilter=VALUE
                             Principal Name Filter
      --PermissionTypeFilter=VALUE
                             Database Permission Type Filter
      --RoleOwnerFilter=VALUE
                             Role Owner Filter
      --RolePrincipalNameFilter=VALUE
                             Role Principal Name Filter
      --SchemaFilter=VALUE   Database Schema Name Filter
      --DatabaseUserFilter=VALUE
                             Database UserName Filter
      --DatabaseLinkName=VALUE
                             Database Link Name Filter
      --StartId=VALUE        Fuzzing Start ID, Defaults to Zero
      --EndId=VALUE          Fuzzing End ID, Defaults to Five
      --CredentialNameFilter=VALUE
                             Database Link Name Filter
      --ProcedureNameFilter=VALUE
                             Database Link Name Filter
      --AutoExecFilter       Database Link Name Filter
      --ShowAllAssemblyFiles Database Link Name Filter
      --TriggerNameFilter=VALUE
                             Trigger Name Filter
      --CaptureUNCPath=VALUE UNC Path to Capture Hashes
      --AuditNameFilter=VALUE

      --AuditSpecificationFilter=VALUE
                             Agent Jobs using Proxy Credentials
      --AuditActionNameFilter=VALUE
                             Agent Job using Specific Proxy
=============================================================

Options per Method:

=============================================================

AgentJob:
        -i InstanceName
        --SubsystemFilter=SUBSYSTEM
        --KeywordFilter=KEYWORD
        --UsingProxyCredentials <Filter for Proxy Credentials>
        --ProxyCredentials=CREDENTIALS

AssemblyFile:
        -i InstanceName
        --AssemblyNameFilter=ASSEMBLY
        --ExportAssembly <Export Assemblies>

AuditDatabaseSpec:
        -i InstanceName

AuditPrivCreateProcedure:
        -i InstanceName

AuditPrivDbChaining:
        -i InstanceName

AuditPrivServerLink:
        -i InstanceName

AuditPrivTrustworthy:
        -i InstanceName

AuditPrivXpDirTree:
        -i InstanceName

AuditPrivXpFileExists:
        -i InstanceName

AuditRoleDbOwner:
        -i InstanceName

AuditServerSpec:
        -i InstanceName
        --AuditNameFilter=NAME
        --AuditSpecificationFilter=SPECIFICATION
        --AuditActionNameFilter=ACTION

AuditSQLiSpExecuteAs:
        -i InstanceName

AuditSQLiSpSigned:
        -i InstanceName

Column:
        -i InstanceName -d DatabaseName
        -n <No Defaults>
        -h <Has Access>
        -s <Is SysAdmin>
        --ColumnFilter=FILTER
        --ColumnSearchFilter=WILDCARD_FILTER

ColumnSampleData:
        -i InstanceName -d DatabaseName
        -n <No Defaults>
        -h <Has Access>
        -s <Is SysAdmin>
        --SearchKeywords=KEYWORDS
        --SampleSize=SIZE
        --ValidateCC <Run Luhn Algorithm on Results>

Connection:
        -i InstanceName

Database:
        -i InstanceName -d DatabaseName
        -n <No Defaults>
        -h <Has Access>
        -s <Is SysAdmin>

DatabasePriv:
        -i InstanceName -d DatabaseName
        -n <No Defaults>
        --PermissionNameFilter=PERMISSION
        --PrincipalNameFilter=PRINCIPAL
        --PermissionTypeFilter=PERMISSION

DatabaseRole:
        -i InstanceName -d DatabaseName
        -n <No Defaults>
        --RoleOwnerFilter=OWNER
        --RolePrincipalNameFilter=PRINCIPAL

DatabaseSchema:
        -i InstanceName -d DatabaseName
        -n <No Defaults>
        --SchemaFilter=SCHEMA

DatabaseUser:
        -i InstanceName -d DatabaseName
        -n <No Defaults>
        --DatabaseUserFilter=USER
        --PrincipalNameFilter=NAME

FuzzDatabaseName:
        -i InstanceName
        -StartId=0
        --EndId=5

FuzzDomainAccount:
        -i InstanceName
        -StartId=0
        --EndId=5

FuzzObjectName:
        -i InstanceName
        -StartId=0
        --EndId=5

FuzzServerLogin:
        -i InstanceName
        --EndId=5

OleDbProvider:
        -i InstanceName

OSCmd:
        -i InstanceName -q COMMAND --RestoreState <Undo any changes made to run command>

OSCmdAgentJob:
        -i InstanceName -q COMMAND

OSCmdOle:
        -i InstanceName -q COMMAND --RestoreState <Undo any changes made to run command>

OSCmdPython:
        -i InstanceName -q COMMAND --RestoreState <Undo any changes made to run command>

OSCmdR:
        -i InstanceName -q COMMAND --RestoreState <Undo any changes made to run command>

Query:
        -i InstanceName -q QUERY

ServerConfiguration:
        -i InstanceName

ServerCredential:
        -i InstanceName
        --CredentialNameFilter=CREDENTIAL

ServerInfo:
        -i InstanceName

ServerLink:
        -i InstanceName
        --DatabaseLinkName=LINK

ServerLinkCrawl:
        -i InstanceName -q QUERY

ServerLogin:
        -i InstanceName
        --PrincipalNameFilter=NAME

ServerLoginDefaultPw:
        -i InstanceName

ServerPasswordHash:
        -i InstanceName

ServerPriv:
        -i InstanceName
        --PermissionNameFilter=PERMISSION

ServerRole:
        -i InstanceName
        --RoleOwnerFilter=ROLE
        --RolePrincipalNameFilter=NAME

ServerRoleMember:
        -i InstanceName
        --PrincipalNameFilter=NAME

ServiceAccount:
        -i InstanceName

Session:
        -i InstanceName
        --PrincipalNameFilter=NAME

StoredProcedure:
        -i InstanceName
        --ProcedureNameFilter=NAME
        --KeywordFilter=KEYWORD
        --AutoExecFilter <Filter fore Auto Exec Stored Procedures>

StoredProcedureAutoExec:
        -i InstanceName
        --ProcedureNameFilter=NAME
        --KeywordFilter=KEYWORD

StoredProcedureCLR:
        -i InstanceName         -d DatabaseName
        -n <No Defaults>
        -h <Has Access>
        -s <Is SysAdmin>
        --ShowAllAssemblyFiles <Display all Assemblies>

StoredProcedureXP:
        -i InstanceName         -d DatabaseName
        -n <No Defaults>
        -h <Has Access>
        -s <Is SysAdmin>
        --ProcedureNameFilter=NAME

SysAdminCheck:
        -i InstanceName

Tables:
        -i InstanceName         -d DatabaseName
        -n <No Defaults>
        -h <Has Access>
        -s <Is SysAdmin>

TriggerDdl:
        -i InstanceName         -d DatabaseName
        -n <No Defaults>
        -h <Has Access>
        -s <Is SysAdmin>
        --TriggerNameFilter=TRIGGER

TriggerDml:
        -i InstanceName         -d DatabaseName
        -n <No Defaults>
        -h <Has Access>
        -s <Is SysAdmin>
        --TriggerNameFilter=TRIGGER

UncPathInjection:
        -i InstanceName         --UNCPath=\\IP\PATH

View:
        -i InstanceName         -d DatabaseName
        -n <No Defaults>
        -h <Has Access>
        --TableNameFilter=TABLE
  </pre>

### Authors
* Alexander Leary (@0xbadjuju) and Scott Sutherland (@_nullbind)

### License
* BSD 3-Clause
