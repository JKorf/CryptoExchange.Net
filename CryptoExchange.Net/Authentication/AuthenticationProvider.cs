using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Converters.SystemTextJson;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
#if NET8_0_OR_GREATER
using NSec.Cryptography;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using CryptoExchange.Net.Sockets;
using CryptoExchange.Net.Sockets.Default;
using System.Net;

namespace CryptoExchange.Net.Authentication
{
    /// <summary>
    /// Base class for authentication providers
    /// </summary>
    public abstract class AuthenticationProvider
    {
        internal IAuthTimeProvider TimeProvider { get; set; } = new AuthTimeProvider();

        /// <summary>
        /// The public identifier for the provided credentials
        /// </summary>
        public abstract string Key { get; }

        /// <summary>
        /// Authenticate a REST request
        /// </summary>
        /// <param name="apiClient">The API client sending the request</param>
        /// <param name="requestConfig">The request configuration</param>
        public abstract void ProcessRequest(RestApiClient apiClient, RestRequestConfiguration requestConfig);

        /// <summary>
        /// Get an authentication query for a websocket
        /// </summary>
        /// <param name="apiClient">The API client sending the request</param>
        /// <param name="connection">The connection to authenticate</param>
        /// <param name="context">Optional context required for creating the authentication query</param>
        public virtual Query? GetAuthenticationQuery(SocketApiClient apiClient, SocketConnection connection, Dictionary<string, object?>? context = null) => null;

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
        protected string SignHMACSHA256(HMACCredential credential, string data, SignOutputType? outputType = null)
            => SignHMACSHA256(credential,Encoding.UTF8.GetBytes(data), outputType);

        /// <summary>
        /// HMACSHA256 sign the data and return the hash
        /// </summary>
        protected string SignHMACSHA256(HMACCredential credential, byte[] data, SignOutputType? outputType = null)
        {
            using var encryptor = new HMACSHA256(credential.GetSBytes());
            var resultBytes = encryptor.ComputeHash(data);
            return outputType == SignOutputType.Base64 ? BytesToBase64String(resultBytes) : BytesToHexString(resultBytes);
        }

        /// <summary>
        /// HMACSHA384 sign the data and return the hash
        /// </summary>
        protected string SignHMACSHA384(HMACCredential credential, string data, SignOutputType? outputType = null)
            => SignHMACSHA384(credential, Encoding.UTF8.GetBytes(data), outputType);

        /// <summary>
        /// HMACSHA384 sign the data and return the hash
        /// </summary>
        protected string SignHMACSHA384(HMACCredential credential, byte[] data, SignOutputType? outputType = null)
        {
            using var encryptor = new HMACSHA384(credential.GetSBytes());
            var resultBytes = encryptor.ComputeHash(data);
            return outputType == SignOutputType.Base64 ? BytesToBase64String(resultBytes) : BytesToHexString(resultBytes);
        }

        /// <summary>
        /// HMACSHA512 sign the data and return the hash
        /// </summary>
        protected string SignHMACSHA512(HMACCredential credential, string data, SignOutputType? outputType = null)
            => SignHMACSHA512(credential, Encoding.UTF8.GetBytes(data), outputType);

        /// <summary>
        /// HMACSHA512 sign the data and return the hash
        /// </summary>
        protected string SignHMACSHA512(HMACCredential credential, byte[] data, SignOutputType? outputType = null)
        {
            using var encryptor = new HMACSHA512(credential.GetSBytes());
            var resultBytes = encryptor.ComputeHash(data);
            return outputType == SignOutputType.Base64 ? BytesToBase64String(resultBytes) : BytesToHexString(resultBytes);
        }

