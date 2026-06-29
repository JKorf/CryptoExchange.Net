using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.TokenManagement
{
    internal static class TokenRegistryProvider
    {
        private static readonly object _lock = new object();
        private static readonly Dictionary<string, TokenRegistry> _registries = new Dictionary<string, TokenRegistry>();

        public static TokenRegistry GetRegistry(string registryKey, ILogger logger, TimeSpan maintenanceInterval)
        {
            lock (_lock)
            {
                if (!_registries.TryGetValue(registryKey, out var registry))
                {
                    registry = new TokenRegistry(registryKey, logger, maintenanceInterval);
                    _registries[registryKey] = registry;
                }

                return registry;
            }
        }
    }
}
