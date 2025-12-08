using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.SharedApis;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        private const string _allowedRandomHexChars = "0123456789ABCDEF";

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
        /// Apply the provided rules to the value
        /// </summary>
        /// <param name="value">Value to be adjusted</param>
        /// <param name="decimals">Max decimal places</param>
        /// <param name="valueStep">The value step for increase/decrease value</param>
        /// <returns></returns>
        public static decimal ApplyRules(
            decimal value,
            int? decimals = null,
            decimal? valueStep = null)
        {
            if (valueStep.HasValue)
            {
                var offset = value % valueStep.Value;
                if (offset != 0)
                {
                    if (offset < valueStep.Value / 2)
                        value -= offset;
                    else value += (valueStep.Value - offset);
                }
            }
            if (decimals.HasValue)
                value = Math.Round(value, decimals.Value);

            return value;
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
        /// Generate a new unique id. The id is statically stored so it is guaranteed to be unique
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

#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
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
        /// <param name="length">Length of the random string</param>
        /// <returns></returns>
        public static string RandomHexString(int length)
        {
#if NET9_0_OR_GREATER
            return "0x" + RandomNumberGenerator.GetHexString(length * 2);
#else
            var randomChars = new char[length * 2];
            var random = new Random();
            for (int i = 0; i < length * 2; i++)
                randomChars[i] = _allowedRandomHexChars[random.Next(0, _allowedRandomHexChars.Length)];
            return "0x" + new string(randomChars);
#endif
        }

        /// <summary>
        /// Generate a long value
        /// </summary>
        /// <param name="maxLength">Max character length</param>
        /// <returns></returns>
        public static long RandomLong(int maxLength)
        {
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
            var value = RandomNumberGenerator.GetInt32(0, int.MaxValue);
#else
            var random = new Random();
            var value = random.Next(0, int.MaxValue);
#endif
            var val = value.ToString();
            if (val.Length > maxLength)
                return int.Parse(val.Substring(0, maxLength));
            else
                return value;
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
        public static async IAsyncEnumerable<ExchangeWebResult<T[]>> ExecutePages<T, U>(Func<U, INextPageToken?, CancellationToken, Task<ExchangeWebResult<T[]>>> paginatedFunc, U request, [EnumeratorCancellation]CancellationToken ct = default)
        {
            var result = new List<T>();
            ExchangeWebResult<T[]> batch;
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
                adjustedPrice = symbol.PriceSignificantFigures.HasValue ? RoundToSignificantDigits(adjustedPrice.Value, symbol.PriceSignificantFigures.Value, RoundingType.Closest) : adjustedPrice;
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

        /// <summary>
        /// Queue updates received from a websocket subscriptions and process them async
        /// </summary>
        /// <typeparam name="T">The queued update type</typeparam>
        /// <param name="subscribeCall">The subscribe call</param>
        /// <param name="asyncHandler">The async update handler</param>
        /// <param name="maxQueuedItems">The max number of updates to be queued up. When happens when the queue is full and a new write is attempted can be specified with <see>fullMode</see></param>
        /// <param name="fullBehavior">What should happen if the queue contains <see>maxQueuedItems</see> pending updates. If no max is set this setting is ignored</param>
        public static async Task<CallResult<UpdateSubscription>> ProcessQueuedAsync<T>(
            Func<Action<DataEvent<T>>, Task<CallResult<UpdateSubscription>>> subscribeCall,
            Func<DataEvent<T>, Task> asyncHandler,
            int? maxQueuedItems = null,
            QueueFullBehavior? fullBehavior = null)
        {
            var processor = new ProcessQueue<DataEvent<T>>(asyncHandler, maxQueuedItems, fullBehavior);
            await processor.StartAsync().ConfigureAwait(false);
            var result = await subscribeCall(upd => processor.Write(upd)).ConfigureAwait(false);
            if (!result)
            {
                await processor.StopAsync().ConfigureAwait(false);
                return result;
            }

            processor.Exception += result.Data._subscription.InvokeExceptionHandler;
            result.Data.SubscriptionStatusChanged += (upd) =>
            {
                if (upd == CryptoExchange.Net.Objects.SubscriptionStatus.Closed)
                    _ = processor.StopAsync(true);
            };

            return result;
        }

        /// <summary>
        /// Queue updates and process them async
        /// </summary>
        /// <typeparam name="T">The queued update type</typeparam>
        /// <param name="subscribeCall">The subscribe call</param>
        /// <param name="asyncHandler">The async update handler</param>
        /// <param name="maxQueuedItems">The max number of updates to be queued up. When happens when the queue is full and a new write is attempted can be specified with <see>fullMode</see></param>
        /// <param name="fullBehavior">What should happen if the queue contains <see>maxQueuedItems</see> pending updates. If no max is set this setting is ignored</param>
        /// <param name="ct">Cancellation token to stop the processing</param>
        public static async Task ProcessQueuedAsync<T>(
            Func<Action<T>, Task> subscribeCall,
            Func<T, Task> asyncHandler,
            CancellationToken ct,
            int? maxQueuedItems = null,
            QueueFullBehavior? fullBehavior = null)
        {
            var processor = new ProcessQueue<T>(asyncHandler, maxQueuedItems, fullBehavior);
            await processor.StartAsync().ConfigureAwait(false);
            ct.Register(() => _ = processor.StopAsync());

            await subscribeCall(upd => processor.Write(upd)).ConfigureAwait(false);
        }

        /// <summary>
        /// Queue updates received from a websocket subscriptions and process them async
        /// </summary>
        /// <typeparam name="TEventType">The type of the queued item</typeparam>
        /// <typeparam name="TOutputType">The type of the item to pass to the processor</typeparam>
        /// <param name="subscribeCall">The subscribe call</param>
        /// <param name="mapper">The mapper function to go from <see>TEventType</see> to <see>TOutputType</see></param>
        /// <param name="asyncHandler">The async update handler</param>
        /// <param name="maxQueuedItems">The max number of updates to be queued up. When happens when the queue is full and a new write is attempted can be specified with <see>fullMode</see></param>
        /// <param name="fullBehavior">What should happen if the queue contains <see>maxQueuedItems</see> pending updates. If no max is set this setting is ignored</param>
        public static async Task<CallResult<UpdateSubscription>> ProcessQueuedAsync<TEventType, TOutputType>(
            Func<ProcessQueue<DataEvent<TEventType>>, Task<CallResult<UpdateSubscription>>> subscribeCall,
            Func<DataEvent<TEventType>, DataEvent<TOutputType>> mapper,
            Func<DataEvent<TOutputType>, Task> asyncHandler,
            int? maxQueuedItems = null,
            QueueFullBehavior? fullBehavior = null
            )
        {
            var processor = new ProcessQueue<DataEvent<TEventType>>((update) => {
                return asyncHandler.Invoke(mapper.Invoke(update));
            }, maxQueuedItems, fullBehavior);
            await processor.StartAsync().ConfigureAwait(false);
            var result = await subscribeCall(processor).ConfigureAwait(false);
            if (!result)
            {
                await processor.StopAsync().ConfigureAwait(false);
                return result;
            }

            processor.Exception += result.Data._subscription.InvokeExceptionHandler;
            result.Data.SubscriptionStatusChanged += (upd) =>
            {
                if (upd == SubscriptionStatus.Closed)
                    _ = processor.StopAsync(true);
            };

            return result;
        }

        /// <summary>
        /// Parse a decimal value from a string
        /// </summary>
        public static decimal? ParseDecimal(string? value)
        {
            // Value is null or empty is the most common case to return null so check before trying to parse
            if (string.IsNullOrEmpty(value))            
                return null;            

            // Try parse, only fails for these reasons:
            // 1. string is null or empty
            // 2. value is larger or smaller than decimal max/min
            // 3. unparsable format
            if (decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var decValue))
                return decValue;

            // Check for values which should be parsed to null
            if (string.Equals("null", value, StringComparison.OrdinalIgnoreCase)
                || string.Equals("NaN", value, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            // Infinity value should be parsed to min/max value
            if (string.Equals("Infinity", value, StringComparison.OrdinalIgnoreCase))
                return decimal.MaxValue;
            else if(string.Equals("-Infinity", value, StringComparison.OrdinalIgnoreCase))
                    return decimal.MinValue;

            if (value!.Length > 27 && decimal.TryParse(value.Substring(0, 27), out var overflowValue))
            {
                // Not a valid decimal value and more than 27 chars, from which the first part can be parsed correctly.
                // assume overflow
                if (overflowValue < 0)
                    return decimal.MinValue;
                else
                    return decimal.MaxValue;
            }

            // Unknown decimal format, return null
            return null;
        }
    }
}
