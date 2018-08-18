using System;
using System.Security;

namespace CryptoExchange.Net.Authentication
{
    public class ApiCredentials: IDisposable
    {
        /// <summary>
        /// The api key to authenticate requests
        /// </summary>
        public SecureString Key { get; }

        /// <summary>
        /// The api secret to authenticate requests
        /// </summary>
        public SecureString Secret { get; }

        /// <summary>
        /// The private key to authenticate requests
        /// </summary>
        public PrivateKey PrivateKey { get; }

        /// <summary>
        /// Create Api credentials providing a private key for authentication
        /// </summary>
        /// <param name="privateKey">The private key used for signing</param>
        public ApiCredentials(PrivateKey privateKey)
        {
            PrivateKey = privateKey;
        }

        /// <summary>
        /// Create Api credentials providing a api key and secret for authentication
        /// </summary>
        /// <param name="key">The api key used for identification</param>
        /// <param name="secret">The api secret used for signing</param>
        public ApiCredentials(SecureString key, SecureString secret)
        {
            Key = key;
            Secret = secret;
        }

        /// <summary>
        /// Create Api credentials providing a api key and secret for authentication
        /// </summary>
        /// <param name="key">The api key used for identification</param>
        /// <param name="secret">The api secret used for signing</param>
        public ApiCredentials(string key, string secret)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(secret))
                throw new ArgumentException("Key and secret can't be null/empty");

            var secureApiKey = new SecureString();
            foreach (var c in key)
                secureApiKey.AppendChar(c);
            secureApiKey.MakeReadOnly();
            Key = secureApiKey;

            var secureApiSecret = new SecureString();
            foreach (var c in secret)
                secureApiSecret.AppendChar(c);
            secureApiSecret.MakeReadOnly();
            Secret = secureApiSecret;
        }

        public void Dispose()
        {
            Key?.Dispose();
            Secret?.Dispose();
            PrivateKey?.Dispose();
        }
    }
}
