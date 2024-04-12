using System;

namespace CryptoExchange.Net.RateLimiting.Trackers
{
    internal class DecayWindowTracker : IWindowTracker
    {
        /// <summary>
        /// The time period for this tracker
        /// </summary>
        public TimeSpan TimePeriod { get; }
        /// <summary>
        /// Decrease rate per TimePeriod
        /// </summary>
        public double DecreaseRate { get; }
        /// <summary>
        /// Limit for this tracker
        /// </summary>
        public int Limit { get; }
        /// <summary>
        /// Current
        /// </summary>
        public int Current => _currentWeight;

        private int _currentWeight = 0;
        private DateTime _lastDecrease = DateTime.UtcNow;

        public DecayWindowTracker(int limit, TimeSpan period, double decayRate)
        {
            Limit = limit;
            TimePeriod = period;
            DecreaseRate = decayRate;
        }

        public TimeSpan GetWaitTime(int weight)
        {
            // Remove requests no longer in time period from the history
            DecreaseCounter(DateTime.UtcNow);

            if (Current + weight > Limit)
            {
                if (Current == 0)
                {
                    throw new Exception("Request limit reached without any prior request. " +
                        $"This request can never execute with the current rate limiter. Request weight: {weight}, Ratelimit: {Limit}");
                }

                // Wait until the next entry should be removed from the history
                return DetermineWaitTime(weight);
            }

            return TimeSpan.Zero;
        }

        /// <summary>
        /// Apply a new weighted item
        /// </summary>
        /// <param name="weight"></param>
        public void ApplyWeight(int weight)
        {
            if (_currentWeight == 0)
                _lastDecrease = DateTime.UtcNow;
            _currentWeight += weight;
        }

        /// <summary>
        /// Remove items before a certain type
        /// </summary>
        /// <param name="time"></param>
        protected void DecreaseCounter(DateTime time)
        {
            var dif = (time - _lastDecrease).TotalMilliseconds / TimePeriod.TotalMilliseconds * DecreaseRate;
            var decrease = (int)Math.Floor(dif);
            if (decrease >= 1)
            {
                _currentWeight = Math.Max(0, _currentWeight - (int)Math.Floor(dif));
                _lastDecrease = DateTime.UtcNow;
            }
        }

        private TimeSpan DetermineWaitTime(int requestWeight)
        {
            var weightToRemove = Math.Max(Current - (Limit - requestWeight), 0);
            return TimeSpan.FromMilliseconds(Math.Ceiling(weightToRemove / DecreaseRate) * TimePeriod.TotalMilliseconds);
        }
    }
}
