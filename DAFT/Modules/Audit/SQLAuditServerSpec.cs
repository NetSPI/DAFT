using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DAFT.Modules
{
    class SQLAuditServerSpec : Module
    {
        private const string QUERY1_2 = @"
audit_id as [AuditId],
a.name as [AuditName],
s.name as [AuditSpecification],
d.audit_action_name as [AuditAction],
s.is_state_enabled,
d.is_group,
d.audit_action_id as [AuditActionId],
s.create_date,
s.modify_date
FROM sys.server_audits AS a
JOIN sys.server_audit_specifications AS s
ON a.audit_guid = s.audit_guid
JOIN sys.server_audit_specification_details AS d
ON s.server_specification_id = d.server_specification_id WHERE 1=1
";
        [DataContract]
        public struct ServerSpecification
        {
            [DataMember] public string ComputerName;
            [DataMember] public string Instance;
            [DataMember] public int AuditId;
            [DataMember] public string AuditName;
            [DataMember] public string AuditSpecification;
            [DataMember] public string AuditActionId;
            [DataMember] public string AuditAction;
            [DataMember] public int major_id;
            [DataMember] public string obj;
            [DataMember] public bool is_state_enabled;
            [DataMember] public bool is_group;
            [DataMember] public DateTime create_date;
            [DataMember] public DateTime modify_date;
            [DataMember] public string audited_result;
        }

        private List<ServerSpecification> serverSpecifications;

        private string auditNameFilter = string.Empty;
        private string auditSpecificationFilter = string.Empty;
        private string auditActionNameFilter = string.Empty;

        internal SQLAuditServerSpec(Credentials credentials) : base(credentials)
        {
            serverSpecifications = new List<ServerSpecification>();
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
                serverSpecifications = sql.Query<ServerSpecification>(sb.ToString(), new ServerSpecification());
            }
            return true;
        }

        internal List<ServerSpecification> GetResults()
        {
            return serverSpecifications;
        }
    }
}
