using CryptoExchange.Net.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.Sockets
{
    public class SocketSubscription
    {
        public event Action ConnectionLost;
        public event Action ConnectionRestored;

        public IWebsocket Socket { get; set; }
        public object Request { get; set; }

        private bool lostTriggered;

        public SocketSubscription(IWebsocket socket)
        {
            Socket = socket;

            Socket.OnClose += () =>
            {
                if (lostTriggered)
                    return;

                lostTriggered = true;
                if (Socket.ShouldReconnect)
                    ConnectionLost?.Invoke();
            };
            Socket.OnOpen += () =>
            {
                lostTriggered = false;
                if (Socket.DisconnectTime != null)
                    ConnectionRestored?.Invoke();
            };
        }
    }
}
