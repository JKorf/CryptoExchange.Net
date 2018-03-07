using CryptoExchange.Net.Interfaces;

namespace CryptoExchange.Net.Authentication
{
    public abstract class AuthenticationProvider
    {
        public ApiCredentials Credentials { get; }

        protected AuthenticationProvider(ApiCredentials credentials)
        {
            Credentials = credentials;
        }

        public abstract string AddAuthenticationToUriString(string uri, bool signed);
        public abstract IRequest AddAuthenticationToRequest(IRequest request, bool signed);
        public abstract string Sign(string toSign);

        protected string ByteToString(byte[] buff)
        {
            var sbinary = "";
            foreach (var t in buff)
                sbinary += t.ToString("X2"); /* hex format */
            return sbinary;
        }
    }
}
