using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Trackers.Sessions.Interfaces
{
    public interface ISessionFeature : IAsyncDisposable
    {
        Task<CallResult> StartAsync(CancellationToken ct);
        Task StopAsync();
    }
}
