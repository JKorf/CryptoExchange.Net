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

}
