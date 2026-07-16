using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Options;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Net.Http;

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
            { "Weex", "b-WEEX111124-" },
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

            if (clientOrderId?.Length + reservedLength > maxLength)
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

        private static readonly HashSet<string> _stableCoins = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // USD
            "USDT", "USDC", "DAI", "FDUSD", "USDE", "TUSD", "USDP", "PYUSD", "GUSD",
            "USDD", "LUSD", "USDJ", "SUSD", "ZUSD", "BUSD", "USTC", "USDX", "USDK",
            "CUSD", "USD1", "USD0", "XUSD", "BFUSD", "USDS", "RLUSD",  
            // EUR
            "EURS", "EURC", "EURI", "EURT", "AGEUR", "CEUR", "AEUR",
            // Other
            "CNYT",  // CNY
            "CREAL", // BRL
            "XSGD",  // SGD
            "GYEN",  // JPY
        };

        private static readonly HashSet<string> _commodities = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // Metals
            "XAU", "XAUT", "XAG", "XPT", "XPD", "XPL", "COPPER", "PAXG", "XNI", "XCU", "XAL", 
             // Energy
            "BZ", "NATGAS", "NGAS", "CL", "XTI", "UKOIL"
        };

        private static readonly HashSet<string> _cryptoCurrencies = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // Top 50 cryptos, will need to update periodically
            "BTC", "ETH", "BNB", "XRP", "SOL", "TRX", "HYPE", "DOGE", "RAIN", "ZEC",
            "LEO", "WBT", "XLM", "LINK", "XMR", "ADA", "CC", "BCH", "GRAM", "LTC",
            "SUI", "HBAR", "AVAX", "NEAR", "CRO", "SHIB", "UNI", "TAO", "WLFI", "ONDO",
            "OKB", "ASTER", "DEXE", "HTX", "M", "SKY", "MNT", "AAVE", "DOT", "WLD",
            "MORPHO", "ICP", "BGB", "PEPE", "ETC", "QNT", "STABLE", "KCS", "POL", "ADI"
        };

        public static bool IsStableCoin(string asset, params string[] additionalStableCoins)
        {
            if (string.IsNullOrEmpty(asset))
                return false;

            return _stableCoins.Contains(asset) || (additionalStableCoins != null && additionalStableCoins.Contains(asset, StringComparer.OrdinalIgnoreCase));
        }

        public static bool IsCommodity(string asset, params string[] additionalCommodities)
        {
            if (string.IsNullOrEmpty(asset))
                return false;

            return _commodities.Contains(asset) || (additionalCommodities != null && additionalCommodities.Contains(asset, StringComparer.OrdinalIgnoreCase));
        }

        public static bool IsCryptoCurrency(string asset)
        {
            if (string.IsNullOrEmpty(asset))
                return false;

            return _cryptoCurrencies.Contains(asset);
        }

        /// <summary>
        /// Create a new HttpMessageHandler instance
        /// </summary>  
        public static HttpMessageHandler CreateHttpClientMessageHandler(RestExchangeOptions options)
        {
#if NET5_0_OR_GREATER
            var socketHandler = new SocketsHttpHandler();
            try
            {
                if (options.HttpKeepAliveInterval != null && options.HttpKeepAliveInterval != TimeSpan.Zero)
                {
                    socketHandler.KeepAlivePingPolicy = HttpKeepAlivePingPolicy.Always;
                    socketHandler.KeepAlivePingDelay = options.HttpKeepAliveInterval.Value;
                    socketHandler.KeepAlivePingTimeout = TimeSpan.FromSeconds(10);
                }

                socketHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                socketHandler.DefaultProxyCredentials = CredentialCache.DefaultCredentials;

                socketHandler.EnableMultipleHttp2Connections = options.HttpEnableMultipleHttp2Connections;
                socketHandler.PooledConnectionLifetime = options.HttpPooledConnectionLifetime;
                socketHandler.PooledConnectionIdleTimeout = options.HttpPooledConnectionIdleTimeout;
                socketHandler.MaxConnectionsPerServer = options.HttpMaxConnectionsPerServer;
            }
            catch (PlatformNotSupportedException) { }
            catch (NotImplementedException) { } // Mono runtime throws NotImplementedException

            if (options.Proxy != null)
            {
                socketHandler.Proxy = new WebProxy
                {
                    Address = new Uri($"{options.Proxy.Host}:{options.Proxy.Port}"),
                    Credentials = options.Proxy.Password == null ? null : new NetworkCredential(options.Proxy.Login, options.Proxy.Password)
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

            if (options.Proxy != null)
            {
                httpHandler.Proxy = new WebProxy
                {
                    Address = new Uri($"{options.Proxy.Host}:{options.Proxy.Port}"),
                    Credentials = options.Proxy.Password == null ? null : new NetworkCredential(options.Proxy.Login, options.Proxy.Password)
                };
            }
            return httpHandler;
#endif
        }
    }
}
