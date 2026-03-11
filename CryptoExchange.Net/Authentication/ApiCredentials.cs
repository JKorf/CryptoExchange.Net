using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Authentication
{
    /// <summary>
    /// Api credentials, used to sign requests accessing private endpoints
    /// </summary>
    public class ApiCredentials
    {
        /// <summary>
        /// The credential pairs contained in these API credentials. This can contain multiple credential pairs when the API requires different credentials for different endpoints.
        /// </summary>
        public CredentialPair[] CredentialPairs { get; protected set; } = Array.Empty<CredentialPair>();
        /// <summary>
        /// HMAC credentials
        /// </summary>
        public HMACCredential? Hmac
        {
            get => (HMACCredential?)CredentialPairs.SingleOrDefault(x => x.CredentialType == ApiCredentialsType.Hmac);
            set => AddOrRemoveCredential(ApiCredentialsType.Hmac, value);
        }

        /// <summary>
        /// RSA credentials in XML format
        /// </summary>
        public RSAXmlCredential? RsaXml
        {
            get => (RSAXmlCredential?)CredentialPairs.SingleOrDefault(x => x.CredentialType == ApiCredentialsType.Rsa);
            set => AddOrRemoveCredential(ApiCredentialsType.Rsa, value);
        }

#if NETSTANDARD2_1_OR_GREATER || NET7_0_OR_GREATER
        /// <summary>
        /// RSA credentials in PEM/Base64 format
        /// </summary>
        public RSAPemCredential? RsaPem
        {
            get => (RSAPemCredential?)CredentialPairs.SingleOrDefault(x => x.CredentialType == ApiCredentialsType.Rsa);
            set => AddOrRemoveCredential(ApiCredentialsType.Rsa, value);
        }
#endif

#if NET8_0_OR_GREATER
        /// <summary>
        /// ED25519 credentials
        /// </summary>
        public ED25519Credential? Ed25519
        {
            get => (ED25519Credential?)CredentialPairs.SingleOrDefault(x => x.CredentialType == ApiCredentialsType.Ed25519);
            set => AddOrRemoveCredential(ApiCredentialsType.Ed25519, value);
        }
#endif
        /// <summary>
        /// ECDSA credentials
        /// </summary>
        public ECDSACredential? Ecdsa
        {
            get => (ECDSACredential?)CredentialPairs.SingleOrDefault(x => x.CredentialType == ApiCredentialsType.Ecdsa);
            set => AddOrRemoveCredential(ApiCredentialsType.Ecdsa, value);
        }

        /// <summary>
        /// If credential is null attempt to remove the credential of the type, else add it
        /// </summary>
        protected void AddOrRemoveCredential(ApiCredentialsType type, CredentialPair? credential)
        {
            if (credential is null)
            {
                var cred = CredentialPairs.SingleOrDefault(x => x.CredentialType == type);
                if (cred != null)
                {
                    var newList = CredentialPairs.ToList();
                    newList.Remove(cred);
                    CredentialPairs = newList.ToArray();
                }
            }
            else
            {
                var newList = CredentialPairs.ToList();
                newList.Add(credential);
                CredentialPairs = newList.ToArray();
            }
        }

        /// <summary>
        /// DI constructor
        /// </summary>
        [Obsolete("Parameterless constructor is only for deserialization purposes and should not be used directly. Use static ApiCredentials.HmacCredentials or similar instead.")]
        public ApiCredentials() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="secret"></param>
        /// <param name="pass"></param>
        /// <param name="credentialType"></param>
        /// <exception cref="NotImplementedException"></exception>
        public ApiCredentials(string key, string secret, string? pass = null, ApiCredentialsType credentialType = ApiCredentialsType.Hmac)
        {
            if (credentialType == ApiCredentialsType.Hmac)
            {
                CredentialPairs = new[] { new HMACCredential(key, secret, pass) };
            }
            else if (credentialType == ApiCredentialsType.Rsa)
            {
                if (key.StartsWith("<"))
                    CredentialPairs = new[] { new RSAXmlCredential(key, secret) };
#if NETSTANDARD2_1_OR_GREATER || NET7_0_OR_GREATER
                else
                    CredentialPairs = new[] { new RSAPemCredential(key, secret) };
#else
                else
                    throw new NotImplementedException("Unknown RSA key format");
#endif
            }
#if NET8_0_OR_GREATER
            else if (credentialType == ApiCredentialsType.Ed25519)
            {
                CredentialPairs = new[] { new ED25519Credential(key, secret) };
            }
#endif
            else if (credentialType == ApiCredentialsType.Ecdsa)
            {
                CredentialPairs = new[] { new ECDSACredential(key, secret) };
            }
            else
            {
                throw new NotImplementedException("Unknown API credentials type");
            }
        }

        /// <summary>
        /// Create API credentials using the provided credential pair
        /// </summary>
        public ApiCredentials(CredentialPair credential)
        {
            CredentialPairs = [credential];
        }

        /// <summary>
        /// Create API credentials using the provided credential pairs
        /// </summary>
        public ApiCredentials(params IEnumerable<CredentialPair> credentials)
        {
            CredentialPairs = credentials.ToArray();
        }

        /// <summary>
        /// Is any of the credential types in these credentials supported by the API client
        /// </summary>
        public bool IsAnySupported(ApiCredentialsType[] credentialTypes)
        {
            return CredentialPairs.Any(c => credentialTypes.Contains(c.CredentialType));
        }

        /// <summary>
        /// Get credential of a specific type
        /// </summary>
        public CredentialPair? GetCredential(ApiCredentialsType credentialType)
        {
            return CredentialPairs.SingleOrDefault(c => c.CredentialType == credentialType);
        }

        /// <summary>
        /// Get credentials of a specific type
        /// </summary>
        public T? GetCredential<T>() where T : CredentialPair
        {
            return CredentialPairs.OfType<T>().SingleOrDefault();
        }

        /// <summary>
        /// Create API credentials using an API key and secret generated by the server
        /// </summary>
        public static ApiCredentials HmacCredentials(string apiKey, string apiSecret, string? pass = null)
        {
            return new ApiCredentials(new HMACCredential(apiKey, apiSecret, pass));
        }

#if NETSTANDARD2_1_OR_GREATER || NET7_0_OR_GREATER
        /// <summary>
        /// Create API credentials using an API key and an RSA private key in PEM format
        /// </summary>
        public static ApiCredentials RsaPemCredentials(string publicKey, string privateKey)
        {
            return new ApiCredentials(new RSAPemCredential(publicKey, privateKey));
        }
#endif

        /// <summary>
        /// Create API credentials using an API key and an RSA private key in XML format
        /// </summary>
        public static ApiCredentials RsaXmlCredentials(string publicKey, string privateKey)
        {
            return new ApiCredentials(new RSAXmlCredential(publicKey, privateKey));
        }

#if NET8_0_OR_GREATER
        /// <summary>
        /// Create API credentials using an API key and an Ed25519 private key
        /// </summary>
        public static ApiCredentials Ed25519Credentials(string publicKey, string privateKey)
        {
            return new ApiCredentials(new ED25519Credential(publicKey, privateKey));
        }
#endif

        /// <summary>
        /// Create API credentials using an API key and an Ecdsa private key
        /// </summary>
        public static ApiCredentials EcdsaCredentials(string publicKey, string privateKey)
        {
            return new ApiCredentials(new ECDSACredential(publicKey, privateKey));
        }

        /// <summary>
        /// Load a key from a file
        /// </summary>
        public static string ReadFromFile(string path)
        {
            using var fileStream = File.OpenRead(path);
            using var streamReader = new StreamReader(fileStream);
            return streamReader.ReadToEnd();
        }

        /// <summary>
        /// Copy the credentials
        /// </summary>
        /// <returns></returns>
        public ApiCredentials Copy()
        {
            return new ApiCredentials(CredentialPairs);
        }
    }
}
