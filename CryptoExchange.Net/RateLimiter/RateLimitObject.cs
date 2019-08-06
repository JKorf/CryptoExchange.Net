using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoExchange.Net.RateLimiter
{
    /// <summary>
    /// Rate limiting object
    /// </summary>
    public class RateLimitObject
    {
        /// <summary>
        /// Lock
        /// </summary>
        public object LockObject { get; }
        private List<DateTime> Times { get; }

        /// <summary>
        /// ctor
        /// </summary>
        public RateLimitObject()
        {
            LockObject = new object();
            Times = new List<DateTime>();
        }

        /// <summary>
        /// Get time to wait for a specific time
        /// </summary>
        /// <param name="time"></param>
        /// <param name="limit"></param>
        /// <param name="perTimePeriod"></param>
        /// <returns></returns>
        public int GetWaitTime(DateTime time, int limit, TimeSpan perTimePeriod)
        {
            Times.RemoveAll(d => d < time - perTimePeriod);
            if (Times.Count >= limit)
                return (int)Math.Round((Times.First() - (time - perTimePeriod)).TotalMilliseconds);
            return 0;
        }

        /// <summary>
        /// Add an executed request time
        /// </summary>
        /// <param name="time"></param>
        public void Add(DateTime time)
        {
            Times.Add(time);
            Times.Sort();
        }
    }
}
