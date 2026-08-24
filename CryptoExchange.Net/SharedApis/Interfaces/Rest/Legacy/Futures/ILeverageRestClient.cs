using CryptoExchange.Net.Objects;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for managing the leverage of a symbol
    /// </summary>
    public interface ILeverageRestClient : 
        IGetLeverageRestClient,
        ISetLeverageRestClient
    {
    }
}
