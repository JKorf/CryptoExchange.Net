using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;
using CryptoExchange.Net.Objects;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using CryptoExchange.Net.SharedApis;

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
        public static void AddOptionalParameter(this Dictionary<string, object> parameters, string key, object? value)
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
        public static string CreateParamString(this IDictionary<string, object> parameters, bool urlEncodeValues, ArrayParametersSerialization serializationType)
        {
            var uriString = string.Empty;
            var arraysParameters = parameters.Where(p => p.Value.GetType().IsArray).ToList();
            foreach (var arrayEntry in arraysParameters)
            {
                if (serializationType == ArrayParametersSerialization.Array)
                {
                    uriString += $"{string.Join("&", ((object[])(urlEncodeValues ? Uri.EscapeDataString(arrayEntry.Value.ToString()) : arrayEntry.Value)).Select(v => $"{arrayEntry.Key}[]={string.Format(CultureInfo.InvariantCulture, "{0}", v)}"))}&";
                }
                else if (serializationType == ArrayParametersSerialization.MultipleValues)
                {
                    var array = (Array)arrayEntry.Value;
                    uriString += string.Join("&", array.OfType<object>().Select(a => $"{arrayEntry.Key}={Uri.EscapeDataString(string.Format(CultureInfo.InvariantCulture, "{0}", a))}"));
                    uriString += "&";
                }
                else
                {
                    var array = (Array)arrayEntry.Value;
                    uriString += $"{arrayEntry.Key}=[{string.Join(",", array.OfType<object>().Select(a => string.Format(CultureInfo.InvariantCulture, "{0}", a)))}]&";
                }
            }

            uriString += $"{string.Join("&", parameters.Where(p => !p.Value.GetType().IsArray).Select(s => $"{s.Key}={(urlEncodeValues ? Uri.EscapeDataString(string.Format(CultureInfo.InvariantCulture, "{0}", s.Value)) : string.Format(CultureInfo.InvariantCulture, "{0}", s.Value))}"))}";
            uriString = uriString.TrimEnd('&');
            return uriString;
        }

        /// <summary>
        /// Convert a dictionary to formdata string
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static string ToFormData(this IDictionary<string, object> parameters)
        {
            var formData = HttpUtility.ParseQueryString(string.Empty);
            foreach (var kvp in parameters)
            {
                if (kvp.Value is null)
                    continue;

                if (kvp.Value.GetType().IsArray)
                {
                    var array = (Array)kvp.Value;
                    foreach (var value in array)
                        formData.Add(kvp.Key, string.Format(CultureInfo.InvariantCulture, "{0}", value));
                }
                else
                {
                    formData.Add(kvp.Key, string.Format(CultureInfo.InvariantCulture, "{0}", kvp.Value));
                }
            }
            return formData.ToString();
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
            {
                throw new ArgumentException(
                    $"{value} not allowed for parameter {argumentName}, allowed values: {string.Join(", ", allowedValues)}", argumentName);
            }
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
            {
                throw new ArgumentException(
                    $"{value} not allowed for parameter {argumentName}, min: {minValue}, max: {maxValue}", argumentName);
            }
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
        /// Create a new uri with the provided parameters as query
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="baseUri"></param>
        /// <param name="arraySerialization"></param>
        /// <returns></returns>
        public static Uri SetParameters(this Uri baseUri, IDictionary<string, object> parameters, ArrayParametersSerialization arraySerialization)
        {
            var uriBuilder = new UriBuilder();
            uriBuilder.Scheme = baseUri.Scheme;
            uriBuilder.Host = baseUri.Host;
            uriBuilder.Port = baseUri.Port;
            uriBuilder.Path = baseUri.AbsolutePath;
            var httpValueCollection = HttpUtility.ParseQueryString(string.Empty);
            foreach (var parameter in parameters)
            {
                if (parameter.Value.GetType().IsArray)
                {
                    if (arraySerialization == ArrayParametersSerialization.JsonArray)
                    {
                        httpValueCollection.Add(parameter.Key, $"[{string.Join(",", (object[])parameter.Value)}]");
                    }
                    else
                    {
                        foreach (var item in (object[])parameter.Value)
                        {
                            if (arraySerialization == ArrayParametersSerialization.Array)
                            {
                                httpValueCollection.Add(parameter.Key + "[]", item.ToString());
                            }
                            else
                            {
                                httpValueCollection.Add(parameter.Key, item.ToString());
                            }
                        }
                    }
                }
                else
                {
                    httpValueCollection.Add(parameter.Key, parameter.Value.ToString());
                }
            }
            uriBuilder.Query = httpValueCollection.ToString();
            return uriBuilder.Uri;
        }

        /// <summary>
        /// Create a new uri with the provided parameters as query
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="baseUri"></param>
        /// <param name="arraySerialization"></param>
        /// <returns></returns>
        public static Uri SetParameters(this Uri baseUri, IOrderedEnumerable<KeyValuePair<string, object>> parameters, ArrayParametersSerialization arraySerialization)
        {
            var uriBuilder = new UriBuilder();
            uriBuilder.Scheme = baseUri.Scheme;
            uriBuilder.Host = baseUri.Host;
            uriBuilder.Port = baseUri.Port;
            uriBuilder.Path = baseUri.AbsolutePath;
            var httpValueCollection = HttpUtility.ParseQueryString(string.Empty);
            foreach (var parameter in parameters)
            {
                if (parameter.Value.GetType().IsArray)
                {
                    if (arraySerialization == ArrayParametersSerialization.JsonArray)
                    {
                        httpValueCollection.Add(parameter.Key, $"[{string.Join(",", (object[])parameter.Value)}]");
                    }
                    else
                    {
                        foreach (var item in (object[])parameter.Value)
                        {
                            if (arraySerialization == ArrayParametersSerialization.Array)
                            {
                                httpValueCollection.Add(parameter.Key + "[]", item.ToString());
                            }
                            else
                            {
                                httpValueCollection.Add(parameter.Key, item.ToString());
                            }
                        }
                    }
                }
                else
                {
                    httpValueCollection.Add(parameter.Key, parameter.Value.ToString());
                }
            }
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

        /// <summary>
        /// Decompress using GzipStream
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ReadOnlyMemory<byte> DecompressGzip(this ReadOnlyMemory<byte> data)
        {
            using var decompressedStream = new MemoryStream();
            using var dataStream = MemoryMarshal.TryGetArray(data, out var arraySegment)
                ? new MemoryStream(arraySegment.Array, arraySegment.Offset, arraySegment.Count)
                : new MemoryStream(data.ToArray());
            using var deflateStream = new GZipStream(new MemoryStream(data.ToArray()), CompressionMode.Decompress);
            deflateStream.CopyTo(decompressedStream);
            return new ReadOnlyMemory<byte>(decompressedStream.GetBuffer(), 0, (int)decompressedStream.Length);
        }

        /// <summary>
        /// Decompress using DeflateStream
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static ReadOnlyMemory<byte> Decompress(this ReadOnlyMemory<byte> input)
        {
            var output = new MemoryStream();

            using (var compressStream = new MemoryStream(input.ToArray()))
            using (var decompressor = new DeflateStream(compressStream, CompressionMode.Decompress))
                decompressor.CopyTo(output);

            output.Position = 0;
            return new ReadOnlyMemory<byte>(output.GetBuffer(), 0, (int)output.Length);
        }

        /// <summary>
        /// Whether the trading mode is linear
        /// </summary>
        public static bool IsLinear(this TradingMode type) => type == TradingMode.PerpetualLinear || type == TradingMode.DeliveryLinear;

        /// <summary>
        /// Whether the trading mode is inverse
        /// </summary>
        public static bool IsInverse(this TradingMode type) => type == TradingMode.PerpetualInverse || type == TradingMode.DeliveryInverse;
        
        /// <summary>
        /// Whether the trading mode is perpetual
        /// </summary>
        public static bool IsPerpetual(this TradingMode type) => type == TradingMode.PerpetualInverse || type == TradingMode.PerpetualLinear;

        /// <summary>
        /// Whether the trading mode is delivery
        /// </summary>
        public static bool IsDelivery(this TradingMode type) => type == TradingMode.DeliveryInverse || type == TradingMode.DeliveryLinear;

        /// <summary>
        /// Register rest client interfaces
        /// </summary>
        public static IServiceCollection RegisterSharedRestInterfaces<T>(this IServiceCollection services, Func<IServiceProvider, T> client)
        {
            if (typeof(IAssetsRestClient).IsAssignableFrom(typeof(T)))
                services.AddTransient(x => (IAssetsRestClient)client(x)!);
            if (typeof(IBalanceRestClient).IsAssignableFrom(typeof(T)))
                services.AddTransient(x => (IBalanceRestClient)client(x)!);
            if (typeof(IDepositRestClient).IsAssignableFrom(typeof(T)))
                services.AddTransient(x => (IDepositRestClient)client(x)!);
            if (typeof(IKlineRestClient).IsAssignableFrom(typeof(T)))
                services.AddTransient(x => (IKlineRestClient)client(x)!);
            if (typeof(IListenKeyRestClient).IsAssignableFrom(typeof(T)))
                services.AddTransient(x => (IListenKeyRestClient)client(x)!);
            if (typeof(IOrderBookRestClient).IsAssignableFrom(typeof(T)))
                services.AddTransient(x => (IOrderBookRestClient)client(x)!);
            if (typeof(IRecentTradeRestClient).IsAssignableFrom(typeof(T)))
                services.AddTransient(x => (IRecentTradeRestClient)client(x)!);
            if (typeof(ITradeHistoryRestClient).IsAssignableFrom(typeof(T)))
                services.AddTransient(x => (ITradeHistoryRestClient)client(x)!);
            if (typeof(IWithdrawalRestClient).IsAssignableFrom(typeof(T)))
                services.AddTransient(x => (IWithdrawalRestClient)client(x)!);
            if (typeof(IWithdrawRestClient).IsAssignableFrom(typeof(T)))
                services.AddTransient(x => (IWithdrawRestClient)client(x)!);

            if (typeof(ISpotOrderRestClient).IsAssignableFrom(typeof(T)))
                services.AddTransient(x => (ISpotOrderRestClient)client(x)!);
            if (typeof(ISpotSymbolRestClient).IsAssignableFrom(typeof(T)))
                services.AddTransient(x => (ISpotSymbolRestClient)client(x)!);
            if (typeof(ISpotTickerRestClient).IsAssignableFrom(typeof(T)))
                services.AddTransient(x => (ISpotTickerRestClient)client(x)!);

            if (typeof(IFundingRateRestClient).IsAssignableFrom(typeof(T)))
                services.AddTransient(x => (IFundingRateRestClient)client(x)!);
            if (typeof(IFuturesOrderRestClient).IsAssignableFrom(typeof(T)))
                services.AddTransient(x => (IFuturesOrderRestClient)client(x)!);
            if (typeof(IFuturesSymbolRestClient).IsAssignableFrom(typeof(T)))
                services.AddTransient(x => (IFuturesSymbolRestClient)client(x)!);
            if (typeof(IFuturesTickerRestClient).IsAssignableFrom(typeof(T)))
                services.AddTransient(x => (IFuturesTickerRestClient)client(x)!);
            if (typeof(IIndexPriceKlineRestClient).IsAssignableFrom(typeof(T)))
                services.AddTransient(x => (IIndexPriceKlineRestClient)client(x)!);
            if (typeof(ILeverageRestClient).IsAssignableFrom(typeof(T)))
                services.AddTransient(x => (ILeverageRestClient)client(x)!);
            if (typeof(IMarkPriceKlineRestClient).IsAssignableFrom(typeof(T)))
                services.AddTransient(x => (IMarkPriceKlineRestClient)client(x)!);
            if (typeof(IOpenInterestRestClient).IsAssignableFrom(typeof(T)))
                services.AddTransient(x => (IOpenInterestRestClient)client(x)!);
            if (typeof(IPositionHistoryRestClient).IsAssignableFrom(typeof(T)))
                services.AddTransient(x => (IPositionHistoryRestClient)client(x)!);
            if (typeof(IPositionModeRestClient).IsAssignableFrom(typeof(T)))
                services.AddTransient(x => (IPositionModeRestClient)client(x)!);

            return services;
        }

        /// <summary>
        /// Register socket client interfaces
        /// </summary>
        public static IServiceCollection RegisterSharedSocketInterfaces<T>(this IServiceCollection services, Func<IServiceProvider, T> client)
        {
            if (typeof(IBalanceSocketClient).IsAssignableFrom(typeof(T)))
                services.AddTransient(x => (IBalanceSocketClient)client(x)!);
            if (typeof(IBookTickerSocketClient).IsAssignableFrom(typeof(T)))
                services.AddTransient(x => (IBookTickerSocketClient)client(x)!);
            if (typeof(IKlineSocketClient).IsAssignableFrom(typeof(T)))
                services.AddTransient(x => (IKlineSocketClient)client(x)!);
            if (typeof(IOrderBookRestClient).IsAssignableFrom(typeof(T)))
                services.AddTransient(x => (IOrderBookRestClient)client(x)!);
            if (typeof(ITickerSocketClient).IsAssignableFrom(typeof(T)))
                services.AddTransient(x => (ITickerSocketClient)client(x)!);
            if (typeof(ITickersSocketClient).IsAssignableFrom(typeof(T)))
                services.AddTransient(x => (ITickersSocketClient)client(x)!);
            if (typeof(ITradeSocketClient).IsAssignableFrom(typeof(T)))
                services.AddTransient(x => (ITradeSocketClient)client(x)!);
            if (typeof(IUserTradeSocketClient).IsAssignableFrom(typeof(T)))
                services.AddTransient(x => (IUserTradeSocketClient)client(x)!);

            if (typeof(ISpotOrderSocketClient).IsAssignableFrom(typeof(T)))
                services.AddTransient(x => (ISpotOrderSocketClient)client(x)!);

            if (typeof(IFuturesOrderSocketClient).IsAssignableFrom(typeof(T)))
                services.AddTransient(x => (IFuturesOrderSocketClient)client(x)!);
            if (typeof(IPositionSocketClient).IsAssignableFrom(typeof(T)))
                services.AddTransient(x => (IPositionSocketClient)client(x)!);

            return services;
        }
    }
}

