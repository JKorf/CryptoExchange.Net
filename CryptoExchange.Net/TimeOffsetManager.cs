using System;
using System.Collections.Concurrent;

namespace CryptoExchange.Net
{
    /// <summary>
    /// Manager for timing offsets in APIs
    /// </summary>
    public static class TimeOffsetManager
    {
        class SocketTimeOffset
        {
            private DateTime _lastUpdate;
            private DateTime _lastRollOver = DateTime.UtcNow;
            private double? _fallbackLowest;
            private double? _currentLowestOffset;

            /// <summary>
            /// Get the estimated offset
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
                _lastUpdate = DateTime.UtcNow;
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

            offsetValues.Update(offsetMs);
        }

        /// <summary>
        /// Update REST API offset
        /// </summary>
        /// <param name="api">API name</param>
        /// <param name="offsetMs">Offset in milliseconds</param>
        public static void UpdateRestOffset(string api, double offsetMs)
        {
            if (!_lastRestDelays.TryGetValue(api, out var offsetValues))
            {
                offsetValues = new RestTimeOffset();
                _lastRestDelays.TryAdd(api, offsetValues);
            }

            offsetValues.Update(offsetMs);
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
            if (_lastRestDelays.TryGetValue(api, out var val))
                val.LastUpdate = null;
        }
    }
}
