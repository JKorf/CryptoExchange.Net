using CryptoExchange.Net.Sockets.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#endif

namespace CryptoExchange.Net.Sockets.Default.Routing
{
    /// <summary>
    /// Routing table
    /// </summary>
    public class RoutingTable
    {
#if NET8_0_OR_GREATER
        private FrozenDictionary<string, TypeRoutingCollection> _typeRoutingCollections = new Dictionary<string, TypeRoutingCollection>().ToFrozenDictionary();
#else
        private Dictionary<string, TypeRoutingCollection> _typeRoutingCollections = new();
#endif

        /// <summary>
        /// Update the routing table
        /// </summary>
        /// <param name="processors"></param>
        public void Update(IEnumerable<IMessageProcessor> processors)
        {
            var newTypeMap = new Dictionary<string, TypeRoutingCollection>();
            foreach (var entry in processors)
            {
                foreach (var route in entry.MessageRouter.Routes)
                {
                    if (!newTypeMap.ContainsKey(route.TypeIdentifier))
                        newTypeMap.Add(route.TypeIdentifier, new TypeRoutingCollection(route.DeserializationType));

                    if (!newTypeMap[route.TypeIdentifier].Handlers.Contains(entry))
                        newTypeMap[route.TypeIdentifier].Handlers.Add(entry);
                }
            }

#if NET8_0_OR_GREATER
            _typeRoutingCollections = newTypeMap.ToFrozenDictionary();
#else
            _typeRoutingCollections = newTypeMap;
#endif
        }

        /// <summary>
        /// Get route table entry for a type identifier
        /// </summary>
        public TypeRoutingCollection? GetRouteTableEntry(string typeIdentifier)
        {
            return _typeRoutingCollections.TryGetValue(typeIdentifier, out var entry) ? entry : null;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var entry in _typeRoutingCollections)
            {
                sb.AppendLine($"{entry.Key}, {entry.Value.DeserializationType.Name}");
                foreach(var item in entry.Value.Handlers)
                {
                    sb.AppendLine($" - Processor {item.GetType().Name}");
                    foreach(var route in item.MessageRouter.Routes)
                    {
                        if (route.TypeIdentifier == entry.Key)
                        {
                            if (route.TopicFilter == null)
                                sb.AppendLine($"   - Route without topic filter");
                            else
                                sb.AppendLine($"   - Route with topic filter {route.TopicFilter}");
                        }
                    }
                }
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// Routing table entry
    /// </summary>
    public record TypeRoutingCollection
    {
        /// <summary>
        /// Whether the deserialization type is string
        /// </summary>
        public bool IsStringOutput { get; set; }
        /// <summary>
        /// The deserialization type
        /// </summary>
        public Type DeserializationType { get; set; }
        /// <summary>
        /// Message processors
        /// </summary>
        public List<IMessageProcessor> Handlers { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public TypeRoutingCollection(Type deserializationType)
        {
            IsStringOutput = deserializationType == typeof(string);
            DeserializationType = deserializationType;
            Handlers = new List<IMessageProcessor>();
        }
    }
}
