using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net
{
    /// <summary>
    /// General helpers functions
    /// </summary>
    public static class ExchangeHelpers
    {
        private const string _allowedRandomChars = "ABCDEFGHIJKLMONOPQRSTUVWXYZabcdefghijklmonopqrstuvwxyz0123456789";

        private static readonly Dictionary<int, string> _monthSymbols = new Dictionary<int, string>()
        {
            { 1, "F" },
            { 2, "G" },
            { 3, "H" },
            { 4, "J" },
            { 5, "K" },
            { 6, "M" },
            { 7, "N" },
            { 8, "Q" },
            { 9, "U" },
            { 10, "V" },
            { 11, "X" },
            { 12, "Z" },
        };

        /// <summary>
        /// The last used id, use NextId() to get the next id and up this
        /// </summary>
        private static int _lastId;

        /// <summary>
        /// Clamp a value between a min and max
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static decimal ClampValue(decimal min, decimal max, decimal value)
        {
            value = Math.Min(max, value);
            value = Math.Max(min, value);
            return value;
        }

        /// <summary>
        /// Adjust a value to be between the min and max parameters and rounded to the closest step.
        /// </summary>
        /// <param name="min">The min value</param>
        /// <param name="max">The max value</param>
        /// <param name="step">The step size the value should be floored to. For example, value 2.548 with a step size of 0.01 will output 2.54</param>
        /// <param name="roundingType">How to round</param>
        /// <param name="value">The input value</param>
        /// <returns></returns>
        public static decimal AdjustValueStep(decimal min, decimal max, decimal? step, RoundingType roundingType, decimal value)
        {
            if(step == 0)
                throw new ArgumentException($"0 not allowed for parameter {nameof(step)}, pass in null to ignore the step size", nameof(step));

            value = Math.Min(max, value);
            value = Math.Max(min, value);
            if (step == null)
                return value;

            var offset = value % step.Value;
            if(roundingType == RoundingType.Down)
            {
                value -= offset;
            }
            else if(roundingType == RoundingType.Up)
            {
                if (offset != 0)
                    value += (step.Value - offset);
            }
            else
            {
                if (offset < step / 2)
                    value -= offset;
                else value += (step.Value - offset);
            }

            value = RoundDown(value, 8);
                
            return value.Normalize();
        }

        /// <summary>
        /// Adjust a value to be between the min and max parameters and rounded to the closest precision.
        /// </summary>
        /// <param name="min">The min value</param>
        /// <param name="max">The max value</param>
        /// <param name="precision">The precision the value should be rounded to. For example, value 2.554215 with a precision of 5 will output 2.5542</param>
        /// <param name="roundingType">How to round</param>
        /// <param name="value">The input value</param>
        /// <returns></returns>
        public static decimal AdjustValuePrecision(decimal min, decimal max, int? precision, RoundingType roundingType, decimal value)
        {
            value = Math.Min(max, value);
            value = Math.Max(min, value);
            if (precision == null)
                return value;

            return RoundToSignificantDigits(value, precision.Value, roundingType);
        }

        /// <summary>
        /// Round a value to have the provided total number of digits. For example, value 253.12332 with 5 digits would be 253.12 
        /// </summary>
        /// <param name="value">The value to round</param>
        /// <param name="digits">The total amount of digits (NOT decimal places) to round to</param>
        /// <param name="roundingType">How to round</param>
        /// <returns></returns>
        public static decimal RoundToSignificantDigits(decimal value, int digits, RoundingType roundingType)
        {
            var val = (double)value;
            if (value == 0)
                return 0;

            double scale = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(val))) + 1);
            if(roundingType == RoundingType.Closest)
                return (decimal)(scale * Math.Round(val / scale, digits));
            else
                return (decimal)(scale * (double)RoundDown((decimal)(val / scale), digits));
        }

        /// <summary>
        /// Rounds a value down 
        /// </summary>
        public static decimal RoundDown(decimal i, double decimalPlaces)
        {
            var power = Convert.ToDecimal(Math.Pow(10, decimalPlaces));
            return Math.Floor(i * power) / power;
        }

        /// <summary>
        /// Rounds a value up
        /// </summary>
        public static decimal RoundUp(decimal i, double decimalPlaces)
        {
            var power = Convert.ToDecimal(Math.Pow(10, decimalPlaces));
            return Math.Ceiling(i * power) / power;
        }

        /// <summary>
        /// Strips any trailing zero's of a decimal value, useful when converting the value to string.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static decimal Normalize(this decimal value)
        {
            return value / 1.000000000000000000000000000000000m;
        }

        /// <summary>
        /// Generate a new unique id. The id is staticly stored so it is guarenteed to be unique
        /// </summary>
        /// <returns></returns>
        public static int NextId() => Interlocked.Increment(ref _lastId);

        /// <summary>
        /// Return the last unique id that was generated
        /// </summary>
        /// <returns></returns>
        public static int LastId() => _lastId;

        /// <summary>
        /// Generate a random string of specified length
        /// </summary>
        /// <param name="length">Length of the random string</param>
        /// <returns></returns>
        public static string RandomString(int length)
        {
            var randomChars = new char[length];

#if NETSTANDARD2_1_OR_GREATER
            for (int i = 0; i < length; i++)
                randomChars[i] = _allowedRandomChars[RandomNumberGenerator.GetInt32(0, _allowedRandomChars.Length)];
#else
            var random = new Random();
            for (int i = 0; i < length; i++)
                randomChars[i] = _allowedRandomChars[random.Next(0, _allowedRandomChars.Length)];
#endif

            return new string(randomChars);
        }

        /// <summary>
        /// Generate a random string of specified length
        /// </summary>
        /// <param name="source">The initial string</param>
        /// <param name="totalLength">Total length of the resulting string</param>
        /// <returns></returns>
        public static string AppendRandomString(string source, int totalLength)
        {
            if (totalLength < source.Length)
                throw new ArgumentException("Total length smaller than source string length", nameof(totalLength));

            if (totalLength == source.Length)
                return source;

            return source + RandomString(totalLength - source.Length);
        }

        /// <summary>
        /// Get the month representation for futures symbol based on the delivery month
        /// </summary>
        /// <param name="time">Delivery time</param>
        /// <returns></returns>
        public static string GetDeliveryMonthSymbol(DateTime time) => _monthSymbols[time.Month];

        /// <summary>
        /// Execute multiple requests to retrieve multiple pages of the result set
        /// </summary>
        /// <typeparam name="T">Type of the client</typeparam>
        /// <typeparam name="U">Type of the request</typeparam>
        /// <param name="paginatedFunc">The func to execute with each request</param>
        /// <param name="request">The request parameters</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        public static async IAsyncEnumerable<ExchangeWebResult<IEnumerable<T>>> ExecutePages<T, U>(Func<U, INextPageToken?, CancellationToken, Task<ExchangeWebResult<IEnumerable<T>>>> paginatedFunc, U request, [EnumeratorCancellation]CancellationToken ct = default)
        {
            var result = new List<T>();
            ExchangeWebResult<IEnumerable<T>> batch;
            INextPageToken? nextPageToken = null;
            while (true)
            {
                batch = await paginatedFunc(request, nextPageToken, ct).ConfigureAwait(false);
                yield return batch;
                if (!batch || ct.IsCancellationRequested)
                    break;

                result.AddRange(batch.Data);
                nextPageToken = batch.NextPageToken;
                if (nextPageToken == null)
                    break;
            }
        }

        /// <summary>
        /// Apply the rules (price and quantity step size and decimals precision, min/max quantity) from the symbol to the quantity and price
        /// </summary>
        /// <param name="symbol">The symbol as retrieved from the exchange</param>
        /// <param name="quantity">Quantity to trade</param>
        /// <param name="price">Price to trade at</param>
        /// <param name="adjustedQuantity">Quantity adjusted to match all trading rules</param>
        /// <param name="adjustedPrice">Price adjusted to match all trading rules</param>
        public static void ApplySymbolRules(SharedSpotSymbol symbol, decimal quantity, decimal? price, out decimal adjustedQuantity, out decimal? adjustedPrice)
        {
            adjustedPrice = price;
            adjustedQuantity = quantity;
            var minNotionalAdjust = false;

            if (price != null)
            {
                adjustedPrice = AdjustValueStep(0, decimal.MaxValue, symbol.PriceStep, RoundingType.Down, price.Value);
                adjustedPrice = symbol.PriceDecimals.HasValue ? RoundDown(price.Value, symbol.PriceDecimals.Value) : adjustedPrice;
                if (adjustedPrice != 0 && adjustedPrice * quantity < symbol.MinNotionalValue)
                {
                    adjustedQuantity = symbol.MinNotionalValue.Value / adjustedPrice.Value;
                    minNotionalAdjust = true;
                }
            }

            adjustedQuantity = AdjustValueStep(symbol.MinTradeQuantity ?? 0, symbol.MaxTradeQuantity ?? decimal.MaxValue, symbol.QuantityStep, minNotionalAdjust ? RoundingType.Up : RoundingType.Down, adjustedQuantity);
            adjustedQuantity = symbol.QuantityDecimals.HasValue ? (minNotionalAdjust ? RoundUp(adjustedQuantity, symbol.QuantityDecimals.Value) : RoundDown(adjustedQuantity, symbol.QuantityDecimals.Value)) : adjustedQuantity;

        }
    }
}
