using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Testing;
using CryptoExchange.Net.UnitTests.Implementations;
using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.UnitTests.ClientTests
{
    [TestFixture]
    public class SocketClientTests
    {
        [TestCase]
        public void SettingOptions_Should_ResultInOptionsSet()
        {
            //arrange
            //act
            var client = new TestSocketClient(options =>
            {
                options.ExchangeOptions.MaxSocketConnections = 1;
            });

            //assert
            Assert.That(1 == client.ApiClient1.ApiOptions.MaxSocketConnections);
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task ConnectSocket_Should_ReturnConnectionResult(bool canConnect)
        {
            //arrange
            var client = new TestSocketClient();
            var socket = TestHelpers.ConfigureSocketClient(client, "wss://localhost");
            socket.CanConnect = canConnect;

            //act
            var connectResult = await client.ApiClient1.SubscribeToUpdatesAsync<TestObject>(x => { }, false, default);

            //assert
            Assert.That(connectResult.Success == canConnect);
        }

        [TestCase]
        public async Task SocketMessages_Should_BeProcessedInDataHandlers()
        {
            var client = new TestSocketClient();
            var socket = TestHelpers.ConfigureSocketClient(client, "wss://localhost");

            var expected = new TestObject() { DecimalData = 1.23M, IntData = 10, StringData = "Some data" };
            var strData = JsonSerializer.Serialize(expected, new JsonSerializerOptions { TypeInfoResolver = new TestSerializerContext() });

            TestObject? received = null;
            var resetEvent = new AsyncResetEvent(false);

            await client.ApiClient1.SubscribeToUpdatesAsync<TestObject>(x =>
            {
                received = x.Data;
                resetEvent.Set();
            }, false, default);

            socket.InvokeMessage(strData);
            await resetEvent.WaitAsync(TimeSpan.FromSeconds(1));

            Assert.That(received != null);
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task SocketMessages_Should_ContainOriginalDataIfEnabled(bool enabled)
        {
            // arrange
            var client = new TestSocketClient(options =>
            {
                options.ReconnectInterval = TimeSpan.Zero;
                options.ExchangeOptions.OutputOriginalData = enabled;
            });
            var socket = TestHelpers.ConfigureSocketClient(client, "wss://localhost");
            var expected = new TestObject() { DecimalData = 1.23M, IntData = 10, StringData = "Some data" };
            var strData = JsonSerializer.Serialize(expected, new JsonSerializerOptions { TypeInfoResolver = new TestSerializerContext() });

            string? originalData = null;
            var resetEvent = new AsyncResetEvent(false);

            await client.ApiClient1.SubscribeToUpdatesAsync<TestObject>(x =>
            {
                originalData = x.OriginalData;
                resetEvent.Set();
            }, false, default);

            socket.InvokeMessage(strData);
            await resetEvent.WaitAsync(TimeSpan.FromSeconds(1));

            // assert
            Assert.That(originalData == (enabled ? strData : null));
        }

        [TestCase()]
        public async Task UnsubscribingStream_Should_CloseTheSocket()
        {
            // arrange
            var client = new TestSocketClient(options =>
            {
                options.ReconnectInterval = TimeSpan.Zero;
            });
            var socket = TestHelpers.ConfigureSocketClient(client, "wss://localhost");

            var result = await client.ApiClient1.SubscribeToUpdatesAsync<TestObject>(x => {}, false, default);

            // act
            await client.UnsubscribeAsync(result.Data!);

            // assert
            Assert.That(socket.Connected == false);
        }

        [TestCase()]
        public async Task UnsubscribingAll_Should_CloseAllSockets()
        {
            // arrange
            var client = new TestSocketClient(options =>
            {
                options.ReconnectInterval = TimeSpan.Zero;
            });
            var socket = TestHelpers.ConfigureSocketClient(client, "wss://localhost");
            var result = await client.ApiClient1.SubscribeToUpdatesAsync<TestObject>(x => { }, false, default);

            var socket2 = TestHelpers.ConfigureSocketClient(client, "wss://localhost");
            var result2 = await client.ApiClient1.SubscribeToUpdatesAsync<TestObject>(x => { }, false, default);

            // act
            await client.UnsubscribeAllAsync();

            // assert
            Assert.That(socket.Connected == false);
            Assert.That(socket2.Connected == false);
        }

        [TestCase()]
        public async Task BatchedSubscription_Should_NotExceedIndividualCombineTarget()
        {
            // arrange
            var client = new TestSocketClient(options =>
            {
                options.SocketSubscriptionsCombineTarget = 10;
                options.SocketIndividualSubscriptionCombineTarget = 10;
            });
            TestHelpers.ConfigureSocketClient(client, "wss://localhost");

            // act
            await client.ApiClient1.SubscribeToUpdatesAsync<TestObject>(x => { }, false, default, individualSubscriptionCount: 6);
            TestHelpers.ConfigureSocketClient(client, "wss://localhost");
            await client.ApiClient1.SubscribeToUpdatesAsync<TestObject>(x => { }, false, default, individualSubscriptionCount: 6);

            // assert
            Assert.That(client.ApiClient1._socketConnections.Count == 2);
            Assert.That(client.ApiClient1._socketConnections.Values.All(connection => connection.Subscriptions.Sum(subscription => subscription.IndividualSubscriptionCount) <= 10));
        }

        [TestCase()]
        public async Task BatchedSubscription_FullIndividualConnection_Should_NotPreventEligibleConnectionReuse()
        {
            // arrange
            var client = new TestSocketClient(options =>
            {
                options.SocketSubscriptionsCombineTarget = 5;
                options.SocketIndividualSubscriptionCombineTarget = 10;
            });
            TestHelpers.ConfigureSocketClient(client, "wss://localhost");
            await client.ApiClient1.SubscribeToUpdatesAsync<TestObject>(x => { }, false, default);
            await client.ApiClient1.SubscribeToUpdatesAsync<TestObject>(x => { }, false, default);

            TestHelpers.ConfigureSocketClient(client, "wss://localhost");
            await client.ApiClient1.SubscribeToUpdatesAsync<TestObject>(x => { }, false, default, individualSubscriptionCount: 10);

            TestHelpers.ConfigureSocketClient(client, "wss://localhost");

            // act
            await client.ApiClient1.SubscribeToUpdatesAsync<TestObject>(x => { }, false, default);

            // assert
            Assert.That(
                client.ApiClient1._socketConnections.Count,
                Is.EqualTo(2),
                "The eligible connection should be reused instead of opening a new connection after selecting a full individual-subscription connection");

            var fullConnection = client.ApiClient1._socketConnections.Values
                .Single(connection => connection.Subscriptions.Sum(subscription => subscription.IndividualSubscriptionCount) == 10);
            Assert.That(
                fullConnection.UserSubscriptionCount,
                Is.EqualTo(1),
                "The full connection should not receive the normal subscription");

            var eligibleConnection = client.ApiClient1._socketConnections.Values.Single(connection => connection != fullConnection);
            Assert.That(
                eligibleConnection.UserSubscriptionCount,
                Is.EqualTo(3),
                "The existing eligible connection should receive the normal subscription");
        }

        [TestCase()]
        public async Task ErrorResponse_ShouldNot_ConfirmSubscription()
        {
            // arrange
            var client = new TestSocketClient(opt =>
            {
                opt.OutputOriginalData = true;
            });

            var socket = TestHelpers.ConfigureSocketClient(client, "wss://localhost");
            var subTask = client.ApiClient1.SubscribeToUpdatesAsync<TestObject>(x => { }, true, default);

            socket.InvokeMessage(JsonSerializer.Serialize(new TestSocketMessage { Id = 1, Data = "ErrorWithSub" }));

            var result = await subTask;

            // assert
            Assert.That(result.Success == false);
            Assert.That(result.Error!.Message!.Contains("ErrorWithSub"));
        }

        [TestCase()]
        public async Task SuccessResponse_Should_ConfirmSubscription()
        {
            var client = new TestSocketClient();
            var socket = TestHelpers.ConfigureSocketClient(client, "wss://localhost");
            var subTask = client.ApiClient1.SubscribeToUpdatesAsync<TestObject>(x => { }, true, default);

            socket.InvokeMessage(JsonSerializer.Serialize(new TestSocketMessage { Id = 1, Data = "OK" }));

            var result = await subTask;

            var subscription = client.ApiClient1._socketConnections.Single().Value.Subscriptions.Single();
            Assert.That(subscription.Status == SubscriptionStatus.Subscribed);
        }

        [Test]
        public async Task PartialOverlap_ShouldUseDifferentSocket()
        {
            using var client = new TestSocketClient(options => options.SocketSubscriptionsCombineTarget = 10);
            TestHelpers.ConfigureSocketClient(client, "wss://localhost");

            var first = await client.ApiClient1.SubscribeToUpdatesAsync<TestObject>(_ => { }, false, default, subscriptionIdentifiers: ["A", "B"]);
            var same = await client.ApiClient1.SubscribeToUpdatesAsync<TestObject>(_ => { }, false, default, subscriptionIdentifiers: ["B", "A"]);
            var disjoint = await client.ApiClient1.SubscribeToUpdatesAsync<TestObject>(_ => { }, false, default, subscriptionIdentifiers: ["Z"]);

            TestHelpers.ConfigureSocketClient(client, "wss://localhost");
            var overlap = await client.ApiClient1.SubscribeToUpdatesAsync<TestObject>(_ => { }, false, default, subscriptionIdentifiers: ["B"]);

            Assert.That(first.Success && same.Success && disjoint.Success && overlap.Success, Is.True);
            Assert.That(same.Data!.SocketId, Is.EqualTo(first.Data!.SocketId));
            Assert.That(disjoint.Data!.SocketId, Is.EqualTo(first.Data!.SocketId));
            Assert.That(overlap.Data!.SocketId, Is.Not.EqualTo(first.Data!.SocketId));
        }

        [Test]
        public async Task PartialOverlapAtConnectionLimit_ShouldFail()
        {
            using var client = new TestSocketClient(options =>
            {
                options.SocketSubscriptionsCombineTarget = 10;
                options.MaxSocketConnections = 1;
            });
            TestHelpers.ConfigureSocketClient(client, "wss://localhost");

            var first = await client.ApiClient1.SubscribeToUpdatesAsync<TestObject>(_ => { }, false, default, subscriptionIdentifiers: ["A", "B"]);
            var overlap = await client.ApiClient1.SubscribeToUpdatesAsync<TestObject>(_ => { }, false, default, subscriptionIdentifiers: ["B", "C"]);

            Assert.That(first.Success, Is.True);
            Assert.That(overlap.Success, Is.False);
            Assert.That(client.ApiClient1._socketConnections.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task DisjointSubscriptions_ShouldWaitForAcknowledgmentsInParallel()
        {
            var timeout = TimeSpan.FromSeconds(5);
            using var client = new TestSocketClient(options => options.SocketSubscriptionsCombineTarget = 10);
            var socket = TestHelpers.ConfigureSocketClient(client, "wss://localhost");
            var firstSent = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var secondSent = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            socket.OnMessageSend += data =>
            {
                var request = JsonSerializer.Deserialize<TestSocketMessage>(data)!;
                if (request.Id == 1)
                    firstSent.TrySetResult();
                else if (request.Id == 3)
                    secondSent.TrySetResult();
            };

            var firstTask = client.ApiClient1.SubscribeToUpdatesAsync<TestObject>(_ => { }, true, default, subscriptionIdentifiers: ["A"]);
            await firstSent.Task.WaitAsync(timeout);
            var secondTask = client.ApiClient1.SubscribeToUpdatesAsync<TestObject>(_ => { }, true, default, subscriptionIdentifiers: ["B"], subscribeRequestId: 3);
            await secondSent.Task.WaitAsync(timeout);

            socket.InvokeMessage(JsonSerializer.Serialize(new TestSocketMessage { Id = 3, Data = "OK" }));
            var second = await secondTask.WaitAsync(timeout);
            Assert.That(second.Success, Is.True);
            Assert.That(firstTask.IsCompleted, Is.False);

            socket.InvokeMessage(JsonSerializer.Serialize(new TestSocketMessage { Id = 1, Data = "OK" }));
            var first = await firstTask.WaitAsync(timeout);
            Assert.That(first.Success, Is.True);
            Assert.That(first.Data!.SocketId, Is.EqualTo(second.Data!.SocketId));
        }

        [Test]
        public async Task IdenticalSubscriptions_ShouldShareWireOperations()
        {
            var timeout = TimeSpan.FromSeconds(5);
            using var client = new TestSocketClient(options => options.SocketSubscriptionsCombineTarget = 10);
            var socket = TestHelpers.ConfigureSocketClient(client, "wss://localhost");
            var requests = new ConcurrentQueue<string>();
            var firstSubscribeSent = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var reconnectSubscribeSent = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var subscribeCount = 0;
            socket.OnMessageSend += data =>
            {
                var request = JsonSerializer.Deserialize<TestSocketMessage>(data)!;
                requests.Enqueue(request.Data);
                if (request.Id == 1)
                {
                    switch (Interlocked.Increment(ref subscribeCount))
                    {
                        case 1: firstSubscribeSent.TrySetResult(); return;
                        case 2: reconnectSubscribeSent.TrySetResult(); return;
                    }
                }

                socket.InvokeMessage(JsonSerializer.Serialize(new TestSocketMessage { Id = request.Id, Data = "OK" }));
            };

            var firstTask = client.ApiClient1.SubscribeToUpdatesAsync<TestObject>(_ => { }, true, default, subscriptionIdentifiers: ["A", "B"]);
            await firstSubscribeSent.Task.WaitAsync(timeout);
            var sameTask = client.ApiClient1.SubscribeToUpdatesAsync<TestObject>(_ => { }, true, default, subscriptionIdentifiers: ["B", "A"]);
            socket.InvokeMessage(JsonSerializer.Serialize(new TestSocketMessage { Id = 1, Data = "OK" }));
            var subscriptions = await Task.WhenAll(firstTask, sameTask).WaitAsync(timeout);

            Assert.That(subscriptions.All(x => x.Success), Is.True);
            Assert.That(requests.ToArray(), Is.EqualTo(new[] { "Sub" }));

            // Keep the connection open when the last listener for the shared streams is closed.
            var other = await client.ApiClient1.SubscribeToUpdatesAsync<TestObject>(_ => { }, false, default, subscriptionIdentifiers: ["Z"]);
            Assert.That(other.Success, Is.True);
            var restored = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            subscriptions[0].Data!.ConnectionRestored += _ => restored.TrySetResult();
            await socket.ReconnectAsync().WaitAsync(timeout);
            await reconnectSubscribeSent.Task.WaitAsync(timeout);
            socket.InvokeMessage(JsonSerializer.Serialize(new TestSocketMessage { Id = 1, Data = "OK" }));
            await restored.Task.WaitAsync(timeout);

            Assert.That(requests.ToArray(), Is.EqualTo(new[] { "Sub", "Sub" }));
            await subscriptions[0].Data!.ResubscribeAsync().WaitAsync(timeout);
            Assert.That(subscriptions[1].Data!.SubscriptionStatus, Is.EqualTo(SubscriptionStatus.Subscribed));
            Assert.That(requests.ToArray(), Is.EqualTo(new[] { "Sub", "Sub", "Unsub", "Sub" }));

            await subscriptions[0].Data!.CloseAsync().WaitAsync(timeout);
            Assert.That(requests.Count, Is.EqualTo(4));
            await subscriptions[1].Data!.CloseAsync().WaitAsync(timeout);
            Assert.That(requests.ToArray(), Is.EqualTo(new[] { "Sub", "Sub", "Unsub", "Sub", "Unsub" }));
            await other.Data!.CloseAsync().WaitAsync(timeout);
        }
    }
}
