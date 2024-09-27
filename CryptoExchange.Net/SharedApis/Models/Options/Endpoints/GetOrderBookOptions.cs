using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting order book
    /// </summary>
    public class GetOrderBookOptions : EndpointOptions<GetOrderBookRequest>
    {
        /// <summary>
        /// Supported order book depths
        /// </summary>
        public IEnumerable<int>? SupportedLimits { get; set; }

        /// <summary>
        /// The min order book depth
        /// </summary>
        public int? MinLimit { get; set; }
        /// <summary>
        /// The max order book depth
        /// </summary>
        public int? MaxLimit { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public GetOrderBookOptions(int minLimit, int maxLimit, bool authenticated) : base(authenticated)
        {
            MinLimit = minLimit;
            MaxLimit = maxLimit;
        }

        /// <summary>
        /// ctor
        /// </summary>
        public GetOrderBookOptions(IEnumerable<int> supportedLimits, bool authenticated) : base(authenticated)
        {
            SupportedLimits = supportedLimits;
        }

        /// <inheritdoc />
        public override Error? ValidateRequest(string exchange, GetOrderBookRequest request, TradingMode? tradingMode, TradingMode[] supportedApiTypes)
        {
            if (request.Limit == null)
                return null;

            if (MaxLimit.HasValue && request.Limit.Value > MaxLimit)
                return new ArgumentError($"Max limit is {MaxLimit}");

            if (MinLimit.HasValue && request.Limit.Value < MinLimit)
                return new ArgumentError($"Min limit is {MaxLimit}");

            if (SupportedLimits != null && !SupportedLimits.Contains(request.Limit.Value))
                return new ArgumentError($"Limit should be one of " + string.Join(", ", SupportedLimits));

            return base.ValidateRequest(exchange, request, tradingMode, supportedApiTypes);
        }

        /// <inheritdoc />
        public override string ToString(string exchange)
        {
            var sb = new StringBuilder(base.ToString(exchange));
            sb.AppendLine($"Supported limit values: [{(SupportedLimits == null ? string.Join(", ", SupportedLimits) : $"{MinLimit}..{MaxLimit}")}]");
            return sb.ToString();
        }
    }
}
