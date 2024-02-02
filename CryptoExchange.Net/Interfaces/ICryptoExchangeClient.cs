using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Interfaces.CommonClients;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.Clients
{
    public interface ICryptoExchangeClient
    {
        IEnumerable<ISpotClient> GetSpotClients();
        T? TryGet<T>();
    }
}
