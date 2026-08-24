using CryptoExchange.Net.Objects;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for request funding rate records
    /// </summary>
    public interface IFundingRateRestClient : IGetFundingRateHistoryRestClient
    {
    }
}
