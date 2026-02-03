using System;

namespace CryptoExchange.Net.Trackers.UserData.Objects
{
    /// <summary>
    /// Tracker configuration
    /// </summary>
    public class TrackerItemConfig
    {
        /// <summary>
        /// Interval to poll data at as backup, even when the websocket stream is still connected.
        /// </summary>
        public TimeSpan PollIntervalConnected { get; set; } = TimeSpan.Zero;
        /// <summary>
        /// Interval to poll data at while the websocket is disconnected.
        /// </summary>
        public TimeSpan PollIntervalDisconnected { get; set; } = TimeSpan.FromSeconds(30);
        /// <summary>
        /// Whether to poll for data initially when starting the tracker.
        /// </summary>
        public bool PollAtStart { get; set; } = true;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="pollAtStart">Whether to poll for data initially when starting the tracker</param>
        /// <param name="pollIntervalConnected">Interval to poll data at as backup, even when the websocket stream is still connected</param>
        /// <param name="pollIntervalDisconnected">Interval to poll data at while the websocket is disconnected</param>
        public TrackerItemConfig(bool pollAtStart, TimeSpan pollIntervalConnected, TimeSpan pollIntervalDisconnected)
        {
            PollAtStart = pollAtStart;
            PollIntervalConnected = pollIntervalConnected;
            PollIntervalDisconnected = pollIntervalDisconnected;
        }
    }
}
