using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Trackers.UserData.Objects;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Trackers.UserData.ItemTrackers
{
    /// <summary>
    /// Balance tracker implementation
    /// </summary>
    public class BalanceTracker : UserDataItemTracker<SharedBalance>
    {
        private readonly IBalanceRestClient _restClient;
        private readonly IBalanceSocketClient? _socketClient;
        private readonly ExchangeParameters? _exchangeParameters;
        private readonly SharedAccountType _accountType;

        /// <summary>
        /// ctor
        /// </summary>
        public BalanceTracker(
            ILogger logger,
            IBalanceRestClient restClient,
            IBalanceSocketClient? socketClient,
            SharedAccountType accountType,
            TrackerItemConfig config,
            ExchangeParameters? exchangeParameters = null
            ) : base(logger, UserDataType.Balances, config, false, null)
        {
            _restClient = restClient;
            _socketClient = socketClient;
            _exchangeParameters = exchangeParameters;
            _accountType = accountType;
        }

        /// <inheritdoc />
        protected override bool Update(SharedBalance existingItem, SharedBalance updateItem)
        {
            var changed = false;
            if (existingItem.Total != updateItem.Total)
            {
                existingItem.Total = updateItem.Total;
                changed = true;
            }

            if (existingItem.Available != updateItem.Available)
            {
                existingItem.Available = updateItem.Available;
                changed = true;
            }

            return changed;
        }

        /// <inheritdoc />
        protected override string GetKey(SharedBalance item) => item.Asset + item.IsolatedMarginSymbol;

        /// <inheritdoc />
        protected override bool? CheckIfUpdateShouldBeApplied(SharedBalance existingItem, SharedBalance updateItem) => true;

        /// <inheritdoc />
        protected override Task<CallResult<UpdateSubscription?>> DoSubscribeAsync(string? listenKey)
        {
            if (_socketClient == null)
                return Task.FromResult(new CallResult<UpdateSubscription?>(data: null));

            var accountType = _accountType == SharedAccountType.Spot ? TradingMode.Spot :
                              _accountType == SharedAccountType.PerpetualInverseFutures ? TradingMode.PerpetualInverse :
                              _accountType == SharedAccountType.DeliveryLinearFutures ? TradingMode.DeliveryLinear :
                              _accountType == SharedAccountType.DeliveryInverseFutures ? TradingMode.DeliveryInverse :
                              TradingMode.PerpetualLinear;
            return ExchangeHelpers.ProcessQueuedAsync<SharedBalance[]>(
                async handler => await _socketClient.SubscribeToBalanceUpdatesAsync(new SubscribeBalancesRequest(listenKey, accountType, exchangeParameters: _exchangeParameters), handler, ct: _cts!.Token).ConfigureAwait(false),
                x => HandleUpdateAsync(UpdateSource.Push, x.Data))!;
        }

        /// <inheritdoc />
        protected override async Task<bool> DoPollAsync()
        {
            var balances = await _restClient.GetBalancesAsync(new GetBalancesRequest(accountType: _accountType, exchangeParameters: _exchangeParameters)).ConfigureAwait(false);
            if (balances.Success)
                await HandleUpdateAsync(UpdateSource.Poll, balances.Data).ConfigureAwait(false);

            return balances.Success;
        }
    }
}
