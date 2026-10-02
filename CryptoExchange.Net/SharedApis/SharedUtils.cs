using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Shared interfaces utilities
    /// </summary>
    public static class SharedUtils
    {
        /// <summary>
        /// Get client information including supported features
        /// </summary>
        public static SharedClientInfo GetClientInfo(PlatformInfo platformInfo, ISharedApi client)
        {
            return new SharedClientInfo
            {
                Exchange = client.Exchange,
                TypeName = client.GetType().Name,
                SupportedEnvironments = platformInfo.SupportedEnvironments,
                SupportedTradingModes = client.SupportedTradingModes,
                CentralizationType = platformInfo.CentralizationType,
                Transport = client.Transport,
                Authenticated = client.Authenticated,
                Capabilities = client.Capabilities.Where(x => x.Supported).ToArray()
            };
        }

        /// <summary>
        /// Apply symbols request filter for asset type and trading mode
        /// </summary>
        public static T[] ApplySymbolFilter<T>(T[] symbols, GetSymbolsRequest request) where T : SharedSpotSymbol
        {
            IEnumerable<T> resultData = symbols;
            if (request.TradingMode != null)
                resultData = resultData.Where(x => x.TradingMode == request.TradingMode);
            if (request.BaseAssetType != null)
                resultData = resultData.Where(x => x.BaseAssetType == request.BaseAssetType);
            if (request.QuoteAssetType != null)
                resultData = resultData.Where(x => x.QuoteAssetType == request.QuoteAssetType);
            if (request.BaseAssetSubType != null)
                resultData = resultData.Where(x => x.BaseAssetSubType == request.BaseAssetSubType);
            if (request.QuoteAssetSubType != null)
                resultData = resultData.Where(x => x.QuoteAssetSubType == request.QuoteAssetSubType);
            return resultData.ToArray();
        }

        /// <summary>
        /// Register Shared API client in DI container
        /// </summary>
        public static IServiceCollection RegisterSharedApiClient<
            TSharedApiClient,
#if NET5_0_OR_GREATER
    [DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicConstructors)]
