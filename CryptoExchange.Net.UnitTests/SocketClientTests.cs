using System;
using System.Threading;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sockets;
using CryptoExchange.Net.UnitTests.TestImplementations;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

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
            var client = new TestSocketClient(new SocketClientOptions("")
            {
                BaseAddress = "http://test.address.com",
                ReconnectInterval = TimeSpan.FromSeconds(6)
            });


            //assert
            Assert.IsTrue(client.BaseAddress == "http://test.address.com/");
            Assert.IsTrue(client.ReconnectInterval.TotalSeconds == 6);
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
            var connectResult = client.ConnectSocketSub(new SocketConnection(client, socket));

            //assert
            Assert.IsTrue(connectResult.Success == canConnect);
        }

        [TestCase]
        public void SocketMessages_Should_BeProcessedInDataHandlers()
        {
            // arrange
            var client = new TestSocketClient(new SocketClientOptions("") { ReconnectInterval = TimeSpan.Zero, LogLevel = LogLevel.Debug });
            var socket = client.CreateSocket();
            socket.ShouldReconnect = true;
            socket.CanConnect = true;
            socket.DisconnectTime = DateTime.UtcNow;
            var sub = new SocketConnection(client, socket);
            var rstEvent = new ManualResetEvent(false);
            JToken result = null;
            sub.AddSubscription(SocketSubscription.CreateForIdentifier(10, "TestHandler", true, (messageEvent) =>
            {
                result = messageEvent.JsonData;
                rstEvent.Set();
            }));
            client.ConnectSocketSub(sub);

            // act
            socket.InvokeMessage("{\"property\": 123}");
            rstEvent.WaitOne(1000);

            // assert
            Assert.IsTrue((int)result["property"] == 123);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void SocketMessages_Should_ContainOriginalDataIfEnabled(bool enabled)
        {
            // arrange
            var client = new TestSocketClient(new SocketClientOptions("") { ReconnectInterval = TimeSpan.Zero, LogLevel = LogLevel.Debug, OutputOriginalData = enabled });
            var socket = client.CreateSocket();
            socket.ShouldReconnect = true;
            socket.CanConnect = true;
            socket.DisconnectTime = DateTime.UtcNow;
            var sub = new SocketConnection(client, socket);
            var rstEvent = new ManualResetEvent(false);
            string original = null;
            sub.AddSubscription(SocketSubscription.CreateForIdentifier(10, "TestHandler", true, (messageEvent) =>
            {
                original = messageEvent.OriginalData;
                rstEvent.Set();
            }));
            client.ConnectSocketSub(sub);

            // act
            socket.InvokeMessage("{\"property\": 123}");
            rstEvent.WaitOne(1000);

            // assert
            Assert.IsTrue(original == (enabled ? "{\"property\": 123}" : null));
        }

        [TestCase]
        public void DisconnectedSocket_Should_Reconnect()
        {
            // arrange
            bool reconnected = false;
            var client = new TestSocketClient(new SocketClientOptions("") { ReconnectInterval = TimeSpan.Zero, LogLevel = LogLevel.Debug });
            var socket = client.CreateSocket();
            socket.ShouldReconnect = true;
            socket.CanConnect = true;
            socket.DisconnectTime = DateTime.UtcNow;
            var sub = new SocketConnection(client, socket);
            sub.ShouldReconnect = true;
            client.ConnectSocketSub(sub);
            var rstEvent = new ManualResetEvent(false);
            sub.ConnectionRestored += (a) =>
            {
                reconnected = true;
                rstEvent.Set();
            };

            // act
            socket.InvokeClose();
            rstEvent.WaitOne(1000);

            // assert
            Assert.IsTrue(reconnected);
        }

        [TestCase()]
        public void UnsubscribingStream_Should_CloseTheSocket()
        {
            // arrange
            var client = new TestSocketClient(new SocketClientOptions("") { ReconnectInterval = TimeSpan.Zero, LogLevel = LogLevel.Debug });
            var socket = client.CreateSocket();
            socket.CanConnect = true;
            var sub = new SocketConnection(client, socket);
            client.ConnectSocketSub(sub);
            var ups = new UpdateSubscription(sub, SocketSubscription.CreateForIdentifier(10, "Test", true, (e) => {}));

            // act
            client.UnsubscribeAsync(ups).Wait();

            // assert
            Assert.IsTrue(socket.Connected == false);
        }

        [TestCase()]
        public void UnsubscribingAll_Should_CloseAllSockets()
        {
            // arrange
            var client = new TestSocketClient(new SocketClientOptions("") { ReconnectInterval = TimeSpan.Zero, LogLevel = LogLevel.Debug });
            var socket1 = client.CreateSocket();
            var socket2 = client.CreateSocket();
            socket1.CanConnect = true;
            socket2.CanConnect = true;
            var sub1 = new SocketConnection(client, socket1);
            var sub2 = new SocketConnection(client, socket2);
            client.ConnectSocketSub(sub1);
            client.ConnectSocketSub(sub2);

            // act
            client.UnsubscribeAllAsync().Wait();

            // assert
            Assert.IsTrue(socket1.Connected == false);
            Assert.IsTrue(socket2.Connected == false);
        }

        [TestCase()]
        public void FailingToConnectSocket_Should_ReturnError()
        {
            // arrange
            var client = new TestSocketClient(new SocketClientOptions("") { ReconnectInterval = TimeSpan.Zero, LogLevel = LogLevel.Debug });
            var socket = client.CreateSocket();
            socket.CanConnect = false;
            var sub = new SocketConnection(client, socket);

            // act
            var connectResult = client.ConnectSocketSub(sub);

            // assert
            Assert.IsFalse(connectResult.Success);
        }
    }
}
