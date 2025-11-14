using CryptoExchange.Net.Objects;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CryptoExchange.Net
{
    /// <summary>
    /// Helpers for client libraries
    /// </summary>
    public static class LibraryHelpers
    {
        private static ILogger? _staticLogger;
        /// <summary>
        /// Static logger
        /// </summary>
        public static ILogger? StaticLogger
        {
            get => _staticLogger;
            internal set  
            {
                if (_staticLogger != null)
                    return;

                _staticLogger = value;
            }
        }

        /// <summary>
        /// Client order id separator
        /// </summary>
        public const string ClientOrderIdSeparator = "JK";

        private static Dictionary<string, string> _defaultClientReferences = new Dictionary<string, string>()
        {
            { "Binance.Spot", "x-VICEW9VV" },
            { "Binance.Futures", "x-d63tKbx3" },
            { "BingX", "easytrading" },
            { "Bitfinex", "kCCe-CNBO" },
            { "Bitget", "6x21p" },
            { "BitMart", "EASYTRADING0001" },
            { "BitMEX", "Sent from JKorf" },
            { "BloFin", "5c07cf695885c282" },
            { "Bybit", "Zx000356" },
            { "CoinEx", "x-147866029-" },
            { "GateIo", "copytraderpw" },
            { "HTX", "AA1ef14811" },
            { "Kucoin.FuturesName", "Easytradingfutures" },
            { "Kucoin.FuturesKey", "9e08c05f-454d-4580-82af-2f4c7027fd00" },
            { "Kucoin.SpotName", "Easytrading" },
            { "Kucoin.SpotKey", "f8ae62cb-2b3d-420c-8c98-e1c17dd4e30a" },
            { "Mexc", "EASYT" },
            { "OKX", "1425d83a94fbBCDE" },
            { "XT", "4XWeqN10M1fcoI5L" },
        };

        /// <summary>
        /// Apply broker id to a client order id
        /// </summary>
        /// <param name="clientOrderId"></param>
        /// <param name="brokerId"></param>
        /// <param name="maxLength"></param>
        /// <param name="allowValueAdjustment"></param>
        /// <returns></returns>
        public static string ApplyBrokerId(string? clientOrderId, string brokerId, int maxLength, bool allowValueAdjustment)
        {
            var reservedLength = brokerId.Length + ClientOrderIdSeparator.Length;

            if ((clientOrderId?.Length + reservedLength) > maxLength)
                return clientOrderId!;

            if (!string.IsNullOrEmpty(clientOrderId))
            {
                if (allowValueAdjustment)
                    clientOrderId = brokerId + ClientOrderIdSeparator + clientOrderId;

                return clientOrderId!;
            }
            else
            {
                clientOrderId = ExchangeHelpers.AppendRandomString(brokerId + ClientOrderIdSeparator, maxLength);
            }

            return clientOrderId;
        }

        /// <summary>
        /// Get the client reference for an exchange if available
        /// </summary>
        public static string GetClientReference(Func<string?> optionsReference, string exchange, string? topic = null)
        {
            var optionsValue = optionsReference();
            if (!string.IsNullOrEmpty(optionsValue))
                return optionsValue!;

            var key = exchange;
            if (topic != null)
                key += "." + topic;

            return _defaultClientReferences.TryGetValue(key, out var id) ? id : throw new KeyNotFoundException($"{exchange} not found in configuration");
        }

        /// <summary>
        /// Create a new HttpMessageHandler instance
        /// </summary>  
        public static HttpMessageHandler CreateHttpClientMessageHandler(ApiProxy? proxy, TimeSpan? keepAliveInterval)
        {
#if NET5_0_OR_GREATER
            var socketHandler = new SocketsHttpHandler();
            try
            {
                if (keepAliveInterval != null && keepAliveInterval != TimeSpan.Zero)
                {
                    socketHandler.KeepAlivePingPolicy = HttpKeepAlivePingPolicy.Always;
                    socketHandler.KeepAlivePingDelay = keepAliveInterval.Value;
                    socketHandler.KeepAlivePingTimeout = TimeSpan.FromSeconds(10);
                }

                socketHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                socketHandler.DefaultProxyCredentials = CredentialCache.DefaultCredentials;
            }
            catch (PlatformNotSupportedException) { }
            catch (NotImplementedException) { } // Mono runtime throws NotImplementedException

            if (proxy != null)
            {
                socketHandler.Proxy = new WebProxy
                {
                    Address = new Uri($"{proxy.Host}:{proxy.Port}"),
                    Credentials = proxy.Password == null ? null : new NetworkCredential(proxy.Login, proxy.Password)
                };
            }
            return socketHandler;
#else
            var httpHandler = new HttpClientHandler();
            try
            {
                httpHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                httpHandler.DefaultProxyCredentials = CredentialCache.DefaultCredentials;
            }
            catch (PlatformNotSupportedException) { }
            catch (NotImplementedException) { } // Mono runtime throws NotImplementedException

            if (proxy != null)
            {
                httpHandler.Proxy = new WebProxy
                {
                    Address = new Uri($"{proxy.Host}:{proxy.Port}"),
                    Credentials = proxy.Password == null ? null : new NetworkCredential(proxy.Login, proxy.Password)
                };
            }
            return httpHandler;
#endif
        }

        public static async ValueTask WhenAll(IReadOnlyList<ValueTask> tasks)
        {
            if (tasks.Count == 0)
                return;

            List<Task>? toAwait = null;

            int completedTasks = 0;

            for (int i = 0; i < tasks.Count; i++)
            {
                if (!tasks[i].IsCompletedSuccessfully)
                {
                    toAwait ??= new();
                    toAwait.Add(tasks[i].AsTask());
                }
                else
                {
                    completedTasks++;
                }
            }

            if (completedTasks != tasks.Count)
                await Task.WhenAll(toAwait!).ConfigureAwait(false);
        }

        public static ValueTask WhenAll(IEnumerable<ValueTask> tasks)
        {
            return WhenAll(tasks.ToList());
        }

    }
}
