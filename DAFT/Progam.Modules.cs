using System;
using System.Collections.Generic;

using DAFT.Modules;

namespace DAFT
{
    sealed partial class App
    {
        private void _SQLAgentJob(SqlInstances sqlInstances)
        {
            SQLAgentJob sAJ = new SQLAgentJob(credentials);
            sAJ.SetComputerName(sqlInstances.Server);
            sAJ.SetInstance(sqlInstances.ServerInstance);
            if (!string.IsNullOrEmpty(subsystemFilter)) { sAJ.SetSubsystemFilter(subsystemFilter); };
            if (!string.IsNullOrEmpty(keywordFilter)) { sAJ.SetKeywordFilter(keywordFilter); };
            if (usingProxyCredentials) { sAJ.SetUsingProxyCredFilter(); };
            if (!string.IsNullOrEmpty(proxyCredentials)) { sAJ.SetProxyCredentialFilter(proxyCredentials); };
            sAJ.Query();
            _PrintOutput(sAJ.GetResults());
        }

        private void _SQLAssemblyFile(SqlInstances sqlInstances)
        {
            SQLAssemblyFile sAF = new SQLAssemblyFile(credentials);
            sAF.SetComputerName(sqlInstances.Server);
            sAF.SetInstance(sqlInstances.ServerInstance);
            if (!string.IsNullOrEmpty(assemblyNameFilter)) { sAF.SetAssemblyNameFilter(assemblyNameFilter); };
            if (exportAssembly) { sAF.SetExportAssembly(); };
            sAF.Query();
            foreach (var a in sAF.GetResults())
                Misc.PrintStruct(a);
        }

        private void _SQLAuditDatabaseSpec(SqlInstances sqlInstances)
        {
            SQLAuditDatabaseSpec sADS = new SQLAuditDatabaseSpec(credentials);
            sADS.SetComputerName(sqlInstances.Server);
            sADS.SetInstance(sqlInstances.ServerInstance);
            if (!string.IsNullOrEmpty(subsystemFilter)) { sADS.SetAuditNameFilter(AuditNameFilter); }
            if (!string.IsNullOrEmpty(subsystemFilter)) { sADS.SetAuditSpecificationFilter(AuditSpecificationFilter); }
            if (!string.IsNullOrEmpty(subsystemFilter)) { sADS.SetAuditActionNameFilter(AuditActionNameFilter); }
            sADS.Query();
            _PrintOutput(sADS.GetResults());
        }

        private void _SQLAuditPrivAutoExecSp(SqlInstances sqlInstances)
        {
            if (0 == databases.Count)
            {
                sD = new SQLDatabase(credentials);
                sD.SetInstance(sqlInstances.ServerInstance);
                if (nodefaults) { sD.EnableNoDefaultsFilter(); }
                if (hasAccess) { sD.EnableHasAccessFilter(); }
                if (sysadmin) { sD.EnableSysAdminFilter(); }
                sD.Query();
                databases = sD.GetResults();
            }
            SQLAuditPrivAutoExecSp sAPAES = new SQLAuditPrivAutoExecSp(credentials);
            sAPAES.SetComputerName(sqlInstances.Server);
            sAPAES.SetInstance(sqlInstances.ServerInstance);
            if (!string.IsNullOrEmpty(database)) { sAPAES.SetDatabase(database); }
            var results = new List<SQLAuditPrivAutoExecSp.Trustworthy>();
            foreach (var d in databases)
            {
                sAPAES.SetDatabase(d.DatabaseName);
                sAPAES.Query();
                results.AddRange(sAPAES.GetResults());
            }
            _PrintOutput(results);
        }

        private void _SQLAuditPrivCreateProcedure(SqlInstances sqlInstances)
        {
            SQLAuditPrivCreateProcedure sAPCP = new SQLAuditPrivCreateProcedure(credentials);
            sAPCP.SetComputerName(sqlInstances.Server);
            sAPCP.SetInstance(sqlInstances.ServerInstance);
            sAPCP.Query();
            _PrintOutput(sAPCP.GetResults());
        }

        private void _SQLAuditPrivDbChaining(SqlInstances sqlInstances)
        {
            SQLAuditPrivDbChaining sAPDC = new SQLAuditPrivDbChaining(credentials);
            sAPDC.SetComputerName(sqlInstances.Server);
            sAPDC.SetInstance(sqlInstances.ServerInstance);
            sAPDC.Query();
            _PrintOutput(sAPDC.GetResults());
        }

        private void _SQLAuditPrivImpersonateLogin(SqlInstances sqlInstances)
        {
            SQLAuditPrivImpersonateLogin sAPDC = new SQLAuditPrivImpersonateLogin(credentials);
            sAPDC.SetComputerName(sqlInstances.Server);
            sAPDC.SetInstance(sqlInstances.ServerInstance);
            sAPDC.Query();
            _PrintOutput(sAPDC.GetResults());
        }

