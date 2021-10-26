namespace CryptoExchange.Net.ExchangeInterfaces
{
    /// <summary>
    /// Common ticker
    /// </summary>
    public interface ICommonTicker
    {
        /// <summary>
        /// Symbol name
        /// </summary>
        public string CommonSymbol { get; }
        /// <summary>
        /// High price
        /// </summary>
        public decimal CommonHighPrice { get; }
        /// <summary>
        /// Low price
        /// </summary>
        public decimal CommonLowPrice { get; }
        /// <summary>
        /// Volume
        /// </summary>
        public decimal CommonVolume { get; }
    }
}
