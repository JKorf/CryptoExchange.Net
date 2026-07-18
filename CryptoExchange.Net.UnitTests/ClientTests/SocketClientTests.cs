using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Testing;
using CryptoExchange.Net.UnitTests.Implementations;
using NUnit.Framework;
using System;
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
    }
}
