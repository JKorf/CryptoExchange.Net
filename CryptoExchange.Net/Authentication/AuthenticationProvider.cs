using CryptoExchange.Net.Interfaces;
using System.Collections.Generic;

namespace CryptoExchange.Net.Authentication
{
    public abstract class AuthenticationProvider
    {
        public ApiCredentials Credentials { get; }

        protected AuthenticationProvider(ApiCredentials credentials)
        {
            Credentials = credentials;
        }

        public virtual Dictionary<string, object> AddAuthenticationToParameters(string uri, string method, Dictionary<string, object> parameters, bool signed)
        {
            return parameters;
        }

        public virtual Dictionary<string, string> AddAuthenticationToHeaders(string uri, string method, Dictionary<string, object> parameters, bool signed)
        {
            return new Dictionary<string, string>();
        }

        public virtual string Sign(string toSign)
        {
            return toSign;
        }

        public virtual byte[] Sign(byte[] toSign)
        {
            return toSign;
        }

        protected string ByteToString(byte[] buff)
        {
            var sbinary = "";
            foreach (var t in buff)
                sbinary += t.ToString("X2"); /* hex format */
            return sbinary;
        }
    }
}
