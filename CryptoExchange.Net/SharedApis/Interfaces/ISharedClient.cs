using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Interfaces
{
    public interface ISharedClient
    {
        string Exchange { get; }

        TradingMode[] SupportedApiTypes { get; }

        void SetDefaultExchangeParameter(string key, object value);
        void ResetDefaultExchangeParameters();
    }
}
