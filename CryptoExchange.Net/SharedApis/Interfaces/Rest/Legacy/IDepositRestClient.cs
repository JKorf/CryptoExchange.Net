using CryptoExchange.Net.Objects;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for requesting deposit addresses and deposit records
    /// </summary>
    public interface IDepositRestClient : 
        IGetDepositAddressesRestClient, 
        IGetDepositHistoryRestClient
    {
    }
}
