namespace CryptoExchange.Net.ExchangeInterfaces
{
    /// <summary>
    /// Common balance
    /// </summary>
    public interface ICommonBalance
    {
        /// <summary>
        /// The asset name
        /// </summary>
        public string CommonAsset { get; }
        /// <summary>
        /// Quantity available
        /// </summary>
        public decimal CommonAvailable { get; }
        /// <summary>
        /// Total quantity
        /// </summary>
        public decimal CommonTotal { get; }
    }
}
