using CryptoExchange.Net.Objects;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for managing the position mode setting
    /// </summary>
    public interface IPositionModeRestClient :
        IGetPositionModeRestClient,
        ISetPositionModeRestClient
    {
    }
}
