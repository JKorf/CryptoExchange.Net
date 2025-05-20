using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;

#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
#pragma warning disable IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.

namespace CryptoExchange.Net.Testing.Implementations
{
    internal class TestSocket : IWebsocket
    {
        public event Action<string>? OnMessageSend;

        public bool CanConnect { get; set; } = true;
        public bool Connected { get; set; }

        public event Func<Task>? OnClose;
#pragma warning disable 0067
        public event Func<Task>? OnReconnected;
        public event Func<Task>? OnReconnecting;
        public event Func<int, Task>? OnRequestRateLimited;
        public event Func<Task>? OnConnectRateLimited;
        public event Func<Exception, Task>? OnError;
#pragma warning restore 0067
        public event Func<int, Task>? OnRequestSent;
        public event Func<WebSocketMessageType, ReadOnlyMemory<byte>, Task>? OnStreamMessage;
        public event Func<Task>? OnOpen;

        public int Id { get; }
        public bool IsClosed => !Connected;
        public bool IsOpen => Connected;
        public double IncomingKbps => 0;
        public Uri Uri { get; set; }
        public Func<Task<Uri?>>? GetReconnectionUrl { get; set; }

        public static int lastId = 0;
        public static object lastIdLock = new object();

        public TestSocket(string address)
        {
            Uri = new Uri(address);
            lock (lastIdLock)
            {
                Id = lastId + 1;
                lastId++;
            }
        }

        public Task<CallResult> ConnectAsync(CancellationToken ct)
        {
            Connected = CanConnect;
            return Task.FromResult(CanConnect ? new CallResult(null) : new CallResult(new CantConnectError()));
        }

        public bool Send(int requestId, string data, int weight)
        {
            if (!Connected)
                throw new Exception("Socket not connected");

            OnRequestSent?.Invoke(requestId);
            OnMessageSend?.Invoke(data);
            return true;
        }

        public Task CloseAsync()
        {
            Connected = false;
            return Task.FromResult(0);
        }

        public void InvokeClose()
        {
            Connected = false;
            OnClose?.Invoke();
        }

        public void InvokeOpen()
        {
            OnOpen?.Invoke();
        }

        public void InvokeMessage(string data)
        {
            OnStreamMessage?.Invoke(WebSocketMessageType.Text, new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(data))).Wait();
        }

        public void InvokeMessage<T>(T data)
        {
            OnStreamMessage?.Invoke(WebSocketMessageType.Text, new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data)))).Wait();
        }

        public Task ReconnectAsync() => throw new NotImplementedException();
        public void Dispose() { }

        public void UpdateProxy(ApiProxy? proxy) => throw new NotImplementedException();
    }
}
