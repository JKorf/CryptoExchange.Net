using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Enums
{
    public enum SharedTimeInForce
    {
        GoodTillCanceled,
        ImmediateOrCancel,
        FillOrKill,
        GoodTillDate
    }
}
