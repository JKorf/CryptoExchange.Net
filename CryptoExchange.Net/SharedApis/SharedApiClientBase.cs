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
        /// Shared API's available on this client
        /// </summary>
        IReadOnlyList<ISharedApi> SharedApis { get; }

        /// <summary>
        /// Get all capabilities matching the provided capability type.
        /// Example:
        /// <code>client.GetCapability&lt;IPlaceSpotOrder&gt;()</code>
        /// </summary>
        /// <typeparam name="T">The capability type</typeparam>
        /// <param name="tradingMode">Filter by supported trading mode</param>
        /// <param name="transport">Filter by transport method</param>
        IReadOnlyList<SharedCapabilityResolution<T>> GetCapabilities<T>(TradingMode? tradingMode = null, SharedTransport? transport = null)
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
        SharedCapabilityResolution<T>? GetCapability<T>(SharedTransport? transport = null)
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
        SharedCapabilityResolution<T>? GetCapability<T>(TradingMode tradingMode, SharedTransport? transport = null)
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
        SharedCapabilityResolution<T>? GetCapability<T>(SharedCapabilityReference<T> capability)
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
        SharedCapabilityResolution<T>? GetCapability<T>(SharedCapabilityReference<T> capability, TradingMode tradingMode)
            where T : ISharedApiCapability;
    }

    /// <summary>
    /// Base client for Shared API clients
    /// </summary>
    public abstract class SharedApiClientBase : ISharedApiClientBase
    {
        private readonly ISharedApi[] _sharedApis;
        private readonly SharedTransport[] _preferredTransports;

        /// <inheritdoc />
        public IReadOnlyList<ISharedApi> SharedApis => _sharedApis;

        /// <summary>
        /// ctor
        /// </summary>
        public SharedApiClientBase(
            IEnumerable<SharedTransport> transportPreference,
            params ISharedApi[] sharedApis)
        {
            _preferredTransports = transportPreference.Distinct().ToArray();
            _sharedApis = sharedApis.Distinct().ToArray();
        }

        /// <inheritdoc />
        public SharedCapabilityResolution<T>? GetCapability<T>(SharedTransport? transport = null)
            where T : ISharedApiCapability
        {
            return GetCapabilityCore<T>(null, transport);
        }

        /// <inheritdoc />
        public SharedCapabilityResolution<T>? GetCapability<T>(TradingMode tradingMode, SharedTransport? transport = null)
            where T : ISharedApiCapability
        {
            return GetCapabilityCore<T>(tradingMode, transport);
        }

        /// <inheritdoc />
        public SharedCapabilityResolution<T>? GetCapability<T>(SharedCapabilityReference<T> capability)
            where T : ISharedApiCapability
        {
            return GetCapabilityCore<T>(null, null);
        }

        /// <inheritdoc />
        public SharedCapabilityResolution<T>? GetCapability<T>(SharedCapabilityReference<T> capability, TradingMode tradingMode)
            where T : ISharedApiCapability
        {
            return GetCapabilityCore<T>(tradingMode, null);
        }

        /// <inheritdoc />
        public IReadOnlyList<SharedCapabilityResolution<T>> GetCapabilities<T>(
            TradingMode? tradingMode = null,
            SharedTransport? transport = null)
            where T : ISharedApiCapability
        {
            var result = new List<SharedCapabilityResolution<T>>();

            foreach (var sharedApi in _sharedApis)
            {
                if (sharedApi is not T capability)
                    continue;

                if (transport != null && sharedApi.Transport != transport)
                    continue;

                var options = GetMatchingOptions<T>(sharedApi, tradingMode);
                if (options == null)
                    continue;

                result.Add(new SharedCapabilityResolution<T>(
                    capability,
                    options));
            }

            // Preserve Shared API registration order within each transport.
            return result
                .OrderBy(x => GetTransportPriority(x.Capability.Transport))
                .ToArray();
        }

        private SharedCapabilityResolution<T>? GetCapabilityCore<T>(
            TradingMode? tradingMode,
            SharedTransport? selectedTransport = null)
            where T : ISharedApiCapability
        {
            return GetCapabilities<T>(tradingMode, selectedTransport)
                .FirstOrDefault();
        }

        private static CapabilityOptions? GetMatchingOptions<T>(
            ISharedApi sharedApi,
            TradingMode? tradingMode)
            where T : ISharedApiCapability
        {
            var requestedType = typeof(T);

            return sharedApi.Capabilities
                .Where(options =>
                    options.Supported
                    && IsCapabilityTypeMatch(
                        requestedType,
                        options.CapabilityType)
                    && (tradingMode == null
                        || options.SupportedTradingModes.Contains(
                            tradingMode.Value)))
                // Prefer options declared for the exact requested interface.
                .OrderByDescending(options =>
                    options.CapabilityType == requestedType)
                .FirstOrDefault();
        }

        private static bool IsCapabilityTypeMatch(
            Type requestedType,
            Type optionsCapabilityType)
        {
            return optionsCapabilityType.IsAssignableFrom(requestedType)
                || requestedType.IsAssignableFrom(optionsCapabilityType);
        }

        private int GetTransportPriority(SharedTransport transport)
        {
            var index = Array.IndexOf(_preferredTransports, transport);
            return index == -1 ? int.MaxValue : index;
        }
    }
}
