namespace CryptoExchange.Net.SharedApis.ResponseModels
{
    /// <summary>
    /// Open interest
    /// </summary>
    public record SharedOpenInterest
    {
        /// <summary>
        /// Current open interest
        /// </summary>
        public decimal OpenInterest { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedOpenInterest(decimal openInterest)
        {
            OpenInterest = openInterest;
        }
    }
}
