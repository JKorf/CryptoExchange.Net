using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace CryptoExchange.Net.Authentication
{
    /// <summary>
    /// Base class for authentication providers
    /// </summary>
    public abstract class AuthenticationProvider
    {
        /// <summary>
        /// The provided credentials
        /// </summary>
        public ApiCredentials Credentials { get; }

        /// <summary>
        /// </summary>
        protected byte[] _sBytes;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="credentials"></param>
        protected AuthenticationProvider(ApiCredentials credentials)
        {
            if (credentials.Secret == null)
                throw new ArgumentException("ApiKey/Secret needed");

            Credentials = credentials;
            _sBytes = Encoding.UTF8.GetBytes(credentials.Secret.GetString());
        }

        /// <summary>
        /// Authenticate a request where the parameters need to be in the Uri
        /// </summary>
        /// <param name="apiClient">The Api client sending the request</param>
        /// <param name="uri">The uri for the request</param>
        /// <param name="method">The method of the request</param>
        /// <param name="parameters">The request parameters</param>
        /// <param name="headers">The request headers</param>
        /// <param name="auth">If the requests should be authenticated</param>
        /// <param name="arraySerialization">Array serialization type</param>
        /// <returns></returns>
        public abstract void AuthenticateUriRequest(
            RestApiClient apiClient,
            ref Uri uri,
            HttpMethod method,
            SortedDictionary<string, object> parameters,
            Dictionary<string, string> headers,
            bool auth,
            ArrayParametersSerialization arraySerialization);

        /// <summary>
        /// Authenticate a request where the parameters need to be in the request body
        /// </summary>
        /// <param name="apiClient">The Api client sending the request</param>
        /// <param name="uri">The uri for the request</param>
        /// <param name="method">The method of the request</param>
        /// <param name="parameters">The request parameters</param>
        /// <param name="headers">The request headers</param>
        /// <param name="auth">If the requests should be authenticated</param>
        /// <param name="arraySerialization">Array serialization type</param>
        public abstract void AuthenticateBodyRequest(
            RestApiClient apiClient,
            Uri uri,
            HttpMethod method,
            SortedDictionary<string, object> parameters,
            Dictionary<string, string> headers,
            bool auth,
            ArrayParametersSerialization arraySerialization);

        /// <summary>
        /// SHA256 sign the data and return the hash
        /// </summary>
        /// <param name="data">Data to sign</param>
        /// <param name="outputType">String type</param>
        /// <returns></returns>
        protected static string SignSHA256(string data, SignOutputType? outputType = null)
        {
            using var encryptor = SHA256.Create();
            var resultBytes = encryptor.ComputeHash(Encoding.UTF8.GetBytes(data));
            return outputType == SignOutputType.Base64 ? BytesToBase64String(resultBytes): BytesToHexString(resultBytes);
        }

        /// <summary>
        /// SHA384 sign the data and return the hash
        /// </summary>
        /// <param name="data">Data to sign</param>
        /// <param name="outputType">String type</param>
        /// <returns></returns>
        protected static string SignSHA384(string data, SignOutputType? outputType = null)
        {
            using var encryptor = SHA384.Create();
            var resultBytes = encryptor.ComputeHash(Encoding.UTF8.GetBytes(data));
            return outputType == SignOutputType.Base64 ? BytesToBase64String(resultBytes) : BytesToHexString(resultBytes);
        }

        /// <summary>
        /// SHA512 sign the data and return the hash
        /// </summary>
        /// <param name="data">Data to sign</param>
        /// <param name="outputType">String type</param>
        /// <returns></returns>
        protected static string SignSHA512(string data, SignOutputType? outputType = null)
        {
            using var encryptor = SHA512.Create();
            var resultBytes = encryptor.ComputeHash(Encoding.UTF8.GetBytes(data));
            return outputType == SignOutputType.Base64 ? BytesToBase64String(resultBytes) : BytesToHexString(resultBytes);
        }

        /// <summary>
        /// MD5 sign the data and return the hash
        /// </summary>
        /// <param name="data">Data to sign</param>
        /// <param name="outputType">String type</param>
        /// <returns></returns>
        protected static string SignMD5(string data, SignOutputType? outputType = null)
        {
            using var encryptor = MD5.Create();
            var resultBytes = encryptor.ComputeHash(Encoding.UTF8.GetBytes(data));
            return outputType == SignOutputType.Base64 ? BytesToBase64String(resultBytes) : BytesToHexString(resultBytes);
        }

        /// <summary>
        /// HMACSHA256 sign the data and return the hash
        /// </summary>
        /// <param name="data">Data to sign</param>
        /// <param name="outputType">String type</param>
        /// <returns></returns>
        protected string SignHMACSHA256(string data, SignOutputType? outputType = null)
        {
            using var encryptor = new HMACSHA256(_sBytes);
            var resultBytes = encryptor.ComputeHash(Encoding.UTF8.GetBytes(data));
            return outputType == SignOutputType.Base64 ? BytesToBase64String(resultBytes) : BytesToHexString(resultBytes);
        }

        /// <summary>
        /// HMACSHA384 sign the data and return the hash
        /// </summary>
        /// <param name="data">Data to sign</param>
        /// <param name="outputType">String type</param>
        /// <returns></returns>
        protected string SignHMACSHA384(string data, SignOutputType? outputType = null)
        {
            using var encryptor = new HMACSHA384(_sBytes);
            var resultBytes = encryptor.ComputeHash(Encoding.UTF8.GetBytes(data));
            return outputType == SignOutputType.Base64 ? BytesToBase64String(resultBytes) : BytesToHexString(resultBytes);
        }

        /// <summary>
        /// HMACSHA512 sign the data and return the hash
        /// </summary>
        /// <param name="data">Data to sign</param>
        /// <param name="outputType">String type</param>
        /// <returns></returns>
        protected string SignHMACSHA512(string data, SignOutputType? outputType = null)
        {
            using var encryptor = new HMACSHA512(_sBytes);
            var resultBytes = encryptor.ComputeHash(Encoding.UTF8.GetBytes(data));
            return outputType == SignOutputType.Base64 ? BytesToBase64String(resultBytes) : BytesToHexString(resultBytes);
        }

        /// <summary>
        /// Sign a string
        /// </summary>
        /// <param name="toSign"></param>
        /// <returns></returns>
        public virtual string Sign(string toSign)
        {
            return toSign;
        }

        /// <summary>
        /// Sign a byte array
        /// </summary>
        /// <param name="toSign"></param>
        /// <returns></returns>
        public virtual byte[] Sign(byte[] toSign)
        {
            return toSign;
        }

        /// <summary>
        /// Convert byte array to hex string
        /// </summary>
        /// <param name="buff"></param>
        /// <returns></returns>
        protected static string BytesToHexString(byte[] buff)
        {
            var result = string.Empty;
            foreach (var t in buff)
                result += t.ToString("X2");
            return result;
        }

        /// <summary>
        /// Convert byte array to base64 string
        /// </summary>
        /// <param name="buff"></param>
        /// <returns></returns>
        protected static string BytesToBase64String(byte[] buff)
        {
            return Convert.ToBase64String(buff);
        }
    }
}
