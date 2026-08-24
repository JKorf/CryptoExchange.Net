using System.Threading.Tasks;
using System.Threading;
using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for managing futures orders using a client order id
    /// </summary>
    public interface IFuturesOrderClientIdRestClient : 
        IGetFuturesOrderByClientOrderIdRestClient,
        ICancelFuturesOrderByClientOrderIdRestClient
    {        
    }
}
