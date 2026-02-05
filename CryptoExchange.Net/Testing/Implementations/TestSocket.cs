using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sockets.Default;
using CryptoExchange.Net.Sockets.Default.Interfaces;

#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
#pragma warning disable IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.

namespace CryptoExchange.Net.Testing.Implementations
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class TestSocket : IWebsocket
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
        public event Func<Task>? OnOpen;

        public int Id { get; }
        public bool IsClosed => !Connected;
        public bool IsOpen => Connected;
        public double IncomingKbps => 0;
        public Uri Uri { get; set; }
        public Func<Task<Uri?>>? GetReconnectionUrl { get; set; }

        public static int lastId = 0;
        public DateTime? LastReceiveTime { get; }
#if NET9_0_OR_GREATER
        public static readonly Lock lastIdLock = new Lock();
#else
        public static readonly object lastIdLock = new object();
#endif

        public SocketConnection? Connection { get; set; }

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

        public bool Send(int requestId, byte[] data, int weight)
        {
            if (!Connected)
                throw new Exception("Socket not connected");

            OnRequestSent?.Invoke(requestId);
            OnMessageSend?.Invoke(Encoding.UTF8.GetString(data));
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
            if (Connection == null)
                throw new ArgumentNullException(nameof(Connection));

            Connection.HandleStreamMessage2(WebSocketMessageType.Text, Encoding.UTF8.GetBytes(data));
        }

        public async Task ReconnectAsync()
        {
            if (OnReconnecting != null)
                await OnReconnecting().ConfigureAwait(false);

            await Task.Delay(10).ConfigureAwait(false);

            if (OnReconnected != null)
                await OnReconnected().ConfigureAwait(false);
        }

        public void Dispose() { }

        public void UpdateProxy(ApiProxy? proxy) => throw new NotImplementedException();
    }
}
