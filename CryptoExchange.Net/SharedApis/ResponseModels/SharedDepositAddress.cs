namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Deposit address info
    /// </summary>
    public record SharedDepositAddress
    {
        /// <summary>
        /// Asset the address is for
        /// </summary>
        public string Asset { get; set; }
        /// <summary>
        /// Deposit address
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// The network
        /// </summary>
        public string? Network { get; set; }
        /// <summary>
        /// Tag or memo
        /// </summary>
        public string? TagOrMemo { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedDepositAddress(string asset, string address)
        {
            Asset = asset;
            Address = address;
        }
    }

}
