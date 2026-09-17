using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.RateLimiting
{
    /// <summary>
    /// Rate limit admission decision result
    /// </summary>
    public record RateLimitAdmission
    {
        /// <summary>
        /// Value ratio between 0 and 1
        /// </summary>
        public double MaxUtilizationRatio { get; }

        private RateLimitAdmission(double maxUtilizationValue) { 
            if (maxUtilizationValue <= 0 || maxUtilizationValue > 1)
                throw new ArgumentOutOfRangeException(nameof(maxUtilizationValue), "Max utilization value must be bigger than 0 and less than or equal to 1");

            MaxUtilizationRatio = maxUtilizationValue;
        }

        /// <summary>
        /// Only allow the request when below a certain ratio of the rate limit, for example 0.5 means it can use a max of 50% of the rate limit, 
        /// 1 means it's allowed to use the full rate limit.
        /// </summary>
        /// <param name="value">0.5 means a max use 50% of the rate limit, 1 means the request is allowed to use the full rate limit</param>
        public static RateLimitAdmission WithMaxUtilizationRatio(double value)
            => new RateLimitAdmission(value);
    }
}
