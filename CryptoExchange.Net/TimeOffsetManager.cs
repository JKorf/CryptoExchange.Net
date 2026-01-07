using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net
{
    /// <summary>
    /// Manager for timing offsets in APIs
    /// </summary>
    public static class TimeOffsetManager
    {
        class SocketTimeOffset
        {
            private DateTime _lastRollOver = DateTime.UtcNow;
            private double? _fallbackLowest;
            private double? _currentLowestOffset;

            /// <summary>
            /// Get the estimated offset, resolves to the lowest offset in time measured in the last two minutes
            /// </summary>
            public double? Offset
            {
                get
                {
                    if (_currentLowestOffset == null)
                        // If there is no current lowest offset return the fallback (which might or might not be null)
                        return _fallbackLowest;

                    if (_fallbackLowest == null)
                        // If there is no fallback return the current lowest offset
                        return _currentLowestOffset;

                    // If there is both a fallback and a current offset return the min offset of those
                    return Math.Min(_currentLowestOffset.Value, _fallbackLowest.Value);
                }
            }

            public void Update(double offsetMs)
            {
                if (_currentLowestOffset == null || _currentLowestOffset > offsetMs)
                {
                    _currentLowestOffset = offsetMs;
                    _fallbackLowest = offsetMs;
                }

                if (DateTime.UtcNow - _lastRollOver > TimeSpan.FromMinutes(1))
                {
                    _fallbackLowest = _currentLowestOffset;
                    _currentLowestOffset = null;
                    _lastRollOver = DateTime.UtcNow;
                }    
            }
        }

        class RestTimeOffset
        {
            public SemaphoreSlim SemaphoreSlim { get; } = new SemaphoreSlim(1, 1);
            public DateTime? LastUpdate { get; set; }
            public double? Offset { get; set; }

            public void Update(double offsetMs)
            {
                LastUpdate = DateTime.UtcNow;
                Offset = offsetMs;
            }
        }

        private static ConcurrentDictionary<string, SocketTimeOffset> _lastSocketDelays = new ConcurrentDictionary<string, SocketTimeOffset>();
        private static ConcurrentDictionary<string, RestTimeOffset> _lastRestDelays = new ConcurrentDictionary<string, RestTimeOffset>();

        /// <summary>
        /// Update WebSocket API offset
        /// </summary>
        /// <param name="api">API name</param>
        /// <param name="offsetMs">Offset in milliseconds</param>
        public static void UpdateSocketOffset(string api, double offsetMs)
        {
            if (!_lastSocketDelays.TryGetValue(api, out var offsetValues))
            {
                offsetValues = new SocketTimeOffset();
                _lastSocketDelays.TryAdd(api, offsetValues);
            }

            _lastSocketDelays[api].Update(offsetMs);
        }

        /// <summary>
        /// Update REST API offset
        /// </summary>
        /// <param name="api">API name</param>
        /// <param name="offsetMs">Offset in milliseconds</param>
        public static void UpdateRestOffset(string api, double offsetMs)
        {
            _lastRestDelays[api].Update(offsetMs);
        }

        /// <summary>
        /// Get REST API offset
        /// </summary>
        /// <param name="api">API name</param>
        public static TimeSpan? GetRestOffset(string api) => _lastRestDelays.TryGetValue(api, out var val) && val.Offset != null ? TimeSpan.FromMilliseconds(val.Offset.Value) : null;

        /// <summary>
        /// Get REST API last update time
        /// </summary>
        /// <param name="api">API name</param>
        public static DateTime? GetRestLastUpdateTime(string api) => _lastRestDelays.TryGetValue(api, out var val) && val.LastUpdate != null ? val.LastUpdate.Value : null;

        /// <summary>
        /// Register a REST API client to be tracked
        /// </summary>
        /// <param name="api"></param>
        internal static void RegisterRestApi(string api)
        {
            _lastRestDelays[api] = new RestTimeOffset();
        }

        /// <summary>
        /// Enter exclusive access for the API to update the time offset
        /// </summary>
        /// <param name="api"></param>
        /// <returns></returns>
        public static async ValueTask EnterAsync(string api) 
        {
            await _lastRestDelays[api].SemaphoreSlim.WaitAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Release exclusive access for the API
        /// </summary>
        /// <param name="api"></param>
        public static void Release(string api) => _lastRestDelays[api].SemaphoreSlim.Release();

        /// <summary>
        /// Get WebSocket API offset
        /// </summary>
        /// <param name="api">API name</param>
        public static TimeSpan? GetSocketOffset(string api) => _lastSocketDelays.TryGetValue(api, out var val) && val.Offset != null ? TimeSpan.FromMilliseconds(val.Offset.Value) : null;

        /// <summary>
        /// Reset the WebSocket API update timestamp to trigger a new time offset calculation
        /// </summary>
        /// <param name="api">API name</param>
        public static void ResetRestUpdateTime(string api)
        {
            _lastRestDelays[api].LastUpdate = null;
        }
    }
}
