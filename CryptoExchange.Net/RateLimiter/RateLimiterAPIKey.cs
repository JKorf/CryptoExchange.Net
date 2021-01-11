using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net.RateLimiter
{
    /// <summary>
    /// Limits the amount of requests per time period to a certain limit, counts the request per API key.
    /// </summary>
    public class RateLimiterAPIKey: IRateLimiter, IDisposable
    {
        internal Dictionary<string, RateLimitObject> history = new Dictionary<string, RateLimitObject>();

        private readonly SHA256 encryptor;
        private readonly int limitPerKey;
        private readonly TimeSpan perTimePeriod;
        private readonly object historyLock = new object();

        /// <summary>
        /// Create a new RateLimiterAPIKey. This rate limiter limits the amount of requests per time period to a certain limit, counts the request per API key.
        /// </summary>
        /// <param name="limitPerApiKey">The amount to limit to</param>
        /// <param name="perTimePeriod">The time period over which the limit counts</param>
        public RateLimiterAPIKey(int limitPerApiKey, TimeSpan perTimePeriod)
        {
            limitPerKey = limitPerApiKey;
            encryptor = SHA256.Create();
            this.perTimePeriod = perTimePeriod;
        }

        /// <inheritdoc />
        public CallResult<double> LimitRequest(RestClient client, string url, RateLimitingBehaviour limitBehaviour, int credits = 1)
        {
            if(client.authProvider?.Credentials?.Key == null)
                return new CallResult<double>(0, null);

            var keyBytes = encryptor.ComputeHash(Encoding.UTF8.GetBytes(client.authProvider.Credentials.Key.GetString()));
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < keyBytes.Length; i++)
            {
                builder.Append(keyBytes[i].ToString("x2"));
            }

            var key = builder.ToString();

            int waitTime;
            RateLimitObject rlo;
            lock (historyLock)
            {
                if (history.ContainsKey(key))
                    rlo = history[key];
                else
                {
                    rlo = new RateLimitObject();
                    history.Add(key, rlo);
                }
            }

            var sw = Stopwatch.StartNew();
            lock (rlo.LockObject)
            {
                sw.Stop();
                waitTime = rlo.GetWaitTime(DateTime.UtcNow, limitPerKey, perTimePeriod);
                if (waitTime != 0)
                {
                    if (limitBehaviour == RateLimitingBehaviour.Fail)
                        return new CallResult<double>(waitTime, new RateLimitError($"endpoint limit of {limitPerKey} reached on api key " + key));

                    Thread.Sleep(Convert.ToInt32(waitTime));
                    waitTime += (int)sw.ElapsedMilliseconds;
                }

                rlo.Add(DateTime.UtcNow);
            }

            return new CallResult<double>(waitTime, null);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            encryptor.Dispose();
        }
    }
}
