using System;
using System.IO;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Authentication
{
    /// <summary>
    /// Api credentials, used to sign requests accessing private endpoints
    /// </summary>
    public class ApiCredentials
    {
        /// <summary>
        /// The api key / label to authenticate requests
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The api secret or private key to authenticate requests
        /// </summary>
        public string Secret { get; set; }

        /// <summary>
        /// The api passphrase. Not needed on all exchanges
        /// </summary>
        public string? Pass { get; set; }

        /// <summary>
        /// Type of the credentials
        /// </summary>
        public ApiCredentialsType CredentialType { get; set; }

        /// <summary>
        /// Create Api credentials providing an api key and secret for authentication
        /// </summary>
        /// <param name="key">The api key / label used for identification</param>
        /// <param name="secret">The api secret or private key used for signing</param>
        /// <param name="pass">The api pass for the key. Not always needed</param>
        /// <param name="credentialType">The type of credentials</param>
        public ApiCredentials(string key, string secret, string? pass = null, ApiCredentialsType credentialType = ApiCredentialsType.Hmac)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(secret))
                throw new ArgumentException("Key and secret can't be null/empty");

            CredentialType = credentialType;
            Key = key;
            Secret = secret;
            Pass = pass;
        }

        /// <summary>
        /// Load a private key from a file path
        /// </summary>
        public async Task<string> LoadPrivateKey(string path)
        {
            using var filestream = File.OpenRead(path);
            using var streamReader = new StreamReader(filestream);
            return await streamReader.ReadToEndAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Copy the credentials
        /// </summary>
        /// <returns></returns>
        public virtual ApiCredentials Copy()
        {
            return new ApiCredentials(Key, Secret, Pass, CredentialType);
        }
    }
}
