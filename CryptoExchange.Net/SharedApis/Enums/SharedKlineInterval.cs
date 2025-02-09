namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Kline interval
    /// </summary>
    public enum SharedKlineInterval
    {
        /// <summary>
        /// 1 min
        /// </summary>
        OneMinute = 60,
        /// <summary>
        /// 3 min
        /// </summary>
        ThreeMinutes = 60 * 3,
        /// <summary>
        /// 5 min
        /// </summary>
        FiveMinutes = 60 * 5,
        /// <summary>
        /// 15 min
        /// </summary>
        FifteenMinutes = 60 * 15,
        /// <summary>
        /// Thirty minutes
        /// </summary>
        ThirtyMinutes = 60 * 30,
        /// <summary>
        /// 1 hour
        /// </summary>
        OneHour = 60 * 60,
        /// <summary>
        /// 2 hours
        /// </summary>
        TwoHours = 60 * 60 * 2,
        /// <summary>
        /// 4 hours
        /// </summary>
        FourHours = 60 * 60 * 4,
        /// <summary>
        /// 6 hours
        /// </summary>
        SixHours = 60 * 60 * 6,
        /// <summary>
        /// 8 hours
        /// </summary>
        EightHours = 60 * 60 * 8,
        /// <summary>
        /// 12 hours
        /// </summary>
        TwelveHours = 60 * 60 * 12,
        /// <summary>
        /// 1 day
        /// </summary>
        OneDay = 60 * 60 * 24,
        /// <summary>
        /// 1 week
        /// </summary>
        OneWeek = 60 * 60 * 24 * 7,
        /// <summary>
        /// 1 month
        /// </summary>
        OneMonth = 60 * 60 * 24 * 30
    }
}
