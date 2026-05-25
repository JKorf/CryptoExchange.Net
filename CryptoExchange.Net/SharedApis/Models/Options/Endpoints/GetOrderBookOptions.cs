using CryptoExchange.Net.Objects;
using System;
using System.Linq;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting order book
    /// </summary>
    public class GetOrderBookOptions : EndpointOptions<GetOrderBookRequest, IOrderBookRestClient>
    {
        /// <summary>
        /// Supported order book depths
        /// </summary>
        public int[]? SupportedLimits { get; set; }

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
        public GetOrderBookOptions(string exchange, int minLimit, int maxLimit, bool authenticated) : base(exchange, authenticated)
        {
            MinLimit = minLimit;
            MaxLimit = maxLimit;
        }

        /// <summary>
        /// ctor
        /// </summary>
        public GetOrderBookOptions(string exchange, int[] supportedLimits, bool authenticated) : base(exchange, authenticated)
        {
            SupportedLimits = supportedLimits;
        }

        /// <inheritdoc />
        public override Error? ValidateRequest(GetOrderBookRequest request, IOrderBookRestClient client)
        {
            if (request.Limit == null)
                return base.ValidateRequest(request, client);

            if (MaxLimit.HasValue && request.Limit.Value > MaxLimit)
                return ArgumentError.Invalid(nameof(GetOrderBookRequest.Limit), $"Max limit is {MaxLimit}");

            if (MinLimit.HasValue && request.Limit.Value < MinLimit)
                return ArgumentError.Invalid(nameof(GetOrderBookRequest.Limit), $"Min limit is {MinLimit}");

            if (SupportedLimits != null && !SupportedLimits.Contains(request.Limit.Value))
                return ArgumentError.Invalid(nameof(GetOrderBookRequest.Limit), $"Limit should be one of " + string.Join(", ", SupportedLimits));

            return base.ValidateRequest(request, client);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringBuilder(base.ToString());
            sb.AppendLine($"Supported limit values: [{(SupportedLimits != null ? string.Join(", ", SupportedLimits) : $"{MinLimit}..{MaxLimit}")}]");
            return sb.ToString();
        }
    }
}
