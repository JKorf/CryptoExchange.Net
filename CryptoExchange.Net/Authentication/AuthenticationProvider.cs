using System.Collections.Generic;
using System.Net.Http;

namespace CryptoExchange.Net.Authentication
{
    /// <summary>
    /// Base class for authentication providers
    /// </summary>
    public abstract class AuthenticationProvider
    {
        /// <summary>
        /// The provided credentials
        /// </summary>
        public ApiCredentials Credentials { get; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="credentials"></param>
        protected AuthenticationProvider(ApiCredentials credentials)
        {
            Credentials = credentials;
        }

        /// <summary>
        /// Add authentication to the parameter list
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <param name="signed"></param>
        /// <returns></returns>
        public virtual Dictionary<string, object> AddAuthenticationToParameters(string uri, HttpMethod method, Dictionary<string, object> parameters, bool signed)
        {
            return parameters;
        }

        /// <summary>
        /// Add authentication to the header dictionary
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <param name="signed"></param>
        /// <returns></returns>
        public virtual Dictionary<string, string> AddAuthenticationToHeaders(string uri, HttpMethod method, Dictionary<string, object> parameters, bool signed)
        {
            return new Dictionary<string, string>();
        }

        /// <summary>
        /// Sign a string
        /// </summary>
        /// <param name="toSign"></param>
        /// <returns></returns>
        public virtual string Sign(string toSign)
        {
            return toSign;
        }

        /// <summary>
        /// Sign a byte array
        /// </summary>
        /// <param name="toSign"></param>
        /// <returns></returns>
        public virtual byte[] Sign(byte[] toSign)
        {
            return toSign;
        }

        /// <summary>
        /// Convert byte array to hex
        /// </summary>
        /// <param name="buff"></param>
        /// <returns></returns>
        protected string ByteToString(byte[] buff)
        {
            var result = "";
            foreach (var t in buff)
                result += t.ToString("X2"); /* hex format */
            return result;
        }
    }
}
