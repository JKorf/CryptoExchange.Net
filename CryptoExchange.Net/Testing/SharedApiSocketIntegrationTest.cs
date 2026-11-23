using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.SharedApis;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Testing
{
    /// <summary>
    /// Socket integration test
    /// </summary>
    public abstract class SharedApiSocketIntegrationTest
    {
        /// <summary>
        /// Whether to run the test in a manual run
        /// </summary>
        protected abstract bool ManualRun { get; }

        /// <summary>
        /// Get config for a client
        /// </summary>
        protected abstract SharedApiTestConfig GetConfig(ISharedSocket client, CapabilityOptions options);
        /// <summary>
        /// Get client
        /// </summary>
        protected abstract ISharedApiClientBase GetClient();

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
        /// Kline interval to subscribe
        /// </summary>
        public virtual SharedKlineInterval KlineInterval { get; } = SharedKlineInterval.OneHour;

        /// <summary>
        /// Execute the test
        /// </summary>
        protected async Task<List<Exception>> ExecuteAsync()
        {
            if (!ShouldRun())
                return new();

            var results = new List<Exception>();

            await TestCapability<ISubscribeFundingInfoSocket, SharedFundingInfo>(results, (config, cap, handler) =>
                cap.SubscribeToFundingInfoUpdatesAsync(new SubscribeFundingInfoRequest(config.Symbol), handler)).ConfigureAwait(false);
            await TestCapability<ISubscribeIndexPriceSocket, SharedIndexPrice>(results, (config, cap, handler) =>
                cap.SubscribeToIndexPriceUpdatesAsync(new SubscribeIndexPriceRequest(config.Symbol), handler)).ConfigureAwait(false);
            await TestCapability<ISubscribeKlinesSocket, SharedKline>(results, (config, cap, handler) =>
                cap.SubscribeToKlineUpdatesAsync(new SubscribeKlineRequest(config.Symbol, KlineInterval), handler)).ConfigureAwait(false);
            await TestCapability<ISubscribeMarkPriceSocket, SharedMarkPrice>(results, (config, cap, handler) =>
                cap.SubscribeToMarkPriceUpdatesAsync(new SubscribeMarkPriceRequest(config.Symbol), handler)).ConfigureAwait(false);
            await TestCapability<ISubscribeOrderBookSocket, SharedOrderBook>(results, (config, cap, handler) =>
                cap.SubscribeToOrderBookUpdatesAsync(new SubscribeOrderBookRequest(config.Symbol), handler)).ConfigureAwait(false);
            await TestCapability<ISubscribeIncrementalOrderBookSocket, SharedIncrementalOrderBook>(results, (config, cap, handler) =>
                cap.SubscribeToIncrementalOrderBookUpdatesAsync(new SubscribeOrderBookRequest(config.Symbol), handler)).ConfigureAwait(false);
            await TestCapability<ISubscribeTickerSocket, SharedTicker>(results, (config, cap, handler) =>
                cap.SubscribeToTickerUpdatesAsync(new SubscribeTickerRequest(config.Symbol), handler)).ConfigureAwait(false);
            await TestCapability<ISubscribeAllTickersSocket, SharedTicker[]>(results, (config, cap, handler) =>
                cap.SubscribeToAllTickersUpdatesAsync(new SubscribeAllTickersRequest(cap.SubscribeAllTickersOptions.SupportedTradingModes[0]), handler)).ConfigureAwait(false);
            await TestCapability<ISubscribeBookTickerSocket, SharedBookTicker>(results, (config, cap, handler) =>
                cap.SubscribeToBookTickerUpdatesAsync(new SubscribeBookTickerRequest(config.Symbol), handler)).ConfigureAwait(false);
            await TestCapability<ISubscribeTradesSocket, SharedTrade[]>(results, (config, cap, handler) =>
                cap.SubscribeToTradeUpdatesAsync(new SubscribeTradeRequest(config.Symbol), handler)).ConfigureAwait(false);

            var exceptions = results.Where(x => x is not OperationCanceledException).ToList();
            if (exceptions.Count > 0)
                throw new AggregateException(exceptions);

            return results.Except(exceptions).ToList();
        }

        private async Task TestCapability<TCapability, TResult>(
            List<Exception> output,
            Func<SharedApiTestConfig, TCapability, Action<DataEvent<TResult>>, Task<WebSocketResult<UpdateSubscription>>> method)
            where TCapability : ISharedSocket
        {
            var client = GetClient();
            var capability = client.GetCapabilities<TCapability>();
            foreach(var capabilityItem in capability)
            {
                if (capabilityItem.Options.NeedsAuthentication)
                    continue;

                Debug.WriteLine($"Testing {capabilityItem.Options.OperationName} - [{string.Join(", ", capabilityItem.Options.SupportedTradingModes)}]");
                var config = GetConfig(capabilityItem.Capability, capabilityItem.Options);
                DataEvent<TResult>? dataEvent = null;
                var wait = new AsyncResetEvent(false, false);
                var result = await method(config, capabilityItem.Capability, x => { dataEvent = x; wait.Set(); }).ConfigureAwait(false);
                if (!result.Success)
                {
                    output.Add(new Exception($"{capabilityItem.Options.OperationName} - [{string.Join(", ", capabilityItem.Options.SupportedTradingModes)}] failed: {result.Error}"));
                }
                else
                {
                    await wait.WaitAsync(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
                    if (dataEvent == null)
                        output.Add(new OperationCanceledException($"{capabilityItem.Options.OperationName} - [{string.Join(", ", capabilityItem.Options.SupportedTradingModes)}] did not receive any data"));
                }
            }

            await client.UnsubscribeAllAsync().ConfigureAwait(false);
        }
    }
}
