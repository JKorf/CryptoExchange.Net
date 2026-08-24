using System.Threading.Tasks;
using System.Threading;
using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for requesting user trading fees
    /// </summary>
    public interface IFeeRestClient : IGetFeesRestClient
    {        
    }
}
