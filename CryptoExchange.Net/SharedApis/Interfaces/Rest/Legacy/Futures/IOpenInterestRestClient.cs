using CryptoExchange.Net.Objects;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for getting the open interest for a symbol
    /// </summary>
    public interface IOpenInterestRestClient : IGetOpenInterestRestClient
    {
    }
}
