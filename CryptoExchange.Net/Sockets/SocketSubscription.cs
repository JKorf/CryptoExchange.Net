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

        /// <summary>
        /// Message handlers for this subscription. Should return true if the message is handled and should not be distributed to the other handlers
        /// </summary>
        public List<Func<SocketSubscription, JToken, bool>> MessageHandlers { get; set; }
        public List<SocketEvent> Events { get; set; }

        public IWebsocket Socket { get; set; }
        public SocketRequest Request { get; set; }

        private bool lostTriggered;
        private List<SocketEvent> waitingForEvents;


        public SocketSubscription(IWebsocket socket)
        {
            Socket = socket;
            Events = new List<SocketEvent>();
            waitingForEvents = new List<SocketEvent>();

            MessageHandlers = new List<Func<SocketSubscription, JToken, bool>>();

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
            var waitingEvent = waitingForEvents.SingleOrDefault(e => e.WaitingId == id);
            if (waitingEvent != null)
            {
                waitingEvent.Set(success, error);
                waitingForEvents.Remove(waitingEvent);
            }
        }

        public SocketEvent GetWaitingEvent(string name)
        {
            return waitingForEvents.SingleOrDefault(w => w.Name == name);
        }

        public Task<CallResult<bool>> WaitForEvent(string name, int timeout)
        {
            return Task.Run(() =>
            {
                var evnt = Events.Single(e => e.Name == name);
                waitingForEvents.Add(evnt);
                return evnt.Wait(timeout);
            });
        }

        public Task<CallResult<bool>> WaitForEvent(string name, int id, int timeout)
        {
            return Task.Run(() =>
            {
                var evnt = Events.Single(e => e.Name == name);
                evnt.WaitingId = id;
                waitingForEvents.Add(evnt);
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
