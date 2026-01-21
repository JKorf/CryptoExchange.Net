using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;

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
            var uriString = new StringBuilder();
            bool first = true;
            foreach(var parameter in parameters)
            {
                if (!first)
                    uriString.Append("&");

                first = false;

                if (parameter.Value.GetType().IsArray)
                {
                    if (serializationType == ArrayParametersSerialization.Array)
                    {
                        bool firstArrayValue = true;
                        foreach (var entry in (object[])parameter.Value)
                        {
                            if (!firstArrayValue)
                                uriString.Append('&');
                            firstArrayValue = false;

                            uriString.Append(parameter.Key);
                            uriString.Append("[]=");
                            if (urlEncodeValues)
                                uriString.Append(Uri.EscapeDataString(string.Format(CultureInfo.InvariantCulture, "{0}", entry)));
                            else
                                uriString.Append(string.Format(CultureInfo.InvariantCulture, "{0}", entry));
                        }
                    }
                    else if (serializationType == ArrayParametersSerialization.MultipleValues)
                    {
                        bool firstArrayValue = true;
                        foreach (var entry in (object[])parameter.Value)
                        {
                            if (!firstArrayValue)
                                uriString.Append('&');
                            firstArrayValue = false;
                            uriString.Append(parameter.Key);
                            uriString.Append("=");
                            if (urlEncodeValues)
                                uriString.Append(Uri.EscapeDataString(string.Format(CultureInfo.InvariantCulture, "{0}", entry)));
                            else
                                uriString.Append(string.Format(CultureInfo.InvariantCulture, "{0}", entry));
                        }
                    }
                    else
                    {
                        uriString.Append('[');
                        var firstArrayEntry = true;
                        foreach (var entry in (object[])parameter.Value)
                        {
                            if (!firstArrayEntry)
                                uriString.Append(',');                            

                            firstArrayEntry = false;
                            if (urlEncodeValues)
                                uriString.Append(Uri.EscapeDataString(string.Format(CultureInfo.InvariantCulture, "{0}", entry)));
                            else
                                uriString.Append(string.Format(CultureInfo.InvariantCulture, "{0}", entry));
                        }
                        uriString.Append(']');
                    }
                }
                else 
                {
                    uriString.Append(parameter.Key);
                    uriString.Append('=');
                    if (urlEncodeValues)
                        uriString.Append(Uri.EscapeDataString(string.Format(CultureInfo.InvariantCulture, "{0}", parameter.Value)));
                    else
                        uriString.Append(string.Format(CultureInfo.InvariantCulture, "{0}", parameter.Value));
                }
            }

            return uriString.ToString();
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

            return formData.ToString()!;
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
        /// Format a string to RFC3339/ISO8601 string
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ToRfc3339String(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fffzzz", DateTimeFormatInfo.InvariantInfo);
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
        public static string AppendPath(this string url, params string[] path)
        {
            var sb = new StringBuilder(url.TrimEnd('/'));
            foreach (var subPath in path)
            {
                sb.Append('/');
                sb.Append(subPath.Trim('/'));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Decompress using GzipStream
        /// </summary>
        public static ReadOnlySpan<byte> DecompressGzip(this ReadOnlySpan<byte> data)
        {
            using var decompressedStream = new MemoryStream();
            using var deflateStream = new GZipStream(new MemoryStream(data.ToArray()), CompressionMode.Decompress);
            deflateStream.CopyTo(decompressedStream);
            return new ReadOnlySpan<byte>(decompressedStream.GetBuffer(), 0, (int)decompressedStream.Length);
        }

        /// <summary>
        /// Decompress using GzipStream
        /// </summary>
        public static ReadOnlySpan<byte> Decompress(this ReadOnlySpan<byte> input)
        {
            using var output = new MemoryStream();
            using var compressStream = new MemoryStream(input.ToArray());
            using var decompressor = new DeflateStream(compressStream, CompressionMode.Decompress);
            decompressor.CopyTo(output);
            return new ReadOnlySpan<byte>(output.GetBuffer(), 0, (int)output.Length);
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
        /// Whether the account type is a futures account
        /// </summary>
        public static bool IsFuturesAccount(this SharedAccountType type) =>
            type == SharedAccountType.PerpetualLinearFutures
            || type == SharedAccountType.DeliveryLinearFutures
            || type == SharedAccountType.PerpetualInverseFutures
            || type == SharedAccountType.DeliveryInverseFutures;

        /// <summary>
        /// Whether the account type is a margin account
        /// </summary>
        public static bool IsMarginAccount(this SharedAccountType type) =>
            type == SharedAccountType.CrossMargin
            || type == SharedAccountType.IsolatedMargin;

        /// <summary>
        /// Map a TradingMode value to a SharedAccountType enum value
        /// </summary>
        public static SharedAccountType ToAccountType(this TradingMode mode)
        {
            if (mode == TradingMode.Spot) return SharedAccountType.Spot;
            if (mode == TradingMode.PerpetualLinear) return SharedAccountType.PerpetualLinearFutures;
            if (mode == TradingMode.PerpetualInverse) return SharedAccountType.PerpetualInverseFutures;
            if (mode == TradingMode.DeliveryInverse) return SharedAccountType.DeliveryInverseFutures;
            if (mode == TradingMode.DeliveryLinear) return SharedAccountType.DeliveryLinearFutures;

            throw new ArgumentException(nameof(mode), "Unmapped trading mode");
        }

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
            if (typeof(IFeeRestClient).IsAssignableFrom(typeof(T)))
                services.AddTransient(x => (IFeeRestClient)client(x)!);
            if (typeof(IBookTickerRestClient).IsAssignableFrom(typeof(T)))
                services.AddTransient(x => (IBookTickerRestClient)client(x)!);
            if (typeof(ITransferRestClient).IsAssignableFrom(typeof(T)))
                services.AddTransient(x => (ITransferRestClient)client(x)!);

            if (typeof(ISpotOrderRestClient).IsAssignableFrom(typeof(T)))
                services.AddTransient(x => (ISpotOrderRestClient)client(x)!);
            if (typeof(ISpotSymbolRestClient).IsAssignableFrom(typeof(T)))
                services.AddTransient(x => (ISpotSymbolRestClient)client(x)!);
            if (typeof(ISpotTickerRestClient).IsAssignableFrom(typeof(T)))
                services.AddTransient(x => (ISpotTickerRestClient)client(x)!);
            if (typeof(ISpotTriggerOrderRestClient).IsAssignableFrom(typeof(T)))
                services.AddTransient(x => (ISpotTriggerOrderRestClient)client(x)!);
            if (typeof(ISpotOrderClientIdRestClient).IsAssignableFrom(typeof(T)))
                services.AddTransient(x => (ISpotOrderClientIdRestClient)client(x)!);

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
            if (typeof(IFuturesTpSlRestClient).IsAssignableFrom(typeof(T)))
                services.AddTransient(x => (IFuturesTpSlRestClient)client(x)!);
            if (typeof(IFuturesTriggerOrderRestClient).IsAssignableFrom(typeof(T)))
                services.AddTransient(x => (IFuturesTriggerOrderRestClient)client(x)!);
            if (typeof(IFuturesOrderClientIdRestClient).IsAssignableFrom(typeof(T)))
                services.AddTransient(x => (IFuturesOrderClientIdRestClient)client(x)!);

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
            if (typeof(IOrderBookSocketClient).IsAssignableFrom(typeof(T)))
                services.AddTransient(x => (IOrderBookSocketClient)client(x)!);
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

        /// <summary>
        /// Convert a hex encoded string to byte array
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static byte[] HexStringToBytes(this string hexString)
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
    }
}

