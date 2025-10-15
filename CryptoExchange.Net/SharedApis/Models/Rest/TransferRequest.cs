namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to transfer funds between account types
    /// </summary>
    public record TransferRequest : SharedRequest
    {
        /// <summary>
        /// Asset
        /// </summary>
        public string Asset { get; set; }
        /// <summary>
        /// Quantity
        /// </summary>
        public decimal Quantity { get; set; }
        /// <summary>
        /// From symbol
        /// </summary>
        public string? FromSymbol { get; set; }
        /// <summary>
        /// To symbol
        /// </summary>
        public string? ToSymbol { get; set; }

        /// <summary>
        /// From account type
        /// </summary>
        public SharedAccountType FromAccountType { get; set; }
        /// <summary>
        /// To account type
        /// </summary>
        public SharedAccountType ToAccountType { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="asset">The asset to transfer</param>
        /// <param name="quantity">Quantity to transfer</param>
        /// <param name="fromAccount">From account type</param>
        /// <param name="toAccount">To account type</param>
        /// <param name="fromSymbol">From symbol</param>
        /// <param name="toSymbol">To symbol</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public TransferRequest(
            string asset,
            decimal quantity,
            SharedAccountType fromAccount,
            SharedAccountType toAccount,
            string? fromSymbol = null,
            string? toSymbol = null,
            ExchangeParameters? exchangeParameters = null) : base(exchangeParameters)
        {
            Asset = asset;
            Quantity = quantity;
            FromAccountType = fromAccount;
            ToAccountType = toAccount;
            FromSymbol = fromSymbol;
            ToSymbol = toSymbol;
        }
    }
}
