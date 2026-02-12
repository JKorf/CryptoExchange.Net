using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models
{
    public interface PageRequest
    {

    }

    public record PageParameters
    {
        public string? Cursor { get; set; }
        public string? FromId { get; set; }
        public int? Page { get; set; }
        public int? Offset { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        public SharedPaginationSupport? Direction { get; set; }
    }
}
