using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects.Sockets;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Sockets
{
    internal class SocketListenerManager
    {
        private ILogger _logger;
        private object _lock = new object();
        private Dictionary<string, Type> _typeMap;
        private Dictionary<string, List<IMessageProcessor>> _listeners;

        public SocketListenerManager(ILogger logger)
        {
            _typeMap = new Dictionary<string, Type>();
            _logger = logger;
        }

        public Dictionary<string, Type> GetMapping()
        {
            lock (this)
                return _typeMap;
        }

        public void Add(IMessageProcessor processor)
        {
            lock (_lock)
            {
                foreach (var identifier in processor.Identifiers)
                {
                    if (!_listeners.TryGetValue(identifier, out var list))
                    {
                        list = new List<IMessageProcessor>();
                        _listeners.Add(identifier, list);
                    }

                    list.Add(processor);
                }

                UpdateMap();
            }
        }

        public async Task<bool> InvokeListenersAsync(string id, BaseParsedMessage data)
        {
            List<IMessageProcessor> listeners;
            lock (_lock)
            {
                if (!_listeners.TryGetValue(id, out var idListeners))
                    return false;

                listeners = idListeners.ToList();
            }

            foreach (var listener in listeners)
            {
                //_logger.Log(LogLevel.Trace, $"Socket {SocketId} Message mapped to processor {messageProcessor.Id} with identifier {result.Identifier}");
                if (listener is BaseQuery query)
                {
                    Remove(listener);

                    if (query.PendingRequest != null)
                        _pendingRequests.Remove(query.PendingRequest);

                    if (query.PendingRequest?.Completed == true)
                    {
                        // Answer to a timed out request
                        //_logger.Log(LogLevel.Warning, $"Socket {SocketId} Received after request timeout. Consider increasing the RequestTimeout");
                    }
                }

                // Matched based on identifier
                var userSw = Stopwatch.StartNew();
                var dataEvent = new DataEvent<BaseParsedMessage>(data, null, data.OriginalData, DateTime.UtcNow, null);
                await listener.HandleMessageAsync(dataEvent).ConfigureAwait(false);
                userSw.Stop();
            }

            return true;
        }

        public List<Subscription> GetSubscriptions()
        {
            lock (_lock)
                return _listeners.Values.SelectMany(v => v.OfType<Subscription>()).ToList();
        }

        public List<BaseQuery> GetQueries()
        {
            lock (_lock)
                return _listeners.Values.SelectMany(v => v.OfType<BaseQuery>()).ToList();
        }

        public bool Contains(IMessageProcessor processor)
        {
            lock (_lock)
                return _listeners.Any(l => l.Value.Contains(processor));
        }

        public bool Remove(IMessageProcessor processor)
        {
            lock (_lock)
            {
                var removed = false;
                foreach (var identifier in processor.Identifiers)
                {
                    if (_listeners[identifier].Remove(processor))
                        removed = true;

                    if (!_listeners[identifier].Any())
                        _listeners.Remove(identifier);
                }

                UpdateMap();
                return removed;
            }
        }

        private void UpdateMap()
        {
            _typeMap = _listeners.ToDictionary(x => x.Key, x => x.Value.First().ExpectedMessageType);
        }
    }
}
