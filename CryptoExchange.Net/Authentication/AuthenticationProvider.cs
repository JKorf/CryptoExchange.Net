using CryptoExchange.Net.Interfaces;
using System.Security;

namespace CryptoExchange.Net.Authentication
{
    public abstract class AuthenticationProvider
    {
        public ApiCredentials Credentials { get; }

        public SecureString PrivateKey { get; }

        protected AuthenticationProvider(ApiCredentials credentials)
        {
            Credentials = credentials;
        }

        protected AuthenticationProvider(SecureString privateKey)
        {
            PrivateKey = privateKey;
        }

        public virtual string AddAuthenticationToUriString(string uri, bool signed)
        {
            return uri;    
        }

        public virtual IRequest AddAuthenticationToRequest(IRequest request, bool signed)
        {
            return request;
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
