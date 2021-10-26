namespace CryptoExchange.Net.ExchangeInterfaces
{
    /// <summary>
    /// Common symbol
    /// </summary>
    public interface ICommonSymbol
    {
        /// <summary>
        /// Symbol name
        /// </summary>
        public string CommonName { get; }
        /// <summary>
        /// Minimum trade quantity
        /// </summary>
        public decimal CommonMinimumTradeQuantity { get; }
    }
}
