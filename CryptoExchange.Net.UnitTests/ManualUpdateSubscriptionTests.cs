using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Errors;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets.Default;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptoExchange.Net.UnitTests
{
    [TestFixture]
    public class ManualUpdateSubscriptionTests
    {
        [Test]
        public void Constructor_Should_CreateSubscribedVirtualSubscription()
        {
            var controller = new ManualUpdateSubscription(socketId: 12);

            Assert.That(controller.Subscription.SocketId, Is.EqualTo(12));
            Assert.That(controller.Subscription.Id, Is.GreaterThan(0));
            Assert.That(controller.Subscription.SocketStatus, Is.EqualTo(SocketStatus.Connected));
            Assert.That(controller.Subscription.SubscriptionStatus, Is.EqualTo(SubscriptionStatus.Subscribed));
            Assert.That(controller.Subscription.LastReceiveTime, Is.Null);
        }

        [Test]
        public void StateChanges_Should_BeVisibleOnSubscription()
        {
            var controller = new ManualUpdateSubscription();
            var timestamp = new DateTime(2026, 8, 5, 12, 0, 0, DateTimeKind.Utc);
            var statuses = new List<SubscriptionStatus>();
            controller.Subscription.SubscriptionStatusChanged += statuses.Add;

            controller.SetLastReceiveTime(timestamp);
            controller.SetSocketStatus(SocketStatus.Reconnecting);
            controller.SetSubscriptionStatus(SubscriptionStatus.Subscribing);
            controller.SetSubscriptionStatus(SubscriptionStatus.Subscribed);

            Assert.That(controller.Subscription.LastReceiveTime, Is.EqualTo(timestamp));
            Assert.That(controller.Subscription.SocketStatus, Is.EqualTo(SocketStatus.Reconnecting));
            Assert.That(controller.Subscription.SubscriptionStatus, Is.EqualTo(SubscriptionStatus.Subscribed));
            Assert.That(statuses, Is.EqualTo(new[]
            {
                SubscriptionStatus.Subscribing,
                SubscriptionStatus.Subscribed
            }));
        }

        [Test]
        public void LifecycleMethods_Should_InvokeSubscriptionEvents()
        {
            var controller = new ManualUpdateSubscription();
            var error = new ServerError("Test error", ErrorInfo.Unknown);
            var exception = new InvalidOperationException("Test exception");
            var disconnectedPeriod = TimeSpan.FromMinutes(2);
            var lost = 0;
            var restored = TimeSpan.Zero;
            Error? resubscribeError = null;
            var paused = 0;
            var unpaused = 0;
            Exception? receivedException = null;

            controller.Subscription.ConnectionLost += () => lost++;
            controller.Subscription.ConnectionRestored += x => restored = x;
            controller.Subscription.ResubscribingFailed += x => resubscribeError = x;
            controller.Subscription.ActivityPaused += () => paused++;
            controller.Subscription.ActivityUnpaused += () => unpaused++;
            controller.Subscription.Exception += x => receivedException = x;

            controller.InvokeConnectionLost();
            controller.InvokeConnectionRestored(disconnectedPeriod);
            controller.InvokeResubscribingFailed(error);
            controller.InvokeActivityPaused();
            controller.InvokeActivityUnpaused();
            controller.InvokeException(exception);

            Assert.That(lost, Is.EqualTo(1));
            Assert.That(restored, Is.EqualTo(disconnectedPeriod));
            Assert.That(resubscribeError, Is.SameAs(error));
            Assert.That(paused, Is.EqualTo(1));
            Assert.That(unpaused, Is.EqualTo(1));
            Assert.That(receivedException, Is.SameAs(exception));
        }

        [Test]
        public void InvokeConnectionClosed_Should_CloseAndOnlyInvokeOnce()
        {
            var controller = new ManualUpdateSubscription();
            var closed = 0;
            controller.Subscription.ConnectionClosed += () => closed++;

            controller.InvokeConnectionClosed();
            controller.InvokeConnectionClosed();

            Assert.That(closed, Is.EqualTo(1));
            Assert.That(controller.Subscription.SocketStatus, Is.EqualTo(SocketStatus.Closed));
            Assert.That(controller.Subscription.SubscriptionStatus, Is.EqualTo(SubscriptionStatus.Closed));
        }

        [Test]
        public async Task SubscriptionOperations_Should_InvokeCallbacks()
        {
            var closes = 0;
            var reconnects = 0;
            var resubscribes = 0;
            var controller = new ManualUpdateSubscription(
                closeAsync: () =>
                {
                    closes++;
                    return Task.CompletedTask;
                },
                reconnectAsync: () =>
                {
                    reconnects++;
                    return Task.CompletedTask;
                },
                resubscribeAsync: () =>
                {
                    resubscribes++;
                    return Task.FromResult(CallResult.Ok());
                });

            await controller.Subscription.ReconnectAsync();
            var resubscribeResult = await controller.Subscription.ResubscribeAsync();
            await controller.Subscription.CloseAsync();
            await controller.Subscription.CloseAsync();

            Assert.That(reconnects, Is.EqualTo(1));
            Assert.That(resubscribes, Is.EqualTo(1));
            Assert.That(resubscribeResult.Success, Is.True);
            Assert.That(closes, Is.EqualTo(1));
            Assert.That(controller.Subscription.SubscriptionStatus, Is.EqualTo(SubscriptionStatus.Closed));
        }
    }
}
