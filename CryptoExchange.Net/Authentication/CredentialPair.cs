using System;
using System.Security.Cryptography;
using System.Text;

namespace CryptoExchange.Net.Authentication
{
    /// <summary>
    /// Credential pair base class
    /// </summary>
    public abstract class CredentialPair : ApiCredentials
    {
        /// <summary>
        /// The (public) key/identifier for this credential pair
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public CredentialPair(string key)
        {
            Key = key;
        }
    }

    /// <summary>
    /// Api key credentials
    /// </summary>
    public class ApiKeyCredential : CredentialPair
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="key">Api key</param>
        public ApiKeyCredential(string key) : base(key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key can't be null/empty");
        }

        /// <inheritdoc />
        public override ApiCredentials Copy() => new ApiKeyCredential(Key);
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

        /// <inheritdoc />
        public override ApiCredentials Copy() => new HMACCredential(Key, Secret, Pass);
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
        public string? Pass { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="key">Public key</param>
        /// <param name="privateKey">Private key</param>
        /// <param name="pass">Passphrase</param>
        public RSACredential(string key, string privateKey, string? pass = null) : base(key)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(privateKey))
                throw new ArgumentException("Public and private key can't be null/empty");

            PrivateKey = privateKey;
            Pass = pass;
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
        /// <param name="key">Public key</param>
        /// <param name="privateKey">Private key</param>
        /// <param name="passphrase">Passphrase</param>
        public RSAPemCredential(string key, string privateKey, string? passphrase = null) : base(key, privateKey, passphrase)
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

        /// <inheritdoc />
        public override ApiCredentials Copy() => new RSAPemCredential(Key, PrivateKey, Pass);
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
        /// <param name="key">Public key</param>
        /// <param name="privateKey">Private key</param>
        /// <param name="passphrase">Passphrase</param>
        public RSAXmlCredential(string key, string privateKey, string? passphrase = null) : base(key, privateKey, passphrase)
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

        /// <inheritdoc />
        public override ApiCredentials Copy() => new RSAXmlCredential(Key, PrivateKey, Pass);
    }

#if NET8_0_OR_GREATER
    /// <summary>
    /// Credentials in Ed25519 format
    /// </summary>
    public class Ed25519Credential : CredentialPair
    {
        private NSec.Cryptography.Key? _signKey;

        /// <summary>
        /// Private key
        /// </summary>
        public string PrivateKey { get; set; }
        /// <summary>
        /// Passphrase
        /// </summary>
        public string? Pass { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="key">Public key</param>
        /// <param name="privateKey">Private key</param>
        /// <param name="pass">Passphrase</param>
        public Ed25519Credential(string key, string privateKey, string? pass = null) : base(key)
        {
            PrivateKey = privateKey;
            Pass = pass;
        }

        /// <summary>
        /// Get signing key
        /// </summary>
        public NSec.Cryptography.Key GetSigningKey()
        {
            if (_signKey != null)
                return _signKey;

            var key = PrivateKey!
                        .Replace("\n", "")
                        .Replace("-----BEGIN PRIVATE KEY-----", "")
                        .Replace("-----END PRIVATE KEY-----", "")
                        .Trim();
            var keyBytes = Convert.FromBase64String(key);
            _signKey = NSec.Cryptography.Key.Import(NSec.Cryptography.SignatureAlgorithm.Ed25519, keyBytes, NSec.Cryptography.KeyBlobFormat.PkixPrivateKey);
            return _signKey;
        }

        /// <inheritdoc />
        public override ApiCredentials Copy() => new Ed25519Credential(Key, PrivateKey, Pass);
    }
#endif

    /// <summary>
    /// Credentials in ECDsa format
    /// </summary>
    public class ECDsaCredential : CredentialPair
    {
        /// <summary>
        /// Private key
        /// </summary>
        public string PrivateKey { get; set; }
        /// <summary>
        /// Passphrase
        /// </summary>
        public string? Pass { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="key">Public key</param>
        /// <param name="privateKey">Private key</param>
        /// <param name="pass">Passphrase</param>
        public ECDsaCredential(string key, string privateKey, string? pass = null) : base(key)
        {
            PrivateKey = privateKey;
            Pass = pass;
        }

        /// <inheritdoc />
        public override ApiCredentials Copy() => new ECDsaCredential(Key, PrivateKey, Pass);
    }
}
