using CryptoExchange.Net.Objects;
using System;
#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#endif
using System.Collections.Generic;

namespace CryptoExchange.Net.Sockets.Default.Routing
{
    internal class RoutingSubTable
    {
#if NET8_0_OR_GREATER
        // Used for mapping a type identifier to the routes matching it
        private FrozenDictionary<string, RoutingSubTableEntry> _routeMap;
#else
        private Dictionary<string, RoutingSubTableEntry> _routeMap;
#endif

        public RoutingSubTable(IEnumerable<MessageRoute> routes)
        {
            var newMap = new Dictionary<string, RoutingSubTableEntry>();
            foreach (var route in routes)
            {
                if (!newMap.TryGetValue(route.TypeIdentifier, out var typeMap))
                {
                    typeMap = new RoutingSubTableEntry(route.DeserializationType);
                    newMap.Add(route.TypeIdentifier, typeMap);
                }

                typeMap.AddRoute(route.TopicFilter, route);
            }

            foreach(var subEntry in newMap.Values)
                subEntry.Build();

#if NET8_0_OR_GREATER
            _routeMap = newMap.ToFrozenDictionary();
#else
            _routeMap = newMap;
#endif
        }

        /// <summary>
        /// Get routes matching the type identifier
        /// </summary>
        public RoutingSubTableEntry? this[string identifier]
        {
            get => _routeMap.TryGetValue(identifier, out var routes) ? routes : null;
        }
    }

    internal record RoutingSubTableEntry
    {
        public Type DeserializationType { get; }
        public bool MultipleReaders { get; private set; }

        private List<MessageRoute> _routesWithoutTopicFilter;
        private Dictionary<string, List<MessageRoute>> _routesWithTopicFilter;
#if NET8_0_OR_GREATER
        private FrozenDictionary<string, List<MessageRoute>>? _routesWithTopicFilterFrozen;
#endif

        public RoutingSubTableEntry(Type routeType)
        {
            _routesWithoutTopicFilter = new List<MessageRoute>();
            _routesWithTopicFilter = new Dictionary<string, List<MessageRoute>>();

            DeserializationType = routeType;
        }

        public void AddRoute(string? topicFilter, MessageRoute route)
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

            if (route.MultipleReaders)
                MultipleReaders = true;
        }

        public void Build()
        {
#if NET8_0_OR_GREATER
            _routesWithTopicFilterFrozen = _routesWithTopicFilter.ToFrozenDictionary();
#endif
        }

        internal bool Handle(string? topicFilter, SocketConnection connection, DateTime receiveTime, string? originalData, object data, out CallResult? result)
        {
            result = null;

            // Routes without topic filter handle both when the message topic is empty and when it is not, so we always call them
            var handled = false;
            foreach (var route in _routesWithoutTopicFilter)
            {
                var thisResult = route.Handle(connection, receiveTime, originalData, data);
                if (thisResult != null)
                    result ??= thisResult;

                handled = true;
            }

            // Forward to routes with matching topic filter, if any
            if (topicFilter != null)
            {
#if NET8_0_OR_GREATER
                _routesWithTopicFilterFrozen!.TryGetValue(topicFilter, out var matchingTopicRoutes);
#else
                _routesWithTopicFilter.TryGetValue(topicFilter, out var matchingTopicRoutes);
#endif
                foreach (var route in matchingTopicRoutes ?? [])
                {
                    var thisResult = route.Handle(connection, receiveTime, originalData, data);
                    handled = true;

                    if (thisResult != null)
                    {
                        result ??= thisResult;

#warning MultipleReaders is only for queries, subscriptions should always have multiple readers = true. Maybe create different RoutingSubTable implementations for Queries and Subscriptions?
                        if (!MultipleReaders)
                            break;
                    }

                }
            }

            return handled;
        }
    }
}
