using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace CryptoExchange.Net.Clients
{
    /// <summary>
    /// Base crypto client
    /// </summary>
    public class CryptoBaseClient : IDisposable
    {
        private Dictionary<Type, object> _serviceCache = new Dictionary<Type, object>();

        /// <summary>
        /// Service provider
        /// </summary>
        protected readonly IServiceProvider? _serviceProvider;

        /// <summary>
        /// ctor
        /// </summary>
        public CryptoBaseClient() { }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="serviceProvider"></param>
        public CryptoBaseClient(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _serviceCache = new Dictionary<Type, object>();
        }

        /// <summary>
        /// Try get a client by type for the service collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T TryGet<T>(Func<T> createFunc)
        {
            var type = typeof(T);
            if (_serviceCache.TryGetValue(type, out var value))
                return (T)value;

            if (_serviceProvider == null)
            {
                // Create with default options
                var createResult = createFunc();
                _serviceCache.Add(typeof(T), createResult!);
                return createResult;
            }

            var result = _serviceProvider.GetService<T>() 
                ?? throw new InvalidOperationException($"No service was found for {typeof(T).Name}, make sure the exchange is registered in dependency injection with the `services.Add[Exchange]()` method");
            _serviceCache.Add(type, result!);
            return result;
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            _serviceCache.Clear();
        }
    }
}
