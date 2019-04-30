using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;

namespace DAFT
{
    sealed class SQLServers
    {
        private DirectoryEntry directoryEntry;
        private DirectorySearcher directorySearcher;
        private SearchResultCollection resultCollection;

        private string domainController;
        private string ldapPath;
        private int objectCountLimit;

        private string spnFilter = string.Empty;

        internal SQLServers()
        {
            domainController = string.Format("LDAP://{0}", Environment.GetEnvironmentVariable("logonserver").Replace("\\",string.Empty));
        }

        internal void SetDomainController(string domainController)
        {
            this.domainController = string.Format("LDAP://{0}", domainController);
        }

        internal void SetLdapPath(string ldapPath)
        {
            this.ldapPath = ldapPath;
        }

        internal void SetObjectCountLimit(int objectCountLimit)
        {
            this.objectCountLimit = objectCountLimit;
        }

        internal bool Connect(Credentials credentials)
        {
#if DEBUG
            Console.WriteLine("Domain Controller : {0}", domainController);
#endif
            string distinguishedName;

            if (null != credentials)
            {
                try
                {
                    directoryEntry = new DirectoryEntry(domainController, credentials.GetUsername(), credentials.GetPassword());
                    distinguishedName = (string)directoryEntry.Properties["distinguishedName"].Value;
                    if (!string.IsNullOrEmpty(ldapPath))
                    {
                        ldapPath = string.Format("/{0},{1}", ldapPath, distinguishedName);
                        directoryEntry = new DirectoryEntry(domainController + ldapPath, credentials.GetUsername(), credentials.GetPassword());
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine("{0} : {1}", domainController, ex.Message);
                    return false;
                }
            }
            else
            {
                try
                {
                    directoryEntry = new DirectoryEntry(domainController);
                    distinguishedName = (string)directoryEntry.Properties["distinguishedName"].Value;
                    if (!string.IsNullOrEmpty(ldapPath))
                    {
                        ldapPath = string.Format("/{0},{1}", ldapPath, distinguishedName);
                        directoryEntry = new DirectoryEntry(domainController + ldapPath);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("{0} : {1}", domainController, ex.Message);
                    return false;
                }
            }       
#if DEBUG
            Console.WriteLine("DistinguishedName : {0}", distinguishedName);
#endif
            return true;
        }

        internal bool Search()
        {
            string filter = string.Format("(&(servicePrincipalName={0}){1})", "MSSQL*", spnFilter);
            return _Search(filter);
        }

        internal void SetDomainAccountFilter(string accountName)
        {
            spnFilter = string.Format("(objectcategory=person)(SamAccountName={0})", accountName);
        }

        internal void SetComputerAccountFilter(string accountName)
        {
            spnFilter = string.Format("(objectcategory=computer)(SamAccountName={0}$)", accountName);
        }

        private bool _Search(string filter)
        {
#if DEBUG
            Console.WriteLine("Filter : {0}", filter);
#endif
            directorySearcher = new DirectorySearcher(directoryEntry)
            {
                SizeLimit = objectCountLimit,
                Filter = filter,
                SearchScope = SearchScope.Subtree
            };

            try
            {
                resultCollection = directorySearcher.FindAll();
            }
            catch (Exception ex)
            {
                if (ex is InvalidOperationException)
                    Console.WriteLine("{0} : Invalid Operation Exception Occured: {1}", domainController, ex.Message);
                else if (ex is NotSupportedException)
                    Console.WriteLine("{0} : Not Supported Exception Occured: {1}", domainController, ex.Message);
                else
                    Console.WriteLine("{0} : {1}", domainController, ex.Message);
                return false;
            }
            return true;
        }

        internal bool ParseCollection(bool lookupIP, ref List<App.SqlInstances> instances)
        {
            Console.WriteLine("SPNs Returned : {0}", resultCollection.Count);
            foreach (SearchResult item in resultCollection)
            {
                foreach (string spn in item.Properties["ServicePrincipalName"])
                {
                    string spnService = spn.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries).First();
                    if (!spnService.Contains("MSSQL"))
                        continue;

                    string spnServer = spn.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries).Last()
                        .Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries).First()
                        .Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).First();

                    string instance = spn.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();

                    int port = 0;
                    string serverInstance = spn.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries).Last()
                        .Replace(':', int.TryParse(instance, out port) ? ',' : '\\');

                    DateTime lastLogon = DateTime.MinValue;

                    if (null != item.Properties["lastlogontimestamp"])
                    {
                        lastLogon = DateTime.FromFileTime((long)item.Properties["lastlogontimestamp"][0]);
                    }

                    instances.Add(
                        new App.SqlInstances()
                        {
                            SPN = spn,
                            ServerInstance = serverInstance,
                            Server = spnServer,
                            Service = spnService,
                            UserSid = item.Properties["objectsid"],
                            User = item.Properties["samaccountname"][0].ToString(),
                            UserCN = item.Properties["cn"][0].ToString(),
                            LastLogon = lastLogon,
                            Description = item.Properties["description"][0].ToString()
                        }
                    );
                }
            }
            return true;
        }
    }
}
