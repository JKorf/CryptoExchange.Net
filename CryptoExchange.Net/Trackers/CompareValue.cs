using System;

namespace CryptoExchange.Net.Trackers
{

    /// <summary>
    /// Compare value
    /// </summary>
    public record CompareValue
    {
        /// <summary>
        /// The value difference
        /// </summary>
        public decimal? Difference { get; set; }
        /// <summary>
        /// The value difference percentage
        /// </summary>
        public decimal? PercentageDifference { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public CompareValue(decimal? value1, decimal? value2)
        {
            if (value1 == null || value2 == null)
                return;

            Difference = value2 - value1;
            PercentageDifference = value1.Value == 0 ? null : Math.Round(value2.Value / value1.Value * 100 - 100, 4);
        }
    }
}
