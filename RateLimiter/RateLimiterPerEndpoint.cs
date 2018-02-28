using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace CryptoExchange.Net.RateLimiter
{
    /// <summary>
    /// Limits the amount of requests per time period to a certain limit, counts the request per endpoint.
    /// </summary>
    public class RateLimiterPerEndpoint: IRateLimiter
    {
        internal Dictionary<string, RateLimitObject> history = new Dictionary<string, RateLimitObject>();

        private int limitPerEndpoint;
        private TimeSpan perTimePeriod;
        private object historyLock = new object();

        /// <summary>
        /// Create a new RateLimiterPerEndpoint. This rate limiter limits the amount of requests per time period to a certain limit, counts the request per endpoint.
        /// </summary>
        /// <param name="limitPerEndpoint">The amount to limit to</param>
        /// <param name="perTimePeriod">The time period over which the limit counts</param>
        public RateLimiterPerEndpoint(int limitPerEndpoint, TimeSpan perTimePeriod)
        {
            this.limitPerEndpoint = limitPerEndpoint;
            this.perTimePeriod = perTimePeriod;
        }

        public double LimitRequest(string url)
        {
            double waitTime;
            RateLimitObject rlo;
            lock (historyLock)
            {
                if (history.ContainsKey(url))
                    rlo = history[url];
                else
                {
                    rlo = new RateLimitObject();
                    history.Add(url, rlo);
                }
            }

            var sw = Stopwatch.StartNew();
            lock (rlo.LockObject)
            {
                sw.Stop();
                waitTime = rlo.GetWaitTime(DateTime.UtcNow, limitPerEndpoint, perTimePeriod);
                if (waitTime != 0)
                {
                    Thread.Sleep(Convert.ToInt32(waitTime));
                    waitTime += sw.ElapsedMilliseconds;
                }

                rlo.Add(DateTime.UtcNow);
            }

            return waitTime;
        }
    }
}