        private void _SQLAuditPrivServerLink(SqlInstances sqlInstances)
        {
            SQLAuditPrivServerLink sAPSL = new SQLAuditPrivServerLink(credentials);
            sAPSL.SetComputerName(sqlInstances.Server);
            sAPSL.SetInstance(sqlInstances.ServerInstance);
            sAPSL.Query();
            _PrintOutput(sAPSL.GetResults());
        }

        private void _SQLAuditPrivTrustworthy(SqlInstances sqlInstances)
        {
            SQLAuditPrivTrustworthy sAPT = new SQLAuditPrivTrustworthy(credentials);
            sAPT.SetComputerName(sqlInstances.Server);
            sAPT.SetInstance(sqlInstances.ServerInstance);
            sAPT.Query();
            _PrintOutput(sAPT.GetResults());
        }

        private void _SQLAuditPrivXpDirTree(SqlInstances sqlInstances)
        {
            SQLAuditPrivXp sAPX = new SQLAuditPrivXp(credentials);
            sAPX.SetComputerName(sqlInstances.Server);
            sAPX.SetInstance(sqlInstances.ServerInstance);
            sAPX.SetExtendedProcedure("xp_dirtree");
            sAPX.Query();
            _PrintOutput(sAPX.GetResults());
        }

        private void _SQLAuditPrivXpFileExists(SqlInstances sqlInstances)
        {
            SQLAuditPrivXp sAPX = new SQLAuditPrivXp(credentials);
            sAPX.SetComputerName(sqlInstances.Server);
            sAPX.SetInstance(sqlInstances.ServerInstance);
            sAPX.SetExtendedProcedure("xp_fileexists");
            sAPX.Query();
            _PrintOutput(sAPX.GetResults());
        }

        private void _SQLAuditRoleDbOwner(SqlInstances sqlInstances)
        {
            SQLAuditRole sARD = new SQLAuditRole(credentials);
            sARD.SetComputerName(sqlInstances.Server);
            sARD.SetInstance(sqlInstances.ServerInstance);
            sARD.SetRole("DB_OWNER");
            sARD.Query();
            _PrintOutput(sARD.GetResults());
        }

        private void _SQLAuditRoleDBDDLADMIN(SqlInstances sqlInstances)
        {
            SQLAuditRole sARD = new SQLAuditRole(credentials);
            sARD.SetComputerName(sqlInstances.Server);
            sARD.SetInstance(sqlInstances.ServerInstance);
            sARD.SetRole("DB_DDLADMIN");
            sARD.Query();
            _PrintOutput(sARD.GetResults());
        }

        private void _SQLAuditServerSpec(SqlInstances sqlInstances)
        {
            SQLAuditServerSpec sADS = new SQLAuditServerSpec(credentials);
            sADS.SetComputerName(sqlInstances.Server);
            sADS.SetInstance(sqlInstances.ServerInstance);
            if (!string.IsNullOrEmpty(AuditNameFilter)) { sADS.SetAuditNameFilter(AuditNameFilter); }
            if (!string.IsNullOrEmpty(AuditSpecificationFilter)) { sADS.SetAuditSpecificationFilter(AuditSpecificationFilter); }
            if (!string.IsNullOrEmpty(AuditActionNameFilter)) { sADS.SetAuditActionNameFilter(AuditActionNameFilter); }
            sADS.Query();
            _PrintOutput(sADS.GetResults());
        }

        private void _SQLAuditSQLiSpExecuteAs(SqlInstances sqlInstances)
        {
            SQLAuditSQLiSpExecuteAs sAPX = new SQLAuditSQLiSpExecuteAs(credentials);
            sAPX.SetComputerName(sqlInstances.Server);
            sAPX.SetInstance(sqlInstances.ServerInstance);
            sAPX.Query();
            _PrintOutput(sAPX.GetResults());
        }

        private void _SQLAuditSQLiSpSigned(SqlInstances sqlInstances)
        {
            SQLAuditSQLiSpSigned sASSS = new SQLAuditSQLiSpSigned(credentials);
            sASSS.SetComputerName(sqlInstances.Server);
            sASSS.SetInstance(sqlInstances.ServerInstance);
            sASSS.Query();
            _PrintOutput(sASSS.GetResults());
        }

        private void _SQLColumn(SqlInstances sqlInstances)
        {
            if (0 == databases.Count)
            {
                sD = new SQLDatabase(credentials);
                sD.SetInstance(sqlInstances.ServerInstance);
                if (nodefaults) { sD.EnableNoDefaultsFilter(); }
                if (hasAccess) { sD.EnableHasAccessFilter(); }
                if (sysadmin) { sD.EnableSysAdminFilter(); }
                sD.Query();
                databases = sD.GetResults();
            }
            SQLColumn sC = new SQLColumn(credentials);
            sC.SetComputerName(sqlInstances.Server);
            sC.SetInstance(sqlInstances.ServerInstance);
            var results = new List<SQLColumn.Column>();
            foreach (var d in sD.GetResults())
            {
                sC.SetDatabase(d.DatabaseName);
                if (!string.IsNullOrEmpty(ColumnFilter)) { sC.SetColumnFilter(ColumnFilter); }
                if (!string.IsNullOrEmpty(ColumnSearchFilter))
                {
                    foreach (var c in ColumnSearchFilter.Split(','))
                    {
                        sC.AddColumnSearchFilter(c);
                    }
                }
                if (!string.IsNullOrEmpty(TableNameFilter)) { sC.SetTableNameFilter(TableNameFilter); }
                sC.Query();
                results.AddRange(sC.GetResults());
            }
            _PrintOutput(results);
        }

