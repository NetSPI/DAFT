using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace DAFT.Modules
{
    class SQLAssemblyFile : Module
    {
        const string QUERY1_2 = @"
SELECT af.assembly_id,
a.name as assembly_name,
af.file_id,					  	
af.name as file_name,
a.clr_name,
af.content, 
a.permission_set_desc,
a.create_date,
a.modify_date,
a.is_user_defined
FROM sys.assemblies a INNER JOIN sys.assembly_files af ON a.assembly_id = af.assembly_id 
";
        [DataContract]
        public struct Assembly
        {
            [DataMember] public string ComputerName;
            [DataMember] public string Instance;
            [DataMember] public string DatabaseName;
            [DataMember] public int assembly_id;
            [DataMember] public string assembly_name;
            [DataMember] public int file_id;
            [DataMember] public string file_name;
            [DataMember] public string clr_name;
            [DataMember] public byte[] content;
            [DataMember] public string permission_set_desc;
            [DataMember] public DateTime create_date;
            [DataMember] public DateTime modify_date;
            [DataMember] public bool is_user_defined;
        }

        private List<Assembly> assemblies;

        private string assemblyNameFilter = string.Empty;
        private bool exportAssembly = false;

        internal SQLAssemblyFile(Credentials credentials) : base(credentials)
        {
            assemblies = new List<Assembly>();
        }

        internal void SetAssemblyNameFilter(string assemblyNameFilter)
        {
            this.assemblyNameFilter = string.Format(" WHERE af.name LIKE \'%{0}%\'", assemblyNameFilter);
        }

        internal void SetExportAssembly()
        {
            exportAssembly = true;
        }

        internal override bool Query()
        {
            using (SQLConnection sql = new SQLConnection(instance))
            {
                sql.BuildConnectionString(credentials);
                if (!sql.Connect())
                    return false;

                string query1_1 = string.Format("" +
                    "SELECT \'{0}\' as [ComputerName],\n" +
                    "\'{1}\' as [Instance],\n" +
                    "\'{3}\' as [DatabaseName],\n",
                    computerName, instance, database);

                StringBuilder sb = new StringBuilder();
                sb.Append(query1_1);
                sb.Append(QUERY1_2);
                if (!string.IsNullOrEmpty(assemblyNameFilter)) { sb.Append(assemblyNameFilter); }

#if DEBUG
                Console.WriteLine(sb.ToString());
#endif
                table = sql.Query(sb.ToString());
            }

            foreach (DataRow row in table.AsEnumerable())
            {
                try
                {
                    Assembly a = new Assembly
                    {
                        ComputerName = computerName,
                        Instance = instance,
                        DatabaseName = database,
                        assembly_id = (int)row["assembly_id"],
                        assembly_name = (string)row["assembly_name"],
                        file_id = (int)row["file_id"],
                        file_name = (string)row["file_name"],
                        clr_name = (string)row["clr_name"],
                        content = (byte[])row["content"],
                        permission_set_desc = (string)row["permission_set_desc"],
                        create_date = (DateTime)row["create_date"],
                        modify_date = (DateTime)row["modify_date"],
                        is_user_defined = (bool)row["is_user_defined"]
                    };
#if DEBUG
                    Misc.PrintStruct<Assembly>(a);
#endif
                    assemblies.Add(a);
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

        internal List<Assembly> GetResults()
        {
            return assemblies;
        }

        internal void Export()
        {
            foreach (var a in assemblies)
            {
                using (var w = new BinaryWriter(File.OpenWrite(a.assembly_name)))
                {
                    w.Write(a.content);
                }
            }
        }
    }
}