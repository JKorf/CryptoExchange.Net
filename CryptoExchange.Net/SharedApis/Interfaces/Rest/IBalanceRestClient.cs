﻿using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Models;
using CryptoExchange.Net.SharedApis.RequestModels;
using CryptoExchange.Net.SharedApis.ResponseModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis.Interfaces
{
    public interface IBalanceRestClient : ISharedClient
    {
        Task<ExchangeWebResult<IEnumerable<SharedBalance>>> GetBalancesAsync(ApiType? apiType = default, ExchangeParameters? exchangeParameters = null, CancellationToken ct = default);
    }
}