using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoExchange.Net.RateLimiter
{
    public class RateLimitObject
    {
        public object LockObject { get; }
        private List<DateTime> Times { get; }

        public RateLimitObject()
        {
            LockObject = new object();
            Times = new List<DateTime>();
        }

        public int GetWaitTime(DateTime time, int limit, TimeSpan perTimePeriod)
        {
            Times.RemoveAll(d => d < time - perTimePeriod);
            if (Times.Count >= limit)
                return (int)Math.Round((Times.First() - (time - perTimePeriod)).TotalMilliseconds);
            return 0;
        }

        public void Add(DateTime time)
        {
            Times.Add(time);
            Times.Sort();
        }
    }
}
