using CryptoExchange.Net.Objects;
using System;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for placing a new futures order
    /// </summary>
    public class PlaceFuturesOrderOptions : CapabilityOptions<PlaceFuturesOrderRequest, IPlaceFuturesOrder>
    {
        /// <inheritdoc />
        public override string Description => "Place a new futures order";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<PlaceFuturesOrderRequest>.Required(x => x.Symbol, "The symbol to place the order on", new SharedSymbol(TradingMode.PerpetualLinear, "ETH", "USDT")),
            RequestParameterRule<PlaceFuturesOrderRequest>.Required(x => x.Side, "The order side", SharedOrderSide.Buy),
            RequestParameterRule<PlaceFuturesOrderRequest>.Required(x => x.OrderType, "The order type", SharedOrderType.Limit),
            RequestParameterRule<PlaceFuturesOrderRequest>.Optional(x => x.TimeInForce, "The order time in force", SharedTimeInForce.GoodTillCanceled),
            RequestParameterRule<PlaceFuturesOrderRequest>.Optional(x => x.Quantity, "The order quantity", SharedQuantity.Base(0.1m)),
            RequestParameterRule<PlaceFuturesOrderRequest>.Optional(x => x.Price, "The order price", 1m),
            RequestParameterRule<PlaceFuturesOrderRequest>.Optional(x => x.ClientOrderId, "The client order id", "123"),
            RequestParameterRule<PlaceFuturesOrderRequest>.Optional(x => x.PositionSide, "The position side of the order", SharedPositionSide.Long),
            RequestParameterRule<PlaceFuturesOrderRequest>.Optional(x => x.MarginMode, "The margin mode of the order", SharedMarginMode.Cross),
            RequestParameterRule<PlaceFuturesOrderRequest>.Optional(x => x.ReduceOnly, "Whether the order should only reduce a position", false),
            RequestParameterRule<PlaceFuturesOrderRequest>.Optional(x => x.Leverage, "The leverage for the position", 10m),
            RequestParameterRule<PlaceFuturesOrderRequest>.Optional(x => x.TakeProfitPrice, "The take profit price", 1.1m),
            RequestParameterRule<PlaceFuturesOrderRequest>.Optional(x => x.StopLossPrice, "The stop loss price", 0.9m),
        };

        /// <summary>
        /// Whether or not the API supports setting take profit / stop loss with the order
        /// </summary>
        public bool SupportsTpSl { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public PlaceFuturesOrderOptions(string exchange, bool supportsTpSl) : base(exchange, true, nameof(IPlaceFuturesOrder.PlaceFuturesOrderAsync), _defaultParameterRules)
        {
            SupportsTpSl = supportsTpSl;
        }

        /// <summary>
        /// Validate a request
        /// </summary>
        public override Error? ValidateRequest(
            PlaceFuturesOrderRequest request,
            IPlaceFuturesOrder client
            )
        {
            if (!SupportsTpSl && (request.StopLossPrice != null || request.TakeProfitPrice != null))
                return ArgumentError.Invalid(nameof(PlaceFuturesOrderRequest.StopLossPrice) + " / " + nameof(PlaceFuturesOrderRequest.TakeProfitPrice), "Tp/Sl parameters not supported");

            if (request.OrderType == SharedOrderType.Other)
                throw new ArgumentException("OrderType can't be `Other`", nameof(request.OrderType));

            if (!client.FuturesSupportedOrderTypes.Contains(request.OrderType))
                return ArgumentError.Invalid(nameof(PlaceFuturesOrderRequest.OrderType), "Order type not supported");

            if (request.TimeInForce != null && !client.FuturesSupportedTimeInForce.Contains(request.TimeInForce.Value))
                return ArgumentError.Invalid(nameof(PlaceFuturesOrderRequest.TimeInForce), "Order time in force not supported");

            var quantityError = client.FuturesSupportedOrderQuantity.Validate(request.Side, request.OrderType, request.Quantity);
            if (quantityError != null)
                return quantityError;

            return base.ValidateRequest(request, client);
        }

    }
}
