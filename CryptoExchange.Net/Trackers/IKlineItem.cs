using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.Trackers
{
    public interface IKlineItem
    {
        DateTime OpenTime { get; }
        decimal OpenPrice { get; }
        decimal HighPrice { get; }
        decimal LowPrice { get; }
        decimal ClosePrice { get; }
        decimal Volume { get; }
    }
}
    