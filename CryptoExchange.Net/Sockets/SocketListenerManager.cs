using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects.Sockets;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Sockets
{
    public class SocketListenerManager
    {
        private ILogger _logger;
        private int _socketId;
        private object _lock = new object();
        //private Dictionary<int, IMessageProcessor> _idMap;
        //private Dictionary<string, Dictionary<string, Type>> _typeMap;
        private Dictionary<string, List<IMessageProcessor>> _listeners;

        public SocketListenerManager(ILogger logger, int socketId)
        {
            //_idMap = new Dictionary<int, IMessageProcessor>();
            _listeners = new Dictionary<string, List<IMessageProcessor>>();
            //_typeMap = new Dictionary<string, Dictionary<string, Type>>();
            _logger = logger;
            _socketId = socketId;
        }

        public Type? IdToType(string streamIdentifier, string typeIdentifier)
        {
            lock (_lock)
            {
                _listeners.TryGetValue(streamIdentifier, out var listeners);
                if (listeners == null)
                    return null;

                listeners.First().TypeMapping.TryGetValue(typeIdentifier ?? "", out var type);
                return type;
            }
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
                if (processor.StreamIdentifiers?.Any() == true)
                {
                    foreach (var identifier in processor.StreamIdentifiers)
                    {
                        if (!_listeners.TryGetValue(identifier, out var list))
                        {
                            list = new List<IMessageProcessor>();
                            _listeners.Add(identifier, list);
                        }

                        list.Add(processor);
                    }
                }

            }
        }

        public void Reset(IMessageProcessor processor)
        {
            lock (_lock)
            {
                Debug.WriteLine("4 Resetting");
                Remove(processor);
                Add(processor);
            }
        }

        public async Task<bool> InvokeListenersAsync(SocketConnection connection, string id, BaseParsedMessage data)
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
                _logger.Log(LogLevel.Trace, $"Socket {_socketId} Message mapped to processor {listener.Id} with identifier {data.StreamIdentifier}");
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
                try
                {
                    await listener.HandleMessageAsync(connection, dataEvent).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    // TODO
                }

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
                var val = _listeners.Values.SelectMany(x => x).FirstOrDefault(x => x.Id == id);
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
                if (processor.StreamIdentifiers?.Any() != true)
                    return;

                foreach(var kv in _listeners)
                {
                    if (kv.Value.Contains(processor))
                        kv.Value.Remove(processor);
                }
            }
        }

    }
}
