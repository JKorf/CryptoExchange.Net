using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using CryptoExchange.Net.Testing.Implementations;
using CryptoExchange.Net.UnitTests.TestImplementations;
using CryptoExchange.Net.UnitTests.TestImplementations.Sockets;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.UnitTests
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
                options.SubOptions.ApiCredentials = new Authentication.ApiCredentials("1", "2");
                options.SubOptions.MaxSocketConnections = 1;
            });

            //assert
            ClassicAssert.NotNull(client.SubClient.ApiOptions.ApiCredentials);
            Assert.That(1 == client.SubClient.ApiOptions.MaxSocketConnections);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ConnectSocket_Should_ReturnConnectionResult(bool canConnect)
        {
            //arrange
            var client = new TestSocketClient();
            var socket = client.CreateSocket();
            socket.CanConnect = canConnect;

            //act
            var connectResult = client.SubClient.ConnectSocketSub(
                new SocketConnection(new TraceLogger(), new TestWebsocketFactory(socket), new WebSocketParameters(new Uri("https://localhost/"), ReconnectPolicy.Disabled), client.SubClient, ""));

            //assert
            Assert.That(connectResult.Success == canConnect);
        }

        [TestCase]
        public void SocketMessages_Should_BeProcessedInDataHandlers()
        {
            // arrange
            var client = new TestSocketClient(options => {
                options.ReconnectInterval = TimeSpan.Zero;
            });
            var socket = client.CreateSocket();
            socket.CanConnect = true;
            var sub = new SocketConnection(new TraceLogger(), new TestWebsocketFactory(socket), new WebSocketParameters(new Uri("https://localhost/"), ReconnectPolicy.Disabled), client.SubClient, "");
            var rstEvent = new ManualResetEvent(false);
            Dictionary<string, string> result = null;

            client.SubClient.ConnectSocketSub(sub);

            var subObj = new TestSubscription<Dictionary<string, string>>(Mock.Of<ILogger>(), (messageEvent) =>
            {
                result = messageEvent.Data;
                rstEvent.Set();
            });
            sub.AddSubscription(subObj);

            // act
            socket.InvokeMessage("{\"property\": \"123\", \"action\": \"update\", \"topic\": \"topic\"}");
            rstEvent.WaitOne(1000);

            // assert
            Assert.That(result["property"] == "123");
        }

        [TestCase(false)]
        [TestCase(true)]
        public void SocketMessages_Should_ContainOriginalDataIfEnabled(bool enabled)
        {
            // arrange
            var client = new TestSocketClient(options =>
            {
                options.ReconnectInterval = TimeSpan.Zero;
                options.SubOptions.OutputOriginalData = enabled;
            });
            var socket = client.CreateSocket();
            socket.CanConnect = true;
            var sub = new SocketConnection(new TraceLogger(), new TestWebsocketFactory(socket), new WebSocketParameters(new Uri("https://localhost/"), ReconnectPolicy.Disabled), client.SubClient, "");
            var rstEvent = new ManualResetEvent(false);
            string original = null;

            client.SubClient.ConnectSocketSub(sub);
            var subObj = new TestSubscription<Dictionary<string, string>>(Mock.Of<ILogger>(), (messageEvent) =>
            {
                original = messageEvent.OriginalData;
                rstEvent.Set();
            });
            sub.AddSubscription(subObj);
            var msgToSend = JsonSerializer.Serialize(new { topic = "topic", action = "update", property = "123" });

            // act
            socket.InvokeMessage(msgToSend);
            rstEvent.WaitOne(1000);

            // assert
            Assert.That(original == (enabled ? msgToSend : null));
        }

        [TestCase()]
        public void UnsubscribingStream_Should_CloseTheSocket()
        {
            // arrange
            var client = new TestSocketClient(options =>
            {
                options.ReconnectInterval = TimeSpan.Zero;
            });
            var socket = client.CreateSocket();
            socket.CanConnect = true;
            var sub = new SocketConnection(new TraceLogger(), new TestWebsocketFactory(socket), new WebSocketParameters(new Uri("https://localhost/"), ReconnectPolicy.Disabled), client.SubClient, "");
            client.SubClient.ConnectSocketSub(sub);

            var subscription = new TestSubscription<Dictionary<string, string>>(Mock.Of<ILogger>(), (messageEvent) => { });
            var ups = new UpdateSubscription(sub, subscription);
            sub.AddSubscription(subscription);

            // act
            client.UnsubscribeAsync(ups).Wait();

            // assert
            Assert.That(socket.Connected == false);
        }

        [TestCase()]
        public void UnsubscribingAll_Should_CloseAllSockets()
        {
            // arrange
            var client = new TestSocketClient(options => { options.ReconnectInterval = TimeSpan.Zero; });
            var socket1 = client.CreateSocket();
            var socket2 = client.CreateSocket();
            socket1.CanConnect = true;
            socket2.CanConnect = true;
            var sub1 = new SocketConnection(new TraceLogger(), new TestWebsocketFactory(socket1), new WebSocketParameters(new Uri("https://localhost/"), ReconnectPolicy.Disabled), client.SubClient, "");
            var sub2 = new SocketConnection(new TraceLogger(), new TestWebsocketFactory(socket2), new WebSocketParameters(new Uri("https://localhost/"), ReconnectPolicy.Disabled), client.SubClient, "");
            client.SubClient.ConnectSocketSub(sub1);
            client.SubClient.ConnectSocketSub(sub2);
            var subscription1 = new TestSubscription<Dictionary<string, string>>(Mock.Of<ILogger>(), (messageEvent) => { });
            var subscription2 = new TestSubscription<Dictionary<string, string>>(Mock.Of<ILogger>(), (messageEvent) => { });

            sub1.AddSubscription(subscription1);
            sub2.AddSubscription(subscription2);
            var ups1 = new UpdateSubscription(sub1, subscription1);
            var ups2 = new UpdateSubscription(sub2, subscription2);

            // act
            client.UnsubscribeAllAsync().Wait();

            // assert
            Assert.That(socket1.Connected == false);
            Assert.That(socket2.Connected == false);
        }

        [TestCase()]
        public void FailingToConnectSocket_Should_ReturnError()
        {
            // arrange
            var client = new TestSocketClient(options => { options.ReconnectInterval = TimeSpan.Zero; });
            var socket = client.CreateSocket();
            socket.CanConnect = false;
            var sub1 = new SocketConnection(new TraceLogger(), new TestWebsocketFactory(socket), new WebSocketParameters(new Uri("https://localhost/"), ReconnectPolicy.Disabled), client.SubClient, "");

            // act
            var connectResult = client.SubClient.ConnectSocketSub(sub1);

            // assert
            ClassicAssert.IsFalse(connectResult.Success);
        }

        [TestCase()]
        public async Task ErrorResponse_ShouldNot_ConfirmSubscription()
        {
            // arrange
            var channel = "trade_btcusd";
            var client = new TestSocketClient(opt =>
            {
                opt.OutputOriginalData = true;
                opt.SocketSubscriptionsCombineTarget = 1;
            });
            var socket = client.CreateSocket();
            socket.CanConnect = true;
            client.SubClient.ConnectSocketSub(new SocketConnection(new TraceLogger(), new TestWebsocketFactory(socket), new WebSocketParameters(new Uri("https://localhost/"), ReconnectPolicy.Disabled), client.SubClient, ""));

            // act
            var sub = client.SubClient.SubscribeToSomethingAsync(channel, onUpdate => {}, ct: default);
            socket.InvokeMessage(JsonSerializer.Serialize(new { channel, action = "subscribe", status = "error" }));
            await sub;

            // assert
            ClassicAssert.IsTrue(client.SubClient.TestSubscription.Status != SubscriptionStatus.Subscribed);
        }

        [TestCase()]
        public async Task SuccessResponse_Should_ConfirmSubscription()
        {
            // arrange
            var channel = "trade_btcusd";
            var client = new TestSocketClient(opt =>
            {
                opt.OutputOriginalData = true;
                opt.SocketSubscriptionsCombineTarget = 1;
            });
            var socket = client.CreateSocket();
            socket.CanConnect = true;
            client.SubClient.ConnectSocketSub(new SocketConnection(new TraceLogger(), new TestWebsocketFactory(socket), new WebSocketParameters(new Uri("https://localhost/"), ReconnectPolicy.Disabled), client.SubClient, ""));

            // act
            var sub = client.SubClient.SubscribeToSomethingAsync(channel, onUpdate => {}, ct: default);
            socket.InvokeMessage(JsonSerializer.Serialize(new { channel, action = "subscribe", status = "confirmed" }));
            await sub;

            // assert
            Assert.That(client.SubClient.TestSubscription.Status == SubscriptionStatus.Subscribed);
        }
    }
}
