﻿using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Enums;
using CryptoExchange.Net.SharedApis.Models;
using CryptoExchange.Net.SharedApis.Models.FilterOptions;
using CryptoExchange.Net.SharedApis.Models.Rest;
using CryptoExchange.Net.SharedApis.ResponseModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis.Interfaces
{
    public interface ISpotOrderRestClient : ISharedClient
    {
        PlaceSpotOrderOptions PlaceSpotOrderOptions { get; }

        Task<ExchangeWebResult<SharedId>> PlaceOrderAsync(PlaceSpotOrderRequest request, ExchangeParameters? exchangeParameters = null, CancellationToken ct = default);
        Task<ExchangeWebResult<SharedSpotOrder>> GetOrderAsync(GetOrderRequest request, ExchangeParameters? exchangeParameters = null, CancellationToken ct = default);
        Task<ExchangeWebResult<IEnumerable<SharedSpotOrder>>> GetOpenOrdersAsync(GetSpotOpenOrdersRequest request, ExchangeParameters? exchangeParameters = null, CancellationToken ct = default);
        Task<ExchangeWebResult<IEnumerable<SharedSpotOrder>>> GetClosedOrdersAsync(GetSpotClosedOrdersRequest request, INextPageToken? nextPageToken = null, ExchangeParameters? exchangeParameters = null, CancellationToken ct = default);
        Task<ExchangeWebResult<IEnumerable<SharedUserTrade>>> GetOrderTradesAsync(GetOrderTradesRequest request, ExchangeParameters? exchangeParameters = null, CancellationToken ct = default);
        Task<ExchangeWebResult<IEnumerable<SharedUserTrade>>> GetUserTradesAsync(GetUserTradesRequest request, INextPageToken? nextPageToken = null, ExchangeParameters? exchangeParameters = null, CancellationToken ct = default);
        Task<ExchangeWebResult<SharedId>> CancelOrderAsync(CancelOrderRequest request, ExchangeParameters? exchangeParameters = null, CancellationToken ct = default);

    }
}