        private void _SQLColumnSampleData(SqlInstances sqlInstances)
        {
            if (0 == databases.Count)
            {
                sD = new SQLDatabase(credentials);
                sD.SetInstance(sqlInstances.ServerInstance);
                if (nodefaults) { sD.EnableNoDefaultsFilter(); }
                if (hasAccess) { sD.EnableHasAccessFilter(); }
                if (sysadmin) { sD.EnableSysAdminFilter(); }
                sD.Query();
                databases = sD.GetResults();
            }

            SQLColumnSampleData sCSD = new SQLColumnSampleData(credentials);
            Console.WriteLine("=====");
            Console.WriteLine("Instance : {0}", sqlInstances.ServerInstance);
            Console.WriteLine("=====");
            Console.WriteLine("{0,-20}{1,-20}{2,-20}{3,-20}", "Database", "Table", "Column", "Data");
            Console.WriteLine("{0,-20}{1,-20}{2,-20}{3,-20}", "========", "=====", "======", "====");

            sCSD.SetComputerName(sqlInstances.Server);
            sCSD.SetInstance(sqlInstances.ServerInstance);
            var results = new List<SQLColumnSampleData.SampleData>();
            foreach (var d in sD.GetResults())
            {
                sCSD.SetDatabase(d.DatabaseName);
                foreach (var f in SearchKeywords.Split(','))
                {
                    sCSD.AddSearchKeywords(f);
                }
                if (ValidateCC) { sCSD.EnableValidateCC(); }
                if (!string.IsNullOrEmpty(SampleSize))
                {
                    int size = 5;
                    if (int.TryParse(SampleSize, out size))
                    {
                        sCSD.SetSampleSize(size);
                    }
                }
                sCSD.Query();
                var r = sCSD.GetResults();
                results.AddRange(r);
                foreach (var s in r)
                {
                    Console.WriteLine("{0,-20}{1,-20}{2,-20}{3,-20}", s.DatabaseName, s.TableName, s.ColumnName, s.ColumnData);
                }
            }
            _PrintOutput(results);
        }

        private void _SQLConnection(SqlInstances sqlInstances)
        {
            using (SQLConnection sql = new SQLConnection(sqlInstances.ServerInstance))
            {
                sql.BuildConnectionString(credentials);
                sql.Connect();
                connections.Add(sql.GetConnectionResult());
            }
        }

        private void _SQLDatabase(SqlInstances sqlInstances)
        {
            sD = new SQLDatabase(credentials);
            sD.SetComputerName(sqlInstances.Server);
            sD.SetInstance(sqlInstances.ServerInstance);
            if (nodefaults) { sD.EnableNoDefaultsFilter(); }
            if (hasAccess) { sD.EnableHasAccessFilter(); }
            if (sysadmin) { sD.EnableSysAdminFilter(); }
            sD.Query();
            _PrintOutput(sD.GetResults());
        }

        private void _SQLDatabasePriv(SqlInstances sqlInstances)
        {
            if (0 == databases.Count)
            {
                sD = new SQLDatabase(credentials);
                sD.SetComputerName(sqlInstances.Server);
                sD.SetInstance(sqlInstances.ServerInstance);
                if (nodefaults) { sD.EnableNoDefaultsFilter(); }
                if (hasAccess) { sD.EnableHasAccessFilter(); }
                if (sysadmin) { sD.EnableSysAdminFilter(); }
                sD.Query();
                databases = sD.GetResults();
            }
            SQLDatabasePriv sDP = new SQLDatabasePriv(credentials);
            sDP.SetComputerName(sqlInstances.Server);
            sDP.SetInstance(sqlInstances.ServerInstance);
            if (!string.IsNullOrEmpty(PermissionNameFilter)) { sDP.SetPermissionNameFilter(PermissionNameFilter); };
            if (!string.IsNullOrEmpty(PrincipalNameFilter)) { sDP.SetPrincipalNameFilter(PrincipalNameFilter); };
            if (!string.IsNullOrEmpty(PermissionTypeFilter)) { sDP.SetPermissionTypeFilter(PermissionTypeFilter); };
            sDP.SetComputerName(sqlInstances.Server);
            sDP.SetInstance(sqlInstances.ServerInstance);
            var results = new List<SQLDatabasePriv.DatabasePrivilege>();
            foreach (var d in databases)
            {
                sDP.SetDatabase(d.DatabaseName);
                sDP.Query();
                results.AddRange(sDP.GetResults());
            }
            _PrintOutput(results);
        }

