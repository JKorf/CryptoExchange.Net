using System.Threading.Tasks;
using System.Threading;
using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for managing spot orders using a client order id
    /// </summary>
    public interface ISpotOrderClientIdRestClient : 
        ICancelSpotOrderByClientOrderIdRestClient,
        IGetSpotOrderByClientOrderIdRestClient
    {
    }
}
