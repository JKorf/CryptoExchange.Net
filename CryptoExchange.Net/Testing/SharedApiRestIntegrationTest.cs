using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Testing
{
    /// <summary>
    /// Shared API config
    /// </summary>
    public class SharedApiTestConfig
    {
        /// <summary>
        /// Symbol to use
        /// </summary>
        public SharedSymbol Symbol { get; }
        /// <summary>
        /// Asset to use
        /// </summary>
        public string? Asset { get; }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedApiTestConfig(SharedSymbol symbol, string? asset = null)
        {
            Symbol = symbol;
            Asset = asset;
        }
    }

    /// <summary>
    /// Rest integration test
    /// </summary>
    public abstract class SharedApiRestIntegrationTest
    {
        /// <summary>
        /// Whether to run the test in a manual run
        /// </summary>
        protected abstract bool ManualRun { get; }

        /// <summary>
        /// Get config for a client
        /// </summary>
        protected abstract SharedApiTestConfig GetConfig(ISharedRest client, CapabilityOptions options);

        /// <summary>
        /// Get client
        /// </summary>
        protected abstract ISharedApiClientBase GetClient();

        /// <summary>
        /// Kline interval to request
        /// </summary>
        public virtual SharedKlineInterval KlineInterval { get; } = SharedKlineInterval.OneHour;

        /// <summary>
        /// Check if integration tests should be executed
        /// </summary>
        /// <returns></returns>
        protected bool ShouldRun()
        {
            var integrationTests = Environment.GetEnvironmentVariable("INTEGRATION");
            if (!ManualRun && integrationTests != "1")
                return false;

            return true;
        }

        /// <summary>
        /// Run the test, should call <see cref="ExecuteAsync"/>
        /// </summary>
        public abstract Task RunAsync();

        /// <summary>
        /// Execute the test
        /// </summary>
        protected async Task ExecuteAsync()
        {
            if (!ShouldRun())
                return;

            var results = new List<Exception>();

            await TestCapability<IGetAssetRest, SharedAsset>(results, (config, cap) =>
                cap.GetAssetAsync(new GetAssetRequest(config.Asset ?? throw new InvalidOperationException("Asset is not configured")))).ConfigureAwait(false);
            await TestCapability<IGetAllAssetsRest, SharedAsset[]>(results, (config, cap) =>
                cap.GetAllAssetsAsync(new GetAssetsRequest())).ConfigureAwait(false);
            await TestCapability<IGetFeesRest, SharedFee>(results, (config, cap) =>
                cap.GetFeesAsync(new GetFeeRequest(config.Symbol))).ConfigureAwait(false);
            await TestCapability<IGetFundingInfoRest, SharedFundingInfo>(results, (config, cap) =>
                cap.GetFundingInfoAsync(new GetFundingInfoRequest(config.Symbol))).ConfigureAwait(false);
            await TestCapability<IGetIndexPriceRest, SharedIndexPrice>(results, (config, cap) =>
                cap.GetIndexPriceAsync(new GetIndexPriceRequest(config.Symbol))).ConfigureAwait(false);
            await TestCapability<IGetAllIndexPricesRest, SharedIndexPrice[]>(results, (config, cap) =>
                cap.GetAllIndexPricesAsync(new GetAllIndexPricesRequest(cap.GetAllIndexPricesOptions.SupportedTradingModes[0]))).ConfigureAwait(false);
            await TestCapability<IGetKlinesRest, SharedKline[]>(results, (config, cap) =>
                cap.GetKlinesAsync(new GetKlinesRequest(config.Symbol, KlineInterval))).ConfigureAwait(false);
            await TestCapability<IGetMarkPriceKlinesRest, SharedFuturesKline[]>(results, (config, cap) =>
                cap.GetMarkPriceKlinesAsync(new GetKlinesRequest(config.Symbol, KlineInterval))).ConfigureAwait(false);
            await TestCapability<IGetIndexPriceKlinesRest, SharedFuturesKline[]>(results, (config, cap) =>
                cap.GetIndexPriceKlinesAsync(new GetKlinesRequest(config.Symbol, KlineInterval))).ConfigureAwait(false);
            await TestCapability<IGetMarkPriceRest, SharedMarkPrice>(results, (config, cap) =>
                cap.GetMarkPriceAsync(new GetMarkPriceRequest(config.Symbol))).ConfigureAwait(false);
            await TestCapability<IGetAllMarkPricesRest, SharedMarkPrice[]>(results, (config, cap) =>
                cap.GetAllMarkPricesAsync(new GetAllMarkPricesRequest(cap.GetAllMarkPricesOptions.SupportedTradingModes[0]))).ConfigureAwait(false);
            await TestCapability<IGetOpenInterestRest, SharedOpenInterest>(results, (config, cap) =>
                cap.GetOpenInterestAsync(new GetOpenInterestRequest(config.Symbol))).ConfigureAwait(false);
            await TestCapability<IGetBookTickerRest, SharedBookTicker>(results, (config, cap) =>
                cap.GetBookTickerAsync(new GetBookTickerRequest(config.Symbol))).ConfigureAwait(false);
            await TestCapability<IGetOrderBookRest, SharedOrderBook>(results, (config, cap) =>
                cap.GetOrderBookAsync(new GetOrderBookRequest(config.Symbol))).ConfigureAwait(false);
            await TestCapability<IGetSpotSymbolsRest, SharedSpotSymbol[]>(results, (config, cap) =>
                cap.GetSpotSymbolsAsync(new GetSymbolsRequest())).ConfigureAwait(false);
            await TestCapability<IGetFuturesSymbolsRest, SharedFuturesSymbol[]>(results, (config, cap) =>
                cap.GetFuturesSymbolsAsync(new GetSymbolsRequest(cap.GetFuturesSymbolsOptions.SupportedTradingModes[0]))).ConfigureAwait(false);
            await TestCapability<IGetTickerRest, SharedTicker>(results, (config, cap) =>
                cap.GetTickerAsync(new GetTickerRequest(config.Symbol))).ConfigureAwait(false);
            await TestCapability<IGetAllTickersRest, SharedTicker[]>(results, (config, cap) =>
                cap.GetAllTickersAsync(new GetTickersRequest(cap.GetAllTickersOptions.SupportedTradingModes[0]))).ConfigureAwait(false);
            await TestCapability<IGetRecentTradesRest, SharedTrade[]>(results, (config, cap) =>
                cap.GetRecentTradesAsync(new GetRecentTradesRequest(config.Symbol))).ConfigureAwait(false);
            await TestCapability<IGetTradeHistoryRest, SharedTrade[]>(results, (config, cap) =>
                cap.GetTradeHistoryAsync(new GetTradeHistoryRequest(config.Symbol, DateTime.UtcNow.AddHours(-1)))).ConfigureAwait(false);

            if (results.Count > 0)
                throw new AggregateException(results);
        }

        /// <summary>
        /// Test a capability
        /// </summary>
        protected virtual async Task TestCapability<TCapability, TResult>(
            List<Exception> output,
            Func<SharedApiTestConfig, TCapability, Task<HttpResult<TResult>>> method)
            where TCapability : ISharedRest
        {
            var capability = GetClient().GetCapabilities<TCapability>();
            foreach(var capabilityItem in capability)
            {
                if (capabilityItem.Options.NeedsAuthentication)
                    continue;

                Debug.WriteLine($"Testing {capabilityItem.Options.OperationName} - [{string.Join(", ", capabilityItem.Options.SupportedTradingModes)}]");
                var config = GetConfig(capabilityItem.Capability, capabilityItem.Options);
                var result = await method(config, capabilityItem.Capability).ConfigureAwait(false);
                if (!result.Success)
                    output.Add(new Exception($"{capabilityItem.Options.OperationName} - [{string.Join(", ", capabilityItem.Options.SupportedTradingModes)}] failed: {result.Error}"));
                if (result.Data is Array arr && arr.Length == 0)
                    output.Add(new Exception($"{capabilityItem.Options.OperationName} - [{string.Join(", ", capabilityItem.Options.SupportedTradingModes)}] returned empty array"));
            }
        }
    }
}
