using CryptoExchange.Net.Interfaces;

namespace CryptoExchange.Net.Authentication
{
    public abstract class AuthenticationProvider
    {
        protected ApiCredentials credentials;

        protected AuthenticationProvider(ApiCredentials credentials)
        {
            this.credentials = credentials;
        }

        public abstract string AddAuthenticationToUriString(string uri, bool signed);
        public abstract IRequest AddAuthenticationToRequest(IRequest request, bool signed);
        
        protected string ByteToString(byte[] buff)
        {
            var sbinary = "";
            foreach (var t in buff)
                sbinary += t.ToString("X2"); /* hex format */
            return sbinary;
        }
    }
}
