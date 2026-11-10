using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Trackers.UserData.Objects;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Trackers.Sessions.Interfaces
{
    public interface IFuturesUserDataUpdateHandler
    {
        Task HandleFuturesBalancesUpdateAsync(
            string userIdentifier,
            TradingMode tradingMode,
            UserDataUpdate<SharedBalance[]> update);

        Task HandleFuturesOrdersUpdateAsync(
            string userIdentifier,
            TradingMode tradingMode,
            UserDataUpdate<SharedFuturesOrder[]> update);

        Task HandleFuturesTradesUpdateAsync(
            string userIdentifier,
            TradingMode tradingMode,
            UserDataUpdate<SharedUserTrade[]> update);

        Task HandleFuturesPositionsUpdateAsync(
            string userIdentifier,
            TradingMode tradingMode,
            UserDataUpdate<SharedPosition[]> update);
    }
}