        private void _SQLDatabaseRole(SqlInstances sqlInstances)
        {
            if (0 == databases.Count)
            {
                sD = new SQLDatabase(credentials);
                sD.SetComputerName(sqlInstances.Server);
                sD.SetInstance(sqlInstances.ServerInstance);
                if (nodefaults) { sD.EnableNoDefaultsFilter(); }
                if (hasAccess) { sD.EnableHasAccessFilter(); }
                if (sysadmin) { sD.EnableSysAdminFilter(); }
                sD.Query();
                databases = sD.GetResults();
            }
            SQLDatabaseRole sDR = new SQLDatabaseRole(credentials);
            sDR.SetComputerName(sqlInstances.Server);
            sDR.SetInstance(sqlInstances.ServerInstance);
            if (!string.IsNullOrEmpty(RoleOwnerFilter)) { sDR.SetRoleOwnerFilter(RoleOwnerFilter); }
            if (!string.IsNullOrEmpty(RolePrincipalNameFilter)) { sDR.SetRolePrincipalNameFilter(RolePrincipalNameFilter); }
            var results = new List<SQLDatabaseRole.DatabaseRole>();
            foreach (var d in databases)
            {
                sDR.SetDatabase(d.DatabaseName);
                sDR.Query();
                results.AddRange(sDR.GetResults());
            }
            _PrintOutput(results);
        }

        private void _SQLDatabaseRoleMember(SqlInstances sqlInstances)
        {
            if (0 == databases.Count)
            {
                sD = new SQLDatabase(credentials);
                sD.SetComputerName(sqlInstances.Server);
                sD.SetInstance(sqlInstances.ServerInstance);
                if(nodefaults) { sD.EnableNoDefaultsFilter(); }
                if (hasAccess) { sD.EnableHasAccessFilter(); }
                if (sysadmin) { sD.EnableSysAdminFilter(); }
                sD.Query();
                databases = sD.GetResults();
            }
            SQLDatabaseRoleMember sDRM = new SQLDatabaseRoleMember(credentials);
            sDRM.SetComputerName(sqlInstances.Server);
            sDRM.SetInstance(sqlInstances.ServerInstance);
            if (!string.IsNullOrEmpty(PrincipalNameFilter)) { sDRM.SetPrincipalNameFilter(PrincipalNameFilter); }
            if (!string.IsNullOrEmpty(RolePrincipalNameFilter)) { sDRM.SetRolePrincipalNameFilter(RolePrincipalNameFilter); }
            var results = new List<SQLDatabaseRoleMember.UserRole>();
            foreach (var d in databases)
            {
                sDRM.SetDatabase(d.DatabaseName);
                sDRM.Query();
                results.AddRange(sDRM.GetResults());
            }
            _PrintOutput(results);
        }

        private void _SQLDatabaseSchema(SqlInstances sqlInstances)
        {
            if (0 == databases.Count)
            {
                sD = new SQLDatabase(credentials);
                sD.SetComputerName(sqlInstances.Server);
                sD.SetInstance(sqlInstances.ServerInstance);
                if (nodefaults) { sD.EnableNoDefaultsFilter(); }
                if (hasAccess) { sD.EnableHasAccessFilter(); }
                if (sysadmin) { sD.EnableSysAdminFilter(); }
                sD.Query();
                databases = sD.GetResults();
            }
            SQLDatabaseSchema sDS = new SQLDatabaseSchema(credentials);
            sDS.SetComputerName(sqlInstances.Server);
            sDS.SetInstance(sqlInstances.ServerInstance);
            if (!string.IsNullOrEmpty(SchemaFilter)) { sDS.SetSchemaFilter(SchemaFilter); }
            var results = new List<SQLDatabaseSchema.Schema>();
            foreach (var d in databases)
            {
                sDS.SetDatabase(d.DatabaseName);
                sDS.Query();
                results.AddRange(sDS.GetResults());
            }
            _PrintOutput(results);
        }

        private void _SQLDatabaseUser(SqlInstances sqlInstances)
        {
            if (0 == databases.Count)
            {
                sD = new SQLDatabase(credentials);
                sD.SetComputerName(sqlInstances.Server);
                sD.SetInstance(sqlInstances.ServerInstance);
                if (nodefaults) { sD.EnableNoDefaultsFilter(); }
                if (hasAccess) { sD.EnableHasAccessFilter(); }
                if (sysadmin) { sD.EnableSysAdminFilter(); }
                sD.Query();
                databases = sD.GetResults();
            }
            SQLDatabaseUser sDU = new SQLDatabaseUser(credentials);
            sDU.SetComputerName(sqlInstances.Server);
            sDU.SetInstance(sqlInstances.ServerInstance);
            if (!string.IsNullOrEmpty(DatabaseUserFilter)) { sDU.SetDatabaseUserFilter(DatabaseUserFilter); }
            if (!string.IsNullOrEmpty(PrincipalNameFilter)) { sDU.SetPrincipalNameFilter(PrincipalNameFilter); }
            var results = new List<SQLDatabaseUser.DatabaseUsers>();
            foreach (var d in databases)
            {
                sDU.SetDatabase(d.DatabaseName);
                sDU.Query();
                results.AddRange(sDU.GetResults());
            }
            _PrintOutput(results);
        }

