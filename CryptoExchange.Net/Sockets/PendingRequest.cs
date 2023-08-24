using CryptoExchange.Net.Objects;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;

namespace CryptoExchange.Net.Sockets
{
    internal class PendingRequest
    {
        public int Id { get; set; }
        public Func<JToken, bool> Handler { get; }
        public JToken? Result { get; private set; }
        public bool Completed { get; private set; }
        public AsyncResetEvent Event { get; }
        public DateTime RequestTimestamp { get; set; }
        public TimeSpan Timeout { get; }
        public SocketSubscription? Subscription { get; }

        private CancellationTokenSource? _cts;

        public PendingRequest(int id, Func<JToken, bool> handler, TimeSpan timeout, SocketSubscription? subscription)
        {
            Id = id;
            Handler = handler;
            Event = new AsyncResetEvent(false, false);
            Timeout = timeout;
            RequestTimestamp = DateTime.UtcNow;
            Subscription = subscription;
        }

        public void IsSend()
        {
            // Start timeout countdown
            _cts = new CancellationTokenSource(Timeout);
            _cts.Token.Register(Fail, false);
        }

        public bool CheckData(JToken data)
        {
            return Handler(data);
        }

        public bool Succeed(JToken data)
        {
            Result = data;
            Completed = true;
            Event.Set();
            return true;
        }

        public void Fail()
        {
            Completed = true;
            Event.Set();
        }
    }
}
