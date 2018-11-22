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
                socket.ShouldReconnect = true;
                SocketOpened(socket);
            };
            return socket;
        }

        protected abstract void SocketOpened(IWebsocket socket);
        protected abstract void SocketClosed(IWebsocket socket);
        protected abstract void SocketError(IWebsocket socket, Exception ex);
        protected abstract bool SocketReconnect(SocketSubscription socket, TimeSpan disconnectedTime);

        protected virtual CallResult<SocketSubscription> ConnectSocket(IWebsocket socket)
        {
            if (socket.Connect().Result)
            {
                var subscription = new SocketSubscription(socket);
                lock (sockets)
                    sockets.Add(subscription);
                return new CallResult<SocketSubscription>(subscription, null);
            }

            socket.Dispose();
            return new CallResult<SocketSubscription>(null, new CantConnectError());
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

                    SocketReconnected(subscription, DateTime.UtcNow - socket.DisconnectTime.Value);                    
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

        protected virtual async Task<CallResult<string>> SendAndWait<T>(IWebsocket socket, T obj, Func<JToken, bool> waitingFor, int timeout=5000) 
        {
            return await Task.Run(() =>
            {
                var data = JsonConvert.SerializeObject(obj);
                ManualResetEvent evnt = new ManualResetEvent(false);
                string result = null;
                var onMessageAction = new Action<string>((msg) =>
                {
                    if (!waitingFor(JToken.Parse(msg)))
                        return;

                    log.Write(LogVerbosity.Debug, "Socket received query response: " + msg);
                    result = msg;
                    evnt?.Set();
                });

                socket.OnMessage += onMessageAction;
                Send(socket, data);
                evnt.WaitOne(timeout);
                socket.OnMessage -= onMessageAction;                
                evnt.Dispose();
                evnt = null;
                if (result == null)
                    return new CallResult<string>(null, new ServerError("No response from server"));
                return new CallResult<string>(result, null);
            }).ConfigureAwait(false);
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
