using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Enums;
using CryptoExchange.Net.SharedApis.Models.Rest;
using CryptoExchange.Net.SharedApis.Models.Socket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.FilterOptions
{

    public record SubscribeOrderBookOptions : SubscriptionOptions<SubscribeOrderBookRequest>
    {
        public IEnumerable<int> SupportedLimits { get; }

        public SubscribeOrderBookOptions(bool needsAuthentication, IEnumerable<int> limits) : base(needsAuthentication)
        {
            SupportedLimits = limits;
        }

        public override Error? ValidateRequest(string exchange, SubscribeOrderBookRequest request, ApiType? apiType, ApiType[] supportedApiTypes)
        {
            if (request.Limit != null && !SupportedLimits.Contains(request.Limit.Value))
                return new ArgumentError("Limit not supported");

            return base.ValidateRequest(exchange, request, apiType, supportedApiTypes);
        }
    }
}
