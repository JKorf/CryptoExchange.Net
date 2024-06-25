using System;
using CryptoExchange.Net.RateLimiting.Interfaces;

namespace CryptoExchange.Net.RateLimiting.Trackers
{
    internal class DecayWindowTracker : IWindowTracker
    {
        /// <inheritdoc />
        public TimeSpan TimePeriod { get; }
        /// <summary>
        /// Decrease rate per TimePeriod
        /// </summary>
        public double DecreaseRate { get; }
        /// <inheritdoc />
        public int Limit { get; }
        /// <inheritdoc />
        public int Current => _currentWeight;

        private int _currentWeight = 0;
        private DateTime _lastDecrease = DateTime.UtcNow;

        public DecayWindowTracker(int limit, TimeSpan period, double decayRate)
        {
            Limit = limit;
            TimePeriod = period;
            DecreaseRate = decayRate;
        }

        /// <inheritdoc />
        public TimeSpan GetWaitTime(int weight)
        {
            // Decrease the counter based on the last update time and decay rate
            DecreaseCounter(DateTime.UtcNow);

            if (Current + weight > Limit)
            {
                // The weight would cause the rate limit to be passed
                if (Current == 0)
                {
                    throw new Exception("Request limit reached without any prior request. " +
                        $"This request can never execute with the current rate limiter. Request weight: {weight}, Ratelimit: {Limit}");
                }

                // Determine the time to wait before this weight can be applied without going over the rate limit
                return DetermineWaitTime(weight);
            }

            // Weight can fit without going over limit
            return TimeSpan.Zero;
        }

        /// <inheritdoc />
        public void ApplyWeight(int weight)
        {
            if (_currentWeight == 0)
                _lastDecrease = DateTime.UtcNow;
            _currentWeight += weight;
        }

        /// <summary>
        /// Decrease the counter based on time passed since last update and the decay rate
        /// </summary>
        /// <param name="time"></param>
        protected void DecreaseCounter(DateTime time)
        {
            var dif = (time - _lastDecrease).TotalMilliseconds / TimePeriod.TotalMilliseconds * DecreaseRate;
            var decrease = (int)Math.Floor(dif);
            if (decrease >= 1)
            {
                _currentWeight = Math.Max(0, _currentWeight - (int)Math.Floor(dif));
                _lastDecrease = time;
            }
        }

        /// <summary>
        /// Determine the time to wait before the weight would fit
        /// </summary>
        /// <param name="requestWeight"></param>
        /// <returns></returns>
        private TimeSpan DetermineWaitTime(int requestWeight)
        {
            var weightToRemove = Math.Max(Current - (Limit - requestWeight), 0);
            var result = TimeSpan.FromMilliseconds(Math.Ceiling(weightToRemove / DecreaseRate) * TimePeriod.TotalMilliseconds);
            if (result < TimeSpan.Zero)
                return TimeSpan.Zero;
            return result;
        }
    }
}
