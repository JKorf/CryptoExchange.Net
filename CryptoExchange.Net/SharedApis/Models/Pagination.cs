using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{

    public enum PageDirection
    {
        Ascending, // Old to new
        Descending // New to old
    }

    public class PageRequest
    {
        public string? NextCursor { get; set; }
        public int? NextPage { get; set; }
        public int? NextOffset { get; set; }
        public string? NextFromId { get; set; }
        public DateTime? NextStartTime { get; set; }
        public DateTime? NextEndTime { get; set; }
    

        public static PageRequest FromNextCursor(string nextCursor) => new PageRequest { NextCursor = nextCursor };
        public static PageRequest FromNextPage(int nextPage) => new PageRequest { NextPage = nextPage };
        public static PageRequest FromNextOffset(int nextOffset) => new PageRequest { NextOffset = nextOffset };

        public static PageRequest FromNextFromIdAsc(IEnumerable<long> idSelector) => new PageRequest { NextFromId = (idSelector.Max() + 1).ToString() };
        public static PageRequest FromNextFromIdDesc(IEnumerable<long> idSelector) => new PageRequest { NextFromId = (idSelector.Min() - 1).ToString() };
        public static PageRequest FromNextStartTimeAsc(IEnumerable<DateTime> timestampSelector) => new PageRequest { NextStartTime = timestampSelector.Max().AddMilliseconds(1) };
        public static PageRequest FromNextStartTimeDesc(IEnumerable<DateTime> timestampSelector) => new PageRequest { NextStartTime = timestampSelector.Min().AddMilliseconds(-1) };
        public static PageRequest FromNextEndTimeAsc(IEnumerable<DateTime> timestampSelector) => new PageRequest { NextEndTime = timestampSelector.Max().AddMilliseconds(1) };
        public static PageRequest FromNextEndTimeDesc(IEnumerable<DateTime> timestampSelector) => new PageRequest { NextEndTime = timestampSelector.Min().AddMilliseconds(-1) };
    }

}
