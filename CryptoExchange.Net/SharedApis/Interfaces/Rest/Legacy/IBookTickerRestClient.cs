using CryptoExchange.Net.Objects;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for retrieving the current best bid/ask price
    /// </summary>
    public interface IBookTickerRestClient : IGetBookTickerRestClient
    {   
    }
}
