using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DAFT.Modules
{
    class SQLStoredProcedureXP : Module
    {
        const string QUERY1_2 = @"
    o.object_id,
	o.parent_object_id,
	o.schema_id,
	o.type,
	o.type_desc,
	o.name,
	o.principal_id,
	s.text,
	s.ctext,
	s.status,
	o.create_date,
	o.modify_date,
	o.is_ms_shipped,
	o.is_published,
	o.is_schema_published,
	s.colid,
	s.compressed,
	s.encrypted,
	s.id,
	s.language,
	s.number,
	s.texttype
FROM sys.objects o 
INNER JOIN sys.syscomments s
	ON o.object_id = s.id
WHERE o.type = 'x' 
";
        [DataContract]
        public struct Procedure
        {
            [DataMember] public string ComputerName;
            [DataMember] public string Instance;
            [DataMember] public string DatabaseName;
            [DataMember] public int object_id;
            [DataMember] public int parent_object_id;
            [DataMember] public int schema_id;
            [DataMember] public string type;
            [DataMember] public string type_desc;
            [DataMember] public string name;
            [DataMember] public int principal_id;
            [DataMember] public string text;
            [DataMember] public string ctext;
            [DataMember] public string status;
            [DataMember] public DateTime create_date;
            [DataMember] public DateTime modify_date;
            [DataMember] public bool is_ms_shipped;
            [DataMember] public bool is_published;
            [DataMember] public bool is_schema_published;
            [DataMember] public string colid;
            [DataMember] public string compressed;
            [DataMember] public string encrypted;
            [DataMember] public int id;
            [DataMember] public string language;
            [DataMember] public int number;
            [DataMember] public string texttype;
        }

        private List<Procedure> procedures;

        private string procedureNameFilter = string.Empty;

        internal SQLStoredProcedureXP(Credentials credentials) : base(credentials)
        {
            procedures = new List<Procedure>();
        }

        internal void SetProcedureNameFilter(string procedureNameFilter)
        {
            this.procedureNameFilter = string.Format(" AND NAME like \'{0}\'", procedureNameFilter);
        }

        internal override bool Query()
        {
            using (SQLConnection sql = new SQLConnection(instance))
            {
                sql.BuildConnectionString(credentials);
                if (!sql.Connect())
                    return false;

                string query1_1 = string.Format("use [{0}];\n" +
                    "SELECT \'{1}\' as [ComputerName],\n" +
                    "\'{2}\' as [Instance],\n" +
                    "\'{3}\' as [DatabaseName],\n",
                    database, computerName, instance, database);

                StringBuilder sb = new StringBuilder();
                sb.Append(query1_1);
                sb.Append(QUERY1_2);
                if (!string.IsNullOrEmpty(procedureNameFilter)) { sb.Append(procedureNameFilter); }

#if DEBUG
                Console.WriteLine(sb.ToString());
#endif
                procedures = sql.Query<Procedure>(sb.ToString(), new Procedure());
            }
            return true;
        }

        internal List<Procedure> GetResults()
        {
            return procedures;
        }
    }
}