        private void _SQLFuzzDatabaseName(SqlInstances sqlInstances)
        {
            SQLFuzzDatabaseName sFDN = new SQLFuzzDatabaseName(credentials);
            sFDN.SetComputerName(sqlInstances.Server);
            sFDN.SetInstance(sqlInstances.ServerInstance);

            int nStartId = 0;
            if(int.TryParse(StartId, out nStartId))
                sFDN.SetStartId(nStartId);

            int nEndId = 5;
            if (int.TryParse(EndId, out nEndId))
                sFDN.SetEndId(nEndId);

            sFDN.Query();
            _PrintOutput(sFDN.GetResults());
        }

        private void _SQLFuzzDomainAccount(SqlInstances sqlInstances)
        {
            SQLFuzzDomainAccount sFDA = new SQLFuzzDomainAccount(credentials);
            sFDA.SetComputerName(sqlInstances.Server);
            sFDA.SetInstance(sqlInstances.ServerInstance);

            int nStartId = 0;
            if (int.TryParse(StartId, out nStartId))
                sFDA.SetStartId(nStartId);

            int nEndId = 5;
            if (int.TryParse(EndId, out nEndId))
                sFDA.SetEndId(nEndId);

            sFDA.Query();
            _PrintOutput(sFDA.GetResults());
        }

        private void _SQLFuzzObjectName(SqlInstances sqlInstances)
        {
            SQLFuzzObjectName sFON = new SQLFuzzObjectName(credentials);
            sFON.SetComputerName(sqlInstances.Server);
            sFON.SetInstance(sqlInstances.ServerInstance);

            int nStartId = 0;
            if (int.TryParse(StartId, out nStartId))
                sFON.SetStartId(nStartId);

            int nEndId = 5;
            if (int.TryParse(EndId, out nEndId))
                sFON.SetEndId(nEndId);

            sFON.Query();
            _PrintOutput(sFON.GetResults());
        }

        private void _SQLFuzzServerLogin(SqlInstances sqlInstances)
        {
            SQLFuzzServerLogin sFSL = new SQLFuzzServerLogin(credentials);
            sFSL.SetComputerName(sqlInstances.Server);
            sFSL.SetInstance(sqlInstances.ServerInstance);

            int nEndId = 5;
            if (int.TryParse(EndId, out nEndId))
                sFSL.SetEndId(nEndId);

            sFSL.Query();
            _PrintOutput(sFSL.GetResults());
        }

        private void _SQLOleDbProvider(SqlInstances sqlInstances)
        {
            SQLOleDbProvider sODP = new SQLOleDbProvider(credentials);
            sODP.SetComputerName(sqlInstances.Server);
            sODP.SetInstance(sqlInstances.ServerInstance);
            sODP.Query();
            _PrintOutput(sODP.GetResults());
        }

        private void _SQLOSCmd(SqlInstances sqlInstances)
        {
            SQLOSCmd sOC = new SQLOSCmd(credentials);
            if (restoreState) { sOC.RestoreState(); }
            sOC.SetComputerName(sqlInstances.Server);
            sOC.SetInstance(sqlInstances.ServerInstance);
            sOC.Query(query);
        }

        private void _SQLOSCmdAgentJob(SqlInstances sqlInstances)
        {
            SQLOSCmdAgentJob sOC = new SQLOSCmdAgentJob(credentials);
            sOC.SetComputerName(sqlInstances.Server);
            sOC.SetInstance(sqlInstances.ServerInstance);
            sOC.SetSubSystem("jscript");
            sOC.Query(query);
        }

        private void _SQLOSCmdOle(SqlInstances sqlInstances)
        {
            SQLOSCmdOle sOCO = new SQLOSCmdOle(credentials);
            sOCO.RestoreState();
            sOCO.SetComputerName(sqlInstances.Server);
            sOCO.SetInstance(sqlInstances.ServerInstance);
            sOCO.Query(query);
        }

        private void _SQLOSCmdPython(SqlInstances sqlInstances)
        {
            SQLOSCmdPython sOCP = new SQLOSCmdPython(credentials);
            sOCP.RestoreState();
            sOCP.SetComputerName(sqlInstances.Server);
            sOCP.SetInstance(sqlInstances.ServerInstance);
            sOCP.Query(query);
        }

        private void _SQLOSCmdR(SqlInstances sqlInstances)
        {
            SQLOSCmdR sOCR = new SQLOSCmdR(credentials);
            sOCR.RestoreState();
            sOCR.SetComputerName(sqlInstances.Server);
            sOCR.SetInstance(sqlInstances.ServerInstance);
            sOCR.Query(query);
        }

        private void _SQLQuery(SqlInstances sqlInstances)
        {
            SQLQuery sQ = new SQLQuery(credentials);
            sQ.SetInstance(sqlInstances.ServerInstance);
            sQ.Query(query);
        }

        private void _SQLServerConfiguration(SqlInstances sqlInstances)
        {
            SQLServerConfiguration sSC = new SQLServerConfiguration(credentials);
            sSC.SetComputerName(sqlInstances.Server);
            sSC.SetInstance(sqlInstances.ServerInstance);
            sSC.Query();
            _PrintOutput(sSC.GetResults());
        }

