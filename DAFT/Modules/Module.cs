using System.Data;

namespace DAFT.Modules
{
    internal abstract class Module
    {
        protected readonly Credentials credentials;
        protected string instance = string.Empty;
        protected string computerName = string.Empty;
        protected string database = string.Empty;
        protected DataTable table;

        protected Module(Credentials credentials)
        {
            this.credentials = credentials;
            table = new DataTable();
        }

        internal void SetComputerName(string computerName)
        {
            this.computerName = computerName;
        }

        internal void SetInstance(string instance)
        {
            this.instance = instance;
        }

        internal void SetDatabase(string database)
        {
            this.database = database;
        }

        internal abstract bool Query();
    }
}
