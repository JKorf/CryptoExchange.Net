#if NET8_0_OR_GREATER
using NSec.Cryptography;
#endif
using System;
using System.Security.Cryptography;
using System.Text;

namespace CryptoExchange.Net.Authentication
{
    /// <summary>
    /// Credential pair base class
    /// </summary>
    public abstract class CredentialPair
    {
        /// <summary>
        /// API credentials identifier
        /// </summary>
        public string PublicKey { get; set; }
        /// <summary>
        /// Type of credentials
        /// </summary>
        public abstract ApiCredentialsType CredentialType { get; }

        /// <summary>
        /// ctor
        /// </summary>
        public CredentialPair(string publicKey)
        {
            PublicKey = publicKey;
        }
    }

    /// <summary>
    /// Api key credentials
    /// </summary>
    public class ApiKeyCredential : CredentialPair
    {
        /// <inheritdoc />
        public override ApiCredentialsType CredentialType => ApiCredentialsType.ApiKey;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="key">Api key</param>
        public ApiKeyCredential(string key) : base(key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key can't be null/empty");
        }
    }

    /// <summary>
    /// HMAC credentials
    /// </summary>
    public class HMACCredential : CredentialPair
    {
        private byte[]? _sBytes;

        /// <summary>
        /// API secret
        /// </summary>
        public string Secret { get; set; }
        /// <summary>
        /// Passphrase, not needed on all exchanges
        /// </summary>
        public string? Pass { get; set; }

        /// <inheritdoc />
        public override ApiCredentialsType CredentialType => ApiCredentialsType.Hmac;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="key">Api key/label</param>
        /// <param name="secret">Api secret</param>
        /// <param name="pass">Optional passphrase</param>
        public HMACCredential(string key, string secret, string? pass = null) : base(key)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(secret))
                throw new ArgumentException("Key and secret can't be null/empty");

            Secret = secret;
            Pass = pass;
        }

        /// <summary>
        /// Get the secret value bytes
        /// </summary>
        /// <returns></returns>
        public byte[] GetSBytes()
        {
            return _sBytes ??= Encoding.UTF8.GetBytes(Secret);
        }
    }

    /// <summary>
    /// RSA credentials
    /// </summary>
    public abstract class RSACredential : CredentialPair
    {
        /// <summary>
        /// Private key
        /// </summary>
        public string PrivateKey { get; set; }
        /// <summary>
        /// Passphrase
        /// </summary>
        public string? Passphrase { get; set; }

        /// <inheritdoc />
        public override ApiCredentialsType CredentialType => ApiCredentialsType.Rsa;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="publicKey">Public key</param>
        /// <param name="privateKey">Private key</param>
        /// <param name="passphrase">Passphrase</param>
        public RSACredential(string publicKey, string privateKey, string? passphrase = null) : base(publicKey)
        {
            if (string.IsNullOrEmpty(publicKey) || string.IsNullOrEmpty(privateKey))
                throw new ArgumentException("Public and private key can't be null/empty");

            PrivateKey = privateKey;
            Passphrase = passphrase;
        }

        /// <summary>
        /// Get RSA signer
        /// </summary>
        public abstract RSA GetSigner();
    }

#if NETSTANDARD2_1_OR_GREATER || NET7_0_OR_GREATER
    /// <summary>
    /// RSA credentials in PEM/base64 format
    /// </summary>
    public class RSAPemCredential : RSACredential
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="publicKey">Public key</param>
        /// <param name="privateKey">Private key</param>
        /// <param name="passphrase">Passphrase</param>
        public RSAPemCredential(string publicKey, string privateKey, string? passphrase = null) : base(publicKey, privateKey, passphrase)
        {
        }

        /// <summary>
        /// Get RSA signer
        /// </summary>
        public override RSA GetSigner()
        {
            var rsa = RSA.Create();
            var key = PrivateKey!
                        .Replace("\n", "")
                        .Replace("-----BEGIN PRIVATE KEY-----", "")
                        .Replace("-----END PRIVATE KEY-----", "")
                        .Trim();
            rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(
                key)
                , out _);

            return rsa;
        }
    }
#endif

    /// <summary>
    /// RSA credentials in XML format
    /// </summary>
    public class RSAXmlCredential : RSACredential
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="publicKey">Public key</param>
        /// <param name="privateKey">Private key</param>
        /// <param name="passphrase">Passphrase</param>
        public RSAXmlCredential(string publicKey, string privateKey, string? passphrase = null) : base(publicKey, privateKey, passphrase)
        {
        }

        /// <summary>
        /// Get RSA signer
        /// </summary>
        public override RSA GetSigner()
        {
            var rsa = RSA.Create();
            rsa.FromXmlString(PrivateKey);
            return rsa;
        }
    }

#if NET8_0_OR_GREATER
    /// <summary>
    /// Credentials in ED25519 format
    /// </summary>
    public class ED25519Credential : CredentialPair
    {
        private Key? _signKey;

        /// <summary>
        /// Private key
        /// </summary>
        public string PrivateKey { get; set; }
        /// <summary>
        /// Passphrase
        /// </summary>
        public string? Passphrase { get; set; }

        /// <inheritdoc />
        public override ApiCredentialsType CredentialType => ApiCredentialsType.Ed25519;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="publicKey">Public key</param>
        /// <param name="privateKey">Private key</param>
        /// <param name="passphrase">Passphrase</param>
        public ED25519Credential(string publicKey, string privateKey, string? passphrase = null) : base(publicKey)
        {
            PrivateKey = privateKey;
            Passphrase = passphrase;
        }

        /// <summary>
        /// Get signing key
        /// </summary>
        public Key GetSigningKey()
        {
            if (_signKey != null)
                return _signKey;

            var key = PrivateKey!
                        .Replace("\n", "")
                        .Replace("-----BEGIN PRIVATE KEY-----", "")
                        .Replace("-----END PRIVATE KEY-----", "")
                        .Trim();
            var keyBytes = Convert.FromBase64String(key);
            _signKey = Key.Import(SignatureAlgorithm.Ed25519, keyBytes, KeyBlobFormat.PkixPrivateKey);
            return _signKey;
        }
    }
#endif

    /// <summary>
    /// Credentials in ECDSA format
    /// </summary>
    public class ECDSACredential : CredentialPair
    {
        /// <summary>
        /// Private key
        /// </summary>
        public string PrivateKey { get; set; }
        /// <summary>
        /// Passphrase
        /// </summary>
        public string? Passphrase { get; set; }

        /// <inheritdoc />
        public override ApiCredentialsType CredentialType => ApiCredentialsType.Ecdsa;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="publicKey">Public key</param>
        /// <param name="privateKey">Private key</param>
        /// <param name="passphrase">Passphrase</param>
        public ECDSACredential(string publicKey, string privateKey, string? passphrase = null) : base(publicKey)
        {
            PrivateKey = privateKey;
            Passphrase = passphrase;
        }
    }
}
