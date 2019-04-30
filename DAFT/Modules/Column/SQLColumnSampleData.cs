using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;

namespace DAFT.Modules
{
    sealed class SQLColumnSampleData : SQLColumn
    {
        private bool checkLuhn = false;
        private string[] keywords;
        private int sampleSize = 3;

        [DataContract]
        public struct SampleData
        {
            [DataMember] public string Instance;
            [DataMember] public string DatabaseName;
            [DataMember] public string TableName;
            [DataMember] public string ColumnName;
            [DataMember] public object ColumnData;
        }

        private List<SampleData> results;

        internal SQLColumnSampleData(Credentials credentials) : base(credentials)
        {
            results = new List<SampleData>();
        }

        internal void AddSearchKeywords(string keyword)
        {
            keywords = keyword.Split(',').ToArray();
            foreach (string k in keywords)
                AddColumnSearchFilter(k);
        }

        internal void EnableValidateCC()
        {
            checkLuhn = true;
        }

        internal void DisableValidateCC()
        {
            checkLuhn = false;
        }

        internal void SetSampleSize(int sampleSize)
        {
            this.sampleSize = sampleSize;
        }

        internal override bool Query()
        {
            if (!base.Query())
                return false;

            using (SQLConnection sql = new SQLConnection(instance))
            {
                sql.BuildConnectionString(credentials);
                if (!sql.Connect())
                    return false;

                foreach (var c in base.columns)
                {
                    string query = string.Format(
                        "USE {0}; SELECT TOP {1} [{2}] FROM {3} WHERE [{2}] is not null",
                        c.DatabaseName, sampleSize, c.ColumnName, c.TableName);
#if DEBUG
                    Console.WriteLine(query);
#endif
                    DataTable table = sql.Query(query);
                    try
                    {
                        foreach (DataRow row in table.AsEnumerable())
                        {
                            if (!string.IsNullOrEmpty(c.ColumnName))
                            {
                                if (checkLuhn && row[c.ColumnName] is string)
                                    if (!Misc.CheckLuhn((string)row[c.ColumnName]))
                                        continue;

                                results.Add(new SampleData
                                {
                                    Instance = instance,
                                    DatabaseName = c.DatabaseName,
                                    TableName = c.TableName,
                                    ColumnName = c.ColumnName,
                                    ColumnData = row[c.ColumnName]
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex is ArgumentNullException)
                            Console.WriteLine("Empty Response");
                        else
                            Console.WriteLine(ex.Message);
                    }
                }
            }
            return true;
        }

        internal new List<SampleData> GetResults()
        {
            return results;
        }
    }
}
