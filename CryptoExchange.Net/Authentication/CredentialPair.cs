using System;
using System.Security.Cryptography;
using System.Text;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

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
        public CredentialPair() { }

        /// <summary>
        /// ctor
        /// </summary>
        public CredentialPair(string key)
        {
            Key = key;
        }

        /// <summary>
        /// Validate the API credential
        /// </summary>
        public override void Validate()
        {
            if (string.IsNullOrEmpty(Key))
                throw new ArgumentException("Key unset", nameof(Key));
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
        /// ctor
        /// </summary>
        public HMACCredential()
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="key">Api key/label</param>
        /// <param name="secret">Api secret</param>
        public HMACCredential(string key, string secret) : base(key)
        {
            Secret = secret;
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
        public override ApiCredentials Copy() => new HMACCredential(Key, Secret);

        /// <inheritdoc />
        public override void Validate()
        {
            base.Validate();
            if (string.IsNullOrEmpty(Secret))
                throw new ArgumentException("Secret unset", nameof(Secret));
        }
    }

    /// <summary>
    /// HMAC credentials
    /// </summary>
    public class HMACPassCredential : HMACCredential
    {
        /// <summary>
        /// Passphrase
        /// </summary>
        public string Pass { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public HMACPassCredential()
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="key">Api key/label</param>
        /// <param name="secret">Api secret</param>
        /// <param name="pass">Passphrase</param>
        public HMACPassCredential(string key, string secret, string pass) : base(key, secret)
        {
            Pass = pass;
        }


        /// <inheritdoc />
        public override ApiCredentials Copy() => new HMACPassCredential(Key, Secret, Pass);

        /// <inheritdoc />
        public override void Validate()
        {
            base.Validate();
            if (string.IsNullOrEmpty(Pass))
                throw new ArgumentException("Pass unset", nameof(Pass));
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
        /// ctor
        /// </summary>
        public RSACredential()
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="key">Public key</param>
        /// <param name="privateKey">Private key</param>
        public RSACredential(string key, string privateKey) : base(key)
        {
            PrivateKey = privateKey;
        }

        /// <summary>
        /// Get RSA signer
        /// </summary>
        public abstract RSA GetSigner();

        /// <inheritdoc />
        public override void Validate()
        {
            base.Validate();
            if (string.IsNullOrEmpty(PrivateKey))
                throw new ArgumentException("PrivateKey unset", nameof(PrivateKey));
        }
    }

    /// <summary>
    /// RSA credentials
    /// </summary>
    public abstract class RSAPassCredential : RSACredential
    {
        /// <summary>
        /// Passphrase
        /// </summary>
        public string Pass { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public RSAPassCredential()
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="key">Public key</param>
        /// <param name="privateKey">Private key</param>
        /// <param name="pass">Passphrase</param>
        public RSAPassCredential(string key, string privateKey, string pass) : base(key, privateKey)
        {
            PrivateKey = privateKey;
            Pass = pass;
        }

        /// <inheritdoc />
        public override void Validate()
        {
            base.Validate();
            if (string.IsNullOrEmpty(Pass))
                throw new ArgumentException("Pass unset", nameof(Pass));
        }
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
        public RSAPemCredential()
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="key">Public key</param>
        /// <param name="privateKey">Private key</param>
        public RSAPemCredential(string key, string privateKey) : base(key, privateKey)
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
        public override ApiCredentials Copy() => new RSAPemCredential(Key, PrivateKey);
        
        /// <inheritdoc />
        public override void Validate()
        {
            base.Validate();
            if (string.IsNullOrEmpty(PrivateKey))
                throw new ArgumentException("PrivateKey unset", nameof(PrivateKey));
        }
    }

    /// <summary>
    /// RSA PEM/Base64 credentials
    /// </summary>
    public class RSAPemPassCredential : RSAPassCredential
    {
        /// <summary>
        /// ctor
        /// </summary>
        public RSAPemPassCredential()
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="key">Api key/label</param>
        /// <param name="privateKey">Api secret</param>
        /// <param name="pass">Passphrase</param>
        public RSAPemPassCredential(string key, string privateKey, string pass) : base(key, privateKey, pass)
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
        public override ApiCredentials Copy() => new RSAPemPassCredential(Key, PrivateKey, Pass);
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
        public RSAXmlCredential()
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="key">Public key</param>
        /// <param name="privateKey">Private key</param>
        public RSAXmlCredential(string key, string privateKey) : base(key, privateKey)
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
        public override ApiCredentials Copy() => new RSAXmlCredential(Key, PrivateKey);
    }

    /// <summary>
    /// RSA XML credentials
    /// </summary>
    public class RSAXmlPassCredential : RSAPassCredential
    {
        /// <summary>
        /// ctor
        /// </summary>
        public RSAXmlPassCredential()
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="key">Api key/label</param>
        /// <param name="privateKey">Api secret</param>
        /// <param name="pass">Passphrase</param>
        public RSAXmlPassCredential(string key, string privateKey, string pass) : base(key, privateKey, pass)
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
        public override ApiCredentials Copy() => new RSAXmlPassCredential(Key, PrivateKey, Pass);
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
        /// ctor
        /// </summary>
        public Ed25519Credential()
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="key">Public key</param>
        /// <param name="privateKey">Private key</param>
        public Ed25519Credential(string key, string privateKey) : base(key)
        {
            PrivateKey = privateKey;
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
        public override ApiCredentials Copy() => new Ed25519Credential(Key, PrivateKey);
        
        /// <inheritdoc />
        public override void Validate()
        {
            base.Validate();
            if (string.IsNullOrEmpty(PrivateKey))
                throw new ArgumentException("PrivateKey unset", nameof(PrivateKey));
        }
    }

    /// <summary>
    /// Ed25519 credentials
    /// </summary>
    public class Ed25519PassCredential : Ed25519Credential
    {
        /// <summary>
        /// Passphrase
        /// </summary>
        public string Pass { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public Ed25519PassCredential()
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="key">Api key/label</param>
        /// <param name="privateKey">Private key</param>
        /// <param name="pass">Passphrase</param>
        public Ed25519PassCredential(string key, string privateKey, string pass) : base(key, privateKey)
        {
            Pass = pass;
        }

        /// <inheritdoc />
        public override ApiCredentials Copy() => new Ed25519PassCredential(Key, PrivateKey, Pass);

        /// <inheritdoc />
        public override void Validate()
        {
            base.Validate();
            if (string.IsNullOrEmpty(Pass))
                throw new ArgumentException("Pass unset", nameof(Pass));
        }
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
        /// ctor
        /// </summary>
        public ECDsaCredential()
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="key">Public key</param>
        /// <param name="privateKey">Private key</param>
        public ECDsaCredential(string key, string privateKey) : base(key)
        {
            PrivateKey = privateKey;
        }

        /// <inheritdoc />
        public override ApiCredentials Copy() => new ECDsaCredential(Key, PrivateKey);

        /// <inheritdoc />
        public override void Validate()
        {
            base.Validate();
            if (string.IsNullOrEmpty(PrivateKey))
                throw new ArgumentException("PrivateKey unset", nameof(PrivateKey));
        }
    }

    /// <summary>
    /// ECDsa credentials
    /// </summary>
    public class ECDsaPassCredential : ECDsaCredential
    {
        /// <summary>
        /// Passphrase
        /// </summary>
        public string Pass { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public ECDsaPassCredential()
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="key">Api key/label</param>
        /// <param name="privateKey">Private key</param>
        /// <param name="pass">Passphrase</param>
        public ECDsaPassCredential(string key, string privateKey, string pass) : base(key, privateKey)
        {
            Pass = pass;
        }

        /// <inheritdoc />
        public override ApiCredentials Copy() => new ECDsaPassCredential(Key, PrivateKey, Pass);

        /// <inheritdoc />
        public override void Validate()
        {
            base.Validate();
            if (string.IsNullOrEmpty(Pass))
                throw new ArgumentException("Pass unset", nameof(Pass));
        }
    }
}
