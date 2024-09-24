namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to withdraw funds from the exchange
    /// </summary>
    public record WithdrawRequest : SharedRequest
    {
        /// <summary>
        /// Asset to withdraw
        /// </summary>
        public string Asset { get; set; }
        /// <summary>
        /// Address to withdraw to
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// Quantity to withdraw
        /// </summary>
        public decimal Quantity { get; set; }
        /// <summary>
        /// Address tag
        /// </summary>
        public string? AddressTag { get; set; }
        /// <summary>
        /// Network to use
        /// </summary>
        public string? Network { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="asset">Asset to withdraw</param>
        /// <param name="quantity">Quantity to withdraw</param>
        /// <param name="address">Address to withdraw to</param>
        /// <param name="network">Network to use</param>
        /// <param name="addressTag">Address tag</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public WithdrawRequest(string asset, decimal quantity, string address, string? network = null, string? addressTag = null, ExchangeParameters? exchangeParameters = null) : base(exchangeParameters)
        {
            Asset = asset;
            Address = address;
            Quantity = quantity;
            Network = network;
            AddressTag = addressTag;
        }
    }
}
