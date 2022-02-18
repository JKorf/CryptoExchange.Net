using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Web;
using CryptoExchange.Net.Logging;
using CryptoExchange.Net.Objects;
using Microsoft.Extensions.Logging;
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
        /// <param name="converter"></param>
        public static void AddParameter(this Dictionary<string, object> parameters, string key, string value, JsonConverter converter)
        {
            parameters.Add(key, JsonConvert.SerializeObject(value, converter));
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
        /// Add a parameter
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="converter"></param>
        public static void AddParameter(this Dictionary<string, object> parameters, string key, object value, JsonConverter converter)
        {
            parameters.Add(key, JsonConvert.SerializeObject(value, converter));
        }

        /// <summary>
        /// Add an optional parameter. Not added if value is null
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void AddOptionalParameter(this Dictionary<string, object> parameters, string key, object? value)
        {
            if (value != null)
                parameters.Add(key, value);
        }

        /// <summary>
        /// Add an optional parameter. Not added if value is null
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="converter"></param>
        public static void AddOptionalParameter(this Dictionary<string, object> parameters, string key, object? value, JsonConverter converter)
        {
            if (value != null)
                parameters.Add(key, JsonConvert.SerializeObject(value, converter));
        }

        /// <summary>
        /// Add an optional parameter. Not added if value is null
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void AddOptionalParameter(this Dictionary<string, string> parameters, string key, string? value)
        {
            if (value != null)
                parameters.Add(key, value);
        }

        /// <summary>
        /// Add an optional parameter. Not added if value is null
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="converter"></param>
        public static void AddOptionalParameter(this Dictionary<string, string> parameters, string key, string? value, JsonConverter converter)
        {
            if (value != null)
                parameters.Add(key, JsonConvert.SerializeObject(value, converter));
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
            var uriString = string.Empty;
            var arraysParameters = parameters.Where(p => p.Value.GetType().IsArray).ToList();
            foreach (var arrayEntry in arraysParameters)
            {
                if (serializationType == ArrayParametersSerialization.Array)
                    uriString += $"{string.Join("&", ((object[])(urlEncodeValues ? Uri.EscapeDataString(arrayEntry.Value.ToString()) : arrayEntry.Value)).Select(v => $"{arrayEntry.Key}[]={v}"))}&";
                else
                {
                    var array = (Array)arrayEntry.Value;
                    uriString += string.Join("&", array.OfType<object>().Select(a => $"{arrayEntry.Key}={Uri.EscapeDataString(a.ToString())}"));
                    uriString += "&";
                }
            }

            uriString += $"{string.Join("&", parameters.Where(p => !p.Value.GetType().IsArray).Select(s => $"{s.Key}={(urlEncodeValues ? Uri.EscapeDataString(s.Value.ToString()) : s.Value)}"))}";
            uriString = uriString.TrimEnd('&');
            return uriString;
        }

        /// <summary>
        /// Convert a dictionary to formdata string
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static string ToFormData(this SortedDictionary<string, object> parameters)
        {
            var formData = HttpUtility.ParseQueryString(string.Empty);
            foreach (var kvp in parameters)
            {
                if (kvp.Value.GetType().IsArray)
                {
                    var array = (Array)kvp.Value;
                    foreach (var value in array)
                        formData.Add(kvp.Key, value.ToString());
                }
                else
                    formData.Add(kvp.Key, kvp.Value.ToString());
            }
            return formData.ToString();
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
        /// Are 2 secure strings equal
        /// </summary>
        /// <param name="ss1">Source secure string</param>
        /// <param name="ss2">Compare secure string</param>
        /// <returns>True if equal by value</returns>
        public static bool IsEqualTo(this SecureString ss1, SecureString ss2)
        {
            IntPtr bstr1 = IntPtr.Zero;
            IntPtr bstr2 = IntPtr.Zero;
            try
            {
                bstr1 = Marshal.SecureStringToBSTR(ss1);
                bstr2 = Marshal.SecureStringToBSTR(ss2);
                int length1 = Marshal.ReadInt32(bstr1, -4);
                int length2 = Marshal.ReadInt32(bstr2, -4);
                if (length1 == length2)
                {
                    for (int x = 0; x < length1; ++x)
                    {
                        byte b1 = Marshal.ReadByte(bstr1, x);
                        byte b2 = Marshal.ReadByte(bstr2, x);
                        if (b1 != b2) return false;
                    }
                }
                else return false;
                return true;
            }
            finally
            {
                if (bstr2 != IntPtr.Zero) Marshal.ZeroFreeBSTR(bstr2);
                if (bstr1 != IntPtr.Zero) Marshal.ZeroFreeBSTR(bstr1);
            }
        }

        /// <summary>
        /// Create a secure string from a string
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static SecureString ToSecureString(this string source)
        {
            var secureString = new SecureString();
            foreach (var c in source)
                secureString.AppendChar(c);
            secureString.MakeReadOnly();
            return secureString;
        }

        /// <summary>
        /// String to JToken
        /// </summary>
        /// <param name="stringData"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        public static JToken? ToJToken(this string stringData, Log? log = null)
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
                log?.Write(LogLevel.Error, info);
                if (log == null) Trace.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss:fff} | Warning | {info}");
                return null;
            }
            catch (JsonSerializationException jse)
            {
                var info = $"Deserialize JsonSerializationException: {jse.Message}. Data: {stringData}";
                log?.Write(LogLevel.Error, info);
                if (log == null) Trace.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss:fff} | Warning | {info}");
                return null;
            }
        }

        /// <summary>
        /// Validates an int is one of the allowed values
        /// </summary>
        /// <param name="value">Value of the int</param>
        /// <param name="argumentName">Name of the parameter</param>
        /// <param name="allowedValues">Allowed values</param>
        public static void ValidateIntValues(this int value, string argumentName, params int[] allowedValues)
        {
            if (!allowedValues.Contains(value))
                throw new ArgumentException(
                    $"{value} not allowed for parameter {argumentName}, allowed values: {string.Join(", ", allowedValues)}", argumentName);
        }

        /// <summary>
        /// Validates an int is between two values
        /// </summary>
        /// <param name="value">The value of the int</param>
        /// <param name="argumentName">Name of the parameter</param>
        /// <param name="minValue">Min value</param>
        /// <param name="maxValue">Max value</param>
        public static void ValidateIntBetween(this int value, string argumentName, int minValue, int maxValue)
        {
            if (value < minValue || value > maxValue)
                throw new ArgumentException(
                    $"{value} not allowed for parameter {argumentName}, min: {minValue}, max: {maxValue}", argumentName);
        }

        /// <summary>
        /// Validates a string is not null or empty
        /// </summary>
        /// <param name="value">The value of the string</param>
        /// <param name="argumentName">Name of the parameter</param>
        public static void ValidateNotNull(this string value, string argumentName)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException($"No value provided for parameter {argumentName}", argumentName);
        }

        /// <summary>
        /// Validates a string is null or not empty
        /// </summary>
        /// <param name="value"></param>
        /// <param name="argumentName"></param>
        public static void ValidateNullOrNotEmpty(this string value, string argumentName)
        {
            if (value != null && string.IsNullOrEmpty(value))
                throw new ArgumentException($"No value provided for parameter {argumentName}", argumentName);
        }

        /// <summary>
        /// Validates an object is not null
        /// </summary>
        /// <param name="value">The value of the object</param>
        /// <param name="argumentName">Name of the parameter</param>
        public static void ValidateNotNull(this object value, string argumentName)
        {
            if (value == null)
                throw new ArgumentException($"No value provided for parameter {argumentName}", argumentName);
        }

        /// <summary>
        /// Validates a list is not null or empty
        /// </summary>
        /// <param name="value">The value of the object</param>
        /// <param name="argumentName">Name of the parameter</param>
        public static void ValidateNotNull<T>(this IEnumerable<T> value, string argumentName)
        {
            if (value == null || !value.Any())
                throw new ArgumentException($"No values provided for parameter {argumentName}", argumentName);
        }

        /// <summary>
        /// Format an exception and inner exception to a readable string
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static string ToLogString(this Exception? exception)
        {
            var message = new StringBuilder();
            var indent = 0;
            while (exception != null)
            {
                for (var i = 0; i < indent; i++)
                    message.Append(' ');
                message.Append(exception.GetType().Name);
                message.Append(" - ");
                message.AppendLine(exception.Message);
                for (var i = 0; i < indent; i++)
                    message.Append(' ');
                message.AppendLine(exception.StackTrace);

                indent += 2;
                exception = exception.InnerException;
            }

            return message.ToString();
        }

        /// <summary>
        /// Append a base url with provided path
        /// </summary>
        /// <param name="url"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string AppendPath(this string url, params string[] path)
        {
            if (!url.EndsWith("/"))
                url += "/";

            foreach (var item in path)
                url += item.Trim('/') + "/";

            return url.TrimEnd('/');
        }

        /// <summary>
        /// Fill parameters in a path. Parameters are specified by '{}' and should be specified in occuring sequence
        /// </summary>
        /// <param name="path">The total path string</param>
        /// <param name="values">The values to fill</param>
        /// <returns></returns>
        public static string FillPathParameters(this string path, params string[] values)
        {
            foreach (var value in values)
            {
                var index = path.IndexOf("{}", StringComparison.Ordinal);
                if (index >= 0)
                {
                    path = path.Remove(index, 2);
                    path = path.Insert(index, value);
                }
            }
            return path;
        }

        /// <summary>
        /// Create a new uri with the provided parameters as query
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="baseUri"></param>
        /// <returns></returns>
        public static Uri SetParameters(this Uri baseUri, SortedDictionary<string, object> parameters)
        {
            var uriBuilder = new UriBuilder();
            uriBuilder.Scheme = baseUri.Scheme;
            uriBuilder.Host = baseUri.Host;
            uriBuilder.Path = baseUri.AbsolutePath;
            var httpValueCollection = HttpUtility.ParseQueryString(string.Empty);
            foreach (var parameter in parameters)
                httpValueCollection.Add(parameter.Key, parameter.Value.ToString());
            uriBuilder.Query = httpValueCollection.ToString();
            return uriBuilder.Uri;
        }

        /// <summary>
        /// Create a new uri with the provided parameters as query
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="baseUri"></param>
        /// <returns></returns>
        public static Uri SetParameters(this Uri baseUri, IOrderedEnumerable<KeyValuePair<string, object>> parameters)
        {
            var uriBuilder = new UriBuilder();
            uriBuilder.Scheme = baseUri.Scheme;
            uriBuilder.Host = baseUri.Host;
            uriBuilder.Path = baseUri.AbsolutePath;
            var httpValueCollection = HttpUtility.ParseQueryString(string.Empty);
            foreach (var parameter in parameters)
                httpValueCollection.Add(parameter.Key, parameter.Value.ToString());
            uriBuilder.Query = httpValueCollection.ToString();
            return uriBuilder.Uri;
        }


        /// <summary>
        /// Add parameter to URI
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Uri AddQueryParmeter(this Uri uri, string name, string value)
        {
            var httpValueCollection = HttpUtility.ParseQueryString(uri.Query);

            httpValueCollection.Remove(name);
            httpValueCollection.Add(name, value);

            var ub = new UriBuilder(uri);
            ub.Query = httpValueCollection.ToString();

            return ub.Uri;
        }

    }
}

