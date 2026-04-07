using CryptoExchange.Net.Sockets.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.Sockets.Default.Routing
{
    /// <summary>
    /// Routing table
    /// </summary>
    public class RoutingTable
    {
        private Dictionary<string, RoutingTableEntry> _routeTableEntries;

        /// <summary>
        /// Create routing table for provided processors
        /// </summary>
        public RoutingTable(IEnumerable<IMessageProcessor> processors)
        {
            _routeTableEntries = new Dictionary<string, RoutingTableEntry>();
            foreach (var entry in processors)
            {
                foreach (var route in entry.MessageRouter.Routes)
                {
                    if (!_routeTableEntries.ContainsKey(route.TypeIdentifier))
                        _routeTableEntries.Add(route.TypeIdentifier, new RoutingTableEntry(route.DeserializationType));                    

                    _routeTableEntries[route.TypeIdentifier].Handlers.Add(entry);
                }
            }
        }

        /// <summary>
        /// Get route table entry for a type identifier
        /// </summary>
        public RoutingTableEntry? GetRouteTableEntry(string typeIdentifier)
        {
            return _routeTableEntries.TryGetValue(typeIdentifier, out var entry) ? entry : null;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var entry in _routeTableEntries)
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
    public record RoutingTableEntry
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
        public RoutingTableEntry(Type deserializationType)
        {
            IsStringOutput = deserializationType == typeof(string);
            DeserializationType = deserializationType;
            Handlers = new List<IMessageProcessor>();
        }
    }
}
