using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Logging;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CryptoExchange.Net
{
    public abstract class SocketClient: BaseClient
    {
        #region fields
        public IWebsocketFactory SocketFactory { get; set; } = new WebsocketFactory();

        private const SslProtocols protocols = SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls;

        protected List<SocketSubscription> sockets = new List<SocketSubscription>();

        protected TimeSpan reconnectInterval;
        protected Func<byte[], string> dataInterpreter;
        #endregion

        protected SocketClient(SocketClientOptions exchangeOptions, AuthenticationProvider authenticationProvider): base(exchangeOptions, authenticationProvider)
        {
            Configure(exchangeOptions);
        }

        /// <summary>
        /// Configure the client using the provided options
        /// </summary>
        /// <param name="exchangeOptions">Options</param>
        protected void Configure(SocketClientOptions exchangeOptions)
        {
            reconnectInterval = exchangeOptions.ReconnectInterval;
        }

        /// <summary>
        /// Set a function to interprete the data, used when the data is received as bytes instead of a string
        /// </summary>
        /// <param name="handler"></param>
        protected void SetDataInterpreter(Func<byte[], string> handler)
        {
            dataInterpreter = handler;
        }
        
        protected virtual IWebsocket CreateSocket(string address)
        {
            var socket = SocketFactory.CreateWebsocket(log, address);
            log.Write(LogVerbosity.Debug, "Created new socket for " + address);

            if (apiProxy != null)
                socket.SetProxy(apiProxy.Host, apiProxy.Port);

            socket.SetEnabledSslProtocols(protocols);
            socket.DataInterpreter = dataInterpreter;
            socket.OnClose += () =>
            {
                if(socket.DisconnectTime == null)
                    socket.DisconnectTime = DateTime.UtcNow;
                SocketOnClose(socket);
            };
            socket.OnError += (e) =>
            {
                log.Write(LogVerbosity.Warning, $"Socket {socket.Id} error: " + e.ToString());
                SocketError(socket, e);
            };
            socket.OnOpen += () =>
            {
                SocketOpened(socket);
            };
            return socket;
        }

        protected virtual void SocketOpened(IWebsocket socket) { }
        protected virtual void SocketClosed(IWebsocket socket) { }
        protected virtual void SocketError(IWebsocket socket, Exception ex) { }
        protected abstract bool SocketReconnect(SocketSubscription socket, TimeSpan disconnectedTime);

        protected virtual async Task<CallResult<bool>> ConnectSocket(SocketSubscription socketSubscription)
        {
            socketSubscription.Socket.OnMessage += data => ProcessMessage(socketSubscription, data);

            if (await socketSubscription.Socket.Connect())
            {
                lock (sockets)
                    sockets.Add(socketSubscription);
                return new CallResult<bool>(true, null);
            }

            socketSubscription.Socket.Dispose();
            return new CallResult<bool>(false, new CantConnectError());
        }

        protected virtual void ProcessMessage(SocketSubscription sub, string data)
        {
            log.Write(LogVerbosity.Debug, $"Socket {sub.Socket.Id} received data: " + data);
            foreach (var handler in sub.DataHandlers)
                handler(sub, JToken.Parse(data));
        }

        protected virtual void SocketOnClose(IWebsocket socket)
        {
            if (socket.ShouldReconnect)
            {
                log.Write(LogVerbosity.Info, $"Socket {socket.Id} Connection lost, going to try to reconnect");
                Task.Run(() =>
                {
                    Thread.Sleep(reconnectInterval);
                    if (!socket.Connect().Result)
                    {
                        log.Write(LogVerbosity.Debug, $"Socket {socket.Id} failed to reconnect");
                        return; // Connect() should result in a SocketClosed event so we end up here again
                    }

                    log.Write(LogVerbosity.Info, $"Socket {socket.Id} reconnected after {DateTime.UtcNow - socket.DisconnectTime.Value}");

                    SocketSubscription subscription;
                    lock(sockets)
                        subscription = sockets.Single(s => s.Socket == socket);

                    if (!SocketReconnect(subscription, DateTime.UtcNow - socket.DisconnectTime.Value))
                        socket.Close().Wait(); // Close so we end up reconnecting again
                    socket.DisconnectTime = null;
                    return;
                });
            }
            else
            {
                socket.Dispose();
                lock (sockets)
                {
                    var subscription = sockets.SingleOrDefault(s => s.Socket == socket);
                    if(subscription != null)
                        sockets.Remove(subscription);
                }
            }
        }

        protected virtual void Send<T>(IWebsocket socket, T obj, NullValueHandling nullValueHandling = NullValueHandling.Ignore)
        {
            Send(socket, JsonConvert.SerializeObject(obj, Formatting.None, new JsonSerializerSettings { NullValueHandling = nullValueHandling }));            
        }

        protected virtual void Send(IWebsocket socket, string data)
        {
            log.Write(LogVerbosity.Debug, $"Socket {socket.Id} sending data: {data}");
            socket.Send(data);
        }

        public virtual async Task Unsubscribe(UpdateSubscription sub)
        {
            await sub.Close();
        }

        public virtual async Task UnsubscribeAll()
        {
            await Task.Run(() =>
            {
                var tasks = new List<Task>();
                foreach (var sub in new List<SocketSubscription>(sockets))
                    tasks.Add(sub.Close());
                Task.WaitAll(tasks.ToArray());
            });
        }

        public override void Dispose()
        {
            lock(sockets)
                foreach (var socket in sockets)
                    socket.Socket.Dispose();
                sockets.Clear();
        }
    }
}
