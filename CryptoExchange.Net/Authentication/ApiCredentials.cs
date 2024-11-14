﻿using System;
using System.IO;
using CryptoExchange.Net.Converters.SystemTextJson;
using CryptoExchange.Net.Converters.MessageParsing;

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
        /// Type of the credentials
        /// </summary>
        public ApiCredentialsType CredentialType { get; set; }

        /// <summary>
        /// Create Api credentials providing an api key and secret for authentication
        /// </summary>
        /// <param name="key">The api key / label used for identification</param>
        /// <param name="secret">The api secret or private key used for signing</param>
        /// <param name="credentialType">The type of credentials</param>
        public ApiCredentials(string key, string secret, ApiCredentialsType credentialType = ApiCredentialsType.Hmac)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(secret))
                throw new ArgumentException("Key and secret can't be null/empty");

            CredentialType = credentialType;
            Key = key;
            Secret = secret;
        }

        /// <summary>
        /// Copy the credentials
        /// </summary>
        /// <returns></returns>
        public virtual ApiCredentials Copy()
        {
            return new ApiCredentials(Key, Secret, CredentialType);
        }

        /// <summary>
        /// Create Api credentials providing a stream containing json data. The json data should include two values: apiKey and apiSecret
        /// </summary>
        /// <param name="inputStream">The stream containing the json data</param>
        /// <param name="identifierKey">A key to identify the credentials for the API. For example, when set to `binanceKey` the json data should contain a value for the property `binanceKey`. Defaults to 'apiKey'.</param>
        /// <param name="identifierSecret">A key to identify the credentials for the API. For example, when set to `binanceSecret` the json data should contain a value for the property `binanceSecret`. Defaults to 'apiSecret'.</param>
        public static ApiCredentials FromStream(Stream inputStream, string? identifierKey = null, string? identifierSecret = null)
        {
            var accessor = new SystemTextJsonStreamMessageAccessor();
            if (!accessor.Read(inputStream, false).Result)
                throw new ArgumentException("Input stream not valid json data");

            var key = accessor.GetValue<string>(MessagePath.Get().Property(identifierKey ?? "apiKey"));
            var secret = accessor.GetValue<string>(MessagePath.Get().Property(identifierSecret ?? "apiSecret"));
            if (key == null || secret == null)
                throw new ArgumentException("apiKey or apiSecret value not found in Json credential file");
            
            inputStream.Seek(0, SeekOrigin.Begin);
            return new ApiCredentials(key, secret);
        }
    }
}
