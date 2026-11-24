using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.SharedApis;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        public static decimal AdjustValueStep(
             decimal min,
             decimal max,
             decimal? step,
             RoundingType roundingType,
             decimal value)
        {
            if (min > max)
                throw new ArgumentException("Minimum cannot exceed maximum.");

            value = ClampValue(min, max, value);

            if (!step.HasValue)
                return value;

            if (step.Value <= 0)
                throw new ArgumentOutOfRangeException(nameof(step));

            // Find the valid step-aligned interval.
            decimal alignedMin = RoundToStep(min, step.Value, RoundingType.Up);
            decimal alignedMax = RoundToStep(max, step.Value, RoundingType.Down);

            if (alignedMin > alignedMax)
                throw new ArgumentException("No step-aligned value exists within the bounds.");

            // Clamp before rounding to avoid rounding beyond decimal's range.
            value = ClampValue(alignedMin, alignedMax, value);

            return RoundToStep(value, step.Value, roundingType).Normalize();
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
        public static decimal AdjustValuePrecision(
            decimal min,
            decimal max,
            int? precision,
            RoundingType roundingType,
            decimal value)
        {
            if (min > max)
                throw new ArgumentException("Minimum cannot exceed maximum.");

            value = ClampValue(min, max, value);

            if (!precision.HasValue)
                return value;

            if (precision.Value < 1)
                throw new ArgumentOutOfRangeException(nameof(precision));

            decimal alignedMin = RoundToSignificantDigits(
                min, precision.Value, RoundingType.Up);

            decimal alignedMax = RoundToSignificantDigits(
                max, precision.Value, RoundingType.Down);

            if (alignedMin > alignedMax)
                throw new ArgumentException(
                    "No value with the requested precision exists within the bounds.");

            value = ClampValue(alignedMin, alignedMax, value);

            return RoundToSignificantDigits(
                value, precision.Value, roundingType);
        }

        private static decimal GetDecimalStep(double decimalPlaces)
        {
            if (double.IsNaN(decimalPlaces) ||
                decimalPlaces < 0 ||
                decimalPlaces > 28 ||
                decimalPlaces != Math.Truncate(decimalPlaces))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(decimalPlaces),
                    "Decimal places must be an integer between 0 and 28.");
            }

            decimal step = 1m;
            for (int i = 0; i < (int)decimalPlaces; i++)
                step /= 10m;

            return step;
        }

        private static decimal RoundToStep(
            decimal value,
            decimal step,
            RoundingType roundingType)
        {
            if (step <= 0)
                throw new ArgumentOutOfRangeException(nameof(step));

            if (roundingType != RoundingType.Down &&
                roundingType != RoundingType.Up &&
                roundingType != RoundingType.Closest)
            {
                throw new ArgumentOutOfRangeException(nameof(roundingType));
            }

            decimal remainder = value % step;
            if (remainder == 0)
                return value;

            decimal truncated = value - remainder;

            switch (roundingType)
            {
                case RoundingType.Down:
                    return value < 0 ? truncated - step : truncated;

                case RoundingType.Up:
                    return value > 0 ? truncated + step : truncated;

                default:
                    decimal distance = Math.Abs(remainder);

                    // Avoid step / 2, which can underflow for tiny steps.
                    if (distance >= step - distance)
                        return truncated + (value > 0 ? step : -step);

                    return truncated;
            }
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
            decimal? decimalStep = decimals.HasValue
                ? GetDecimalStep(decimals.Value)
                : (decimal?)null;

            if (valueStep.HasValue)
                value = RoundToStep(value, valueStep.Value, RoundingType.Closest);

            if (decimalStep.HasValue)
                value = Math.Round(value, decimals!.Value, MidpointRounding.ToEven);

            // Decimal-place rounding must not invalidate the step requirement.
            if (valueStep.HasValue && value % valueStep.Value != 0)
            {
                throw new ArgumentException(
                    "Decimal-place rounding produced a value that violates the step requirement.");
            }

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
            if (digits < 1)
                throw new ArgumentOutOfRangeException(nameof(digits));

            if (value == 0)
                return 0;

            // Determine the decimal exponent without converting to double.
            decimal magnitude = Math.Abs(value);
            int exponent = 0;

            while (magnitude >= 10m)
            {
                magnitude /= 10m;
                exponent++;
            }

            while (magnitude < 1m)
            {
                magnitude *= 10m;
                exponent--;
            }

            // Decimal cannot represent a step smaller than 10^-28.
            // At that precision, the original value already fits.
            if (digits >= exponent + 30)
                return value;

            int stepExponent = exponent - digits + 1;
            decimal step = 1m;

            for (int i = 0; i < stepExponent; i++)
                step *= 10m;

            for (int i = 0; i > stepExponent; i--)
                step /= 10m;

            decimal remainder = value % step;
            if (remainder == 0)
                return value;

            decimal truncated = value - remainder;

            switch (roundingType)
            {
                case RoundingType.Down:
                    return value < 0 ? truncated - step : truncated;

                case RoundingType.Up:
                    return value > 0 ? truncated + step : truncated;

                case RoundingType.Closest:
                    decimal distance = Math.Abs(remainder);
                    decimal halfway = step / 2m;

                    if (distance > halfway ||
                        (distance == halfway && (truncated / step) % 2m != 0))
                    {
                        return truncated + (value > 0 ? step : -step);
                    }

                    return truncated;

                default:
                    throw new ArgumentOutOfRangeException(nameof(roundingType));
            }
        }

        /// <summary>
        /// Rounds a value down 
        /// </summary>
        public static decimal RoundDown(decimal i, double decimalPlaces)
        {
            return RoundToStep(
                i, GetDecimalStep(decimalPlaces), RoundingType.Down);
        }


        /// <summary>
        /// Rounds a value up
        /// </summary>
        public static decimal RoundUp(decimal i, double decimalPlaces)
        {
            return RoundToStep(
                i, GetDecimalStep(decimalPlaces), RoundingType.Up);
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
        /// <param name="maxLength">Max number of digits</param>
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
        /// Generate a long value between two values
        /// </summary>
        /// <param name="minValue">Min value</param>
        /// <param name="maxValue">Max value</param>
        /// <returns></returns>
        public static long RandomLong(long minValue, long maxValue)
        {
#if NET8_0_OR_GREATER
            var buf = RandomNumberGenerator.GetBytes(8);
#else
            byte[] buf = new byte[8];
            var random = new Random();
            random.NextBytes(buf);
#endif
            long longRand = BitConverter.ToInt64(buf, 0);
            return (Math.Abs(longRand % (maxValue - minValue)) + minValue);
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
        public static async IAsyncEnumerable<HttpResult<T[]>> ExecutePages<T, U>(Func<U, PageRequest?, CancellationToken, Task<HttpResult<T[]>>> paginatedFunc, U request, [EnumeratorCancellation]CancellationToken ct = default)
        {
            var result = new List<T>();
            HttpResult<T[]> batch;
            PageRequest? nextPageToken = null;
            while (true)
            {
                batch = await paginatedFunc(request, nextPageToken, ct).ConfigureAwait(false);
                yield return batch;
                if (!batch.Success || ct.IsCancellationRequested)
                    break;

                result.AddRange(batch.Data);
                nextPageToken = batch.NextPageRequest;
                if (nextPageToken == null)
                    break;
            }
        }

        /// <summary>
        /// Apply filters to the data set
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="data">Data set</param>
        /// <param name="timeSelector">Time selector for the data</param>
        /// <param name="startTime">Start time filter</param>
        /// <param name="endTime">End time filter</param>
        /// <param name="direction">Data direction</param>
        public static IEnumerable<T> ApplyFilter<T>(
            IEnumerable<T> data,
            Func<T, DateTime> timeSelector,
            DateTime? startTime,
            DateTime? endTime,
            DataDirection direction)
        {
            if (direction == DataDirection.Ascending)
                data = data.OrderBy(timeSelector);
            else
                data = data.OrderByDescending(timeSelector);

            if (startTime != null)
                data = data.Where(x => timeSelector(x) >= startTime.Value);

            if (endTime != null)
                data = data.Where(x => timeSelector(x) < endTime.Value);

            return data;
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
        public static async Task<WebSocketResult<UpdateSubscription>> ProcessQueuedAsync<T>(
            Func<Action<DataEvent<T>>, Task<WebSocketResult<UpdateSubscription>>> subscribeCall,
            Func<DataEvent<T>, Task> asyncHandler,
            int? maxQueuedItems = null,
            QueueFullBehavior? fullBehavior = null)
        {
            var processor = new ProcessQueue<DataEvent<T>>(asyncHandler, maxQueuedItems, fullBehavior);
            await processor.StartAsync().ConfigureAwait(false);
            var result = await subscribeCall(upd => processor.Write(upd)).ConfigureAwait(false);
            if (!result.Success)
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
            ct.Register(async () =>
            {
                await processor.StopAsync().ConfigureAwait(false);
            });

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
            if (!result.Success)
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
            // 1. string is null or empty (already covered)
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

        /// <summary>
        /// Convert byte array to hex string
        /// </summary>
        /// <param name="buff"></param>
        /// <returns></returns>
        public static string BytesToHexString(byte[] buff)
            => BytesToHexString(new ArraySegment<byte>(buff));

        /// <summary>
        /// Convert byte array to hex string
        /// </summary>
        /// <param name="buff"></param>
        /// <returns></returns>
        public static string BytesToHexString(ArraySegment<byte> buff)
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
        public static byte[] HexToBytesString(string hexString)
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
