using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{

    public enum DataDirection
    {
        Ascending, // Old to new
        Descending // New to old
    }

    public record PaginationParameters
    {
        public DataDirection Direction { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? FromId { get; set; }
        public int? Offset { get; set; }
        public int? Page { get; set; }
    }
    public class PageRequest
    {
        public string? Cursor { get; set; }
        public int? Page { get; set; }
        public int? Offset { get; set; }
        public string? FromId { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    

        public static PageRequest NextCursor(string nextCursor) => new PageRequest { Cursor = nextCursor };
        public static PageRequest NextPage(int nextPage) => new PageRequest { Page = nextPage };
        public static PageRequest NextOffset(int nextOffset) => new PageRequest { Offset = nextOffset };

        public static PageRequest NextFromIdAsc(IEnumerable<long> idSelector) => new PageRequest { FromId = (idSelector.Max() + 1).ToString() };
        public static PageRequest NextFromIdDesc(IEnumerable<long> idSelector) => new PageRequest { FromId = (idSelector.Min() - 1).ToString() };
        public static PageRequest NextStartTimeAsc(IEnumerable<DateTime> timestampSelector) => new PageRequest { StartTime = timestampSelector.Max().AddMilliseconds(1) };
        public static PageRequest NextEndTimeDesc(IEnumerable<DateTime> timestampSelector) => new PageRequest { EndTime = timestampSelector.Min().AddMilliseconds(-1) };
    }
    
    public static class Pagination
    {
        public static PaginationParameters GetPaginationParameters(
            DataDirection direction,
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
                        startTime = endTime.Add(-maxPeriod.Value);
                    else
                        endTime = startTime.Value.Add(maxPeriod.Value);
                }
                else
                {
                    startTime = endTime.Add(-maxPeriod.Value);
                }
            }

            return new PaginationParameters
            {
                StartTime = direction == DataDirection.Ascending || setOtherTimeLimiter ? startTime : null,
                EndTime = direction == DataDirection.Descending || setOtherTimeLimiter ? endTime : null,
                Direction = direction,
                FromId = paginationRequest?.FromId,
                Offset = paginationRequest?.Offset,
                Page = paginationRequest?.Page
            };
        }

        public static PageRequest? GetNextPageRequest(
            Func<PageRequest> nextPageRequest,
            int resultCount,
            IEnumerable<DateTime> timestamps,
            DateTime? requestStartTime,
            DateTime requestEndTime,
            int limit,
            DataDirection direction,
            PaginationParameters lastPaginationData,
            TimeSpan? maxTimespan = null
            )
        {
            if (HasNextPage(resultCount, timestamps, requestStartTime, requestEndTime, limit, direction))
                return nextPageRequest();

            if (maxTimespan != null)
            {
                if (HasNextPeriod(requestStartTime, requestEndTime, direction, lastPaginationData, maxTimespan.Value))
                {
                    var (startTime, endTime) = GetNextPeriod(requestStartTime, requestEndTime, direction, lastPaginationData, maxTimespan.Value);
                    return new PageRequest
                    {
                        StartTime = startTime,
                        EndTime = endTime
                    };
                }
            }

            return null;
        }

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

            if (!timestamps.Any()) //?
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

        public static PageRequest NextPageFromPage(PaginationParameters lastPaginationData)
        {
            return new PageRequest { Page = lastPaginationData.Page + 1 };
        }
        public static PageRequest NextPageFromOffset(PaginationParameters lastPaginationData, int resultCount)
        {
            return new PageRequest { Offset = lastPaginationData.Offset + resultCount };
        }
        public static PageRequest NextPageFromCursor(string nextCursor)
        {
            return new PageRequest { Cursor = nextCursor };
        }
        public static PageRequest NextPageFromId(long nextFromId)
        {
            return new PageRequest { FromId = nextFromId.ToString() };
        }
        public static PageRequest NextPageFromId(string nextFromId)
        {
            return new PageRequest { FromId = nextFromId };
        }
        public static PageRequest NextPageFromTime(PaginationParameters lastPaginationData, DateTime lastTimestamp, bool setOtherTimeLimiter)
        {
            if (lastPaginationData.Direction == DataDirection.Ascending)
                return new PageRequest { StartTime = lastTimestamp.AddMilliseconds(1), EndTime = setOtherTimeLimiter ? lastPaginationData.EndTime : null  };
            else
                return new PageRequest { EndTime = lastTimestamp.AddMilliseconds(-1), StartTime = setOtherTimeLimiter ? lastPaginationData.StartTime : null };
        }

        public static bool HasNextPeriod(
            DateTime? requestStartTime,
            DateTime requestEndTime,
            DataDirection direction,
            PaginationParameters lastPaginationParameters,
            TimeSpan period)
        {
            if (direction == DataDirection.Ascending && lastPaginationParameters.StartTime == null)
                throw new Exception();

            if (direction == DataDirection.Ascending)
            {
                return (requestEndTime - lastPaginationParameters.EndTime!.Value).TotalSeconds > 1;
            }
            else
            {
                if (requestStartTime != null)
                    return (lastPaginationParameters.StartTime!.Value - requestStartTime.Value).TotalSeconds > 1;
                else
                    return (lastPaginationParameters.StartTime!.Value - (lastPaginationParameters.EndTime!.Value - period)).TotalSeconds > 1;
            }
        }

        public static (DateTime? startTime, DateTime? endTime) GetNextPeriod(
            DateTime? requestStartTime,
            DateTime requestEndTime,
            DataDirection direction,
            PaginationParameters lastPaginationParameters,
            TimeSpan period
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

            if (nextEndTime != null && nextEndTime > requestEndTime)
                nextEndTime = requestEndTime;

            return (nextStartTime, nextEndTime);
        }
    }
}
