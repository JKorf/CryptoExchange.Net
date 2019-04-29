using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using CryptoExchange.Net.Logging;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sockets;
using CryptoExchange.Net.UnitTests.TestImplementations;
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
            // arrange
            // act
            var client = new TestSocketClient(new SocketClientOptions()
            {
                BaseAddress = "http://test.address.com",
                ReconnectInterval = TimeSpan.FromSeconds(6)
            });


            // assert
            Assert.IsTrue(client.BaseAddress == "http://test.address.com");
            Assert.IsTrue(client.ReconnectInterval.TotalSeconds == 6);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ConnectSocket_Should_ReturnConnectionResult(bool canConnect)
        {
            // arrange
            var client = new TestSocketClient();
            var socket = client.CreateSocket();
            socket.CanConnect = canConnect;

            // act
            var connectResult = client.ConnectSocketSub(new SocketConnection(client, new Log(), socket));

            // assert
            Assert.IsTrue(connectResult.Success == canConnect);
        }

        //[TestCase]
        //public void SocketMessages_Should_BeProcessedInDataHandlers()
        //{
        //    // arrange
        //    var client = new TestSocketClient(new SocketClientOptions() { ReconnectInterval = TimeSpan.Zero, LogVerbosity = LogVerbosity.Debug });
        //    var socket = client.CreateSocket();
        //    socket.ShouldReconnect = true;
        //    socket.CanConnect = true;
        //    socket.DisconnectTime = DateTime.UtcNow;
        //    var sub = new SocketConnection(socket);
        //    var rstEvent = new ManualResetEvent(false);
        //    JToken result = null;
        //    sub.MessageHandlers.Add("TestHandler", (subs, data) =>
        //    {
        //        result = data;
        //        rstEvent.Set();
        //        return true;

        //    });
        //    client.ConnectSocketSub(sub);

        //    // act
        //    socket.InvokeMessage("{\"property\": 123}");
        //    rstEvent.WaitOne(1000);

        //    // assert
        //    Assert.IsTrue((int)result["property"] == 123);
        //}

        //[TestCase]
        //public void SocketMessages_Should_NotBeProcessedInSubsequentHandlersIfHandlerReturnsTrue()
        //{
        //    // arrange
        //    var client = new TestSocketClient(new SocketClientOptions() { ReconnectInterval = TimeSpan.Zero, LogVerbosity = LogVerbosity.Debug });
        //    var socket = client.CreateSocket();
        //    socket.ShouldReconnect = true;
        //    socket.CanConnect = true;
        //    socket.DisconnectTime = DateTime.UtcNow;
        //    var sub = new SocketConnection(socket);
        //    var rstEvent1 = new ManualResetEvent(false);
        //    var rstEvent2 = new ManualResetEvent(false);
        //    JToken result1 = null;
        //    JToken result2 = null;
        //    sub.MessageHandlers.Add("TestHandler", (subs, data) =>
        //    {
        //        result1 = data;
        //        rstEvent1.Set();
        //        return true;
        //    });
        //    sub.MessageHandlers.Add("TestHandlerNotHit", (subs, data) =>
        //    {
        //        result2 = data;
        //        rstEvent2.Set();
        //        return true;
        //    });
        //    client.ConnectSocketSub(sub);

        //    // act
        //    socket.InvokeMessage("{\"property\": 123}");
        //    rstEvent1.WaitOne(100);
        //    rstEvent2.WaitOne(100);

        //    // assert
        //    Assert.IsTrue((int)result1["property"] == 123);
        //    Assert.IsTrue(result2 == null);
        //}

        //[TestCase]
        //public void SocketMessages_Should_BeProcessedInSubsequentHandlersIfHandlerReturnsFalse()
        //{
        //    // arrange
        //    var client = new TestSocketClient(new SocketClientOptions() { ReconnectInterval = TimeSpan.Zero, LogVerbosity = LogVerbosity.Debug });
        //    var socket = client.CreateSocket();
        //    socket.ShouldReconnect = true;
        //    socket.CanConnect = true;
        //    socket.DisconnectTime = DateTime.UtcNow;
        //    var sub = new SocketConnection(socket);
        //    var rstEvent = new ManualResetEvent(false);
        //    JToken result = null;
        //    sub.MessageHandlers.Add("TestHandlerNotProcessing", (subs, data) =>
        //    {
        //        return false;
        //    });
        //    sub.MessageHandlers.Add("TestHandler", (subs, data) =>
        //    {
        //        result = data;
        //        rstEvent.Set();
        //        return true;
        //    });
        //    client.ConnectSocketSub(sub);

        //    // act
        //    socket.InvokeMessage("{\"property\": 123}");
        //    rstEvent.WaitOne(100);

        //    // assert
        //    Assert.IsTrue((int)result["property"] == 123);
        //}


        //[TestCase]
        //public void DisconnectedSocket_Should_Reconnect()
        //{
        //    // arrange
        //    bool reconnected = false;
        //    var client = new TestSocketClient(new SocketClientOptions(){ReconnectInterval = TimeSpan.Zero ,LogVerbosity = LogVerbosity.Debug});
        //    var socket = client.CreateSocket();
        //    socket.ShouldReconnect = true;
        //    socket.CanConnect = true;
        //    socket.DisconnectTime = DateTime.UtcNow;
        //    var sub = new SocketConnection(socket);
        //    client.ConnectSocketSub(sub);
        //    var rstEvent = new ManualResetEvent(false);
        //    client.OnReconnect += () =>
        //    {
        //        reconnected = true;
        //        rstEvent.Set();
        //        return true;
        //    };

        //    // act
        //    socket.InvokeClose();
        //    rstEvent.WaitOne(1000);

        //    // assert
        //    Assert.IsTrue(reconnected);
        //}

        //[TestCase()]
        //public void UnsubscribingStream_Should_CloseTheSocket()
        //{
        //    // arrange
        //    var client = new TestSocketClient(new SocketClientOptions() { ReconnectInterval = TimeSpan.Zero, LogVerbosity = LogVerbosity.Debug });
        //    var socket = client.CreateSocket();
        //    socket.CanConnect = true;
        //    var sub = new SocketConnection(socket);
        //    client.ConnectSocketSub(sub);
        //    var ups = new UpdateSubscription(sub);

        //    // act
        //    client.Unsubscribe(ups).Wait();

        //    // assert
        //    Assert.IsTrue(socket.Connected == false);
        //}

        //[TestCase()]
        //public void UnsubscribingAll_Should_CloseAllSockets()
        //{
        //    // arrange
        //    var client = new TestSocketClient(new SocketClientOptions() { ReconnectInterval = TimeSpan.Zero, LogVerbosity = LogVerbosity.Debug });
        //    var socket1 = client.CreateSocket();
        //    var socket2 = client.CreateSocket();
        //    socket1.CanConnect = true;
        //    socket2.CanConnect = true;
        //    var sub1 = new SocketConnection(socket1);
        //    var sub2 = new SocketConnection(socket2);
        //    client.ConnectSocketSub(sub1);
        //    client.ConnectSocketSub(sub2);

        //    // act
        //    client.UnsubscribeAll().Wait();

        //    // assert
        //    Assert.IsTrue(socket1.Connected == false);
        //    Assert.IsTrue(socket2.Connected == false);
        //}

        //[TestCase()]
        //public void FailingToConnectSocket_Should_ReturnError()
        //{
        //    // arrange
        //    var client = new TestSocketClient(new SocketClientOptions() { ReconnectInterval = TimeSpan.Zero, LogVerbosity = LogVerbosity.Debug });
        //    var socket = client.CreateSocket();
        //    socket.CanConnect = false;
        //    var sub = new SocketConnection(socket);

        //    // act
        //    var connectResult = client.ConnectSocketSub(sub);

        //    // assert
        //    Assert.IsFalse(connectResult.Success);
        //}
        
        //[Test]
        //public void WhenResubscribeFails_Socket_ShouldReconnect()
        //{
        //    // arrange
        //    int reconnected = 0;
        //    var client = new TestSocketClient(new SocketClientOptions() { ReconnectInterval = TimeSpan.FromMilliseconds(1), LogVerbosity = LogVerbosity.Debug });
        //    var socket = client.CreateSocket();
        //    socket.ShouldReconnect = true;
        //    socket.CanConnect = true;
        //    socket.DisconnectTime = DateTime.UtcNow;
        //    var sub = new SocketConnection(socket);
        //    client.ConnectSocketSub(sub);
        //    var rstEvent = new ManualResetEvent(false);
        //    client.OnReconnect += () =>
        //    {
        //        reconnected++;
        //        rstEvent.Set();
        //        return reconnected == 2;
        //    };

        //    // act
        //    socket.InvokeClose();
        //    rstEvent.WaitOne(1000);
        //    Thread.Sleep(100);

        //    // assert
        //    Assert.IsTrue(reconnected == 2);
        //}
    }
}
