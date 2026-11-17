using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Testing
{
    public class SharedApiTestConfig
    {
        public SharedSymbol Symbol { get; }
        public string? Asset { get; }

        public SharedApiTestConfig(SharedSymbol symbol, string? asset = null)
        {
            Symbol = symbol;
            Asset = asset;
        }
    }

    public abstract class SharedApiRestIntegrationTest
    {
        public abstract bool ManualRun { get; }

        private Dictionary<ISharedApi, SharedApiTestConfig> _apis;

        public abstract Dictionary<ISharedApi, SharedApiTestConfig> GetApis();

        public abstract Task RunAsync();

        public async Task ExecuteAsync()
        {
            if (!ManualRun)
                // Integration test flag
                return;

            _apis = GetApis();

            var results = new List<Exception>();
            results.AddRange(await TestGetAssetAsync().ConfigureAwait(false));
            results.AddRange(await TestGetAllAssetsAsync().ConfigureAwait(false));
            results.AddRange(await TestGetBookTickerAsync().ConfigureAwait(false));
            results.AddRange(await TestGetFeesAsync().ConfigureAwait(false));
            results.AddRange(await TestGetFundingInfo().ConfigureAwait(false));
            results.AddRange(await TestGetFundingRateHistory().ConfigureAwait(false));
            results.AddRange(await TestGetIndexPrice().ConfigureAwait(false));
            results.AddRange(await TestGetAllIndexPrices().ConfigureAwait(false));
            results.AddRange(await TestGetKlines().ConfigureAwait(false));

            if (results.Count > 0)
                throw new AggregateException(results);
        }

        protected virtual async Task<List<Exception>> TestGetBookTickerAsync()
        {
            var results = new List<Exception>();
            foreach(var api in _apis)
            {
                if (api.Key is not IGetBookTickerRest client || client.GetBookTickerOptions.NeedsAuthentication)
                    continue;

                LogTest(client.GetBookTickerOptions, client);
                var result = await client!.GetBookTickerAsync(new GetBookTickerRequest(api.Value.Symbol)).ConfigureAwait(false);
                if (!result.Success)
                    results.Add(Fail(client.GetBookTickerOptions, client, result));
            }

            return results;
        }

        protected virtual async Task<List<Exception>> TestGetAssetAsync()
        {
            var results = new List<Exception>();
            foreach (var api in _apis)
            {
                if (api.Key is not IGetAsset client || client.GetAssetOptions.NeedsAuthentication || api.Value.Asset == null)
                    continue;

                LogTest(client.GetAssetOptions, client);
                var result = await client!.GetAssetAsync(new GetAssetRequest(api.Value.Asset)).ConfigureAwait(false);
                if (!result.Success)
                    results.Add(Fail(client.GetAssetOptions, client, result));
            }

            return results;
        }

        protected virtual async Task<List<Exception>> TestGetFeesAsync()
        {
            var results = new List<Exception>();
            foreach (var api in _apis)
            {
                if (api.Key is not IGetFeesRest client || client.GetFeeOptions.NeedsAuthentication)
                    continue;

                LogTest(client.GetFeeOptions, client);
                var result = await client!.GetFeesAsync(new GetFeeRequest(api.Value.Symbol)).ConfigureAwait(false);
                if (!result.Success)
                    results.Add(Fail(client.GetFeeOptions, client, result));
            }

            return results;
        }

        protected virtual async Task<List<Exception>> TestGetFundingInfo()
        {
            var results = new List<Exception>();
            foreach (var api in _apis)
            {
                if (api.Key is not IGetFundingInfoRest client 
                    || client.GetFundingInfoOptions.NeedsAuthentication)
                    continue;

                LogTest(client.GetFundingInfoOptions, client);
                var result = await client!.GetFundingInfoAsync(new GetFundingInfoRequest(api.Value.Symbol)).ConfigureAwait(false);
                if (!result.Success)
                    results.Add(Fail(client.GetFundingInfoOptions, client, result));
            }

            return results;
        }

        protected virtual async Task<List<Exception>> TestGetFundingRateHistory()
        {
            var results = new List<Exception>();
            foreach (var api in _apis)
            {
                if (api.Key is not IGetFundingRateHistoryRest client
                    || client.GetFundingRateHistoryOptions.NeedsAuthentication)
                    continue;

                LogTest(client.GetFundingRateHistoryOptions, client);
                var result = await client!.GetFundingRateHistoryAsync(new GetFundingRateHistoryRequest(api.Value.Symbol)).ConfigureAwait(false);
                if (!result.Success)
                    results.Add(Fail(client.GetFundingRateHistoryOptions, client, result));
            }

            return results;
        }

        protected virtual async Task<List<Exception>> TestGetIndexPrice()
        {
            var results = new List<Exception>();
            foreach (var api in _apis)
            {
                if (api.Key is not IGetIndexPriceRest client
                    || client.GetIndexPriceOptions.NeedsAuthentication)
                    continue;

                LogTest(client.GetIndexPriceOptions, client);
                var result = await client!.GetIndexPriceAsync(new GetIndexPriceRequest(api.Value.Symbol)).ConfigureAwait(false);
                if (!result.Success)
                    results.Add(Fail(client.GetIndexPriceOptions, client, result));
            }

            return results;
        }

        protected virtual async Task<List<Exception>> TestGetAllIndexPrices()
        {
            var results = new List<Exception>();
            foreach (var api in _apis)
            {
                if (api.Key is not IGetAllIndexPricesRest client
                    || client.GetAllIndexPricesOptions.NeedsAuthentication)
                    continue;

                LogTest(client.GetAllIndexPricesOptions, client);
                var result = await client!.GetAllIndexPricesAsync(new GetAllIndexPricesRequest()).ConfigureAwait(false);
                if (!result.Success)
                    results.Add(Fail(client.GetAllIndexPricesOptions, client, result));
            }

            return results;
        }

        protected virtual async Task<List<Exception>> TestGetKlines()
        {
            var results = new List<Exception>();
            foreach (var api in _apis)
            {
                if (api.Key is not IGetKlinesRest client
                    || client.GetKlinesOptions.NeedsAuthentication)
                    continue;

                LogTest(client.GetKlinesOptions, client);
                var result = await client!.GetKlinesAsync(new GetKlinesRequest(api.Value.Symbol, SharedKlineInterval.OneHour)).ConfigureAwait(false);
                if (!result.Success)
                    results.Add(Fail(client.GetKlinesOptions, client, result));
            }

            return results;
        }

        protected virtual async Task<List<Exception>> TestGetAllAssetsAsync()
        {
            var results = new List<Exception>();
            foreach (var api in _apis)
            {
                if (api.Key is not IGetAllAssetsRest client || client.GetAllAssetsOptions.NeedsAuthentication)
                    continue;

                LogTest(client.GetAllAssetsOptions, client);
                var result = await client!.GetAllAssetsAsync(new GetAssetsRequest()).ConfigureAwait(false);
                if (!result.Success)
                    results.Add(Fail(client.GetAllAssetsOptions, client, result));
            }

            return results;
        }


        private void LogTest(CapabilityOptions options, ISharedApi api)
        {
            Debug.WriteLine($"Testing {options.OperationName} - [{string.Join(", ", api.SupportedTradingModes)}]");
        }

        private Exception Fail(CapabilityOptions options, ISharedApi api, ICallResult result)
        {
            return new Exception($"{options.OperationName} - [{string.Join(", ", api.SupportedTradingModes)}] failed: {result.Error}");
        }
    }
}
