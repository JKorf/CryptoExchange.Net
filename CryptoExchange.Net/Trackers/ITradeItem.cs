using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.Trackers
{
    public interface ITradeItem
    {
        string Id { get; }
        decimal Quantity { get; }
        decimal Price { get; }
        DateTime Timestamp { get; }
    }
}
    