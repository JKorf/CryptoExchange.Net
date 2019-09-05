using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Logging;
using CryptoExchange.Net.Objects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CryptoExchange.Net
{
    /// <summary>
    /// Helper methods
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Add a parameter
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void AddParameter(this Dictionary<string, object> parameters, string key, string value)
        {
            parameters.Add(key, value);
        }

        /// <summary>
        /// Add a parameter
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void AddParameter(this Dictionary<string, object> parameters, string key, object value)
        {
            parameters.Add(key, value);
        }

        /// <summary>
        /// Add an optional parameter. Not added if value is null
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void AddOptionalParameter(this Dictionary<string, object> parameters, string key, object value)
        {
            if(value != null)
                parameters.Add(key, value);
        }

        /// <summary>
        /// Add an optional parameter. Not added if value is null
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void AddOptionalParameter(this Dictionary<string, string> parameters, string key, string value)
        {
            if (value != null)
                parameters.Add(key, value);
        }

        /// <summary>
        /// Create a query string of the specified parameters
        /// </summary>
        /// <param name="parameters">The parameters to use</param>
        /// <param name="urlEncodeValues">Whether or not the values should be url encoded</param>
        /// <param name="serializationType">How to serialize array parameters</param>
        /// <returns></returns>
        public static string CreateParamString(this Dictionary<string, object> parameters, bool urlEncodeValues, ArrayParametersSerialization serializationType)
        {
            var uriString = "";
            var arraysParameters = parameters.Where(p => p.Value.GetType().IsArray).ToList();
            foreach (var arrayEntry in arraysParameters)
            {
                if(serializationType == ArrayParametersSerialization.Array)
                    uriString += $"{string.Join("&", ((object[])(urlEncodeValues ? WebUtility.UrlEncode(arrayEntry.Value.ToString()) : arrayEntry.Value)).Select(v => $"{arrayEntry.Key}[]={v}"))}&";
                else
                {
                    var array = (Array)arrayEntry.Value;
                    uriString += string.Join("&", array.OfType<object>().Select(a => $"{arrayEntry.Key}={WebUtility.UrlEncode(a.ToString())}"));
                    uriString += "&";
                }
            }

            uriString += $"{string.Join("&", parameters.Where(p => !p.Value.GetType().IsArray).Select(s => $"{s.Key}={(urlEncodeValues ? WebUtility.UrlEncode(s.Value.ToString()) : s.Value)}"))}";
            uriString = uriString.TrimEnd('&');
            return uriString;
        }

        /// <summary>
        /// Get the string the secure string is representing
        /// </summary>
        /// <param name="source">The source secure string</param>
        /// <returns></returns>
        public static string GetString(this SecureString source)
        {
            lock (source)
            {
                string result;
                var length = source.Length;
                var pointer = IntPtr.Zero;
                var chars = new char[length];

                try
                {
                    pointer = Marshal.SecureStringToBSTR(source);
                    Marshal.Copy(pointer, chars, 0, length);

                    result = string.Join("", chars);
                }
                finally
                {
                    if (pointer != IntPtr.Zero)
                    {
                        Marshal.ZeroFreeBSTR(pointer);
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Header collection to IEnumerable
        /// </summary>
        /// <param name="headers"></param>
        /// <returns></returns>
        public static IEnumerable<Tuple<string, string>> ToIEnumerable(this WebHeaderCollection headers)
        {
            if (headers == null)
                return null;

            return Enumerable
                .Range(0, headers.Count)
                .SelectMany(i => headers.GetValues(i)
                    .Select(v => Tuple.Create(headers.GetKey(i), v))
                );
        }

        /// <summary>
        /// Wait one async
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="millisecondsTimeout"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<bool> WaitOneAsync(this WaitHandle handle, int millisecondsTimeout, CancellationToken cancellationToken)
        {
            RegisteredWaitHandle registeredHandle = null;
            CancellationTokenRegistration tokenRegistration = default;
            try
            {
                var tcs = new TaskCompletionSource<bool>();
                registeredHandle = ThreadPool.RegisterWaitForSingleObject(
                    handle,
                    (state, timedOut) => ((TaskCompletionSource<bool>)state).TrySetResult(!timedOut),
                    tcs,
                    millisecondsTimeout,
                    true);
                tokenRegistration = cancellationToken.Register(
                    state => ((TaskCompletionSource<bool>)state).TrySetCanceled(),
                    tcs);
                return await tcs.Task.ConfigureAwait(false);
            }
            finally
            {
                registeredHandle?.Unregister(null);
                tokenRegistration.Dispose();
            }
        }
        
        /// <summary>
        /// Wait one async
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static Task<bool> WaitOneAsync(this WaitHandle handle, TimeSpan timeout)
        {
            return handle.WaitOneAsync((int)timeout.TotalMilliseconds, CancellationToken.None);
        }

        /// <summary>
        /// String to JToken
        /// </summary>
        /// <param name="stringData"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        public static JToken ToJToken(this string stringData, Log log = null)
        {
            if (string.IsNullOrEmpty(stringData))
                return null;

            try
            {
                return JToken.Parse(stringData);
            }
            catch (JsonReaderException jre)
            {
                var info = $"Deserialize JsonReaderException: {jre.Message}, Path: {jre.Path}, LineNumber: {jre.LineNumber}, LinePosition: {jre.LinePosition}. Data: {stringData}";
                log?.Write(LogVerbosity.Error, info);
                if (log == null) Debug.WriteLine(info);
                return null;
            }
            catch (JsonSerializationException jse)
            {
                var info = $"Deserialize JsonSerializationException: {jse.Message}. Data: {stringData}";
                log?.Write(LogVerbosity.Error, info);
                if (log == null) Debug.WriteLine(info);
                return null;
            }
        }
    }
}
