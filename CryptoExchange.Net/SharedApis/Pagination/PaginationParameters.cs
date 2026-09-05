using System;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Pagination parameters
    /// </summary>
    public record PaginationParameters
    {
        /// <summary>
        /// Data direction
        /// </summary>
        public DataDirection Direction { get; set; }
        /// <summary>
        /// Start time filter
        /// </summary>
        public DateTime? StartTime { get; set; }
        /// <summary>
        /// End time filter
        /// </summary>
        public DateTime? EndTime { get; set; }
        /// <summary>
        /// Id filter
        /// </summary>
        public string? FromId { get; set; }
        /// <summary>
        /// Result offset
        /// </summary>
        public int? Offset { get; set; }
        /// <summary>
        /// Page number
        /// </summary>
        public int? Page { get; set; }
        /// <summary>
        /// Pagination cursor
        /// </summary>
        public string? Cursor { get; set; }
        /// <summary>
        /// Max number of results
        /// </summary>
        public int Limit { get; set; }
    }
}
