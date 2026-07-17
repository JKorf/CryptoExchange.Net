using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting symbol info
    /// </summary>
    public class GetFuturesSymbolsOptions : EndpointOptions<GetSymbolsRequest, IFuturesSymbolRestClient>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public GetFuturesSymbolsOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IFuturesSymbolRestClient.GetFuturesSymbolsAsync))
        {
        }

        /// <inheritdoc />
        public override Error? ValidateRequest(GetSymbolsRequest request, IFuturesSymbolRestClient client)
        {
            if (request.BaseAssetType != null && request.BaseAssetSubType != null)
            {
                var error = ValidateSymbolType(request.BaseAssetType.Value, request.BaseAssetSubType.Value);
                if (error != null)
                    return error;
            }

            if (request.QuoteAssetType != null && request.QuoteAssetSubType != null)
            {
                var error = ValidateSymbolType(request.QuoteAssetType.Value, request.QuoteAssetSubType.Value);
                if (error != null)
                    return error;
            }

            return base.ValidateRequest(request, client);
        }

        private Error? ValidateSymbolType(SharedAssetType type, SharedAssetSubType subType)
        {
            if (type == SharedAssetType.Crypto
                    && (subType == SharedAssetSubType.Commodity
                    || (subType == SharedAssetSubType.Equity)))
            {
                return ArgumentError.Invalid(nameof(GetSymbolsRequest.BaseAssetSubType), $"Invalid combination of symbol type filters: {type} and {subType}");
            }

            if (type == SharedAssetType.TradFi && subType == SharedAssetSubType.StableCoin)
                return ArgumentError.Invalid(nameof(GetSymbolsRequest.BaseAssetSubType), $"Invalid combination of symbol type filters: {type} and {subType}");

            if (type == SharedAssetType.Fiat)
                return ArgumentError.Invalid(nameof(GetSymbolsRequest.BaseAssetSubType), $"Invalid combination of symbol type filters: {type} and {subType}");

            return null;
        }
    }
}
