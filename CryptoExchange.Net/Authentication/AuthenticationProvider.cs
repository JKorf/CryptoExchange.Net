using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Converters.SystemTextJson;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        internal IAuthTimeProvider TimeProvider { get; set; } = new AuthTimeProvider();

        /// <summary>
        /// Provided credentials
        /// </summary>
        protected internal readonly ApiCredentials _credentials;

        /// <summary>
        /// Byte representation of the secret
        /// </summary>
        protected byte[] _sBytes;

        /// <summary>
        /// Get the API key of the current credentials
        /// </summary>
        public string ApiKey => _credentials.Key;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="credentials"></param>
        protected AuthenticationProvider(ApiCredentials credentials)
        {
            if (credentials.Secret == null)
                throw new ArgumentException("ApiKey/Secret needed");

            _credentials = credentials;
            _sBytes = Encoding.UTF8.GetBytes(credentials.Secret);
        }

        /// <summary>
        /// Authenticate a request. Output parameters should include the providedParameters input
        /// </summary>
        /// <param name="apiClient">The Api client sending the request</param>
        /// <param name="uri">The uri for the request</param>
        /// <param name="method">The method of the request</param>
        /// <param name="auth">If the requests should be authenticated</param>
        /// <param name="arraySerialization">Array serialization type</param>
        /// <param name="requestBodyFormat">The formatting of the request body</param>
        /// <param name="uriParameters">Parameters that need to be in the Uri of the request. Should include the provided parameters if they should go in the uri</param>
        /// <param name="bodyParameters">Parameters that need to be in the body of the request. Should include the provided parameters if they should go in the body</param>
        /// <param name="headers">The headers that should be send with the request</param>
        /// <param name="parameterPosition">The position where the providedParameters should go</param>
        public abstract void AuthenticateRequest(
            RestApiClient apiClient,
            Uri uri,
            HttpMethod method,
            ref IDictionary<string, object>? uriParameters,
            ref IDictionary<string, object>? bodyParameters,
            ref Dictionary<string, string>? headers,
            bool auth,
            ArrayParametersSerialization arraySerialization,
            HttpMethodParameterPosition parameterPosition,
            RequestBodyFormat requestBodyFormat
            );

        /// <summary>
        /// SHA256 sign the data and return the bytes
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected static byte[] SignSHA256Bytes(string data)
        {
            using var encryptor = SHA256.Create();
            return encryptor.ComputeHash(Encoding.UTF8.GetBytes(data));
        }

        /// <summary>
        /// SHA256 sign the data and return the bytes
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected static byte[] SignSHA256Bytes(byte[] data)
        {
            using var encryptor = SHA256.Create();
            return encryptor.ComputeHash(data);
        }

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
            return outputType == SignOutputType.Base64 ? BytesToBase64String(resultBytes) : BytesToHexString(resultBytes);
        }

        /// <summary>
        /// SHA256 sign the data and return the hash
        /// </summary>
        /// <param name="data">Data to sign</param>
        /// <param name="outputType">String type</param>
        /// <returns></returns>
        protected static string SignSHA256(byte[] data, SignOutputType? outputType = null)
        {
            using var encryptor = SHA256.Create();
            var resultBytes = encryptor.ComputeHash(data);
            return outputType == SignOutputType.Base64 ? BytesToBase64String(resultBytes) : BytesToHexString(resultBytes);
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
        /// SHA384 sign the data and return the hash
        /// </summary>
        /// <param name="data">Data to sign</param>
        /// <param name="outputType">String type</param>
        /// <returns></returns>
        protected static string SignSHA384(byte[] data, SignOutputType? outputType = null)
        {
            using var encryptor = SHA384.Create();
            var resultBytes = encryptor.ComputeHash(data);
            return outputType == SignOutputType.Base64 ? BytesToBase64String(resultBytes) : BytesToHexString(resultBytes);
        }

        /// <summary>
        /// SHA384 sign the data and return the hash
        /// </summary>
        /// <param name="data">Data to sign</param>
        /// <returns></returns>
        protected static byte[] SignSHA384Bytes(string data)
        {
            using var encryptor = SHA384.Create();
            return encryptor.ComputeHash(Encoding.UTF8.GetBytes(data));
        }

        /// <summary>
        /// SHA384 sign the data and return the hash
        /// </summary>
        /// <param name="data">Data to sign</param>
        /// <returns></returns>
        protected static byte[] SignSHA384Bytes(byte[] data)
        {
            using var encryptor = SHA384.Create();
            return encryptor.ComputeHash(data);
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
        /// SHA512 sign the data and return the hash
        /// </summary>
        /// <param name="data">Data to sign</param>
        /// <param name="outputType">String type</param>
        /// <returns></returns>
        protected static string SignSHA512(byte[] data, SignOutputType? outputType = null)
        {
            using var encryptor = SHA512.Create();
            var resultBytes = encryptor.ComputeHash(data);
            return outputType == SignOutputType.Base64 ? BytesToBase64String(resultBytes) : BytesToHexString(resultBytes);
        }

        /// <summary>
        /// SHA512 sign the data and return the hash
        /// </summary>
        /// <param name="data">Data to sign</param>
        /// <returns></returns>
        protected static byte[] SignSHA512Bytes(string data)
        {
            using var encryptor = SHA512.Create();
            return encryptor.ComputeHash(Encoding.UTF8.GetBytes(data));
        }

        /// <summary>
        /// SHA512 sign the data and return the hash
        /// </summary>
        /// <param name="data">Data to sign</param>
        /// <returns></returns>
        protected static byte[] SignSHA512Bytes(byte[] data)
        {
            using var encryptor = SHA512.Create();
            return encryptor.ComputeHash(data);
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
        /// MD5 sign the data and return the hash
        /// </summary>
        /// <param name="data">Data to sign</param>
        /// <param name="outputType">String type</param>
        /// <returns></returns>
        protected static string SignMD5(byte[] data, SignOutputType? outputType = null)
        {
            using var encryptor = MD5.Create();
            var resultBytes = encryptor.ComputeHash(data);
            return outputType == SignOutputType.Base64 ? BytesToBase64String(resultBytes) : BytesToHexString(resultBytes);
        }

        /// <summary>
        /// MD5 sign the data and return the hash
        /// </summary>
        /// <param name="data">Data to sign</param>
        /// <returns></returns>
        protected static byte[] SignMD5Bytes(string data)
        {
            using var encryptor = MD5.Create();
            return encryptor.ComputeHash(Encoding.UTF8.GetBytes(data));
        }

        /// <summary>
        /// HMACSHA256 sign the data and return the hash
        /// </summary>
        /// <param name="data">Data to sign</param>
        /// <param name="outputType">String type</param>
        /// <returns></returns>
        protected string SignHMACSHA256(string data, SignOutputType? outputType = null)
            => SignHMACSHA256(Encoding.UTF8.GetBytes(data), outputType);

        /// <summary>
        /// HMACSHA256 sign the data and return the hash
        /// </summary>
        /// <param name="data">Data to sign</param>
        /// <param name="outputType">String type</param>
        /// <returns></returns>
        protected string SignHMACSHA256(byte[] data, SignOutputType? outputType = null)
        {
            using var encryptor = new HMACSHA256(_sBytes);
            var resultBytes = encryptor.ComputeHash(data);
            return outputType == SignOutputType.Base64 ? BytesToBase64String(resultBytes) : BytesToHexString(resultBytes);
        }

        /// <summary>
        /// HMACSHA384 sign the data and return the hash
        /// </summary>
        /// <param name="data">Data to sign</param>
        /// <param name="outputType">String type</param>
        /// <returns></returns>
        protected string SignHMACSHA384(string data, SignOutputType? outputType = null)
            => SignHMACSHA384(Encoding.UTF8.GetBytes(data), outputType);

        /// <summary>
        /// HMACSHA384 sign the data and return the hash
        /// </summary>
        /// <param name="data">Data to sign</param>
        /// <param name="outputType">String type</param>
        /// <returns></returns>
        protected string SignHMACSHA384(byte[] data, SignOutputType? outputType = null)
        {
            using var encryptor = new HMACSHA384(_sBytes);
            var resultBytes = encryptor.ComputeHash(data);
            return outputType == SignOutputType.Base64 ? BytesToBase64String(resultBytes) : BytesToHexString(resultBytes);
        }

        /// <summary>
        /// HMACSHA512 sign the data and return the hash
        /// </summary>
        /// <param name="data">Data to sign</param>
        /// <param name="outputType">String type</param>
        /// <returns></returns>
        protected string SignHMACSHA512(string data, SignOutputType? outputType = null)
            => SignHMACSHA512(Encoding.UTF8.GetBytes(data), outputType);

        /// <summary>
        /// HMACSHA512 sign the data and return the hash
        /// </summary>
        /// <param name="data">Data to sign</param>
        /// <param name="outputType">String type</param>
        /// <returns></returns>
        protected string SignHMACSHA512(byte[] data, SignOutputType? outputType = null)
        {
            using var encryptor = new HMACSHA512(_sBytes);
            var resultBytes = encryptor.ComputeHash(data);
            return outputType == SignOutputType.Base64 ? BytesToBase64String(resultBytes) : BytesToHexString(resultBytes);
        }

        /// <summary>
        /// SHA256 sign the data
        /// </summary>
        /// <param name="data"></param>
        /// <param name="outputType"></param>
        /// <returns></returns>
        protected string SignRSASHA256(byte[] data, SignOutputType? outputType = null)
        {
            using var rsa = CreateRSA();
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(data);
            var resultBytes = rsa.SignHash(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            return outputType == SignOutputType.Base64? BytesToBase64String(resultBytes) : BytesToHexString(resultBytes);
        }

        /// <summary>
        /// SHA384 sign the data
        /// </summary>
        /// <param name="data"></param>
        /// <param name="outputType"></param>
        /// <returns></returns>
        protected string SignRSASHA384(byte[] data, SignOutputType? outputType = null)
        {
            using var rsa = CreateRSA();
            using var sha384 = SHA384.Create();
            var hash = sha384.ComputeHash(data);
            var resultBytes = rsa.SignHash(hash, HashAlgorithmName.SHA384, RSASignaturePadding.Pkcs1);
            return outputType == SignOutputType.Base64 ? BytesToBase64String(resultBytes) : BytesToHexString(resultBytes);
        }

        /// <summary>
        /// SHA512 sign the data
        /// </summary>
        /// <param name="data"></param>
        /// <param name="outputType"></param>
        /// <returns></returns>
        protected string SignRSASHA512(byte[] data, SignOutputType? outputType = null)
        {
            using var rsa = CreateRSA();
            using var sha512 = SHA512.Create();
            var hash = sha512.ComputeHash(data);
            var resultBytes = rsa.SignHash(hash, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
            return outputType == SignOutputType.Base64 ? BytesToBase64String(resultBytes) : BytesToHexString(resultBytes);
        }

        private RSA CreateRSA()
        {
            var rsa = RSA.Create();
            if (_credentials.CredentialType == ApiCredentialsType.RsaPem)
            {
#if NETSTANDARD2_1_OR_GREATER
                // Read from pem private key
                var key = _credentials.Secret!
                        .Replace("\n", "")
                        .Replace("-----BEGIN PRIVATE KEY-----", "")
                        .Replace("-----END PRIVATE KEY-----", "")
                        .Trim();
                rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(
                    key)
                    , out _);
#else
                throw new Exception("Pem format not supported when running from .NetStandard2.0. Convert the private key to xml format.");
#endif
            }
            else if (_credentials.CredentialType == ApiCredentialsType.RsaXml)
            {
                // Read from xml private key format
                rsa.FromXmlString(_credentials.Secret!);
            }
            else
            {
                throw new Exception("Invalid credentials type");
            }

            return rsa;
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

        /// <summary>
        /// Get current timestamp including the time sync offset from the api client
        /// </summary>
        /// <param name="apiClient"></param>
        /// <returns></returns>
        protected DateTime GetTimestamp(RestApiClient apiClient)
        {
            return TimeProvider.GetTime().Add(apiClient.GetTimeOffset() ?? TimeSpan.Zero)!;
        }

        /// <summary>
        /// Get millisecond timestamp as a string including the time sync offset from the api client
        /// </summary>
        /// <param name="apiClient"></param>
        /// <returns></returns>
        protected string GetMillisecondTimestamp(RestApiClient apiClient)
        {
            return DateTimeConverter.ConvertToMilliseconds(GetTimestamp(apiClient)).Value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Return the serialized request body
        /// </summary>
        /// <param name="serializer"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected string GetSerializedBody(IMessageSerializer serializer, IDictionary<string, object> parameters)
        {
            if (parameters.Count == 1 && parameters.ContainsKey(Constants.BodyPlaceHolderKey))
                return serializer.Serialize(parameters[Constants.BodyPlaceHolderKey]);
            else
                return serializer.Serialize(parameters);
        }
    }

    /// <inheritdoc />
    public abstract class AuthenticationProvider<TApiCredentials> : AuthenticationProvider where TApiCredentials : ApiCredentials
    {
        /// <inheritdoc />
        protected new TApiCredentials _credentials => (TApiCredentials)base._credentials;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="credentials"></param>
        protected AuthenticationProvider(TApiCredentials credentials) : base(credentials)
        {
        }
    }
}
