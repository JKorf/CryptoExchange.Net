using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Enums;
using CryptoExchange.Net.SharedApis.Models.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.FilterOptions
{

    public record GetOrderBookOptions : EndpointOptions<GetOrderBookRequest>
    {
        public IEnumerable<int>? SupportedLimits { get; set; }
        public int? MinLimit { get; set; }
        public int? MaxLimit { get; set; }

        public GetOrderBookOptions(int minLimit, int maxLimit, bool authenticated) : base(authenticated)
        {
            MinLimit = minLimit;
            MaxLimit = maxLimit;
        }

        public GetOrderBookOptions(IEnumerable<int> supportedLimits, bool authenticated) : base(authenticated)
        {
            SupportedLimits = supportedLimits;
        }

        public override Error? ValidateRequest(string exchange, GetOrderBookRequest request, ApiType? apiType, ApiType[] supportedApiTypes)
        {
            if (request.Limit == null)
                return null;

            if (MaxLimit.HasValue && request.Limit.Value > MaxLimit)
                return new ArgumentError($"Max limit is {MaxLimit}");

            if (MinLimit.HasValue && request.Limit.Value < MinLimit)
                return new ArgumentError($"Min limit is {MaxLimit}");

            if (SupportedLimits != null && !SupportedLimits.Contains(request.Limit.Value))
                return new ArgumentError($"Limit should be one of " + string.Join(", ", SupportedLimits));

            return base.ValidateRequest(exchange, request, apiType, supportedApiTypes);
        }

        public string ToString(string exchange)
        {
            var sb = new StringBuilder(base.ToString(exchange));
            sb.AppendLine($"Supported limit values: [{(SupportedLimits == null ? string.Join(", ", SupportedLimits) : $"{MinLimit}..{MaxLimit}")}]");
            return sb.ToString();
        }
    }
}
