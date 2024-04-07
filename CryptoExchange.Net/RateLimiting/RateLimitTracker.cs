using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CryptoExchange.Net.RateLimiting
{
    internal class RateLimitTracker
    {
        public TimeSpan Timeperiod { get; }
        public int Limit { get; }
        public int Current => _entries.Sum(e => e.Weight);

        private List<LimitEntry> _entries;
        private int _currentWeight = 0;

        public RateLimitTracker(int limit, TimeSpan period) 
        {
            Limit = limit;
            Timeperiod = period;
            _entries = new List<LimitEntry>();
        }

        public TimeSpan ProcessTopic(int weight)
        {
            // Remove requests no longer in time period from the history
            var checkTime = DateTime.UtcNow;
            for (var i = 0; i < _entries.Count; i++)
            {
                if (_entries[i].Timestamp < checkTime - Timeperiod)
                {
                    Console.WriteLine($"Removing entry {i}, {_entries[i].Timestamp} vs {checkTime}");
                    var entry = _entries[i];
                    _entries.Remove(entry);
                    _currentWeight -= entry.Weight;
                    i--;
                }
                else
                    break;
            }

            if (_currentWeight + weight > Limit)
            {
                if (_currentWeight == 0)
                    throw new Exception("Request limit reached without any prior request. " +
                        $"This request can never execute with the current rate limiter. Request weight: {weight}, Ratelimit: {Limit}");

                // Wait until the next entry should be removed from the history
                return DetermineWaitTime(weight);
            }

            return TimeSpan.Zero;
        }

        private TimeSpan DetermineWaitTime(int requestWeight)
        {
            var weightToRemove = Math.Max(Current - (Limit - requestWeight), 0);
            var removedWeight = 0;
            for (var i = 0; i < _entries.Count; i++)
            {
                var entry = _entries[i];
                removedWeight += entry.Weight;
                if (removedWeight >= weightToRemove)
                {
                    return (entry.Timestamp + Timeperiod) - DateTime.UtcNow;
                }
            }

            throw new Exception("Request not possible to execute with current rate limit guard. " +
                        $" Request weight: {requestWeight}, Ratelimit: {Limit}");
        }

        public void AddEntry(int weight)
        {
            _currentWeight += weight;
            _entries.Add(new LimitEntry(DateTime.UtcNow, weight));
        }

        internal struct LimitEntry
        {
            public DateTime Timestamp { get; set; }
            public int Weight { get; set; }

            public LimitEntry(DateTime timestamp, int weight)
            {
                Timestamp = timestamp;
                Weight = weight;
            }
        }
    }
}
