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
        public static async IAsyncEnumerable<ExchangeWebResult<T[]>> ExecutePages<T, U>(Func<U, PageRequest?, CancellationToken, Task<ExchangeWebResult<T[]>>> paginatedFunc, U request, [EnumeratorCancellation]CancellationToken ct = default)
        {
            var result = new List<T>();
            ExchangeWebResult<T[]> batch;
            PageRequest? nextPageToken = null;
            while (true)
            {
                batch = await paginatedFunc(request, nextPageToken, ct).ConfigureAwait(false);
                yield return batch;
                if (!batch || ct.IsCancellationRequested)
                    break;

                result.AddRange(batch.Data);
                nextPageToken = batch.NextPageRequest;
                if (nextPageToken == null)
                    break;
            }
        }

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


        public static bool CheckForNextPage(
            int resultCount,
            IEnumerable<DateTime> timestamps,
            DateTime? startTime,
            DateTime? endTime,
            int limit,
            DataDirection paginationDirection)
        {
            if (limit <= resultCount)
                return false;

            if (!timestamps.Any())
                return false;

            if (paginationDirection == DataDirection.Ascending)
            {
                if (timestamps.Max() >= endTime)
                    return false;

                return true;
            }
            else
            {
                if (timestamps.Min() < startTime)
                    return false;

                return true;
            }
        }

        public enum PaginationFilterType
        {
            FromId,
            Time,
            Offset,
            Page
        }

        public enum TimeParameterSetType
        {
            None,
            OnlyMatchingDirection,
            Both
        }

        // 1.1 Apply moving time filter, set the startTime to new startTime in future (asc) or endTime to new endTime in the past (desc), while setting other time parameter to null
        // 1.2 Apply moving time filter, set the startTime to new startTime in future (asc) or endTime to new endTime in the past (desc), set the other time parameter to a max period offset value
        // 2. Apply fromId filter, set the fromId to the new fromId (direction depends on id filter direction)
        // 3. Apply offset filter, set the offset to the new offset (direction depends on id filter direction)



        public static PaginationParameters ApplyFromIdFilter(PageRequest pageRequest)
        {
            if (pageRequest.FromId == null)
                throw new Exception();

            return new PaginationParameters { FromId = pageRequest.FromId };
        }

        public static PaginationParameters ApplyPageFilter(PageRequest? pageRequest)
        {
            if (pageRequest?.Page == null || pageRequest.Page == 0)
                return new PaginationParameters() { Page = 1 };

            return new PaginationParameters { Page = pageRequest.Page };
        }

        public static PaginationParameters ApplyOffsetFilter(PageRequest pageRequest)
        {
            if (pageRequest.Offset == null)
                throw new Exception();

            return new PaginationParameters { Offset = pageRequest.Offset };
        }

        public static PaginationParameters ApplyAscendingMovingStartTimeFilter(PageRequest pageRequest)
        {
            if (pageRequest.StartTime == null)
                throw new Exception();

            return new PaginationParameters
            {
                StartTime = pageRequest.StartTime,
            };
        }

        public static PaginationParameters ApplyDescendingMovingEndTimeFilter(PageRequest pageRequest)
        {
            if (pageRequest.EndTime == null)
                throw new Exception();

            return new PaginationParameters
            {
                EndTime = pageRequest.EndTime,
            };
        }

        // Without date/time filter
        public static PageRequest? GetNextPageRequestPageSimple(
            PageRequest? currentPageRequest,
            bool hasNextPage,
            IEnumerable<DateTime> timeSelector,
            DateTime? requestStartTime,
            DateTime? requestEndTime
            )
        {
            if (CheckForNextPage(timeSelector, requestStartTime, requestEndTime, limit, direction))
            {
            }
        }

        public static PageRequest? GetNextPageRequest(
            Func<PageRequest> normalNextPageCallback,
            int responseLength,
            IEnumerable<DateTime> timeSelector,
            int limit,
            PageRequest? previousPageRequest,
            TimeParameterSetType parameterSetType,
            DateTime? currentRequestStartTime,
            DataDirection direction,
            DateTime? requestStartTime,
            DateTime? requestEndTime,
            TimeSpan? maxTimePeriodPerRequest = null
            )
        {
            if (CheckForNextPage(responseLength, timeSelector, previousPageRequest?.StartTime ?? requestStartTime, previousPageRequest?.EndTime ?? requestEndTime, limit, direction))
            {
                var result = normalNextPageCallback();
                if (parameterSetType == TimeParameterSetType.OnlyMatchingDirection)
                {
                    if (direction == DataDirection.Ascending)
                        result.StartTime ??= previousPageRequest?.StartTime ?? requestStartTime;
                    else
                        result.EndTime ??= previousPageRequest?.EndTime ?? requestEndTime;
                }
                else if (parameterSetType == TimeParameterSetType.Both)
                {
                    result.StartTime ??= previousPageRequest?.StartTime ?? requestStartTime;
                    result.EndTime ??= previousPageRequest?.EndTime ?? requestEndTime;
                }

                //if (previousPageRequest != null && result.StartTime == null && result.EndTime == null)
                //{
                //    result.StartTime = previousPageRequest.StartTime;
                //    result.EndTime = previousPageRequest.EndTime;
                //}

                return result;
            }

            if (maxTimePeriodPerRequest == null)
                return null;

            if (requestEndTime == null)
                requestEndTime = DateTime.UtcNow;

#warning this is only for DESC?
            // No next page, for this set, but we might have split for multiple time periods
            PageRequest? nextPageRequest = null;
            if (requestStartTime.HasValue
                && previousPageRequest?.StartTime != requestStartTime
                && (requestEndTime - requestStartTime) > maxTimePeriodPerRequest)
            {
                var currentStartTime = (currentRequestStartTime ?? (requestEndTime.Value.Add(-maxTimePeriodPerRequest.Value)));
                var nextPeriod = currentStartTime.Add(-maxTimePeriodPerRequest.Value);
                var nextStartTime = nextPeriod < requestStartTime ? requestStartTime : nextPeriod;
                if (currentStartTime - nextStartTime > TimeSpan.FromSeconds(1))
                {
                    nextPageRequest = new PageRequest
                    {
                        Offset = 0,
                        EndTime = currentStartTime,
                        StartTime = nextStartTime
                    };
                }
            }

            return nextPageRequest;
        }

        public static PaginationParameters ApplyPaginationParameters(
            DataDirection direction,
            PageRequest? pageRequest,
            PaginationFilterType? filterTypeAsc,
            PaginationFilterType? filterTypeDec,
            TimeParameterSetType parameterSetType,
            DateTime? requestStartTime,
            DateTime? requestEndTime,
            TimeSpan? maxTimePeriodPerRequest = null)
        {
            var filterType = direction == DataDirection.Ascending ? filterTypeAsc : filterTypeDec;
            if (filterType == null)
                throw new Exception();

            if (requestEndTime == null)
                requestEndTime = DateTime.UtcNow;

            if (pageRequest == null)
            {
                // No pagination data yet, initial request
                var result = new PaginationParameters
                {
                    StartTime = ShouldSet(parameterSetType, direction, true) ? requestStartTime : null,
                    EndTime = ShouldSet(parameterSetType, direction, false) ? requestEndTime : null
                };

                ApplyMaxTimePeriodPerRequest(maxTimePeriodPerRequest, direction, result);
                return result;
            }

            if (filterType == PaginationFilterType.Page)
                return ApplyPageFilter(pageRequest);

            if (filterType == PaginationFilterType.FromId)
                // From id doesn't need any other parameters
                return ApplyFromIdFilter(pageRequest);

            if (filterType == PaginationFilterType.Offset)
            {
                var result = ApplyOffsetFilter(pageRequest);
                result.StartTime = pageRequest.StartTime ?? requestStartTime;
                result.EndTime = pageRequest.EndTime ?? requestEndTime;
                ApplyMaxTimePeriodPerRequest(maxTimePeriodPerRequest, direction, result);
                return result;
            }

            if (filterType == PaginationFilterType.Time)
            {
                var result = direction == DataDirection.Ascending 
                    ? ApplyAscendingMovingStartTimeFilter(pageRequest) 
                    : ApplyDescendingMovingEndTimeFilter(pageRequest);

                if (parameterSetType == TimeParameterSetType.Both)
                {
                    if (direction == DataDirection.Ascending)
                        result.EndTime = requestEndTime;
                    else
                        result.StartTime = requestStartTime; 

                    ApplyMaxTimePeriodPerRequest(maxTimePeriodPerRequest, direction, result);
                }

                return result;
            }

            throw new Exception();
        }

        private static void ApplyMaxTimePeriodPerRequest(TimeSpan? maxTimePeriodPerRequest, DataDirection direction, PaginationParameters parameters)
        {
            if (maxTimePeriodPerRequest != null
                && parameters.StartTime.HasValue
                && parameters.EndTime.HasValue
                && (parameters.EndTime - parameters.StartTime) > maxTimePeriodPerRequest)
            {
                if (direction == DataDirection.Ascending)
                    parameters.EndTime = parameters.StartTime.Value.Add(maxTimePeriodPerRequest.Value);
                else
                    parameters.StartTime = parameters.EndTime.Value.Add(-maxTimePeriodPerRequest.Value);
            }
        }

        private static bool ShouldSet(TimeParameterSetType type, DataDirection direction, bool startTime)
        {
            if (type == TimeParameterSetType.None)
                return false;

            if (type == TimeParameterSetType.Both)
                return true;

            if (direction == DataDirection.Ascending)
                // If ascending startTime is the moving
                return startTime;

            // If descending startTime is the opposite
            return !startTime;
        }

        public static (DateTime? startTime, DateTime? endTime, long? fromId) ApplyPaginationRequestFilters(
            PaginationFilterType? ascendingFilterType,
            PaginationFilterType? descendingFilterType,
            DataDirection defaultDirection,
            DataDirection requestDirection,
            DateTime? userFilterStartTime,
            DateTime? userFilterEndTime,
            DateTime? paginationStartTime,
            DateTime? paginationEndTime,
            TimeSpan? maxTimePeriod,
            string? paginationFromId)
        {
            if (requestDirection == DataDirection.Ascending && userFilterStartTime == null)
                throw new InvalidOperationException("Ascending without start time");

            var filterType = requestDirection == DataDirection.Ascending ? ascendingFilterType : descendingFilterType;
            if (filterType == PaginationFilterType.FromId)
            {
                long? fromId = ApplyFromId(paginationFromId);
                DateTime? startTime = fromId == null ? userFilterStartTime : null;
                DateTime? endTime = fromId == null ? userFilterEndTime : null;

                if (defaultDirection != requestDirection
                    && fromId == null
                    && startTime == null)
                {
                    if (requestDirection == DataDirection.Ascending)
                    {
                        // If user requests in ASC order but the default is DESC order and user provides no parameters
                        // set startTime to a specific date to search data from that time forward
                        if (endTime == null)
                        {
                            startTime = DateTime.UtcNow.AddYears(-1);
                        }
                        else
                        {
                            startTime = endTime?.AddYears(-1);
                            endTime = null;
                        }
                    }
                    else
                    {
                        // If user requests in DESC order but the default is ASC order and user provides no parameters
                        // set endTime to now to search data from now back

                        // Need same structure as ascending?
                        endTime = DateTime.UtcNow;
                    }
                }

                if (fromId == null && startTime != null && endTime != null)
                {
                    // If we're not filtering by fromId we don't want to specify bot start and end time to prevent limitations in time period
                    if (requestDirection == DataDirection.Ascending) endTime = null;
                    if (requestDirection == DataDirection.Descending) startTime = null;
                }

                return (startTime, endTime, fromId);
            }
            else // Time
            {
                DateTime? startTime = requestDirection == DataDirection.Descending ? null : ApplyTime(userFilterStartTime, paginationStartTime);
                DateTime? endTime = (requestDirection == DataDirection.Ascending && paginationStartTime != null) ? null : ApplyTime(userFilterEndTime, paginationEndTime);
                if (maxTimePeriod.HasValue && endTime - startTime > maxTimePeriod.Value)
                {
                    if (requestDirection == DataDirection.Ascending)
                        endTime = startTime.Value.Add(maxTimePeriod.Value);
                    else
                        startTime = endTime.Value.Add(-maxTimePeriod.Value);
                }

                return (startTime, endTime, null);
            }
        }

        private static DateTime? ApplyTime(DateTime? userFilterTime, DateTime? paginationTime)
        {
            return paginationTime ?? userFilterTime;
        }

        //private static DateTime? ApplyTimeIfFromIdNull(string? fromId, DateTime? userFilterTime, DateTime? paginationTime)
        //{
        //    return fromId != null ? null: paginationTime ?? userFilterTime;
        //}

        private static long? ApplyFromId(string? paginationFromId)
        {
            return paginationFromId == null ? null : long.Parse(paginationFromId);
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
