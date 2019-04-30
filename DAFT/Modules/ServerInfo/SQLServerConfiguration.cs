using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;

namespace DAFT.Modules
{
    sealed class SQLServerConfiguration : Module
    {
        private bool restoreState = false;

        [DataContract]
        public struct Config
        {
            [DataMember] public string ComputerName;
            [DataMember] public string Instance;
            [DataMember] public string Name;
            [DataMember] public int Minimum;
            [DataMember] public int Maximum;
            [DataMember] public int config_value;
            [DataMember] public int run_value;
        }

        private List<Config> configs;

        internal SQLServerConfiguration(Credentials credentials) : base(credentials)
        {
            configs = new List<Config>();
        }

        internal void RestoreState()
        {
            restoreState = true;
        }

        internal override bool Query()
        {
            using (SQLConnection sql = new SQLConnection(instance))
            {
                sql.BuildConnectionString(credentials);
                if (!sql.Connect())
                    return false;

                int sao_value = (int)_Query(sql, @"sp_configure 'Show Advanced Options'", "config_value");
                if (0 == sao_value)
                    _Query(sql, @"sp_configure 'Show Advanced Options',1;RECONFIGURE", string.Empty);

                //table = sql.Query("sp_configure");
                configs = sql.Query<Config>("sp_configure", new Config());

                if (0 == sao_value && restoreState)
                    _Query(sql, @"sp_configure 'Show Advanced Options',0;RECONFIGURE", string.Empty);
            }

            /*
            foreach (DataRow row in table.AsEnumerable())
            {
                try
                {
                    Config c = new Config
                    {
                        ComputerName = computerName,
                        Instance = instance,
                        Name = (string)row["Name"],
                        Minimum = (int)row["Minimum"],
                        Maximum = (int)row["Maximum"],
                        config_value = (int)row["config_value"],
                        run_value = (int)row["run_value"]
                    };
#if DEBUG
                    Misc.PrintStruct<Config>(c);
#endif
                    configs.Add(c);
                }
                catch (Exception ex)
                {
                    if (ex is ArgumentNullException)
                        Console.WriteLine("Empty Response");
                    else
                        Console.WriteLine(ex);
                    return false;
                }
            }
            */
            return true;
        }

        private static object _Query(SQLConnection sql, string query, string value)
        {
            DataTable table = sql.Query(query);
            try
            {
                foreach (DataRow row in table.AsEnumerable())
                    if (!string.IsNullOrEmpty(value))
                        return row[value];
            }
            catch (Exception ex)
            {
                if (ex is ArgumentNullException)
                    Console.WriteLine("Empty Response");
                else
                    Console.WriteLine(ex.Message);
            }
            return string.Empty;
        }

        internal List<Config> GetResults()
        {
            return configs;
        }
    }
}
