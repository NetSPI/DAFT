using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DAFT.Modules
{
    class SQLTriggerDml : Module
    {
        const string QUERY1_2 = @"
name as [TriggerName],
object_id as [TriggerId],
[TriggerType] = 'DATABASE',
type_desc as [ObjectType],
parent_class_desc as [ObjectClass],
OBJECT_DEFINITION(OBJECT_ID) as [TriggerDefinition],
create_date,
modify_date,
is_ms_shipped,
is_disabled,
is_not_for_replication,
is_instead_of_trigger
";

        public struct TriggerDml
        {
            public string ComputerName;
            public string Instance;
            public string TriggerName;
            public int TriggerId;
            public string TriggerType;
            public string ObjectType;
            public string ObjectClass;
            public string TriggerDefinition;
            public DateTime create_date;
            public DateTime modify_date;
            public bool is_ms_shipped;
            public bool is_disabled;
            public bool is_not_for_replication;
            public bool is_instead_of_trigger;
        }

        private List<TriggerDml> triggers;

        private string triggerNameFilter = string.Empty;

        internal SQLTriggerDml(Credentials credentials) : base(credentials)
        {
            triggers = new List<TriggerDml>();
        }

        internal void SetTriggerNameFilter(string triggerNameFilter)
        {
            this.triggerNameFilter = string.Format(" AND name like \'{0}\'", triggerNameFilter);
        }

        internal override bool Query()
        {
            using (SQLConnection sql = new SQLConnection(instance))
            {
                sql.BuildConnectionString(credentials);
                if (!sql.Connect())
                    return false;

                string query1_1 = string.Format(
                "SELECT  \'{0}\' as [ComputerName],\n" +
                "\'{1}\' as [Instance],",
                computerName, instance);

                string query1_3 = string.Format(
                "FROM [{0}].[sys].[triggers] WHERE 1=1",
                database);

                StringBuilder sb = new StringBuilder();
                sb.Append(query1_1);
                sb.Append(QUERY1_2);
                sb.Append(query1_3);
                if (!string.IsNullOrEmpty(triggerNameFilter)) { sb.Append(triggerNameFilter); }

#if DEBUG
                Console.WriteLine(sb.ToString());
#endif
                //table = sql.Query(sb.ToString());
                triggers = sql.Query<TriggerDml>(sb.ToString(), new TriggerDml());
            }

            /*
            foreach (DataRow row in table.AsEnumerable())
            {
                try
                {
                    TriggerDml td = new TriggerDml
                    {
                        ComputerName = computerName,
                        Instance = instance,
                        TriggerName = (string)row["TriggerName"],
                        TriggerId = (int)row["TriggerId"],
                        TriggerType = (string)row["TriggerType"],
                        ObjectType = (string)row["ObjectType"],
                        ObjectClass = (string)row["ObjectClass"],
                        TriggerDefinition = (string)row["TriggerDefinition"],
                        create_date = (DateTime)row["create_date"],
                        modify_date = (DateTime)row["modify_date"],
                        is_ms_shipped = (bool)row["is_ms_shipped"],
                        is_disabled = (bool)row["is_disabled"],
                        is_not_for_replication = (bool)row["is_not_for_replication"],
                        is_instead_of_trigger = (bool)row["is_instead_of_trigger"],
                    };
#if DEBUG
                    Misc.PrintStruct<TriggerDml>(td);
#endif
                    triggers.Add(td);
                    return true;
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

        internal List<TriggerDml> GetResults()
        {
            return triggers;
        }
    }
}