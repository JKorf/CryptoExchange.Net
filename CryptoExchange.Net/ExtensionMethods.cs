using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net
{
    public static class ExtensionMethods
    {
        public static void AddParameter(this Dictionary<string, object> parameters, string key, string value)
        {
            parameters.Add(key, value);
        }

        public static void AddParameter(this Dictionary<string, object> parameters, string key, object value)
        {
            parameters.Add(key, value);
        }

        public static void AddOptionalParameter(this Dictionary<string, object> parameters, string key, object value)
        {
            if(value != null)
                parameters.Add(key, value);
        }

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
        /// <returns></returns>
        public static string CreateParamString(this Dictionary<string, object> parameters, bool urlEncodeValues)
        {
            var uriString = "";
            var arraysParameters = parameters.Where(p => p.Value.GetType().IsArray).ToList();
            foreach (var arrayEntry in arraysParameters)
            {
                uriString += $"{string.Join("&", ((object[])(urlEncodeValues ? WebUtility.UrlEncode(arrayEntry.Value.ToString()) : arrayEntry.Value)).Select(v => $"{arrayEntry.Key}[]={v}"))}&";
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

        public static async Task<bool> WaitOneAsync(this WaitHandle handle, int millisecondsTimeout, CancellationToken cancellationToken)
        {
            RegisteredWaitHandle registeredHandle = null;
            CancellationTokenRegistration tokenRegistration = default(CancellationTokenRegistration);
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
                return await tcs.Task;
            }
            finally
            {
                registeredHandle?.Unregister(null);
                tokenRegistration.Dispose();
            }
        }

        public static Task<bool> WaitOneAsync(this WaitHandle handle, TimeSpan timeout)
        {
            return handle.WaitOneAsync((int)timeout.TotalMilliseconds, CancellationToken.None);
        }
    }
}
