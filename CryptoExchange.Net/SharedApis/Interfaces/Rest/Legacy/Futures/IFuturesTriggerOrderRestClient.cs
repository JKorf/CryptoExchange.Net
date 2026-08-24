using CryptoExchange.Net.Objects;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for placing trigger orders
    /// </summary>
    public interface IFuturesTriggerOrderRestClient :
        IPlaceFuturesTriggerOrderRestClient,
        IGetFuturesTriggerOrderRestClient,
        ICancelFuturesTriggerOrderRestClient
    {
    }
}
