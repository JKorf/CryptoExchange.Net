using CryptoExchange.Net.Objects;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for retrieving the order book for a symbol
    /// </summary>
    public interface IOrderBookRestClient : IGetOrderBookRestClient
    {   
    }
}
