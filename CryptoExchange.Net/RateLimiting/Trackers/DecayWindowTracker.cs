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
        private double _decayProgress;

        public DecayWindowTracker(
            int limit,
            TimeSpan period,
            double decayRate)
        {
            if (period <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(period));

            if (decayRate <= 0)
                throw new ArgumentOutOfRangeException(nameof(decayRate));
            Limit = limit;
            TimePeriod = period;
            DecreaseRate = decayRate;
        }

        /// <inheritdoc />
        public void Reset(int? amount)
        {
            if (amount == null)
            {
                ResetState();
                return;
            }

            _currentWeight = Math.Max(0, _currentWeight - amount.Value);
            if (_currentWeight == 0)
                ResetState();
        }

        /// <inheritdoc />
        public TimeSpan GetWaitTime(int weight, double allowedRateRatio)
        {
            var now = DateTime.UtcNow;
            DecreaseCounter(now);

            if ((Current + weight) / (double)Limit <= allowedRateRatio)
                return TimeSpan.Zero;

            if (Current == 0)
            {
                if (allowedRateRatio < 1)
                {
                    throw new Exception(
                        "Request limit reached max utilization. " +
                        "This request can never execute with the current rate limiter configuration. " +
                        $"Request weight: {weight}, RateLimit: {Limit}, " +
                        $"Request ratio: {(Current + weight) / (double)Limit}, " +
                        $"AllowedRateRatio: {allowedRateRatio}");
                }

                throw new Exception(
                    "Request limit reached without any prior request. " +
                    "This request can never execute with the current rate limiter. " +
                    $"Request weight: {weight}, RateLimit: {Limit}");
            }

            return DetermineWaitTime(
                weight,
                allowedRateRatio);
        }

        public void ApplyWeight(int weight)
        {
            if (_currentWeight == 0)
            {
                _lastDecrease = DateTime.UtcNow;
                _decayProgress = 0;
            }

            _currentWeight += weight;
        }

        private void DecreaseCounter(DateTime now)
        {
            if (_currentWeight == 0)
            {
                _lastDecrease = now;
                _decayProgress = 0;
                return;
            }

            var elapsed = now - _lastDecrease;
            if (elapsed <= TimeSpan.Zero)
                return;

            var elapsedDecay = elapsed.Ticks / (double)TimePeriod.Ticks * DecreaseRate;

            var totalDecay = _decayProgress + elapsedDecay;
            var completedDecay = (int)Math.Floor(totalDecay);

            _lastDecrease = now;

            if (completedDecay == 0)
            {
                _decayProgress = totalDecay;
                return;
            }

            _currentWeight = Math.Max(0, _currentWeight - completedDecay);
            if (_currentWeight == 0)
            {
                // Decay cannot accumulate as credit while the counter is empty.
                _decayProgress = 0;
            }
            else
            {
                _decayProgress = totalDecay - completedDecay;
            }
        }

        private TimeSpan DetermineWaitTime(
            int requestWeight,
            double allowedRateRatio)
        {
            var weightToRemove = Current + requestWeight - Limit * allowedRateRatio;

            // The counter is integer-valued, so enough whole weight units
            // must decay before the request can be admitted.
            var requiredDecay = Math.Ceiling(weightToRemove);
            var remainingDecay = Math.Max(0, requiredDecay - _decayProgress);

            var waitTicks = Math.Ceiling(remainingDecay / DecreaseRate * TimePeriod.Ticks);
            return waitTicks <= 0
                ? TimeSpan.Zero
                : TimeSpan.FromTicks((long)waitTicks);
        }

        private void ResetState()
        {
            _currentWeight = 0;
            _decayProgress = 0;
            _lastDecrease = DateTime.UtcNow;
        }
    }
}
