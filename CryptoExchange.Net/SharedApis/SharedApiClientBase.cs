using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Base interface for Shared API clients
    /// </summary>
    public interface ISharedApiClientBase
    {
        /// <summary>
        /// Get all capabilities matching the provided capability type.
        /// Example:
        /// <code>client.GetCapability&lt;IPlaceSpotOrder&gt;()</code>
        /// </summary>
        /// <typeparam name="T">The capability type</typeparam>
        /// <param name="tradingMode">Filter by supported trading mode</param>
        /// <param name="transport">Filter by transport method</param>
        IReadOnlyList<T> GetCapabilities<T>(TradingMode? tradingMode = null, SharedTransport? transport = null)
            where T : ISharedApiCapability;

        /// <summary>
        /// Get the requested capability type. If more than one implementation is available the first one is returned.
        /// In this case prefer using <see cref="GetCapability{T}(TradingMode, SharedTransport?)"/> to select a specific trading mode.
        /// Example:
        /// <code>client.GetCapability&lt;IPlaceSpotOrder&gt;(SharedTransport.Socket)</code>
        /// </summary>
        /// <typeparam name="T">The capability type</typeparam>
        /// <param name="transport">The transport method</param>
        /// <returns>The requested capability type or null if not found</returns>
        T? GetCapability<T>(SharedTransport? transport = null)
            where T : ISharedApiCapability;

        /// <summary>
        /// Get the requested capability type for a specific trading mode.
        /// Example:
        /// <code>client.GetCapability&lt;IPlaceFuturesOrder&gt;(TradingMode.PerpetualLinear, SharedTransport.Rest)</code>
        /// </summary>
        /// <typeparam name="T">The capability type</typeparam>
        /// <param name="tradingMode">The trading mode</param>
        /// <param name="transport">The transport method</param>
        /// <returns>The requested capability type or null if not found</returns>
        T? GetCapability<T>(TradingMode tradingMode, SharedTransport? transport = null)
            where T : ISharedApiCapability;

        /// <summary>
        /// Get the capability matching the provided capability reference. Get a capability reference using <see cref="SharedCapabilities"/>.
        /// If more than one implementation is available the first one is returned.<br />
        /// Example:
        /// <code>client.GetCapability(SharedCapabilities.Orders.Spot.PlaceOrder.Rest)</code>
        /// </summary>
        /// <typeparam name="T">The capability type</typeparam>
        /// <param name="capability">The capability reference</param>
        /// <returns>The requested capability type or null if not found</returns>
        T? GetCapability<T>(SharedCapabilityReference<T> capability)
            where T : ISharedApiCapability;

        /// <summary>
        /// Get the capability matching the provided capability reference for a specific trading mode. Get a capability reference using <see cref="SharedCapabilities"/>.
        /// If more than one implementation is available the first one is returned.<br />
        /// Example:
        /// <code>client.GetCapability(SharedCapabilities.Orders.Futures.PlaceOrder.Rest, TradingMode.PerpetualLinear)</code>
        /// </summary>
        /// <typeparam name="T">The capability type</typeparam>
        /// <param name="capability">The capability reference</param>
        /// <param name="tradingMode">The trading mode</param>
        /// <returns>The requested capability type or null if not found</returns>
        T? GetCapability<T>(SharedCapabilityReference<T> capability, TradingMode tradingMode)
            where T : ISharedApiCapability;
    }

    /// <summary>
    /// Base client for Shared API clients
    /// </summary>
    public abstract class SharedApiClientBase : ISharedApiClientBase
    {
        private readonly ISharedApiCapability[] _sharedApis;
        private readonly SharedTransport[] _preferredTransports;

        /// <summary>
        /// ctor
        /// </summary>
        public SharedApiClientBase(SharedTransport[] transportPref, params ISharedApiCapability[] allCapabilities)
        {
            _preferredTransports = transportPref;
            _sharedApis = allCapabilities.ToArray();
        }

        /// <inheritdoc />
        public IReadOnlyList<T> GetCapabilities<T>(TradingMode? tradingMode = null, SharedTransport? transport = null)
            where T : ISharedApiCapability
        {
            return _sharedApis
                .OfType<T>()
                .Where(x =>
                    (tradingMode == null || x.SupportedTradingModes.Contains(tradingMode.Value))
                    && (transport == null || x.Transport == transport)
                    )
                .ToArray();
        }

        /// <inheritdoc />
        public T? GetCapability<T>(SharedTransport? transport = null)
            where T : ISharedApiCapability
        {
            return GetCapabilityCore<T>(null, transport);
        }

        /// <inheritdoc />
        public T? GetCapability<T>(TradingMode tradingMode, SharedTransport? transport = null)
            where T : ISharedApiCapability
        {
            return GetCapabilityCore<T>(tradingMode, transport);
        }

        /// <inheritdoc />
        public T? GetCapability<T>(SharedCapabilityReference<T> capability)
            where T : ISharedApiCapability
        {
            return GetCapabilityCore<T>(null, null);
        }

        /// <inheritdoc />
        public T? GetCapability<T>(SharedCapabilityReference<T> capability, TradingMode tradingMode)
            where T : ISharedApiCapability
        {
            return GetCapabilityCore<T>(tradingMode, null);
        }

        private T? GetCapabilityCore<T>(TradingMode? tradingMode, SharedTransport? selectedTransport = null)
                where T : ISharedApiCapability
        {
            var implementations = _sharedApis
                .OfType<T>()
                .Where(x => tradingMode == null
                    || x.SupportedTradingModes.Contains(tradingMode.Value))
                .ToArray();

            foreach (var transport in selectedTransport != null ? new[] { selectedTransport! } : _preferredTransports)
            {
                var matching = implementations
                    .Where(x => x.Transport == transport)
                    .ToArray();

                if (matching.Length > 0)
                    return matching[0];
            }

            return default;
        }
    }
}
