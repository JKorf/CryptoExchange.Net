using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Objects.Sockets
{
    internal class PendingRequest : IStreamMessageListener
    {
        public int Id { get; set; }
        public Func<StreamMessage, bool> MessageMatchesHandler { get; }
        public bool Completed { get; private set; }
        public AsyncResetEvent Event { get; }
        public DateTime RequestTimestamp { get; set; }
        public TimeSpan Timeout { get; }
        public SocketSubscriptionListener? Subscription { get; }

        private CancellationTokenSource? _cts;

        public int Priority => 100;

        public PendingRequest(int id, Func<StreamMessage, bool> messageMatchesHandler, TimeSpan timeout, SocketSubscriptionListener? subscription)
        {
            Id = id;
            MessageMatchesHandler = messageMatchesHandler;
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

        public void Fail()
        {
            Completed = true;
            Event.Set();
        }

        public bool MessageMatches(StreamMessage message)
        {
            return MessageMatchesHandler(message);
        }

        public Task ProcessAsync(StreamMessage message)
        {
            Completed = true;
            Event.Set();
            return Task.CompletedTask;
        }
    }
}
