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
            var client = new TestSocketClient(options =>
            {
                options.SubOptions.ApiCredentials = new Authentication.ApiCredentials("1", "2");
                options.SubOptions.MaxSocketConnections = 1;
            });


            //assert
            Assert.NotNull(client.SubClient.ApiOptions.ApiCredentials);
            Assert.AreEqual(1, client.SubClient.ApiOptions.MaxSocketConnections);
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
            var connectResult = client.SubClient.ConnectSocketSub(new SocketConnection(new TraceLogger(), client.SubClient, socket, null));

            //assert
            Assert.IsTrue(connectResult.Success == canConnect);
        }

        [TestCase]
        public void SocketMessages_Should_BeProcessedInDataHandlers()
        {
            // arrange
            var client = new TestSocketClient(options => {
                options.ReconnectInterval = TimeSpan.Zero;
            });
            var socket = client.CreateSocket();
            socket.ShouldReconnect = true;
            socket.CanConnect = true;
            socket.DisconnectTime = DateTime.UtcNow;
            var sub = new SocketConnection(new TraceLogger(), client.SubClient, socket, null);
            var rstEvent = new ManualResetEvent(false);
            JToken result = null;
            sub.AddSubscription(SocketSubscription.CreateForIdentifier(10, "TestHandler", true, false, (messageEvent) =>
            {
                result = messageEvent.JsonData;
                rstEvent.Set();
            }));
            client.SubClient.ConnectSocketSub(sub);

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
            var client = new TestSocketClient(options => {
                options.ReconnectInterval = TimeSpan.Zero;
                options.SubOptions.OutputOriginalData = enabled;
            });
            var socket = client.CreateSocket();
            socket.ShouldReconnect = true;
            socket.CanConnect = true;
            socket.DisconnectTime = DateTime.UtcNow;
            var sub = new SocketConnection(new TraceLogger(), client.SubClient, socket, null);
            var rstEvent = new ManualResetEvent(false);
            string original = null;
            sub.AddSubscription(SocketSubscription.CreateForIdentifier(10, "TestHandler", true, false, (messageEvent) =>
            {
                original = messageEvent.OriginalData;
                rstEvent.Set();
            }));
            client.SubClient.ConnectSocketSub(sub);

            // act
            socket.InvokeMessage("{\"property\": 123}");
            rstEvent.WaitOne(1000);

            // assert
            Assert.IsTrue(original == (enabled ? "{\"property\": 123}" : null));
        }

        [TestCase()]
        public void UnsubscribingStream_Should_CloseTheSocket()
        {
            // arrange
            var client = new TestSocketClient(options => {
                options.ReconnectInterval = TimeSpan.Zero;
            }); 
            var socket = client.CreateSocket();
            socket.CanConnect = true;
            var sub = new SocketConnection(new TraceLogger(), client.SubClient, socket, null);
            client.SubClient.ConnectSocketSub(sub);
            var us = SocketSubscription.CreateForIdentifier(10, "Test", true, false, (e) => { });
            var ups = new UpdateSubscription(sub, us);
            sub.AddSubscription(us);

            // act
            client.UnsubscribeAsync(ups).Wait();

            // assert
            Assert.IsTrue(socket.Connected == false);
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
            var sub1 = new SocketConnection(new TraceLogger(), client.SubClient, socket1, null);
            var sub2 = new SocketConnection(new TraceLogger(), client.SubClient, socket2, null);
            client.SubClient.ConnectSocketSub(sub1);
            client.SubClient.ConnectSocketSub(sub2);
            var us1 = SocketSubscription.CreateForIdentifier(10, "Test1", true, false, (e) => { });
            var us2 = SocketSubscription.CreateForIdentifier(11, "Test2", true, false, (e) => { });
            sub1.AddSubscription(us1);
            sub2.AddSubscription(us2);
            var ups1 = new UpdateSubscription(sub1, us1);
            var ups2 = new UpdateSubscription(sub2, us2);

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
            var client = new TestSocketClient(options => { options.ReconnectInterval = TimeSpan.Zero; });
            var socket = client.CreateSocket();
            socket.CanConnect = false;
            var sub1 = new SocketConnection(new TraceLogger(), client.SubClient, socket, null);

            // act
            var connectResult = client.SubClient.ConnectSocketSub(sub1);

            // assert
            Assert.IsFalse(connectResult.Success);
        }
    }
}
