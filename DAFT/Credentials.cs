using System;
using System.Net;
using System.Security;

namespace DAFT
{
    sealed class Credentials
    {
        private readonly string username;
        private readonly SecureString password;
        private readonly bool sqlCredential;

        internal Credentials(string username, string password)
        {
            this.username = username;
            sqlCredential = !username.Contains("\\");                
            this.password = new NetworkCredential("", password).SecurePassword;
            this.password.MakeReadOnly();
            password = string.Empty;
            GC.Collect();
        }

        internal string GetUsername()
        {
            return username;
        }

        internal string GetPassword()
        {
            return new NetworkCredential("", password).Password;
        }

        internal bool IsSqlAccount()
        {
            return sqlCredential;
        }
    }
}
