using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Sockets
{
    public interface ISocketConnection
    {
        SocketApiClient ApiClient { get; set; }
        bool Authenticated { get; set; }
        bool Connected { get; }
        Uri ConnectionUri { get; }
        int SocketId { get; }
        string Tag { get; set; }

        event Action? ConnectionClosed;

        Task<CallResult> ConnectAsync(CancellationToken ct);
        Task CloseAsync();
        void Dispose();

        //ValueTask<CallResult> SendStringAsync(int requestId, string data, int weight);
        //ValueTask<CallResult> SendAsync<T>(int requestId, T obj, int weight);
        //ValueTask<CallResult> SendBytesAsync(int requestId, byte[] data, int weight);
    }
}