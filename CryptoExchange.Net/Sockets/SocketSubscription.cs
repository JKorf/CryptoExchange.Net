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
        public Dictionary<string, Func<SocketSubscription, JToken, bool>> MessageHandlers { get; set; }
        public List<SocketEvent> Events { get; set; }

        public IWebsocket Socket { get; set; }
        public SocketRequest Request { get; set; }

        public SocketType Type { get; set; }

        private bool lostTriggered;
        private readonly List<SocketEvent> waitingForEvents;
        private object eventLock = new object();


        public SocketSubscription(IWebsocket socket)
        {
            Socket = socket;
            Events = new List<SocketEvent>();
            waitingForEvents = new List<SocketEvent>();

            MessageHandlers = new Dictionary<string, Func<SocketSubscription, JToken, bool>>();

            Socket.OnClose += () =>
            {
                if (lostTriggered)
                    return;

                Socket.DisconnectTime = DateTime.UtcNow;
                lostTriggered = true;

                lock (eventLock)
                {
                    foreach (var events in Events)
                        events.Reset();
                }

                if (Socket.ShouldReconnect)
                    ConnectionLost?.Invoke();
            };
            Socket.OnOpen += () =>
            {
                if (lostTriggered)
                {
                    lostTriggered = false;
                    ConnectionRestored?.Invoke(Socket.DisconnectTime.HasValue ? DateTime.UtcNow - Socket.DisconnectTime.Value: TimeSpan.FromSeconds(0));
                }
            };
        }

        public void AddEvent(string name)
        {
            lock (eventLock)
                Events.Add(new SocketEvent(name));
        }

        public void SetEventByName(string name, bool success, Error error)
        {
            lock (eventLock)
            {
                var waitingEvent = waitingForEvents.SingleOrDefault(e => e.Name == name);
                if (waitingEvent != null)
                {
                    waitingEvent.Set(success, error);
                    waitingForEvents.Remove(waitingEvent);
                }
            }
        }

        public void SetEventById(string id, bool success, Error error)
        {
            lock (eventLock)
            {
                var waitingEvent = waitingForEvents.SingleOrDefault(e => e.WaitingId == id);
                if (waitingEvent != null)
                {
                    waitingEvent.Set(success, error);
                    waitingForEvents.Remove(waitingEvent);
                }
            }
        }

        public SocketEvent GetWaitingEvent(string name)
        {
            lock (eventLock)
                return waitingForEvents.SingleOrDefault(w => w.Name == name);
        }
        
        public Task<CallResult<bool>> WaitForEvent(string name, TimeSpan timeout)
        {
            lock (eventLock)
                return WaitForEvent(name, (int)Math.Round(timeout.TotalMilliseconds, 0));
        }

        public Task<CallResult<bool>> WaitForEvent(string name, int timeout)
        {
            lock (eventLock)
            {
                var evnt = Events.Single(e => e.Name == name);
                waitingForEvents.Add(evnt);
                return Task.Run(() => evnt.Wait(timeout));
            }
        }

        public Task<CallResult<bool>> WaitForEvent(string name, string id, TimeSpan timeout)
        {
            lock (eventLock)
                return WaitForEvent(name, id, (int)Math.Round(timeout.TotalMilliseconds, 0));
        }

        public Task<CallResult<bool>> WaitForEvent(string name, string id, int timeout)
        {
            lock (eventLock)
            {
                var evnt = Events.Single(e => e.Name == name);
                evnt.WaitingId = id;
                waitingForEvents.Add(evnt);
                return Task.Run(() => evnt.Wait(timeout));
            }
        }

        public void ResetEvents()
        {
            lock (eventLock)
            {
                foreach (var waiting in waitingForEvents)
                    waiting.Set(false, new UnknownError("Connection reset"));
                waitingForEvents.Clear();
            }
        }

        public async Task Close()
        {
            Socket.ShouldReconnect = false;
            await Socket.Close().ConfigureAwait(false);
            Socket.Dispose();
        }
    }
}
