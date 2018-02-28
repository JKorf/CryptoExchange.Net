using System;

namespace CryptoExchange.Net.Authentication
{
    public class ApiCredentials
    {
        /// <summary>
        /// The api key
        /// </summary>
        public string Key { get; }
        /// <summary>
        /// The api secret
        /// </summary>
        public string Secret { get; }

        public ApiCredentials() { }

        public ApiCredentials(string key, string secret)
        {
            if(string.IsNullOrEmpty(key) || string.IsNullOrEmpty(secret))
                throw new ArgumentException("Apikey or apisecret not provided");

            Key = key;
            Secret = secret;
        }
    }
}
