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
        Rwa
    }

    public enum SharedAssetSubType
    {
        // Crypto sub types
        StableCoin,

        // Rwa sub types
        Stock,
        Commodity,
    }
}
