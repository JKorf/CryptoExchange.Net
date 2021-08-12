using System;

namespace CryptoExchange.Net.ExchangeInterfaces
{
    /// <summary>
    /// Common trade
    /// </summary>
    public interface ICommonTrade
    {
        /// <summary>
        /// Id of the trade
        /// </summary>
        public string CommonId { get; }
        /// <summary>
        /// Price of the trade
        /// </summary>
        public decimal CommonPrice { get; }
        /// <summary>
        /// Quantity of the trade
        /// </summary>
        public decimal CommonQuantity { get; }
        /// <summary>
        /// Fee paid for the trade
        /// </summary>
        public decimal CommonFee { get; }
        /// <summary>
        /// The asset fee was paid in
        /// </summary>
        public string? CommonFeeAsset { get; }
        /// <summary>
        /// Trade time
        /// </summary>
        DateTime CommonTradeTime { get; }
    }
}
