namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Trading fee info
    /// </summary>
    public record SharedFee
    {
        /// <summary>
        /// Taker fee percentage
        /// </summary>
        public decimal TakerFee { get; set; }
        /// <summary>
        /// Maker fee percentage
        /// </summary>
        public decimal MakerFee { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedFee(decimal makerFee, decimal takerFee)
        {
            MakerFee = makerFee;
            TakerFee = takerFee;
        }
    }
}
