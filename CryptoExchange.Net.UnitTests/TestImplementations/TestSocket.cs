//using System;
//using System.IO;
//using System.Net.WebSockets;
//using System.Security.Authentication;
//using System.Text;
//using System.Threading.Tasks;
//using CryptoExchange.Net.Interfaces;
//using CryptoExchange.Net.Objects;

//namespace CryptoExchange.Net.UnitTests.TestImplementations
//{
//    public class TestSocket: IWebsocket
//    {
//        public bool CanConnect { get; set; }
//        public bool Connected { get; set; }

//        public event Func<Task> OnClose;
//#pragma warning disable 0067
//        public event Func<Task> OnReconnected;
//        public event Func<Task> OnReconnecting;
//        public event Func<int, Task> OnRequestRateLimited;
//#pragma warning restore 0067
//        public event Func<int, Task> OnRequestSent;
//        public event Func<WebSocketMessageType, ReadOnlyMemory<byte>, Task> OnStreamMessage;
//        public event Func<Exception, Task> OnError;
//        public event Func<Task> OnOpen;
//        public Func<Task<Uri>> GetReconnectionUrl { get; set; }

//        public int Id { get; }
//        public bool ShouldReconnect { get; set; }
//        public TimeSpan Timeout { get; set; }
//        public Func<string, string> DataInterpreterString { get; set; }
//        public Func<byte[], string> DataInterpreterBytes { get; set; }
//        public DateTime? DisconnectTime { get; set; }
//        public string Url { get; }
//        public bool IsClosed => !Connected;
//        public bool IsOpen => Connected;
//        public bool PingConnection { get; set; }
//        public TimeSpan PingInterval { get; set; }
//        public SslProtocols SSLProtocols { get; set; }
//        public Encoding Encoding { get; set; }

//        public int ConnectCalls { get; private set; }
//        public bool Reconnecting { get; set; }
//        public string Origin { get; set; }
//        public int? RatelimitPerSecond { get; set; }

//        public double IncomingKbps => throw new NotImplementedException();

//        public Uri Uri => new Uri("");

//        public TimeSpan KeepAliveInterval { get; set; }

//        public static int lastId = 0;
//        public static object lastIdLock = new object();

//        public TestSocket()
//        {
//            lock (lastIdLock)
//            {
//                Id = lastId + 1;
//                lastId++;
//            }
//        }

//        public Task<CallResult> ConnectAsync()
//        {
//            Connected = CanConnect;
//            ConnectCalls++;
//            if (CanConnect)
//                InvokeOpen();
//            return Task.FromResult(CanConnect ? new CallResult(null) : new CallResult(new CantConnectError()));
//        }

//        public bool Send(int requestId, string data, int weight)
//        {
//            if(!Connected)
//                throw new Exception("Socket not connected");
//            OnRequestSent?.Invoke(requestId);
//            return true;
//        }

//        public void Reset()
//        {
//        }

//        public Task CloseAsync()
//        {
//            Connected = false;
//            DisconnectTime = DateTime.UtcNow;
//            OnClose?.Invoke();
//            return Task.FromResult(0);
//        }

//        public void SetProxy(string host, int port)
//        {
//            throw new NotImplementedException();
//        }
//        public void Dispose()
//        {
//        }

//        public void InvokeClose()
//        {
//            Connected = false;
//            DisconnectTime = DateTime.UtcNow;
//            Reconnecting = true;
//            OnClose?.Invoke();
//        }

//        public void InvokeOpen()
//        {
//            OnOpen?.Invoke();
//        }

//        public void InvokeMessage(string data)
//        {
//            OnStreamMessage?.Invoke(WebSocketMessageType.Text, new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(data))).Wait();
//        }

//        public void SetProxy(ApiProxy proxy)
//        {
//            throw new NotImplementedException();
//        }

//        public void InvokeError(Exception error)
//        {
//            OnError?.Invoke(error);
//        }
//        public Task ReconnectAsync() => Task.CompletedTask;
//    }
//}
