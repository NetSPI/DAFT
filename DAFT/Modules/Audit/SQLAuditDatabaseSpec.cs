using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DAFT.Modules
{
    class SQLAuditDatabaseSpec : Module
    {
        private const string QUERY1_2 = @"
audit_id as [AuditId],
a.name as [AuditName],
s.name as [AuditSpecification],
d.audit_action_id as [AuditActionId],
d.audit_action_name as [AuditAction],
d.major_id,
OBJECT_NAME(d.major_id) as object,	
s.is_state_enabled,
d.is_group,
s.create_date,
s.modify_date,
d.audited_result
FROM sys.server_audits AS a
JOIN sys.database_audit_specifications AS s
ON a.audit_guid = s.audit_guid
JOIN sys.database_audit_specification_details AS d
ON s.database_specification_id = d.database_specification_id WHERE 1=1
";
        [DataContract]
        public struct DatabaseSpecification
        {
            [DataMember] public string ComputerName;
            [DataMember] public string Instance;
            [DataMember] public int AuditId;
            [DataMember] public string AuditName;
            [DataMember] public string AuditSpecification;
            [DataMember] public string AuditActionId;
            [DataMember] public string AuditAction;
            [DataMember] public int major_id;
            [DataMember] public bool is_state_enabled;
            [DataMember] public bool is_group;
            [DataMember] public DateTime create_date;
            [DataMember] public DateTime modify_date;
            [DataMember] public string audited_result;
        }

        private List<DatabaseSpecification> databaseSpecifications;

        private string auditNameFilter = string.Empty;
        private string auditSpecificationFilter = string.Empty;
        private string auditActionNameFilter = string.Empty;

        internal SQLAuditDatabaseSpec(Credentials credentials) : base(credentials)
        {
            databaseSpecifications = new List<DatabaseSpecification>();
        }

        internal void SetAuditNameFilter(string auditNameFilter)
        {
            this.auditNameFilter = string.Format(" and a.name like \'%{0}%\'", auditNameFilter);
        }

        internal void SetAuditSpecificationFilter(string auditSpecificationFilter)
        {
            this.auditSpecificationFilter = string.Format(" and s.name like \'%{0}%\'", auditSpecificationFilter);
        }

        internal void SetAuditActionNameFilter(string auditActionNameFilter)
        {
            this.auditActionNameFilter = string.Format(" and d.audit_action_name like \'%{0}%\'", auditActionNameFilter);
        }

        internal override bool Query()
        {
            string query1_1 = string.Format(
                "SELECT  \'{0}\' as [ComputerName],\n" +
                "\'{1}\' as [Instance],",
                computerName, instance
                );
            using (SQLConnection sql = new SQLConnection(instance))
            {
                sql.BuildConnectionString(credentials);
                if (!sql.Connect())
                    return false;
                StringBuilder sb = new StringBuilder();
                sb.Append(query1_1);
                sb.Append(QUERY1_2);
                if (!string.IsNullOrEmpty(auditNameFilter)) { sb.Append(auditNameFilter); }
                if (!string.IsNullOrEmpty(auditSpecificationFilter)) { sb.Append(auditSpecificationFilter); }
                if (!string.IsNullOrEmpty(auditActionNameFilter)) { sb.Append(auditActionNameFilter); }
#if DEBUG
                Console.WriteLine(sb.ToString());
#endif                
                databaseSpecifications = sql.Query<DatabaseSpecification>(sb.ToString(), new DatabaseSpecification());
            }
            return true;
        }

        internal List<DatabaseSpecification> GetResults()
        {
            return databaseSpecifications;
        }
    }
}
