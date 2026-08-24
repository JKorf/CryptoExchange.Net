using CryptoExchange.Net.Objects;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for placing and managing futures orders
    /// </summary>
    public interface IFuturesOrderRestClient :
        IPlaceFuturesOrderRestClient,
        IGetFuturesOrderRestClient,
        IGetOpenFuturesOrdersRestClient,
        IGetClosedFuturesOrdersRestClient,
        IGetFuturesOrderTradesRestClient,
        IGetFuturesUserTradesRestClient,
        ICancelFuturesOrderRestClient,
        IGetPositionsRestClient,
        IClosePositionRestClient
    {
    }
}