        private void _SQLServerCredential(SqlInstances sqlInstances)
        {
            SQLServerCredential sSC = new SQLServerCredential(credentials);
            sSC.SetComputerName(sqlInstances.Server);
            sSC.SetInstance(sqlInstances.ServerInstance);
            if (!string.IsNullOrEmpty(CredentialNameFilter)) { sSC.SetCredentialNameFilter(CredentialNameFilter); }
            sSC.Query();
            _PrintOutput(sSC.GetResults());
        }

        private void _SQLServerInfo(SqlInstances sqlInstances)
        {
            SQLServerInfo sSI = new SQLServerInfo(credentials);
            sSI.SetComputerName(sqlInstances.Server);
            sSI.SetInstance(sqlInstances.ServerInstance);
            sSI.Query();
            _PrintOutput(sSI.GetResults());
        }

        private void _SQLServerLink(SqlInstances sqlInstances)
        {
            SQLServerLink sSL = new SQLServerLink(credentials);
            sSL.SetComputerName(sqlInstances.Server);
            sSL.SetInstance(sqlInstances.ServerInstance);
            if (!string.IsNullOrEmpty(DatabaseLinkName)) { sSL.SetDatabaseLinkName(DatabaseLinkName); }
            sSL.Query();
            _PrintOutput(sSL.GetResults());
        }

        private void _SQLServerLinkCrawl(SqlInstances sqlInstances)
        {
            SQLServerLinkCrawl sSLC = new SQLServerLinkCrawl(credentials);
            sSLC.SetComputerName(sqlInstances.Server);
            sSLC.SetInstance(sqlInstances.ServerInstance);
            if (!string.IsNullOrEmpty(query)) { sSLC.SetQuery(query); }
            sSLC.Query();
            Dictionary<string,SQLServerLinkCrawl.ServerLink> links = sSLC.GetResults();
            foreach (var l in links.Keys)
            {
                 Misc.PrintStruct(links[l]);
            }
        }

        private void _SQLServerLogin(SqlInstances sqlInstances)
        {
            SQLServerLogin sSL = new SQLServerLogin(credentials);
            sSL.SetComputerName(sqlInstances.Server);
            sSL.SetInstance(sqlInstances.ServerInstance);
            if (!string.IsNullOrEmpty(PrincipalNameFilter)) { sSL.SetPrincipalNameFilter(PrincipalNameFilter); }
            sSL.Query();
            _PrintOutput(sSL.GetResults());
        }

        private void _SQLServerLoginDefaultPw(SqlInstances sqlInstances)
        {
            SQLServerLoginDefaultPw sSLDP = new SQLServerLoginDefaultPw(credentials);
            sSLDP.SetComputerName(sqlInstances.Server);
            sSLDP.SetInstance(sqlInstances.ServerInstance);
            sSLDP.InjestConfig();
            sSLDP.Query();
        }

        private void _SQLServerPasswordHash(SqlInstances sqlInstances)
        {
            SQLServerPasswordHash sPH = new SQLServerPasswordHash(credentials);
            sPH.SetComputerName(sqlInstances.Server);
            sPH.SetInstance(sqlInstances.ServerInstance);
            sPH.Query();
            _PrintOutput(sPH.GetResults());
        }

        private void _SQLServerPriv(SqlInstances sqlInstances)
        {
            SQLServerPriv sSP = new SQLServerPriv(credentials);
            sSP.SetComputerName(sqlInstances.Server);
            sSP.SetInstance(sqlInstances.ServerInstance);
            if (!string.IsNullOrEmpty(PermissionNameFilter)) { sSP.SetPermissionNameFilter(PermissionNameFilter); }
            sSP.Query();
            _PrintOutput(sSP.GetResults());
        }

        private void _SQLServerRole(SqlInstances sqlInstances)
        {
            SQLServerRole sSR = new SQLServerRole(credentials);
            sSR.SetComputerName(sqlInstances.Server);
            sSR.SetInstance(sqlInstances.ServerInstance);
            if (!string.IsNullOrEmpty(RoleOwnerFilter)) { sSR.SetRoleOwnerFilter(RoleOwnerFilter); }
            if (!string.IsNullOrEmpty(RolePrincipalNameFilter)) { sSR.SetRolePrincipalNameFilter(RolePrincipalNameFilter); }
            sSR.Query();
            _PrintOutput(sSR.GetResults());
        }

        private void _SQLServerRoleMember(SqlInstances sqlInstances)
        {
            SQLServerRoleMember sSRM = new SQLServerRoleMember(credentials);
            sSRM.SetComputerName(sqlInstances.Server);
            sSRM.SetInstance(sqlInstances.ServerInstance);
            if (!string.IsNullOrEmpty(PrincipalNameFilter)) { sSRM.SetPrincipalNameFilter(PrincipalNameFilter); }
            sSRM.Query();
            _PrintOutput(sSRM.GetResults());
        }

