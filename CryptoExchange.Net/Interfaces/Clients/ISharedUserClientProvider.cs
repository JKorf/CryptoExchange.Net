using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Interfaces.Clients
{
    public interface ISharedUserClientProvider: IExchangeService
    {
        ISharedApiClientBase GetPublicSharedApiClient(
            TradeEnvironment? environment = null);
        Task<CallResult<ISharedApiClientBase>> TryGetSharedApiClientAsync(
            string userIdentifier, 
            TradeEnvironment? environment = null, 
            CancellationToken ct = default);
    }
}
