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

    public record SubscribeKlineOptions : SubscriptionOptions<SubscribeKlineRequest>
    {
        public IEnumerable<SharedKlineInterval> SupportIntervals { get; }

        public SubscribeKlineOptions(bool needsAuthentication) : base(needsAuthentication)
        {
            SupportIntervals = new[]
            {
                SharedKlineInterval.FiveMinutes,
                SharedKlineInterval.FifteenMinutes,
                SharedKlineInterval.OneHour,
                SharedKlineInterval.FifteenMinutes,
                SharedKlineInterval.OneDay,
                SharedKlineInterval.OneWeek,
                SharedKlineInterval.OneMonth
            };
        }

        public SubscribeKlineOptions(bool needsAuthentication, params SharedKlineInterval[] intervals) : base(needsAuthentication)
        {
            SupportIntervals = intervals;
        }

        public bool IsSupported(SharedKlineInterval interval) => SupportIntervals.Contains(interval);

        public override Error? ValidateRequest(string exchange, SubscribeKlineRequest request, ExchangeParameters? exchangeParameter, ApiType apiType, ApiType[] supportedApiTypes)
        {
            if (!IsSupported(request.Interval))
                return new ArgumentError("Interval not supported");

            return base.ValidateRequest(exchange, request, exchangeParameter, apiType, supportedApiTypes);
        }
    }
}