        private void _SQLServiceAccount(SqlInstances sqlInstances)
        {
            SQLServiceAccount sSA = new SQLServiceAccount(credentials);
            sSA.SetComputerName(sqlInstances.Server);
            sSA.SetInstance(sqlInstances.ServerInstance);
            sSA.Query();
            _PrintOutput(sSA.GetResults());
        }

        private void _SQLSession(SqlInstances sqlInstances)
        {
            SQLSession sS = new SQLSession(credentials);
            sS.SetInstance(sqlInstances.ServerInstance);
            if (!string.IsNullOrEmpty(PrincipalNameFilter)) { sS.SetPrincipalNameFilter(PrincipalNameFilter); }
            sS.Query();
            _PrintOutput(sS.GetResults());
        }

        private void _SQLStoredProcedure(SqlInstances sqlInstances)
        {
            if (0 == databases.Count)
            {
                sD = new SQLDatabase(credentials);
                sD.SetComputerName(sqlInstances.Server);
                sD.SetInstance(sqlInstances.ServerInstance);
                if (nodefaults) { sD.EnableNoDefaultsFilter(); }
                if (hasAccess) { sD.EnableHasAccessFilter(); }
                if (sysadmin) { sD.EnableSysAdminFilter(); }
                sD.Query();
                databases = sD.GetResults();
            }
            SQLStoredProcedure sP = new SQLStoredProcedure(credentials);
            sP.SetComputerName(sqlInstances.Server);
            sP.SetInstance(sqlInstances.ServerInstance);
            if (!string.IsNullOrEmpty(ProcedureNameFilter)) { sP.SetProcedureNameFilter(ProcedureNameFilter); }
            if (!string.IsNullOrEmpty(keywordFilter)) { sP.SetKeywordFilter(keywordFilter); }
            if (AutoExecFilter) { sP.SetAutoExecFilter(); }
            var results = new List<SQLStoredProcedure.AssemblyFiles>();
            foreach (var d in databases)
            {
                sP.SetDatabase(d.DatabaseName);
                sP.Query();
                results.AddRange(sP.GetResults());
            }
            _PrintOutput(results);
        }

        private void _SQLStoredProcedureAutoExec(SqlInstances sqlInstances)
        {
            SQLStoredProcedureAutoExec sP = new SQLStoredProcedureAutoExec(credentials);
            sP.SetComputerName(sqlInstances.Server);
            sP.SetInstance(sqlInstances.ServerInstance);
            if (!string.IsNullOrEmpty(ProcedureNameFilter)) { sP.SetProcedureNameFilter(ProcedureNameFilter); }
            if (!string.IsNullOrEmpty(keywordFilter)) { sP.SetKeywordFilter(keywordFilter); }
            sP.Query();
            _PrintOutput(sP.GetResults());
        }

        private void _SQLStoredProcedureCLR(SqlInstances sqlInstances)
        {
            if (0 == databases.Count && !ShowAllAssemblyFiles)
            {
                sD = new SQLDatabase(credentials);
                sD.SetComputerName(sqlInstances.Server);
                sD.SetInstance(sqlInstances.ServerInstance);
                if (nodefaults) { sD.EnableNoDefaultsFilter(); }
                if (hasAccess) { sD.EnableHasAccessFilter(); }
                if (sysadmin) { sD.EnableSysAdminFilter(); }
                sD.Query();
                databases = sD.GetResults();
            }

            SQLStoredProcedureCLR sP = new SQLStoredProcedureCLR(credentials);
            sP.SetComputerName(sqlInstances.Server);
            sP.SetInstance(sqlInstances.ServerInstance);
            var results = new List<SQLStoredProcedureCLR.AssemblyFiles>();

            if (ShowAllAssemblyFiles)
            {
                sP.EnableShowAll();
                sP.Query();
                results.AddRange(sP.GetResults());
            }
            else
            {
                foreach (var d in databases)
                {
                    sP.SetDatabase(d.DatabaseName);
                    sP.Query();
                    results.AddRange(sP.GetResults());
                }
            }
            _PrintOutput(results);
        }

        private void _SQLStoredProcedureSQLi(SqlInstances sqlInstances)
        {
            SQLStoredProcedureSQLi sP = new SQLStoredProcedureSQLi(credentials);
            sP.SetComputerName(sqlInstances.Server);
            sP.SetInstance(sqlInstances.ServerInstance);
            if (!string.IsNullOrEmpty(ProcedureNameFilter)) { sP.SetProcedureNameFilter(ProcedureNameFilter); }
            if (!string.IsNullOrEmpty(keywordFilter)) { sP.SetKeywordFilter(keywordFilter); }
            if (AutoExecFilter) { sP.SetAutoExecFilter(); }
            sP.Query();
            _PrintOutput(sP.GetResults());
        }

