using CryptoExchange.Net.Objects;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for retrieving withdrawal records
    /// </summary>
    public interface IWithdrawalRestClient : IGetWithdrawalHistoryRestClient
    {
    }
}
