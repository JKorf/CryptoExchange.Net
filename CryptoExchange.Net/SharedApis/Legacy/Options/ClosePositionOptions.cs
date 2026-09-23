using CryptoExchange.Net.Objects;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for closing position
    /// </summary>
#pragma warning disable CS0618 // Type or member is obsolete
    public class ClosePositionOptions : EndpointOptions
#pragma warning restore CS0618 // Type or member is obsolete
    {
        private static PropertyInfo[] _requestProperties = typeof(ClosePositionRequest).GetProperties();

        /// <inheritdoc />
        public override string Description => "Close an open futures position";

        /// <inheritdoc />
        public override Type CapabilityType => typeof(IFuturesOrderRestClient);

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<ClosePositionRequest>.Required(x => x.Symbol, "The symbol of the position to close", new SharedSymbol(TradingMode.PerpetualLinear, "ETH", "USDT")),
            RequestParameterRule<ClosePositionRequest>.Required(x => x.PositionMode, "The current position mode of the account", SharedPositionMode.OneWay),
            RequestParameterRule<ClosePositionRequest>.Optional(x => x.PositionSide, "The side of the position to close", SharedPositionSide.Long),
            RequestParameterRule<ClosePositionRequest>.Optional(x => x.MarginMode, "The margin mode of the position", SharedMarginMode.Cross),
            RequestParameterRule<ClosePositionRequest>.Optional(x => x.Quantity, "The quantity of the position to close", 1m),
        };

        /// <summary>
        /// ctor
        /// </summary>
        public ClosePositionOptions(string exchange, bool authenticated) : base(exchange, nameof(IFuturesOrderRestClient.ClosePositionAsync), true, _defaultParameterRules, SharedTradingModeSets.Futures)
        {
        }

        /// <summary>
        /// Validate request
        /// </summary>
        public Error? ValidateRequest(ClosePositionRequest request, IFuturesOrderRestClient client)
        {
            if (NeedsAuthentication && !client.Authenticated)
                return new NoApiCredentialsError();

            foreach (var param in RequestParameterRules)
            {
                var property = _requestProperties.Single(x => x.Name == param.Name);
                var value = property.GetValue(request);

                if (param.Support == RequestParameterSupport.Required)
                {
                    if (value == null)
                    {
                        return ArgumentError.Invalid(
                            param.Name,
                            $"Request parameter `{param.Name}` for exchange `{Exchange}` is required and should be provided. Example: {param.ExampleValue}");
                    }
                }
            }

            if (request is SharedSymbolRequest symbolsRequest)
            {
                if (symbolsRequest.Symbols != null)
                {
                    if (!SupportsMultipleSymbols)
                        return ArgumentError.Invalid(nameof(SharedSymbolRequest.Symbols), $"Only a single symbol parameter is allowed, multiple symbols are not supported");

                    if (symbolsRequest.Symbols.Length > MaxSymbolCount)
                        return ArgumentError.Invalid(nameof(SharedSymbolRequest.Symbols), $"Max number of symbols is {MaxSymbolCount} but {symbolsRequest.Symbols.Length} were passed");
                }

            }

            return ValidateRequest(request.ExchangeParameters, request.TradingMode, client.SupportedTradingModes);
        }
    }
}
