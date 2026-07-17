using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    public enum SharedAssetType
    {
        Unspecified,
        Crypto,
        Fiat,
        TradFi
    }

    public enum SharedAssetSubType
    {
        // Crypto sub types
        StableCoin,

        // TradFi sub types
        Equity, // Stock, ETF, Index
        Commodity, // Oil, Gas, Metals 
    }
}
