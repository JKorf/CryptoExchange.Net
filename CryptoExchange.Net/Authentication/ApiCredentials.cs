using System;
using System.IO;
using System.Security;
using System.Text;
using Newtonsoft.Json.Linq;

namespace CryptoExchange.Net.Authentication
{
    public class ApiCredentials: IDisposable
    {
        /// <summary>
        /// The api key to authenticate requests
        /// </summary>
        public SecureString Key { get; private set; }

        /// <summary>
        /// The api secret to authenticate requests
        /// </summary>
        public SecureString Secret { get; private set; }

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

            Key = CreateSecureString(key);
            Secret = CreateSecureString(secret);
        }

        /// <summary>
        /// Create Api credentials providing a stream containing json data. The json data should include two values: apiKey and apiSecret
        /// </summary>
        /// <param name="inputStream">The stream containing the json data</param>
        /// <param name="identifierKey">A key to identify the credentials for the API. For example, when set to `binanceKey` the json data should contain a value for the property `binanceKey`. Defaults to 'apiKey'.</param>
        /// <param name="identifierSecret">A key to identify the credentials for the API. For example, when set to `binanceSecret` the json data should contain a value for the property `binanceSecret`. Defaults to 'apiSecret'.</param>
        public ApiCredentials(Stream inputStream, string identifierKey = null, string identifierSecret = null)
        {
            using (var reader = new StreamReader(inputStream, Encoding.ASCII, false, 512, true))
            {
                var stringData = reader.ReadToEnd();
                var jsonData = JToken.Parse(stringData);
                var key = TryGetValue(jsonData, identifierKey ?? "apiKey");
                var secret = TryGetValue(jsonData, identifierSecret ?? "apiSecret");

                if (key == null || secret == null)
                    throw new ArgumentException("apiKey or apiSecret value not found in Json credential file");

                Key = CreateSecureString(key);
                Secret = CreateSecureString(secret);
            }

            inputStream.Seek(0, SeekOrigin.Begin);
        }

        protected string TryGetValue(JToken data, string key)
        {
            if (data[key] == null)
                return null;
            return (string) data[key];
        }

        protected SecureString CreateSecureString(string source)
        {
            var secureString = new SecureString();
            foreach (var c in source)
                secureString.AppendChar(c);
            secureString.MakeReadOnly();
            return secureString;
        }

        public void Dispose()
        {
            Key?.Dispose();
            Secret?.Dispose();
            PrivateKey?.Dispose();
        }
    }
}
