using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
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
        /// <summary>
        /// The factory for creating sockets. Used for unit testing
        /// </summary>
        public virtual IWebsocketFactory SocketFactory { get; set; } = new WebsocketFactory();

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
        
        /// <summary>
        /// Create a socket for an address
        /// </summary>
        /// <param name="address">The address the socket should connect to</param>
        /// <returns></returns>
        protected virtual IWebsocket CreateSocket(string address)
        {
            var socket = SocketFactory.CreateWebsocket(log, address);
            log.Write(LogVerbosity.Debug, "Created new socket for " + address);

            if (apiProxy != null)
                socket.SetProxy(apiProxy.Host, apiProxy.Port);

            socket.DataInterpreter = dataInterpreter;
            socket.OnClose += () =>
            {
                foreach (var sub in sockets)
                    sub.ResetEvents();

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
        /// <summary>
        /// Handler for when a socket reconnects. Should return true if reconnection handling was successful or false if not ( will try to reconnect again ). The handler should
        /// handle functionality like resubscribing and re-authenticating the socket.
        /// </summary>
        /// <param name="subscription">The socket subscription that was reconnected</param>
        /// <param name="disconnectedTime">The time the socket was disconnected</param>
        /// <returns></returns>
        protected abstract bool SocketReconnect(SocketSubscription subscription, TimeSpan disconnectedTime);

        /// <summary>
        /// Connect a socket
        /// </summary>
        /// <param name="socketSubscription">The subscription to connect</param>
        /// <returns></returns>
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

        /// <summary>
        /// The message handler. Normally distributes the received data to all data handlers
        /// </summary>
        /// <param name="subscription">The subscription that received the data</param>
        /// <param name="data">The data received</param>
        protected virtual void ProcessMessage(SocketSubscription subscription, string data)
        {
            log.Write(LogVerbosity.Debug, $"Socket {subscription.Socket.Id} received data: " + data);
            foreach (var handler in subscription.MessageHandlers)
                if (handler(subscription, JToken.Parse(data)))
                    return;
        }

        /// <summary>
        /// Handler for a socket closing. Reconnects the socket if needed, or removes it from the active socket list if not
        /// </summary>
        /// <param name="socket">The socket that was closed</param>
        protected virtual void SocketOnClose(IWebsocket socket)
        {
            if (socket.ShouldReconnect)
            {
                log.Write(LogVerbosity.Info, $"Socket {socket.Id} Connection lost, will try to reconnect");
                Task.Run(() =>
                {
                    Thread.Sleep(reconnectInterval);
                    if (!socket.Connect().Result)
                    {
                        log.Write(LogVerbosity.Debug, $"Socket {socket.Id} failed to reconnect");
                        return; // Connect() should result in a SocketClosed event so we end up here again
                    }
                    var time = socket.DisconnectTime;
                    socket.DisconnectTime = null;
                    if (time == null)
                        return;

                    log.Write(LogVerbosity.Info, $"Socket {socket.Id} reconnected after {DateTime.UtcNow - time}");

                    SocketSubscription subscription;
                    lock (sockets)
                        subscription = sockets.Single(s => s.Socket == socket);

                    if (!SocketReconnect(subscription, DateTime.UtcNow - time.Value))
                        socket.Close().Wait(); // Close so we end up reconnecting again


                    log.Write(LogVerbosity.Info, $"Socket {socket.Id} successfully resubscribed");
                    return;
                });
            }
            else
            {
                log.Write(LogVerbosity.Info, $"Socket {socket.Id} closed");
                socket.Dispose();
                lock (sockets)
                {
                    var subscription = sockets.SingleOrDefault(s => s.Socket == socket);
                    if(subscription != null)
                        sockets.Remove(subscription);
                }
            }
        }

        /// <summary>
        /// Send data to the websocket
        /// </summary>
        /// <typeparam name="T">The type of the object to send</typeparam>
        /// <param name="socket">The socket to send to</param>
        /// <param name="obj">The object to send</param>
        /// <param name="nullValueHandling">How null values should be serialized</param>
        protected virtual void Send<T>(IWebsocket socket, T obj, NullValueHandling nullValueHandling = NullValueHandling.Ignore)
        {
            Send(socket, JsonConvert.SerializeObject(obj, Formatting.None, new JsonSerializerSettings { NullValueHandling = nullValueHandling }));            
        }

        /// <summary>
        /// Send string data to the websocket
        /// </summary>
        /// <param name="socket">The socket to send to</param>
        /// <param name="data">The data to send</param>
        protected virtual void Send(IWebsocket socket, string data)
        {
            log.Write(LogVerbosity.Debug, $"Socket {socket.Id} sending data: {data}");
            socket.Send(data);
        }

        /// <summary>
        /// Unsubscribe from a stream
        /// </summary>
        /// <param name="subscription">The subscription to unsubscribe</param>
        /// <returns></returns>
        public virtual async Task Unsubscribe(UpdateSubscription subscription)
        {
            log.Write(LogVerbosity.Info, $"Closing subscription {subscription.Id}");
            await subscription.Close();
        }

        /// <summary>
        /// Unsubscribe all subscriptions
        /// </summary>
        /// <returns></returns>
        public virtual async Task UnsubscribeAll()
        {
            log.Write(LogVerbosity.Debug, $"Closing all {sockets.Count} subscriptions");
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
            log.Write(LogVerbosity.Debug, "Disposing socket client, closing all subscriptions");
            lock (sockets)
                UnsubscribeAll().Wait();

            base.Dispose();
        }
    }
}
