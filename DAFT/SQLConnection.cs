using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.Serialization;
using System.Text;

namespace DAFT
{
    sealed class SQLConnection : IDisposable
    {
        private SqlConnection connection = null;
        private SqlCommand sqlCommand = null;

        private readonly string instance;
        private string database = "Master";
        private string adminConnection = string.Empty;
        private string connectionTimeout = "1";
        private string applicationName = string.Empty;
        private string encryptString = string.Empty;
        private string trustCertificate = string.Empty;
        private string workstationId = string.Empty;

        [DataContract]
        internal struct Connection
        {
            [DataMember]
            public string Instance;
            [DataMember]
            public bool Accessible;
        }
        Connection connectionResult;

        private readonly string connectionString = string.Empty;

        internal SQLConnection()
        {
            instance = Environment.MachineName;
        }

        internal SQLConnection(string instance)
        {
            this.instance = instance;
        }

        internal SqlConnection GetSQL()
        {
            return connection;
        }

        internal void SetSQL(SqlConnection connection)
        {
            this.connection = connection;
        }

        internal void SetDatabase(string database)
        {
            this.database = database;
        }

        internal void EnableDedicatedAdminConnection()
        {
            adminConnection = "ADMIN:";
        }

        internal void DisableDedicatedAdminConnection()
        {
            adminConnection = string.Empty;
        }

        internal void SetConnectionTimeout(int timeout)
        {
            connectionTimeout = Convert.ToString(timeout);
        }

        internal void SetApplicationName(string applicationName)
        {
            this.applicationName = string.Format("{0};", applicationName);
        }

        internal void EnableEncrypt()
        {
            encryptString = "Encrypt=Yes;";
        }

        internal void DisableEncrypt()
        {
            encryptString = string.Empty;
        }

        internal void EnableTrustCerticate()
        {
            trustCertificate = "TrustServerCertificate=Yes;";
        }

        internal void DisableTrustCerticate()
        {
            trustCertificate = string.Empty;
        }

        internal void SetWorkstationId(string workstationId)
        {
            this.workstationId = string.Format("Workstation Id= \"{0}\";", workstationId);
        }

        internal void BuildConnectionString(Credentials credentials)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Server={0}{1};", adminConnection, instance);
            sb.AppendFormat("Database={0};", database);

            if (null != credentials)
            {
                if (credentials.IsSqlAccount())
                {
                    sb.AppendFormat("User ID={0};Password={1};", credentials.GetUsername(), credentials.GetPassword());
                }
                else
                {
                    sb.Append("Integrated Security=SSPI;");
                    sb.AppendFormat("uid={0};pwd={1};", credentials.GetUsername(), credentials.GetPassword());
                }
            }
            else
            {
                sb.Append("Integrated Security=SSPI;");
            }

            sb.AppendFormat("Connection Timeout={0};", connectionTimeout);

            if (!string.IsNullOrEmpty(applicationName))
                sb.Append(applicationName);

            if (!string.IsNullOrEmpty(encryptString))
                sb.Append(encryptString);

            if (!string.IsNullOrEmpty(trustCertificate))
                sb.Append(trustCertificate);

            if (!string.IsNullOrEmpty(workstationId))
                sb.Append(workstationId);
#if DEBUG
            //Console.WriteLine("ConnectionString: {0}", sb.ToString());
#endif
            connection = new SqlConnection
            {
                ConnectionString = sb.ToString()
            };

            sb.Clear();
            GC.Collect();
        }

        internal bool Connect()
        {
            connectionResult = new Connection
            {
                Instance = instance,
            };

            try
            {
                connection.Open();
            }
            catch (Exception ex)
            {
                if (ex is SqlException)
                {
                    if (ex.Message.Contains("error: 40 - Could not open a connection to SQL Server"))
                        Console.WriteLine("[-] {0} : SQL Exception Occured: {1}", instance, "error: 40 - Could not open a connection to SQL Server");

                    else if (ex.Message.Contains("error: 26 - Error Locating Server/Instance Specified"))
                        Console.WriteLine("[-] {0} : SQL Exception Occured: {1}", instance, "error: 26 - Error Locating Server/Instance Specified");

                    else if (ex.Message.Contains("error: 0 - The wait operation timed out."))
                        Console.WriteLine("[-] {0} : SQL Exception Occured: {1}", instance, "error: 0 - The wait operation timed out.");

                    else if (ex.Message.Contains("error: 0 - The remote computer refused the network connection."))
                        Console.WriteLine("[-] {0} : SQL Exception Occured: {1}", instance, "error: 0 - The remote computer refused the network connection.");

                    else if (ex.Message.Contains("error: 0 - No such host is known."))
                        Console.WriteLine("[-] {0} : SQL Exception Occured: {1}", instance, "error: 0 - No such host is known.");

                    else
                        Console.WriteLine("[-] {0} : SQL Exception Occured: {1}", instance, ex.Message);
                }
                else if (ex is InvalidOperationException)
                    Console.WriteLine("[-] {0} : Invalid Operation Occured: {1}", instance, ex.Message);
                else
                    Console.WriteLine("[-] {0} : {1}", instance, ex);
                connectionResult.Accessible = false;
                connection = null;
                return false;
            }
            connectionResult.Accessible = true;
            Console.WriteLine("[+] {0} : Connection Opened", instance);
            return true;
        }

        internal DataTable Query(string query)
        {
            DataTable table = new DataTable();
            sqlCommand = new SqlCommand(query, connection);
            try
            {
                using (SqlDataReader reader = sqlCommand.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        table.Clear();
                        table.Load(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is SqlException)
                {
                    Console.WriteLine("{0} : SQL Exception Occured: {1}", instance, ex.Message);
                    Console.WriteLine(query);
                }
                else
                    Console.WriteLine(ex);
            }
            return table;
        }

        internal List<T> Query<T>(string query, T s)
        {
            List<T> data = new List<T>();
            sqlCommand = new SqlCommand(query, connection);
            try
            {
                using (SqlDataReader reader = sqlCommand.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            data.Add(Misc.AssignStruct<T>(reader, s));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is SqlException)
                {
                    Console.WriteLine("{0} : SQL Exception Occured: {1}", instance, ex.Message);
                    Console.WriteLine(query);
                }
                else
                    Console.WriteLine(ex);
            }
            return data;
        }

        internal Connection GetConnectionResult()
        {
            return connectionResult;
        }

        public void Dispose()
        {
            if (null != connection && connection.State.HasFlag(ConnectionState.Open))
            {
                try
                {
                    connection.Close();
                    connection = null;
                    GC.Collect();
                }
                catch (SqlException ex)
                {
                    Console.WriteLine("{0} : {1}", instance, ex.Message);
                }
            }
        }

        ~SQLConnection()
        {
            Dispose();
        }
    }
}
