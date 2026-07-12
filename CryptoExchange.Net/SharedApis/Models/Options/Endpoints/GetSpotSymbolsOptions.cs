using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting symbol info
    /// </summary>
    public class GetSpotSymbolsOptions : EndpointOptions<GetSymbolsRequest, ISpotSymbolRestClient>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public GetSpotSymbolsOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(ISpotSymbolRestClient.GetSpotSymbolsAsync))
        {
        }


        /// <inheritdoc />
        public override Error? ValidateRequest(GetSymbolsRequest request, ISpotSymbolRestClient client)
        {
            //if (request.SymbolType != null && request.SymbolSubType != null)
            //{
            //    if (request.SymbolType == SymbolAssetType.Crypto
            //        && (request.SymbolSubType == SymbolAssetSubType.Commodity
            //        || (request.SymbolSubType == SymbolAssetSubType.Stock)))
            //    {
            //        return ArgumentError.Invalid(nameof(GetSymbolsRequest.SymbolType), $"Invalid combination of symbol type filters: {request.SymbolType} and {request.SymbolSubType}");
            //    }

            //    if (request.SymbolType == SymbolAssetType.Rwa && request.SymbolSubType == SymbolAssetSubType.StableCoin)
            //        return ArgumentError.Invalid(nameof(GetSymbolsRequest.SymbolType), $"Invalid combination of symbol type filters: {request.SymbolType} and {request.SymbolSubType}");

            //    if (request.SymbolType == SymbolAssetType.Fiat && request.SymbolSubType != null)
            //        return ArgumentError.Invalid(nameof(GetSymbolsRequest.SymbolType), $"Invalid combination of symbol type filters: {request.SymbolType} and {request.SymbolSubType}");
            //}

            return base.ValidateRequest(request, client);
        }
    }
}
