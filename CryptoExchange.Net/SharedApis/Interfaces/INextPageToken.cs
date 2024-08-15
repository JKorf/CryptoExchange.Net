using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Interfaces
{
    public interface INextPageToken
    {
        public string Exchange { get; set; }
    }

    public record DateTimeToken: INextPageToken
    {
        public string Exchange { get; set; }
        public DateTime LastTime { get; set; }

        public DateTimeToken(DateTime timestamp)
        {
            LastTime = timestamp;
        }
    }

    public record PageToken: INextPageToken
    {
        public string Exchange { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }

        public PageToken(int page, int pageSize)
        {
            Page = page;
            PageSize = pageSize;
        }
    }

    public record FromIdToken : INextPageToken
    {
        public string Exchange { get; set; }
        public string FromToken { get; set; }

        public FromIdToken(string fromToken)
        {
            FromToken = fromToken;
        }
    }

    public record CursorToken : INextPageToken
    {
        public string Exchange { get; set; }
        public string Cursor { get; set; }

        public CursorToken(string cursor)
        {
            Cursor = cursor;
        }
    }

    public record OffsetToken : INextPageToken
    {
        public string Exchange { get; set; }
        public int Offset { get; set; }
        public OffsetToken(int offset)
        {
            Offset = offset;
        }
    }
}
