using System;

namespace CryptoExchange.Net.ExchangeInterfaces
{
    /// <summary>
    /// Common order
    /// </summary>
    public interface ICommonOrder: ICommonOrderId
    {
        /// <summary>
        /// Symbol of the order
        /// </summary>
        public string CommonSymbol { get; }
        /// <summary>
        /// Price of the order
        /// </summary>
        public decimal CommonPrice { get; }
        /// <summary>
        /// Quantity of the order
        /// </summary>
        public decimal CommonQuantity { get; }
        /// <summary>
        /// Status of the order
        /// </summary>
        public IExchangeClient.OrderStatus CommonStatus { get; }
        /// <summary>
        /// Whether the order is active
        /// </summary>
        public bool IsActive { get; }
        /// <summary>
        /// Side of the order
        /// </summary>
        public IExchangeClient.OrderSide CommonSide { get; }
        /// <summary>
        /// Type of the order
        /// </summary>
        public IExchangeClient.OrderType CommonType { get; }
        /// <summary>
        /// order time
        /// </summary>
        DateTime CommonOrderTime { get; }
    }
}
