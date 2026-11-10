using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.SharedApis;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.Trackers.Sessions.Interfaces
{
    public interface ITickerUpdateHandler
    {
        void HandleTickerUpdate(SharedSymbol symbol, DataEvent<SharedTicker> update);
    }
}
