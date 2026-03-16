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
        public HMACCredential? HMAC
        {
            get => (HMACCredential?)CredentialPairs.SingleOrDefault(x => x.CredentialType == ApiCredentialsType.HMAC);
            set => AddOrRemoveCredential(ApiCredentialsType.HMAC, value);
        }

        /// <summary>
        /// RSA credentials
        /// </summary>
        public RSACredential? RSA
        {
            get => (RSACredential?)CredentialPairs.SingleOrDefault(x => x.CredentialType == ApiCredentialsType.RSA);
            set => AddOrRemoveCredential(ApiCredentialsType.RSA, value);
        }

        /// <summary>
        /// RSA credentials in XML format
        /// </summary>
        public RSAXmlCredential? RSAXml
        {
            get => (RSAXmlCredential?)CredentialPairs.SingleOrDefault(x => x.CredentialType == ApiCredentialsType.RSA);
            set => AddOrRemoveCredential(ApiCredentialsType.RSA, value);
        }

#if NETSTANDARD2_1_OR_GREATER || NET7_0_OR_GREATER
        /// <summary>
        /// RSA credentials in PEM/Base64 format
        /// </summary>
        public RSAPemCredential? RSAPem
        {
            get => (RSAPemCredential?)CredentialPairs.SingleOrDefault(x => x.CredentialType == ApiCredentialsType.RSA);
            set => AddOrRemoveCredential(ApiCredentialsType.RSA, value);
        }
#endif

#if NET8_0_OR_GREATER
        /// <summary>
        /// Ed25519 credentials
        /// </summary>
        public Ed25519Credential? Ed25519
        {
            get => (Ed25519Credential?)CredentialPairs.SingleOrDefault(x => x.CredentialType == ApiCredentialsType.Ed25519);
            set => AddOrRemoveCredential(ApiCredentialsType.Ed25519, value);
        }
#endif
        /// <summary>
        /// ECDsa credentials
        /// </summary>
        public ECDsaCredential? ECDsa
        {
            get => (ECDsaCredential?)CredentialPairs.SingleOrDefault(x => x.CredentialType == ApiCredentialsType.ECDsa);
            set => AddOrRemoveCredential(ApiCredentialsType.ECDsa, value);
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
        [Obsolete("Parameterless constructor is only for deserialization purposes and should not be used directly.")]
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
