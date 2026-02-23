using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Next page request info
    /// </summary>
    public class PageRequest
    {
        /// <summary>
        /// Pagination cursor
        /// </summary>
        public string? Cursor { get; set; }
        /// <summary>
        /// Page number
        /// </summary>
        public int? Page { get; set; }
        /// <summary>
        /// Result offset
        /// </summary>
        public int? Offset { get; set; }
        /// <summary>
        /// From id filter
        /// </summary>
        public string? FromId { get; set; }
        /// <summary>
        /// Start time filter
        /// </summary>
        public DateTime? StartTime { get; set; }
        /// <summary>
        /// End time filter
        /// </summary>
        public DateTime? EndTime { get; set; }
    }
}
