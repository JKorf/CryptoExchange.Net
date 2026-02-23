using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Pagination methods
    /// </summary>
    public static class Pagination
    {
        /// <summary>
        /// Get pagination parameters
        /// </summary>
        /// <param name="direction">The data direction</param>
        /// <param name="limit">Result limit</param>
        /// <param name="requestStartTime">User request start time</param>
        /// <param name="requestEndTime">User request end time</param>
        /// <param name="paginationRequest">Provided page request</param>
        /// <param name="setOtherTimeLimiter">Whether to set start time if direction is descending, or end time if direction is ascending</param>
        /// <param name="maxPeriod">Max period the time filters can span</param>
        /// <returns></returns>
        public static PaginationParameters GetPaginationParameters(
            DataDirection direction,
            int limit,
            DateTime? requestStartTime,
            DateTime requestEndTime,
            PageRequest? paginationRequest, 
            bool setOtherTimeLimiter = true,
            TimeSpan? maxPeriod = null
            )
        {
            var startTime = paginationRequest?.StartTime ?? requestStartTime;
            var endTime = paginationRequest?.EndTime ?? requestEndTime;
            if (maxPeriod != null)
            {
                if (direction == DataDirection.Ascending)
                {
                    if (startTime == null)
                    {
                        startTime = endTime.Add(-maxPeriod.Value);
                    }
                    else
                    {
                        endTime = startTime.Value.Add(maxPeriod.Value);
                        if (endTime > DateTime.UtcNow)
                            endTime = DateTime.UtcNow;
                    }
                }
                else
                {
                    startTime = endTime.Add(-maxPeriod.Value);
                }
            }

            return new PaginationParameters
            {
                Limit = limit,
                StartTime = direction == DataDirection.Ascending || setOtherTimeLimiter ? startTime : null,
                EndTime = direction == DataDirection.Descending || setOtherTimeLimiter ? endTime : null,
                Direction = direction,
                FromId = paginationRequest?.FromId,
                Offset = paginationRequest?.Offset,
                Page = paginationRequest?.Page,
                Cursor = paginationRequest?.Cursor
            };
        }

        /// <summary>
        /// Get the next page request parameters from result kline data
        /// </summary>
        /// <param name="nextPageRequest">Callback for returning the next page request</param>
        /// <param name="resultCount">Number of results in data</param>
        /// <param name="timestamps">Timestamps of the result data</param>
        /// <param name="requestStartTime">User request start time</param>
        /// <param name="requestEndTime">User request end time</param>
        /// <param name="lastPaginationData">The last used pagination data</param>
        /// <param name="interval">Kline interval</param>
        /// <returns></returns>
        public static PageRequest? GetNextPageRequestKlines(
            Func<PageRequest?> nextPageRequest,
            int resultCount,
            IEnumerable<DateTime> timestamps,
            DateTime? requestStartTime,
            DateTime requestEndTime,
            PaginationParameters lastPaginationData,
            SharedKlineInterval interval
            )
        {
            if (HasNextPageKlines(resultCount, timestamps, requestStartTime, requestEndTime, lastPaginationData.Limit, lastPaginationData.Direction, interval))
            {
                var result = nextPageRequest();
                if (result != null)
                {
                    result.StartTime ??= lastPaginationData.StartTime;
                    result.EndTime ??= lastPaginationData.EndTime;
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        /// Get the next page request parameters from result data
        /// </summary>
        /// <param name="nextPageRequest">Callback for returning the next page request</param>
        /// <param name="resultCount">Number of results in data</param>
        /// <param name="timestamps">Timestamps of the result data</param>
        /// <param name="requestStartTime">User request start time</param>
        /// <param name="requestEndTime">User request end time</param>
        /// <param name="lastPaginationData">The last used pagination data</param>
        /// <param name="maxPeriod">Max period the time filters can span</param>
        /// <param name="maxAge">Max age of the data</param>
        /// <returns></returns>
        public static PageRequest? GetNextPageRequest(
            Func<PageRequest?> nextPageRequest,
            int resultCount,
            IEnumerable<DateTime> timestamps,
            DateTime? requestStartTime,
            DateTime requestEndTime,
            PaginationParameters lastPaginationData,
            TimeSpan? maxPeriod = null,
            TimeSpan? maxAge = null
            )
        {
            if (HasNextPage(resultCount, timestamps, requestStartTime, requestEndTime, lastPaginationData.Limit, lastPaginationData.Direction))
            {
                var result = nextPageRequest();
                if (result != null) 
                {
                    result.StartTime ??= lastPaginationData.StartTime;
                    result.EndTime ??= lastPaginationData.EndTime;
                    return result;
                }
            }

            if (maxPeriod != null)
            {
                if (HasNextPeriod(requestStartTime, requestEndTime, lastPaginationData.Direction, lastPaginationData, maxPeriod.Value, maxAge))
                {
                    var (startTime, endTime) = GetNextPeriod(requestStartTime, requestEndTime, lastPaginationData.Direction, lastPaginationData, maxPeriod.Value, maxAge);
                    return new PageRequest
                    {
                        StartTime = startTime,
                        EndTime = endTime
                    };
                }
            }

            return null;
        }

        /// <summary>
        /// Check whether there is (potentially) another page available 
        /// </summary>
        /// <param name="resultCount">Number of result entries</param>
        /// <param name="timestamps">Timestamps</param>
        /// <param name="requestStartTime">User request start time</param>
        /// <param name="requestEndTime">User request end time</param>
        /// <param name="limit">Max number of results requested</param>
        /// <param name="direction">Data direction</param>
        /// <param name="interval">Kline interval</param>
        /// <returns></returns>
        public static bool HasNextPageKlines(
            int resultCount,
            IEnumerable<DateTime> timestamps,
            DateTime? requestStartTime,
            DateTime requestEndTime,
            int limit,
            DataDirection direction,
            SharedKlineInterval interval
            )
        {
            if (resultCount < limit)
                return false;

            if (direction == DataDirection.Ascending)
            {
                if (timestamps.Max().AddSeconds((int)interval) >= requestEndTime)
                    return false;

                return true;
            }
            else
            {
                if (timestamps.Min().AddSeconds((int)interval) < requestStartTime)
                    return false;

                return true;
            }
        }

        /// <summary>
        /// Check whether there is (potentially) another page available 
        /// </summary>
        /// <param name="resultCount">Number of result entries</param>
        /// <param name="timestamps">Timestamps</param>
        /// <param name="requestStartTime">User request start time</param>
        /// <param name="requestEndTime">User request end time</param>
        /// <param name="limit">Max number of results requested</param>
        /// <param name="direction">Data direction</param>
        /// <returns></returns>
        public static bool HasNextPage(
            int resultCount,
            IEnumerable<DateTime> timestamps,
            DateTime? requestStartTime,
            DateTime requestEndTime,
            int limit,
            DataDirection direction)
        {
            if (resultCount < limit)
                return false;

            if (!timestamps.Any())
                return false;

            if (direction == DataDirection.Ascending)
            {
                if (timestamps.Max() >= requestEndTime)
                    return false;

                return true;
            }
            else
            {
                if (timestamps.Min() < requestStartTime)
                    return false;

                return true;
            }
        }

        /// <summary>
        /// Get the next page PageRequest
        /// </summary>
        public static PageRequest NextPageFromPage(PaginationParameters lastPaginationData)
        {
            return new PageRequest { Page = (lastPaginationData.Page ?? 1) + 1 };
        }
        /// <summary>
        /// Get the next offset PageRequest
        /// </summary>
        public static PageRequest NextPageFromOffset(PaginationParameters lastPaginationData, int resultCount)
        {
            return new PageRequest { Offset = (lastPaginationData.Offset ?? 0) + resultCount };
        }
        /// <summary>
        /// Get the next page cursor PageRequest
        /// </summary>
        public static PageRequest NextPageFromCursor(string nextCursor)
        {
            return new PageRequest { Cursor = nextCursor };
        }
        /// <summary>
        /// Get the next id PageRequest
        /// </summary>
        public static PageRequest NextPageFromId(long nextFromId)
        {
            return new PageRequest { FromId = nextFromId.ToString() };
        }
        /// <summary>
        /// Get the next id PageRequest
        /// </summary>
        public static PageRequest NextPageFromId(string nextFromId)
        {
            return new PageRequest { FromId = nextFromId };
        }
        /// <summary>
        /// Get the next start/end time PageRequest
        /// </summary>
        public static PageRequest NextPageFromTime(PaginationParameters lastPaginationData, DateTime lastTimestamp, bool setOtherTimeLimiter = true)
        {
            if (lastPaginationData.Direction == DataDirection.Ascending)
                return new PageRequest { StartTime = lastTimestamp.AddMilliseconds(1), EndTime = setOtherTimeLimiter ? lastPaginationData.EndTime : null  };
            else
                return new PageRequest { EndTime = lastTimestamp.AddMilliseconds(-1), StartTime = setOtherTimeLimiter ? lastPaginationData.StartTime : null };
        }

        /// <summary>
        /// Get the next start/end time klines PageRequest
        /// </summary>
        public static PageRequest NextPageFromTimeKlines(DataDirection direction, GetKlinesRequest request, DateTime lastTimestamp, int limit)
        {
            if (direction == DataDirection.Ascending)
            {
                var nextStartTime = lastTimestamp.AddSeconds((int)request.Interval);
                var endTime = nextStartTime.AddSeconds(limit * (int)request.Interval);
                var requestEndTime = request.EndTime ?? DateTime.UtcNow;
                if (endTime > requestEndTime)
                    endTime = requestEndTime;

                return new PageRequest { StartTime = nextStartTime, EndTime = endTime };
            }
            else
            {
                var nextEndTime = lastTimestamp.AddSeconds(-(int)request.Interval);
                var startTime = nextEndTime.AddSeconds(-(limit * (int)request.Interval));
                var requestStartTime = request.StartTime ?? DateTime.UtcNow;
                if (startTime < requestStartTime)
                    startTime = requestStartTime;
                return new PageRequest { StartTime = startTime, EndTime = nextEndTime };
            }
        }

        /// <summary>
        /// Whether another time period is to be requested 
        /// </summary>
        /// <param name="requestStartTime">User request start time</param>
        /// <param name="requestEndTime">User request end time</param>
        /// <param name="direction">Data direction</param>
        /// <param name="lastPaginationParameters">Pagination parameters used</param>
        /// <param name="period">Max time period a request can span</param>
        /// <param name="maxAge">Max age of data that can be requested</param>
        public static bool HasNextPeriod(
            DateTime? requestStartTime,
            DateTime requestEndTime,
            DataDirection direction,
            PaginationParameters lastPaginationParameters,
            TimeSpan period,
            TimeSpan? maxAge)
        {
            if (direction == DataDirection.Ascending && lastPaginationParameters.StartTime == null)
                throw new InvalidOperationException("Invalid pagination data; no start time for ascending pagination");

            if (direction == DataDirection.Ascending)
            {
                return (requestEndTime - lastPaginationParameters.EndTime!.Value).TotalSeconds > 1;
            }
            else
            {
                var lastPageStartTime = lastPaginationParameters.StartTime ?? lastPaginationParameters.EndTime!.Value.Add(-period);
                if (requestStartTime != null)
                {
                    var nextPeriodDuration = lastPageStartTime - requestStartTime.Value;
                    return nextPeriodDuration.TotalSeconds > 1;
                }
                else
                {
                    var nextStartTime = lastPageStartTime - period;
                    if (maxAge != null)
                    {
                        var minStartTime = DateTime.UtcNow - maxAge.Value;
                        if ((nextStartTime.Add(period) - minStartTime).TotalSeconds < 1)
                            return false;
                    }

                    var nextPeriodDuration = lastPageStartTime - nextStartTime;
                    return (nextPeriodDuration).TotalSeconds > 1;
                }
            }
        }

        /// <summary>
        /// Get the start/end time for the next data period
        /// </summary>
        /// <param name="requestStartTime">User request start time</param>
        /// <param name="requestEndTime">User request end time</param>
        /// <param name="direction">Data direction</param>
        /// <param name="lastPaginationParameters">Pagination parameters used</param>
        /// <param name="period">Max time period a request can span</param>
        /// <param name="maxAge">Max age of data that can be requested</param>
        public static (DateTime? startTime, DateTime? endTime) GetNextPeriod(
            DateTime? requestStartTime,
            DateTime requestEndTime,
            DataDirection direction,
            PaginationParameters lastPaginationParameters,
            TimeSpan period,
            TimeSpan? maxAge
            )
        {
            DateTime? nextStartTime = null;
            DateTime? nextEndTime = null;
            if (direction == DataDirection.Ascending)
            {
                if (lastPaginationParameters.StartTime != null)
                    nextStartTime = lastPaginationParameters.StartTime.Value.Add(period);
                if (lastPaginationParameters.EndTime != null)
                    nextEndTime = lastPaginationParameters.EndTime.Value.Add(period);
            }
            else
            {
                if (lastPaginationParameters.StartTime != null)
                    nextStartTime = lastPaginationParameters.StartTime.Value.Add(-period);
                if (lastPaginationParameters.EndTime != null)
                    nextEndTime = lastPaginationParameters.EndTime.Value.Add(-period);
            }

            if (nextStartTime != null && nextStartTime < requestStartTime)
                nextStartTime = requestStartTime;

            if (nextStartTime != null && maxAge != null && nextStartTime < DateTime.UtcNow - maxAge)
            {
                nextStartTime = DateTime.UtcNow.Add(-maxAge.Value);
                // Add 30 seconds to max sure the client/server time offset and latency doesn't push the timestamp over the limit
                nextStartTime = nextStartTime.Value.Add(TimeSpan.FromSeconds(30));
            }

            if (nextEndTime != null && nextEndTime > requestEndTime)
                nextEndTime = requestEndTime;

            return (nextStartTime, nextEndTime);
        }
    }
}
