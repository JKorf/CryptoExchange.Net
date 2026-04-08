using CryptoExchange.Net.Objects;
using System;
#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#endif
using System.Collections.Generic;

namespace CryptoExchange.Net.Sockets.Default.Routing
{
    internal abstract class RouteCollection
    {
        protected List<MessageRoute> _routesWithoutTopicFilter;
        protected Dictionary<string, List<MessageRoute>> _routesWithTopicFilter;
#if NET8_0_OR_GREATER
        protected FrozenDictionary<string, List<MessageRoute>>? _routesWithTopicFilterFrozen;
#endif

        public Type DeserializationType { get; }

        public RouteCollection(Type routeType)
        {
            _routesWithoutTopicFilter = new List<MessageRoute>();
            _routesWithTopicFilter = new Dictionary<string, List<MessageRoute>>();

            DeserializationType = routeType;
        }

        public virtual void AddRoute(string? topicFilter, MessageRoute route)
        {
            if (string.IsNullOrEmpty(topicFilter))
            {
                _routesWithoutTopicFilter.Add(route);
            }
            else
            {
                if (!_routesWithTopicFilter.TryGetValue(topicFilter!, out var list))
                {
                    list = new List<MessageRoute>();
                    _routesWithTopicFilter.Add(topicFilter!, list);
                }

                list.Add(route);
            }
        }

        public void Build()
        {
#if NET8_0_OR_GREATER
            _routesWithTopicFilterFrozen = _routesWithTopicFilter.ToFrozenDictionary();
#endif
        }

        protected List<MessageRoute>? GetRoutesWithMatchingTopicFilter(string topicFilter)
        {
#if NET8_0_OR_GREATER
            _routesWithTopicFilterFrozen!.TryGetValue(topicFilter, out var matchingTopicRoutes);
#else
            _routesWithTopicFilter.TryGetValue(topicFilter, out var matchingTopicRoutes);
#endif
            return matchingTopicRoutes;
        }

        public abstract bool Handle(string? topicFilter, SocketConnection connection, DateTime receiveTime, string? originalData, object data, out CallResult? result);
    }
}
