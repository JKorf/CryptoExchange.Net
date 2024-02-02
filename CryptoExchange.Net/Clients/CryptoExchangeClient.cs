using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Interfaces.CommonClients;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptoExchange.Net.Clients
{
    public class CryptoExchangeClient : ICryptoExchangeClient, IDisposable
    {
        private Dictionary<Type, object?> _serviceCache = new Dictionary<Type, object?>();

        private readonly IServiceProvider _serviceProvider;

        public CryptoExchangeClient(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _serviceCache = new Dictionary<Type, object?>();
        }

        public IEnumerable<ISpotClient> GetSpotClients()
        {
            return _serviceProvider.GetServices<ISpotClient>().ToList();
        }

        public T? TryGet<T>()
        {
            var type = typeof(T);
            if (_serviceCache.TryGetValue(type, out var value))
                return (T?)value;

            var result = _serviceProvider.GetService<T>();
            _serviceCache.Add(type, result);
            return result;
        }

        public void Dispose()
        {
            _serviceCache.Clear();
        }
    }
}
