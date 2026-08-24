using System.Threading.Tasks;
using System.Threading;
using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Take profit / Stop loss client
    /// </summary>
    public interface IFuturesTpSlRestClient : 
        ISetFuturesTpSlRestClient,
        ICancelFuturesTpSlRestClient
    {
    }
}
