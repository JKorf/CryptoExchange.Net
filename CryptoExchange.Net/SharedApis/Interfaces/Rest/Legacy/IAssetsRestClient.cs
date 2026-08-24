using CryptoExchange.Net.Objects;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for requesting asset info
    /// </summary>
    public interface IAssetsRestClient : 
        IGetAssetRestClient,
        IGetAllAssetsRestClient
    {
    }
}
