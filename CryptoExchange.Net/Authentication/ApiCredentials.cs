using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoExchange.Net.Authentication
{
    /// <summary>
    /// Api credentials, used to sign requests accessing private endpoints
    /// </summary>
    public abstract class ApiCredentials
    {
        /// <summary>
        /// The credential pairs contained in these API credentials. This can contain multiple credential pairs when the API requires different credentials for different endpoints.
        /// </summary>
        public CredentialPair[] CredentialPairs { get; protected set; } = Array.Empty<CredentialPair>();

        /// <summary>
        /// The public key/identifier for the provided credentials
        /// </summary>
        public string Key => CredentialPairs.First().Key;

        /// <summary>
        /// HMAC credentials
        /// </summary>
        public HMACCredential? Hmac
        {
            get => (HMACCredential?)CredentialPairs.SingleOrDefault(x => x.CredentialType == ApiCredentialsType.Hmac);
            set => AddOrRemoveCredential(ApiCredentialsType.Hmac, value);
        }

        /// <summary>
        /// RSA credentials
        /// </summary>
        public RSACredential? Rsa
        {
            get => (RSACredential?)CredentialPairs.SingleOrDefault(x => x.CredentialType == ApiCredentialsType.Rsa);
            set => AddOrRemoveCredential(ApiCredentialsType.Rsa, value);
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
        /// API key credentials
        /// </summary>
        public ApiKeyCredential? ApiKey
        {
            get => (ApiKeyCredential?)CredentialPairs.SingleOrDefault(x => x.CredentialType == ApiCredentialsType.ApiKey);
            set => AddOrRemoveCredential(ApiCredentialsType.ApiKey, value);
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
        /// Create API credentials using the provided credential pair
        /// </summary>
        public ApiCredentials(CredentialPair credential)
        {
            CredentialPairs = [credential];
        }

        /// <summary>
        /// Create API credentials using the provided credential pairs
        /// </summary>
        public ApiCredentials(params IEnumerable<CredentialPair?> credentials)
        {
            CredentialPairs = credentials.Where(x => x != null).ToArray()!;
        }

        /// <summary>
        /// Get credentials of a specific type
        /// </summary>
        public T? GetCredential<T>() where T : CredentialPair
        {
            return CredentialPairs.OfType<T>().SingleOrDefault();
        }

        /// <summary>
        /// Copy the credentials
        /// </summary>
        /// <returns></returns>
        public abstract ApiCredentials Copy();
    }
}
