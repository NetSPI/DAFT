using System.Collections.Generic;

namespace DAFT
{
    class SQLConnectionTest
    {
        private readonly Credentials credentials;

        public SQLConnectionTest(Credentials credentials)
        {
            this.credentials = credentials;
        }

        public void Run(ref List<App.SqlInstances> instances, ref List<App.SqlInstances> connectionSuccess)
        {
            foreach (var instance in instances)
            {
                using (SQLConnection connection = new SQLConnection(instance.ServerInstance))
                {
                    connection.BuildConnectionString(credentials);
                    if (connection.Connect())
                    {
                        lock (connectionSuccess)
                        {
                            connectionSuccess.Add(instance);
                        }
                    }
                }
            }
        }

        public void RunThreaded()
        {

        }
    }
}
