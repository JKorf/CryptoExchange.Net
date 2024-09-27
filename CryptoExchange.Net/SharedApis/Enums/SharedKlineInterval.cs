namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Kline interval
    /// </summary>
    public enum SharedKlineInterval
    {
        /// <summary>
        /// 5 min
        /// </summary>
        FiveMinutes = 60 * 5,
        /// <summary>
        /// 15 min
        /// </summary>
        FifteenMinutes = 60 * 15,
        /// <summary>
        /// 1 hour
        /// </summary>
        OneHour = 60 * 60,
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
