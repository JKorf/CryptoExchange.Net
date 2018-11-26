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
        private Dictionary<int, SocketEvent> waitingForIds;


        public SocketSubscription(IWebsocket socket)
        {
            Socket = socket;
            Events = new List<SocketEvent>();
            waitingForIds = new Dictionary<int, SocketEvent>();

            DataHandlers = new List<Action<SocketSubscription, JToken>>();

            Socket.OnClose += () =>
            {
                if (lostTriggered)
                    return;

                Socket.DisconnectTime = DateTime.UtcNow;
                lostTriggered = true;

                foreach (var events in Events)
                    events.Reset();

                if (Socket.ShouldReconnect)
                    ConnectionLost?.Invoke();
            };
            Socket.OnOpen += () =>
            {
                if (lostTriggered)
                {
                    lostTriggered = false;
                    ConnectionRestored?.Invoke(DateTime.UtcNow - Socket.DisconnectTime.Value);
                }
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

        public void SetEvent(int id, bool success, Error error)
        {
            if (waitingForIds.ContainsKey(id))
            {
                waitingForIds[id].Set(success, error);
                waitingForIds.Remove(id);
            }
        }

        public (int, SocketEvent) GetWaitingEvent(string name)
        {
            var result = waitingForIds.SingleOrDefault(w => w.Value.Name == name);
            if (result.Equals(default(KeyValuePair<int, SocketEvent>)))
                return (0, null);

            return (result.Key, result.Value);
        }

        public CallResult<bool> WaitForEvent(string name, int timeout)
        {
            return Events.Single(e => e.Name == name).Wait(timeout);
        }

        public Task<CallResult<bool>> WaitForEvent(string name, int id, int timeout)
        {
            return Task.Run(() =>
            {
                var evnt = Events.Single(e => e.Name == name);
                waitingForIds.Add(id, evnt);
                return evnt.Wait(timeout);
            });
        }

        public async Task Close()
        {
            Socket.ShouldReconnect = false;
            await Socket.Close();
            Socket.Dispose();
        }
    }
}
