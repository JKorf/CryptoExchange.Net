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
        /// Amount available
        /// </summary>
        public decimal CommonAvailable { get; }
        /// <summary>
        /// Total amount
        /// </summary>
        public decimal CommonTotal { get; }
    }
}
