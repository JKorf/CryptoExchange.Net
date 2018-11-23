using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Sockets
{
    public class SocketSubscription
    {
        public event Action ConnectionLost;
        public event Action<TimeSpan> ConnectionRestored;

        public List<Action<SocketSubscription, JToken>> DataHandlers { get; set; }
        public List<SocketEvent> Events { get; set; }

        public IWebsocket Socket { get; set; }
        public object Request { get; set; }

        private bool lostTriggered;

        public SocketSubscription(IWebsocket socket)
        {
            Socket = socket;
            Events = new List<SocketEvent>();

            DataHandlers = new List<Action<SocketSubscription, JToken>>();

            Socket.OnClose += () =>
            {
                if (lostTriggered)
                    return;

                lostTriggered = true;

                foreach (var events in Events)
                    events.Reset();

                if (Socket.ShouldReconnect)
                    ConnectionLost?.Invoke();
            };
            Socket.OnOpen += () =>
            {
                lostTriggered = false;
                if (Socket.DisconnectTime != null)
                    ConnectionRestored?.Invoke(DateTime.UtcNow - Socket.DisconnectTime.Value);
            };
        }

        public void AddEvent(string name)
        {
            Events.Add(new SocketEvent(name));
        }

        public void SetEvent(string name, bool success, Error error)
        {
            Events.SingleOrDefault(e => e.Name == name)?.Set(success, error);
        }

        public CallResult<bool> WaitForEvent(string name)
        {
            return Events.Single(e => e.Name == name).Wait();
        }

        public async Task Close()
        {
            Socket.ShouldReconnect = false;
            await Socket.Close();
            Socket.Dispose();
        }
    }
}
