using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Trackers.UserData.Objects;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Trackers.Sessions.Interfaces
{
    public interface ISpotUserDataUpdateHandler
    {
        Task HandleSpotBalancesUpdateAsync(string userIdentifier, UserDataUpdate<SharedBalance[]> update);
        Task HandleSpotOrdersUpdateAsync(string userIdentifier, UserDataUpdate<SharedSpotOrder[]> update);
        Task HandleSpotTradesUpdateAsync(string userIdentifier, UserDataUpdate<SharedUserTrade[]> update);
    }
}