        private void _SQLStoredProcedureXP(SqlInstances sqlInstances)
        {
            if (0 == databases.Count)
            {
                sD = new SQLDatabase(credentials);
                sD.SetComputerName(sqlInstances.Server);
                sD.SetInstance(sqlInstances.ServerInstance);
                if (nodefaults) { sD.EnableNoDefaultsFilter(); }
                if (hasAccess) { sD.EnableHasAccessFilter(); }
                if (sysadmin) { sD.EnableSysAdminFilter(); }
                sD.Query();
                databases = sD.GetResults();
            }
            SQLStoredProcedureXP sP = new SQLStoredProcedureXP(credentials);
            sP.SetComputerName(sqlInstances.Server);
            sP.SetInstance(sqlInstances.ServerInstance);
            if (!string.IsNullOrEmpty(ProcedureNameFilter)) { sP.SetProcedureNameFilter(ProcedureNameFilter); }
            var results = new List<SQLStoredProcedureXP.Procedure>();
            foreach (var d in databases)
            {
                sP.SetDatabase(d.DatabaseName);
                sP.Query();
                results.AddRange(sP.GetResults());
            }
            _PrintOutput(results);
        }

        private void _SQLSysAdminCheck(SqlInstances sqlInstances)
        {
            Console.WriteLine("{0} : {1}", sqlInstances.ServerInstance, SQLSysadminCheck.Query(sqlInstances.ServerInstance, sqlInstances.Server, credentials));
        }

        private void _SQLTables(SqlInstances sqlInstances)
        {
            if (0 == databases.Count)
            {
                sD = new SQLDatabase(credentials);
                sD.SetComputerName(sqlInstances.Server);
                sD.SetInstance(sqlInstances.ServerInstance);
                if (nodefaults) { sD.EnableNoDefaultsFilter(); }
                if (hasAccess) { sD.EnableHasAccessFilter(); }
                if (sysadmin) { sD.EnableSysAdminFilter(); }
                sD.Query();
                databases = sD.GetResults();
            }

            SQLTables sT = new SQLTables(credentials);
            sT.SetComputerName(sqlInstances.Server);
            sT.SetInstance(sqlInstances.ServerInstance);
            var results = new List<SQLTables.Table>();
            foreach (var d in databases)
            {
                sT.SetDatabase(d.DatabaseName);
                sT.Query();
                results.AddRange(sT.GetResults());
            }
            _PrintOutput(results);
        }

        private void _SQLTriggerDdl(SqlInstances sqlInstances)
        {
            SQLTriggerDdl sTD = new SQLTriggerDdl(credentials);
            sTD.SetComputerName(sqlInstances.Server);
            sTD.SetInstance(sqlInstances.ServerInstance);
            if (!string.IsNullOrEmpty(TriggerNameFilter)) { sTD.SetTriggerNameFilter(TriggerNameFilter); }

            sTD.Query();
            _PrintOutput(sTD.GetResults());
        }

        private void _SQLTriggerDml(SqlInstances sqlInstances)
        {
            if (0 == databases.Count)
            {
                sD = new SQLDatabase(credentials);
                sD.SetComputerName(sqlInstances.Server);
                sD.SetInstance(sqlInstances.ServerInstance);
                if (nodefaults) { sD.EnableNoDefaultsFilter(); }
                if (hasAccess) { sD.EnableHasAccessFilter(); }
                if (sysadmin) { sD.EnableSysAdminFilter(); }
                sD.Query();
                databases = sD.GetResults();
            }

            SQLTriggerDml sTD = new SQLTriggerDml(credentials);
            sTD.SetComputerName(sqlInstances.Server);
            sTD.SetInstance(sqlInstances.ServerInstance);
            if (!string.IsNullOrEmpty(TriggerNameFilter)) { sTD.SetTriggerNameFilter(TriggerNameFilter); }
            var results = new List<SQLTriggerDml.TriggerDml>();
            foreach (var d in databases)
            {
                sTD.SetDatabase(d.DatabaseName);
                sTD.Query();
                results.AddRange(sTD.GetResults());
            }
            _PrintOutput(sTD.GetResults());
        }

        private void _SQLUncPathInjection(SqlInstances sqlInstances)
        {
            SQLUncPathInjection sUPI = new SQLUncPathInjection(credentials);
            sUPI.SetComputerName(sqlInstances.Server);
            sUPI.SetInstance(sqlInstances.ServerInstance);
            sUPI.SetUNCPath(UNCPath);
            sUPI.Query();
        }

        private void _SQLView(SqlInstances sqlInstances)
        {
            if (0 == databases.Count)
            {
                sD = new SQLDatabase(credentials);
                sD.SetComputerName(sqlInstances.Server);
                sD.SetInstance(sqlInstances.ServerInstance);
                if (nodefaults) { sD.EnableNoDefaultsFilter(); }
                if (hasAccess) { sD.EnableHasAccessFilter(); }
                if (sysadmin) { sD.EnableSysAdminFilter(); }
                sD.Query();
                databases = sD.GetResults();
            }

            SQLView sV = new SQLView(credentials);
            sV.SetComputerName(sqlInstances.Server);
            sV.SetInstance(sqlInstances.ServerInstance);
            if (!string.IsNullOrEmpty(TableNameFilter)) { sV.SetTableNameFilter(TableNameFilter); }
            var results = new List<SQLView.View>();
            foreach (var d in databases)
            {
                sV.SetDatabase(d.DatabaseName);
                sV.Query();
                results.AddRange(sV.GetResults());
            }
            _PrintOutput(results);
        }
    }
}
