using System.Collections.Generic;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to place multiple new spot orders
    /// </summary>
    public record PlaceMultipleSpotOrdersRequest : SharedRequest
    {
        /// <summary>
        /// Orders to place
        /// </summary>
        public PlaceSpotOrderRequest[] Orders { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="orders">Orders to place</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public PlaceMultipleSpotOrdersRequest(
            IEnumerable<PlaceSpotOrderRequest> orders,
            ExchangeParameters? exchangeParameters = null) : base(null, exchangeParameters)
        {
            Orders = orders.ToArray();
            TradingMode = Orders.FirstOrDefault()?.TradingMode;
        }
    }
}
