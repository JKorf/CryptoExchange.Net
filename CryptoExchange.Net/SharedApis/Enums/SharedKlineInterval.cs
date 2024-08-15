using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Enums
{
    public enum SharedKlineInterval
    {
        FiveMinutes = 60 * 5,
        FifteenMinutes = 60 * 15,
        OneHour = 60 * 60,
        OneDay = 60 * 60 * 24,
        OneWeek = 60 * 60 * 24 * 7,
        OneMonth = 60 * 60 * 24 * 30
    }
}
