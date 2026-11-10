using CryptoExchange.Net.SharedApis;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.Trackers.Sessions.Features
{
    public static partial class RuntimeSymbolTrackingExtensions
    {
        private static NotSupportedException MissingCapability(
            ISharedApiClientBase client,
            SharedSymbol symbol,
            string feature)
        {
            return new NotSupportedException(
                $"{client.Exchange} does not support {feature} " +
                $"for {symbol.TradingMode}.");
        }
    }
}
