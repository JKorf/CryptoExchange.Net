using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Exceptions;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets.Default.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Sockets.HighPerf
{
    /// <summary>
    /// A single socket connection focused on performance expecting JSON data
    /// </summary>
    /// <typeparam name="T">The type of updates this connection produces</typeparam>
    public class HighPerfJsonSocketConnection<T> : HighPerfSocketConnection<T>
    {
        private JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// ctor
        /// </summary>
        public HighPerfJsonSocketConnection(
            ILogger logger,
            IWebsocketFactory socketFactory,
            WebSocketParameters parameters,
            SocketApiClient apiClient,
            JsonSerializerOptions serializerOptions,
            string tag)
            : base(logger, socketFactory, parameters, apiClient, tag)
        {
            _jsonOptions = serializerOptions;
        }

        /// <inheritdoc />
        protected override async Task ProcessAsync(CancellationToken ct)
        {
            try
            {
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
#pragma warning disable IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
                await foreach (var update in JsonSerializer.DeserializeAsyncEnumerable<T>(_pipe.Reader, true, _jsonOptions, ct).ConfigureAwait(false))
#pragma warning restore IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
                {
                    foreach (var sub in _typedSubscriptions)
                        DelegateToSubscription(_typedSubscriptions[0], update!);
                }
            }
            catch (OperationCanceledException) { }
        }

    }
}
