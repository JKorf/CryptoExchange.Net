using System;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// A token which a request can use to retrieve the next page if there are more pages in the result set
    /// </summary>
    public interface INextPageToken
    {
    }

    /// <summary>
    /// A datetime offset token
    /// </summary>
    public record DateTimeToken: INextPageToken
    {
        /// <summary>
        /// Last result time
        /// </summary>
        public DateTime LastTime { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public DateTimeToken(DateTime timestamp)
        {
            LastTime = timestamp;
        }
    }

    /// <summary>
    /// A current page index token
    /// </summary>
    public record PageToken: INextPageToken
    {
        /// <summary>
        /// The next page index
        /// </summary>
        public int Page { get; set; }
        /// <summary>
        /// Page size
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public PageToken(int page, int pageSize)
        {
            Page = page;
            PageSize = pageSize;
        }
    }

    /// <summary>
    /// A id offset token
    /// </summary>
    public record FromIdToken : INextPageToken
    {
        /// <summary>
        /// The last id from previous result
        /// </summary>
        public string FromToken { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public FromIdToken(string fromToken)
        {
            FromToken = fromToken;
        }
    }

    /// <summary>
    /// A cursor token
    /// </summary>
    public record CursorToken : INextPageToken
    {
        /// <summary>
        /// The next page cursor
        /// </summary>
        public string Cursor { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public CursorToken(string cursor)
        {
            Cursor = cursor;
        }
    }

    /// <summary>
    /// A result offset token
    /// </summary>
    public record OffsetToken : INextPageToken
    {
        /// <summary>
        /// Offset in the result set
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public OffsetToken(int offset)
        {
            Offset = offset;
        }
    }
}
