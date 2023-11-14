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
        private int _socketId;
        private object _lock = new object();
        private Dictionary<int, IMessageProcessor> _idMap;
        private Dictionary<string, Type> _typeMap;
        private Dictionary<string, List<IMessageProcessor>> _listeners;

        public SocketListenerManager(ILogger logger, int socketId)
        {
            _idMap = new Dictionary<int, IMessageProcessor>();
            _listeners = new Dictionary<string, List<IMessageProcessor>>();
            _typeMap = new Dictionary<string, Type>();
            _logger = logger;
            _socketId = socketId;
        }

        public Dictionary<string, Type> GetMapping()
        {
            lock (this)
                return _typeMap;
        }

        public List<string> GetListenIds()
        {
            lock(_lock)
                return _listeners.Keys.ToList();
        }

        public void Add(IMessageProcessor processor)
        {
            lock (_lock)
            {
                _idMap.Add(processor.Id, processor);
                if (processor.Identifiers?.Any() == true)
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
                _logger.Log(LogLevel.Trace, $"Socket {_socketId} Message mapped to processor {listener.Id} with identifier {data.Identifier}");
                if (listener is BaseQuery query)
                {
                    Remove(listener);
                    if (query?.Completed == true)
                    {
                        // Answer to a timed out request
                        _logger.Log(LogLevel.Warning, $"Socket {_socketId} Received after request timeout. Consider increasing the RequestTimeout");
                    }
                }

                // Matched based on identifier
                var userSw = Stopwatch.StartNew();
                var dataEvent = new DataEvent<BaseParsedMessage>(data, null, data.OriginalData, DateTime.UtcNow, null);
                await listener.HandleMessageAsync(dataEvent).ConfigureAwait(false);
                userSw.Stop();
                if (userSw.ElapsedMilliseconds > 500)
                {
                    _logger.Log(LogLevel.Debug, $"Socket {_socketId} {(listener is Subscription ? "subscription " : "query " + listener!.Id)} message processing slow ({(int)userSw.ElapsedMilliseconds}ms), consider offloading data handling to another thread. " +
                                                    "Data from this socket may arrive late or not at all if message processing is continuously slow.");
                }
            }

            return true;
        }

        public T? GetById<T>(int id) where T : BaseQuery
        {
            lock (_lock)
            {
                _idMap.TryGetValue(id, out var val);
                return (T)val;
            }
        }

        public List<Subscription> GetSubscriptions()
        {
            lock (_lock)
                return _listeners.Values.SelectMany(v => v.OfType<Subscription>()).Distinct().ToList();
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

        public void Remove(IMessageProcessor processor)
        {
            lock (_lock)
            {
                _idMap.Remove(processor.Id);
                if (processor.Identifiers?.Any() == true)
                {
                    foreach (var identifier in processor.Identifiers)
                    {
                        _listeners[identifier].Remove(processor);
                        if (!_listeners[identifier].Any())
                            _listeners.Remove(identifier);
                    }
                }

                UpdateMap();
            }
        }

        private void UpdateMap()
        {
            _typeMap = _listeners.ToDictionary(x => x.Key, x => x.Value.First().ExpectedMessageType);
        }
    }
}
