﻿using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Enums;
using CryptoExchange.Net.SharedApis.Models.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.FilterOptions
{

    public record GetOrderBookOptions
    {
        public IEnumerable<int>? SupportedLimits { get; set; }
        public int? MinLimit { get; set; }
        public int? MaxLimit { get; set; }

        public GetOrderBookOptions(int minLimit, int maxLimit)
        {
            MinLimit = minLimit;
            MaxLimit = maxLimit;
        }

        public GetOrderBookOptions(IEnumerable<int> supportedLimits)
        {
            SupportedLimits = supportedLimits;
        }

        public Error? Validate(GetOrderBookRequest request)
        {
            if (request.Limit == null)
                return null;

            if (MaxLimit.HasValue && request.Limit.Value > MaxLimit)
                return new ArgumentError($"Max limit is {MaxLimit}");

            if (MinLimit.HasValue && request.Limit.Value < MinLimit)
                return new ArgumentError($"Min limit is {MaxLimit}");

            if (SupportedLimits != null && !SupportedLimits.Contains(request.Limit.Value))
                return new ArgumentError($"Limit should be one of " + string.Join(", ", SupportedLimits));

            return null;
        }
    }
}