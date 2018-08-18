using System;
using System.Collections.Generic;
using System.Security;
using System.Text;

namespace CryptoExchange.Net.Authentication
{
    public class PrivateKey : IDisposable
    {
        /// <summary>
        /// The private key
        /// </summary>
        public SecureString Key { get; }

        /// <summary>
        /// The private key's passphrase
        /// </summary>
        public SecureString Passphrase { get; }

        /// <summary>
        /// Indicates if the private key is encrypted or not
        /// </summary>
        public bool IsEncrypted { get; }

        /// <summary>
        /// Create a private key providing an encrypted key informations
        /// </summary>
        /// <param name="key">The private key used for signing</param>
        /// <param name="passphrase">The private key's passphrase</param>
        public PrivateKey(SecureString key, SecureString passphrase)
        {
            Key = key;
            Passphrase = passphrase;

            IsEncrypted = true;
        }

        /// <summary>
        /// Create a private key providing an encrypted key informations
        /// </summary>
        /// <param name="key">The private key used for signing</param>
        /// <param name="passphrase">The private key's passphrase</param>
        public PrivateKey(string key, string passphrase)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(passphrase))
                throw new ArgumentException("Key and passphrase can't be null/empty");

            var secureKey = new SecureString();
            foreach (var c in key)
                secureKey.AppendChar(c);
            secureKey.MakeReadOnly();
            Key = secureKey;

            var securePassphrase = new SecureString();
            foreach (var c in passphrase)
                securePassphrase.AppendChar(c);
            securePassphrase.MakeReadOnly();
            Passphrase = securePassphrase;

            IsEncrypted = true;
        }

        /// <summary>
        /// Create a private key providing an unencrypted key informations
        /// </summary>
        /// <param name="key">The private key used for signing</param>
        public PrivateKey(SecureString key)
        {
            Key = key;

            IsEncrypted = false;
        }

        /// <summary>
        /// Create a private key providing an encrypted key informations
        /// </summary>
        /// <param name="key">The private key used for signing</param>
        public PrivateKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key can't be null/empty");

            var secureKey = new SecureString();
            foreach (var c in key)
                secureKey.AppendChar(c);
            secureKey.MakeReadOnly();
            Key = secureKey;

            IsEncrypted = false;
        }

        public void Dispose()
        {
            Key?.Dispose();
            Passphrase?.Dispose();
        }
    }
}
