using System.Collections.Generic;
#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#endif

namespace CryptoExchange.Net.Sockets.Default.Routing
{
    internal abstract class ProcessorRouter
    {
        public abstract RouteCollection? GetRoutes(string identifier);
    }

    internal abstract class ProcessorRouter<T> : ProcessorRouter
        where T : RouteCollection
    {
#if NET8_0_OR_GREATER
        private FrozenDictionary<string, T> _routeMap;
#else
        private Dictionary<string, T> _routeMap;
#endif

        public ProcessorRouter(IEnumerable<MessageRoute> routes)
        {
            var map = BuildFromRoutes(routes);
#if NET8_0_OR_GREATER
            _routeMap = map.ToFrozenDictionary();
#else
            _routeMap = map;
#endif
        }

        public abstract Dictionary<string, T> BuildFromRoutes(IEnumerable<MessageRoute> routes);

        public override RouteCollection? GetRoutes(string identifier) => _routeMap.TryGetValue(identifier, out var routes) ? routes : null;
    }    
}
