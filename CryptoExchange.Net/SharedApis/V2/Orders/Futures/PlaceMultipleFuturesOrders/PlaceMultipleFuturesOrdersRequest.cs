using System.Collections.Generic;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to place multiple new futures orders
    /// </summary>
    public record PlaceMultipleFuturesOrdersRequest : SharedRequest
    {
        /// <summary>
        /// Orders to place
        /// </summary>
        public PlaceFuturesOrderRequest[] Orders { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="orders">Orders to place</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public PlaceMultipleFuturesOrdersRequest(
            IEnumerable<PlaceFuturesOrderRequest> orders,
            ExchangeParameters? exchangeParameters = null) : base(null, exchangeParameters)
        {
            Orders = orders.ToArray();
            TradingMode = Orders.FirstOrDefault()?.TradingMode;
        }
    }
}
