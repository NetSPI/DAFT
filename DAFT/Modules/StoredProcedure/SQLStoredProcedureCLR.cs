using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DAFT.Modules
{
    class SQLStoredProcedureCLR : Module
    {
        const string QUERY1_2 = @"
SELECT SCHEMA_NAME(so.[schema_id]) AS [schema_name], 
af.file_id,					  	
af.name + '.dll' as [file_name],
asmbly.clr_name,
asmbly.assembly_id,           
asmbly.name AS [assembly_name], 
am.assembly_class,
am.assembly_method,
so.object_id as [sp_object_id],
so.name AS [sp_name],
so.[type] as [sp_type],
asmbly.permission_set_desc,
asmbly.create_date,
asmbly.modify_date,
af.content								           
FROM sys.assembly_modules am
INNER JOIN sys.assemblies asmbly
ON asmbly.assembly_id = am.assembly_id
INNER JOIN sys.assembly_files af 
ON asmbly.assembly_id = af.assembly_id 
INNER JOIN sys.objects so
ON so.[object_id] = am.[object_id]
";

        const string QUERY2_1 = @"
UNION ALL
SELECT SCHEMA_NAME(at.[schema_id]) AS [SchemaName], 
af.file_id,					  	
af.name + '.dll' as [file_name],
asmbly.clr_name,
asmbly.assembly_id,
asmbly.name AS [AssemblyName],
at.assembly_class,
NULL AS [assembly_method],
NULL as [sp_object_id],
at.name AS [sp_name],
'UDT' AS [type],
asmbly.permission_set_desc,
asmbly.create_date,
asmbly.modify_date,
af.content								           
FROM sys.assembly_types at
INNER JOIN  sys.assemblies asmbly 
ON asmbly.assembly_id = at.assembly_id
INNER JOIN sys.assembly_files af 
ON asmbly.assembly_id = af.assembly_id
ORDER BY [assembly_name], [assembly_method], [sp_name]
";
        [DataContract]
        public struct AssemblyFiles
        {
            [DataMember] public string ComputerName;
            [DataMember] public string Instance;
            [DataMember] public string DatabaseName;
            [DataMember] public string schema_name;
            [DataMember] public int file_id;
            [DataMember] public string file_name;
            [DataMember] public string clr_name;
            [DataMember] public int assembly_id;
            [DataMember] public string assembly_name;
            [DataMember] public string assembly_class;
            [DataMember] public string assembly_method;
            [DataMember] public int sp_object_id;
            [DataMember] public string sp_name;
            [DataMember] public string sp_type;
            [DataMember] public string permission_set_desc;
            [DataMember] public DateTime create_date;
            [DataMember] public DateTime modify_date;
            [DataMember] public string content;
        }

        private List<AssemblyFiles> files;

        private string assemblyNameFilter = string.Empty;
        private bool showAll = false;

        internal SQLStoredProcedureCLR(Credentials credentials) : base(credentials)
        {
            files = new List<AssemblyFiles>();
        }

        internal void SetAssemblyNameFilter(string assemblyNameFilter)
        {
            this.assemblyNameFilter = string.Format(" WHERE af.name LIKE \'%{0}%\'", assemblyNameFilter);
        }

        internal void EnableShowAll()
        {
            showAll = true;
        }

        internal override bool Query()
        {
            using (SQLConnection sql = new SQLConnection(instance))
            {
                sql.BuildConnectionString(credentials);
                if (!sql.Connect())
                    return false;

                string query1_1 = string.Format("USE {0};",database);

                string query1_3 = string.Format(
                "FROM [{0}].[sys].[triggers] WHERE 1=1",
                database);

                StringBuilder sb = new StringBuilder();
                if (showAll)
                {
                    sb.Append(QUERY2_1);
                }
                else
                { 
                    sb.Append(query1_1);
                    sb.Append(QUERY1_2);
                    sb.Append(query1_3);
                    if (!string.IsNullOrEmpty(assemblyNameFilter)) { sb.Append(assemblyNameFilter); }
                }

#if DEBUG
                Console.WriteLine(sb.ToString());
#endif
                //table = sql.Query(sb.ToString());
                files = sql.Query<AssemblyFiles>(sb.ToString(), new AssemblyFiles());
            }
            /*
            foreach (DataRow row in table.AsEnumerable())
            {
                try
                {
                    AssemblyFiles af = new AssemblyFiles
                    {
                        ComputerName = computerName,
                        Instance = instance,
                        DatabaseName = database,
                        schema_name = (string)row["schema_name"],
                        file_id = (int)row["file_id"],
                        file_name = (string)row["file_name"],
                        clr_name = (string)row["clr_name"],
                        assembly_id = (int)row["assembly_id"],
                        assembly_name = (string)row["assembly_name"],
                        assembly_class = (string)row["assembly_class"],
                        assembly_method = (string)row["assembly_method"],
                        sp_object_id = (int)row["sp_object_id"],
                        sp_name = (string)row["sp_name"],
                        sp_type = (string)row["sp_type"],
                        permission_set_desc = (string)row["permission_set_desc"],
                        create_date = (DateTime)row["create_date"],
                        modify_date = (DateTime)row["modify_date"],
                        content = (string)row["content"],
                    };
#if DEBUG
                    Misc.PrintStruct<AssemblyFiles>(af);
#endif
                    files.Add(af);
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

        internal List<AssemblyFiles> GetResults()
        {
            return files;
        }
    }
}