        /// <summary>
        /// SHA256 sign the data
        /// </summary>
        protected string SignRSASHA256(RSACredential credential, byte[] data, SignOutputType? outputType = null)
        {
            var rsa = credential.GetSigner();
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(data);
            var resultBytes = rsa.SignHash(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            return outputType == SignOutputType.Base64 ? BytesToBase64String(resultBytes) : BytesToHexString(resultBytes);
        }

        /// <summary>
        /// SHA384 sign the data
        /// </summary>
        protected string SignRSASHA384(RSACredential credential, byte[] data, SignOutputType? outputType = null)
        {
            var rsa = credential.GetSigner();
            using var sha384 = SHA384.Create();
            var hash = sha384.ComputeHash(data);
            var resultBytes = rsa.SignHash(hash, HashAlgorithmName.SHA384, RSASignaturePadding.Pkcs1);
            return outputType == SignOutputType.Base64 ? BytesToBase64String(resultBytes) : BytesToHexString(resultBytes);
        }

        /// <summary>
        /// SHA512 sign the data
        /// </summary>
        protected string SignRSASHA512(RSACredential credential, byte[] data, SignOutputType? outputType = null)
        {
            var rsa = credential.GetSigner();
            using var sha512 = SHA512.Create();
            var hash = sha512.ComputeHash(data);
            var resultBytes = rsa.SignHash(hash, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
            return outputType == SignOutputType.Base64 ? BytesToBase64String(resultBytes) : BytesToHexString(resultBytes);
        }

#if NET8_0_OR_GREATER
        /// <summary>
        /// Ed25519 sign the data 
        /// </summary>
        public string SignEd25519(Ed25519Credential credential, string data, SignOutputType? outputType = null)
            => SignEd25519(credential, Encoding.ASCII.GetBytes(data), outputType);

        /// <summary>
        /// Ed25519 sign the data 
        /// </summary>
        public string SignEd25519(Ed25519Credential credential, byte[] data, SignOutputType? outputType = null)
        {
            var signKey = credential.GetSigningKey();
            var resultBytes = SignatureAlgorithm.Ed25519.Sign(signKey, data);
            return outputType == SignOutputType.Base64 ? BytesToBase64String(resultBytes) : BytesToHexString(resultBytes);
        }
#endif

        /// <summary>
        /// Convert byte array to hex string
        /// </summary>
        /// <param name="buff"></param>
        /// <returns></returns>
        protected static string BytesToHexString(byte[] buff)
            => BytesToHexString(new ArraySegment<byte>(buff));

        /// <summary>
        /// Convert byte array to hex string
        /// </summary>
        /// <param name="buff"></param>
        /// <returns></returns>
        protected static string BytesToHexString(ArraySegment<byte> buff)
        {
#if NET9_0_OR_GREATER
            return Convert.ToHexString(buff);
#else
            var result = string.Empty;
            foreach (var t in buff)
                result += t.ToString("X2");
            return result;
#endif
        }

        /// <summary>
        /// Convert a hex encoded string to byte array
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        protected static byte[] HexToBytesString(string hexString)
        {
            if (hexString.StartsWith("0x"))
                hexString = hexString.Substring(2);

            byte[] bytes = new byte[hexString.Length / 2];
            for (int i = 0; i < hexString.Length; i += 2)
            {
                string hexSubstring = hexString.Substring(i, 2);
                bytes[i / 2] = Convert.ToByte(hexSubstring, 16);
            }

            return bytes;
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
        protected DateTime GetTimestamp(RestApiClient apiClient, bool includeOneSecondOffset = true)
        {
            var result =  TimeProvider.GetTime().Add(TimeOffsetManager.GetRestOffset(apiClient.ClientName) ?? TimeSpan.Zero)!;
            if (includeOneSecondOffset)
                result = result.AddSeconds(-1);

            return result;
        }

        /// <summary>
        /// Get current timestamp including the time sync offset from the api client
        /// </summary>
        protected DateTime GetTimestamp(SocketApiClient apiClient, bool includeOneSecondOffset = true)
        {
            var timestamp = TimeProvider.GetTime();
            if(apiClient.ApiOptions.AutoTimestamp ?? apiClient.ClientOptions.AutoTimestamp)
                timestamp = timestamp.Add(-TimeOffsetManager.GetSocketOffset(apiClient.ClientName) ?? TimeSpan.Zero)!;

            if (includeOneSecondOffset)
                timestamp = timestamp.AddSeconds(-1);

            return timestamp;
        }

        /// <summary>
        /// Get millisecond timestamp as a string including the time sync offset from the api client
        /// </summary>
        protected string GetMillisecondTimestamp(RestApiClient apiClient, bool includeOneSecondOffset = true)
            => DateTimeConverter.ConvertToMilliseconds(GetTimestamp(apiClient, includeOneSecondOffset)).Value.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// Get millisecond timestamp as a string including the time sync offset from the api client
        /// </summary>
        protected string GetMillisecondTimestamp(SocketApiClient apiClient, bool includeOneSecondOffset = true)
            => DateTimeConverter.ConvertToMilliseconds(GetTimestamp(apiClient, includeOneSecondOffset)).Value.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// Get millisecond timestamp as a long including the time sync offset from the api client
        /// </summary>
        protected long GetMillisecondTimestampLong(RestApiClient apiClient, bool includeOneSecondOffset = true)
            => DateTimeConverter.ConvertToMilliseconds(GetTimestamp(apiClient, includeOneSecondOffset)).Value;

        /// <summary>
        /// Get millisecond timestamp as a long including the time sync offset from the api client
        /// </summary>
        protected long GetMillisecondTimestampLong(SocketApiClient apiClient, bool includeOneSecondOffset = true)
            => DateTimeConverter.ConvertToMilliseconds(GetTimestamp(apiClient, includeOneSecondOffset)).Value;

        /// <summary>
        /// Return the serialized request body
        /// </summary>
        /// <param name="serializer"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected static string GetSerializedBody(IMessageSerializer serializer, IDictionary<string, object> parameters)
        {
            if (serializer is not IStringMessageSerializer stringSerializer)
                throw new InvalidOperationException("Non-string message serializer can't get serialized request body");

            if (parameters?.Count == 1 && parameters.TryGetValue(Constants.BodyPlaceHolderKey, out object? value))
                return stringSerializer.Serialize(value);
            else
                return stringSerializer.Serialize(parameters);
        }
    }

    /// <inheritdoc />
    public abstract class AuthenticationProvider<TApiCredentials> : AuthenticationProvider
        where TApiCredentials: ApiCredentials
    {
        /// <summary>
        /// API credentials used for signing requests
        /// </summary>
        public TApiCredentials ApiCredentials { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        protected AuthenticationProvider(TApiCredentials credentials)
        {
            credentials.Validate();

            ApiCredentials = credentials;
        }
    }

    /// <inheritdoc />
    public abstract class AuthenticationProvider<TApiCredentials, TCredentialType> : AuthenticationProvider<TApiCredentials>
        where TApiCredentials : ApiCredentials
        where TCredentialType : CredentialSet
    {
        /// <summary>
        /// The specific credential type used for signing requests.
        /// </summary>
        public TCredentialType Credential { get; }

        /// <inheritdoc />
        public override string Key => Credential.Key;

        /// <summary>
        /// ctor
        /// </summary>
        protected AuthenticationProvider(
            TApiCredentials credentials,
            TCredentialType? credential) : base(credentials)
        {
            if (credential == null)
                throw new ArgumentException($"Missing \"{typeof(TCredentialType).Name}\" credentials on \"{credentials.GetType().Name}\"");

            Credential = credential;
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
            if (Credential is not HMACCredential hmacCredential)
                throw new InvalidOperationException($"Invalid HMAC signing without HMAC credentials provided");

            return SignHMACSHA256(hmacCredential, data, outputType);
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
            if (Credential is not HMACCredential hmacCredential)
                throw new InvalidOperationException($"Invalid HMAC signing without HMAC credentials provided");

            return SignHMACSHA384(hmacCredential, data, outputType);
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
            if (Credential is not HMACCredential hmacCredential)
                throw new InvalidOperationException($"Invalid HMAC signing without HMAC credentials provided");

            return SignHMACSHA512(hmacCredential, data, outputType);
        }

        /// <summary>
        /// SHA256 sign the data
        /// </summary>
        /// <param name="data"></param>
        /// <param name="outputType"></param>
        /// <returns></returns>
        protected string SignRSASHA256(byte[] data, SignOutputType? outputType = null)
        {
            if (Credential is not RSACredential rsaCredential)
                throw new InvalidOperationException($"Invalid RSA signing without RSA credentials provided");

            return SignRSASHA256(rsaCredential, data, outputType);
        }

        /// <summary>
        /// SHA384 sign the data
        /// </summary>
        /// <param name="data"></param>
        /// <param name="outputType"></param>
        /// <returns></returns>
        protected string SignRSASHA384(byte[] data, SignOutputType? outputType = null)
        {
            if (Credential is not RSACredential rsaCredential)
                throw new InvalidOperationException($"Invalid RSA signing without RSA credentials provided");

            return SignRSASHA384(rsaCredential, data, outputType);
        }

        /// <summary>
        /// SHA512 sign the data
        /// </summary>
        /// <param name="data"></param>
        /// <param name="outputType"></param>
        /// <returns></returns>
        protected string SignRSASHA512(byte[] data, SignOutputType? outputType = null)
        {
            if (Credential is not RSACredential rsaCredential)
                throw new InvalidOperationException($"Invalid RSA signing without RSA credentials provided");

            return SignRSASHA512(rsaCredential, data, outputType);
        }

#if NET8_0_OR_GREATER
        /// <summary>
        /// Ed25519 sign the data 
        /// </summary>
        public string SignEd25519(string data, SignOutputType? outputType = null)
            => SignEd25519(Encoding.ASCII.GetBytes(data), outputType);

        /// <summary>
        /// Ed25519 sign the data 
        /// </summary>
        public string SignEd25519(byte[] data, SignOutputType? outputType = null)
        {
            if (Credential is not Ed25519Credential ed25519Credential)
                throw new InvalidOperationException($"Invalid Ed25519 signing without Ed25519 credentials provided");

            return SignEd25519(ed25519Credential, data, outputType);
        }
#endif
    }
}