#endif
        TImplementation
            >(this IServiceCollection services, Action<SharedApiClientRegistrationBuilder<TSharedApiClient>> configure)
                where TImplementation : class, TSharedApiClient
                where TSharedApiClient : class, ISharedApiClientBase
        {
            services.AddTransient<TSharedApiClient, TImplementation>();
            services.AddTransient<ISharedApiClientBase>(
                serviceProvider =>
                    serviceProvider.GetRequiredService<TSharedApiClient>());

            var builder =
                new SharedApiClientRegistrationBuilder<TSharedApiClient>(services);

            configure(builder);
            builder.RegisterTransportAgnosticCapabilities();

            return services;
        }

        /// <summary>
        /// Execute GetAssetAsync for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<IExchangeCallResult<SharedAsset>> ExecuteAllAsync(
            this IEnumerable<SharedCapabilityResolution<IGetAsset>> capabilities,
            GetAssetRequest request,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.GetAssetAsync(request, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Execute GetAssetAsync for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<HttpResult<SharedAsset>> ExecuteAllAsync(
            this IEnumerable<SharedCapabilityResolution<IGetAssetRest>> capabilities,
            GetAssetRequest request,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.GetAssetAsync(request, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Execute GetAllAssetsAsync for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<IExchangeCallResult<SharedAsset[]>> ExecuteAllAsync(
            this IEnumerable<SharedCapabilityResolution<IGetAllAssets>> capabilities,
            GetAssetsRequest request,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.GetAllAssetsAsync(request, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Execute GetAllAssetsAsync for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<HttpResult<SharedAsset[]>> ExecuteAllAsync(
            this IEnumerable<SharedCapabilityResolution<IGetAllAssetsRest>> capabilities,
            GetAssetsRequest request,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.GetAllAssetsAsync(request, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Execute GetBalancesAsync for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<HttpResult<SharedBalance[]>> ExecuteAllAsync(
            this IEnumerable<SharedCapabilityResolution<IGetBalancesRest>> capabilities,
            GetBalancesRequest request,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.GetBalancesAsync(request, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Execute GetBalancesAsync for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<IExchangeCallResult<SharedBalance[]>> ExecuteAllAsync(
            this IEnumerable<SharedCapabilityResolution<IGetBalances>> capabilities,
            GetBalancesRequest request,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.GetBalancesAsync(request, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Execute GetFeesAsync for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<HttpResult<SharedFee>> ExecuteAllAsync(
            this IEnumerable<SharedCapabilityResolution<IGetFeesRest>> capabilities,
            GetFeeRequest request,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.GetFeesAsync(request, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Execute GetFeesAsync for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<IExchangeCallResult<SharedFee>> ExecuteAllAsync(
            this IEnumerable<SharedCapabilityResolution<IGetFees>> capabilities,
            GetFeeRequest request,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.GetFeesAsync(request, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Execute GetFundingInfoAsync for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<HttpResult<SharedFundingInfo>> ExecuteAllAsync(
            this IEnumerable<SharedCapabilityResolution<IGetFundingInfoRest>> capabilities,
            GetFundingInfoRequest request,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.GetFundingInfoAsync(request, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Execute GetFundingInfoAsync for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<IExchangeCallResult<SharedFundingInfo>> ExecuteAllAsync(
            this IEnumerable<SharedCapabilityResolution<IGetFundingInfo>> capabilities,
            GetFundingInfoRequest request,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.GetFundingInfoAsync(request, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Execute GetIndexPriceAsync for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<HttpResult<SharedIndexPrice>> ExecuteAllAsync(
            this IEnumerable<SharedCapabilityResolution<IGetIndexPriceRest>> capabilities,
            GetIndexPriceRequest request,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.GetIndexPriceAsync(request, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Execute GetIndexPriceAsync for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<IExchangeCallResult<SharedIndexPrice>> ExecuteAllAsync(
            this IEnumerable<SharedCapabilityResolution<IGetIndexPrice>> capabilities,
            GetIndexPriceRequest request,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.GetIndexPriceAsync(request, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Execute GetMarkPriceAsync for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<HttpResult<SharedMarkPrice>> ExecuteAllAsync(
            this IEnumerable<SharedCapabilityResolution<IGetMarkPriceRest>> capabilities,
            GetMarkPriceRequest request,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.GetMarkPriceAsync(request, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Execute GetMarkPriceAsync for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<IExchangeCallResult<SharedMarkPrice>> ExecuteAllAsync(
            this IEnumerable<SharedCapabilityResolution<IGetMarkPrice>> capabilities,
            GetMarkPriceRequest request,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.GetMarkPriceAsync(request, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Execute GetAllMarkPricesAsync for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<HttpResult<SharedMarkPrice[]>> ExecuteAllAsync(
            this IEnumerable<SharedCapabilityResolution<IGetAllMarkPricesRest>> capabilities,
            GetAllMarkPricesRequest request,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.GetAllMarkPricesAsync(request, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Execute GetAllMarkPricesAsync for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<IExchangeCallResult<SharedMarkPrice[]>> ExecuteAllAsync(
            this IEnumerable<SharedCapabilityResolution<IGetAllMarkPrices>> capabilities,
            GetAllMarkPricesRequest request,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.GetAllMarkPricesAsync(request, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Execute GetAllIndexPricesAsync for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<HttpResult<SharedIndexPrice[]>> ExecuteAllAsync(
            this IEnumerable<SharedCapabilityResolution<IGetAllIndexPricesRest>> capabilities,
            GetAllIndexPricesRequest request,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.GetAllIndexPricesAsync(request, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Execute GetAllIndexPricesAsync for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<IExchangeCallResult<SharedIndexPrice[]>> ExecuteAllAsync(
            this IEnumerable<SharedCapabilityResolution<IGetAllIndexPrices>> capabilities,
            GetAllIndexPricesRequest request,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.GetAllIndexPricesAsync(request, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Execute GetLeverageAsync for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<HttpResult<SharedLeverage>> ExecuteAllAsync(
            this IEnumerable<SharedCapabilityResolution<IGetLeverageRest>> capabilities,
            GetLeverageRequest request,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.GetLeverageAsync(request, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Execute GetLeverageAsync for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<IExchangeCallResult<SharedLeverage>> ExecuteAllAsync(
            this IEnumerable<SharedCapabilityResolution<IGetLeverage>> capabilities,
            GetLeverageRequest request,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.GetLeverageAsync(request, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Execute GetLeverageTiersAsync for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<HttpResult<SharedLeverageTier[]>> ExecuteAllAsync(
            this IEnumerable<SharedCapabilityResolution<IGetLeverageTiersRest>> capabilities,
            GetLeverageTiersRequest request,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.GetLeverageTiersAsync(request, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Execute GetLeverageTiersAsync for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<IExchangeCallResult<SharedLeverageTier[]>> ExecuteAllAsync(
            this IEnumerable<SharedCapabilityResolution<IGetLeverageTiers>> capabilities,
            GetLeverageTiersRequest request,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.GetLeverageTiersAsync(request, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Execute GetOpenInterestAsync for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<HttpResult<SharedOpenInterest>> ExecuteAllAsync(
            this IEnumerable<SharedCapabilityResolution<IGetOpenInterestRest>> capabilities,
            GetOpenInterestRequest request,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.GetOpenInterestAsync(request, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Execute GetOpenInterestAsync for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<IExchangeCallResult<SharedOpenInterest>> ExecuteAllAsync(
            this IEnumerable<SharedCapabilityResolution<IGetOpenInterest>> capabilities,
            GetOpenInterestRequest request,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.GetOpenInterestAsync(request, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Execute GetBookTickerAsync for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<HttpResult<SharedBookTicker>> ExecuteAllAsync(
            this IEnumerable<SharedCapabilityResolution<IGetBookTickerRest>> capabilities,
            GetBookTickerRequest request,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.GetBookTickerAsync(request, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Execute GetBookTickerAsync for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<IExchangeCallResult<SharedBookTicker>> ExecuteAllAsync(
            this IEnumerable<SharedCapabilityResolution<IGetBookTicker>> capabilities,
            GetBookTickerRequest request,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.GetBookTickerAsync(request, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Execute GetOrderBookAsync for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<HttpResult<SharedOrderBook>> ExecuteAllAsync(
            this IEnumerable<SharedCapabilityResolution<IGetOrderBookRest>> capabilities,
            GetOrderBookRequest request,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.GetOrderBookAsync(request, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Execute GetOrderBookAsync for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<IExchangeCallResult<SharedOrderBook>> ExecuteAllAsync(
            this IEnumerable<SharedCapabilityResolution<IGetOrderBook>> capabilities,
            GetOrderBookRequest request,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.GetOrderBookAsync(request, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Execute GetPositionModeAsync for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<HttpResult<SharedPositionModeResult>> ExecuteAllAsync(
            this IEnumerable<SharedCapabilityResolution<IGetPositionModeRest>> capabilities,
            GetPositionModeRequest request,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.GetPositionModeAsync(request, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Execute GetPositionModeAsync for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<IExchangeCallResult<SharedPositionModeResult>> ExecuteAllAsync(
            this IEnumerable<SharedCapabilityResolution<IGetPositionMode>> capabilities,
            GetPositionModeRequest request,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.GetPositionModeAsync(request, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Execute GetPositionsAsync for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<HttpResult<SharedPosition[]>> ExecuteAllAsync(
            this IEnumerable<SharedCapabilityResolution<IGetPositionsRest>> capabilities,
            GetPositionsRequest request,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.GetPositionsAsync(request, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Execute GetPositionsAsync for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<IExchangeCallResult<SharedPosition[]>> ExecuteAllAsync(
            this IEnumerable<SharedCapabilityResolution<IGetPositions>> capabilities,
            GetPositionsRequest request,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.GetPositionsAsync(request, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Execute GetSpotSymbolsAsync for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<HttpResult<SharedSpotSymbol[]>> ExecuteAllAsync(
            this IEnumerable<SharedCapabilityResolution<IGetSpotSymbolsRest>> capabilities,
            GetSymbolsRequest request,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.GetSpotSymbolsAsync(request, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Execute GetSpotSymbolsAsync for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<IExchangeCallResult<SharedSpotSymbol[]>> ExecuteAllAsync(
            this IEnumerable<SharedCapabilityResolution<IGetSpotSymbols>> capabilities,
            GetSymbolsRequest request,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.GetSpotSymbolsAsync(request, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Execute GetFuturesSymbolsAsync for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<HttpResult<SharedFuturesSymbol[]>> ExecuteAllAsync(
            this IEnumerable<SharedCapabilityResolution<IGetFuturesSymbolsRest>> capabilities,
            GetSymbolsRequest request,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.GetFuturesSymbolsAsync(request, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Execute GetFuturesSymbolsAsync for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<IExchangeCallResult<SharedFuturesSymbol[]>> ExecuteAllAsync(
            this IEnumerable<SharedCapabilityResolution<IGetFuturesSymbols>> capabilities,
            GetSymbolsRequest request,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.GetFuturesSymbolsAsync(request, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Execute GetTickerAsync for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<HttpResult<SharedTicker>> ExecuteAllAsync(
            this IEnumerable<SharedCapabilityResolution<IGetTickerRest>> capabilities,
            GetTickerRequest request,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.GetTickerAsync(request, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Execute GetTickerAsync for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<IExchangeCallResult<SharedTicker>> ExecuteAllAsync(
            this IEnumerable<SharedCapabilityResolution<IGetTicker>> capabilities,
            GetTickerRequest request,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.GetTickerAsync(request, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Execute GetAllTickersAsync for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<HttpResult<SharedTicker[]>> ExecuteAllAsync(
            this IEnumerable<SharedCapabilityResolution<IGetAllTickersRest>> capabilities,
            GetTickersRequest request,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.GetAllTickersAsync(request, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Execute GetAllTickersAsync for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<IExchangeCallResult<SharedTicker[]>> ExecuteAllAsync(
            this IEnumerable<SharedCapabilityResolution<IGetAllTickers>> capabilities,
            GetTickersRequest request,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.GetAllTickersAsync(request, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Execute GetRecentTradesAsync for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<HttpResult<SharedTrade[]>> ExecuteAllAsync(
            this IEnumerable<SharedCapabilityResolution<IGetRecentTradesRest>> capabilities,
            GetRecentTradesRequest request,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.GetRecentTradesAsync(request, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Execute GetRecentTradesAsync for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<IExchangeCallResult<SharedTrade[]>> ExecuteAllAsync(
            this IEnumerable<SharedCapabilityResolution<IGetRecentTrades>> capabilities,
            GetRecentTradesRequest request,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.GetRecentTradesAsync(request, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Subscribe to trade updates for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<WebSocketResult<UpdateSubscription>> SubscribeAllAsync(
            this IEnumerable<SharedCapabilityResolution<ISubscribeTradesSocket>> capabilities,
            SubscribeTradeRequest request,
            Action<DataEvent<SharedTrade[]>> onData,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.SubscribeToTradeUpdatesAsync(request, onData, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Subscribe to balance updates for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<WebSocketResult<UpdateSubscription>> SubscribeAllAsync(
            this IEnumerable<SharedCapabilityResolution<ISubscribeBalancesSocket>> capabilities,
            SubscribeBalancesRequest request,
            Action<DataEvent<SharedBalance[]>> onData,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.SubscribeToBalanceUpdatesAsync(request, onData, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Subscribe to index price updates for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<WebSocketResult<UpdateSubscription>> SubscribeAllAsync(
            this IEnumerable<SharedCapabilityResolution<ISubscribeIndexPriceSocket>> capabilities,
            SubscribeIndexPriceRequest request,
            Action<DataEvent<SharedIndexPrice>> onData,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.SubscribeToIndexPriceUpdatesAsync(request, onData, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Subscribe to kline updates for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<WebSocketResult<UpdateSubscription>> SubscribeAllAsync(
            this IEnumerable<SharedCapabilityResolution<ISubscribeKlinesSocket>> capabilities,
            SubscribeKlineRequest request,
            Action<DataEvent<SharedKline>> onData,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.SubscribeToKlineUpdatesAsync(request, onData, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Subscribe to mark price updates for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<WebSocketResult<UpdateSubscription>> SubscribeAllAsync(
            this IEnumerable<SharedCapabilityResolution<ISubscribeMarkPriceSocket>> capabilities,
            SubscribeMarkPriceRequest request,
            Action<DataEvent<SharedMarkPrice>> onData,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.SubscribeToMarkPriceUpdatesAsync(request, onData, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Subscribe to order book updates for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<WebSocketResult<UpdateSubscription>> SubscribeAllAsync(
            this IEnumerable<SharedCapabilityResolution<ISubscribeOrderBookSocket>> capabilities,
            SubscribeOrderBookRequest request,
            Action<DataEvent<SharedOrderBook>> onData,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.SubscribeToOrderBookUpdatesAsync(request, onData, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Subscribe to incremental order book updates for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<WebSocketResult<UpdateSubscription>> SubscribeAllAsync(
            this IEnumerable<SharedCapabilityResolution<ISubscribeIncrementalOrderBookSocket>> capabilities,
            SubscribeOrderBookRequest request,
            Action<DataEvent<SharedIncrementalOrderBook>> onData,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.SubscribeToIncrementalOrderBookUpdatesAsync(request, onData, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Subscribe to futures order updates for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<WebSocketResult<UpdateSubscription>> SubscribeAllAsync(
            this IEnumerable<SharedCapabilityResolution<ISubscribeFuturesOrdersSocket>> capabilities,
            SubscribeFuturesOrderRequest request,
            Action<DataEvent<SharedFuturesOrderUpdate[]>> onData,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.SubscribeToFuturesOrderUpdatesAsync(request, onData, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Subscribe to spot order updates for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<WebSocketResult<UpdateSubscription>> SubscribeAllAsync(
            this IEnumerable<SharedCapabilityResolution<ISubscribeSpotOrdersSocket>> capabilities,
            SubscribeSpotOrderRequest request,
            Action<DataEvent<SharedSpotOrderUpdate[]>> onData,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.SubscribeToSpotOrderUpdatesAsync(request, onData, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Subscribe to position updates for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<WebSocketResult<UpdateSubscription>> SubscribeAllAsync(
            this IEnumerable<SharedCapabilityResolution<ISubscribePositionsSocket>> capabilities,
            SubscribePositionRequest request,
            Action<DataEvent<SharedPosition[]>> onData,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.SubscribeToPositionUpdatesAsync(request, onData, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Subscribe to ticker updates for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<WebSocketResult<UpdateSubscription>> SubscribeAllAsync(
            this IEnumerable<SharedCapabilityResolution<ISubscribeTickerSocket>> capabilities,
            SubscribeTickerRequest request,
            Action<DataEvent<SharedTicker>> onData,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.SubscribeToTickerUpdatesAsync(request, onData, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Subscribe to all ticker updates for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<WebSocketResult<UpdateSubscription>> SubscribeAllAsync(
            this IEnumerable<SharedCapabilityResolution<ISubscribeAllTickersSocket>> capabilities,
            SubscribeAllTickersRequest request,
            Action<DataEvent<SharedTicker[]>> onData,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.SubscribeToAllTickersUpdatesAsync(request, onData, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Subscribe to user trade updates for all capabilities in parallel and return results as they arrive
        /// </summary>
        public static IAsyncEnumerable<WebSocketResult<UpdateSubscription>> SubscribeAllAsync(
            this IEnumerable<SharedCapabilityResolution<ISubscribeUserTradesSocket>> capabilities,
            SubscribeUserTradeRequest request,
            Action<DataEvent<SharedUserTrade[]>> onData,
            CancellationToken ct = default)
        {
            return capabilities
                .Select(x => x.Capability.SubscribeToUserTradeUpdatesAsync(request, onData, ct))
                .ParallelEnumerateAsync();
        }

        /// <summary>
        /// Wait for all results from an IAsyncEnumerable and return them as an array
        /// </summary>
        public static async Task<T[]> WaitAllAsync<T>(this IAsyncEnumerable<T> enumerable)
        {
            var result = new List<T>();
            await foreach (var itemResult in enumerable.ConfigureAwait(false))
                result.Add(itemResult);

            return result.ToArray();
        }
    }
}
