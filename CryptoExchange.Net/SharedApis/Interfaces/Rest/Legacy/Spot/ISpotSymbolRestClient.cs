using CryptoExchange.Net.Objects;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for requesting spot symbols
    /// </summary>
    public interface ISpotSymbolRestClient : IGetSpotSymbolsRestClient
    {
    }
}
