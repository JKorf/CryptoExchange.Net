using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Interfaces.Rest.V2.SpotUserTrades;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for placing and managing spot orders
    /// </summary>
    public interface ISpotOrderRestClient : 
        IPlaceSpotOrderRestClient,
        ICancelSpotOrderRestClient,
        IGetSpotOrderRestClient,
        IGetOpenSpotOrdersRestClient,
        IGetClosedSpotOrdersRestClient,
        IGetSpotOrderTradesRestClient,
        IGetSpotUserTradeHistoryRestClient
    {
    }
}
