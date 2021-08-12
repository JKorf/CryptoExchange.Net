using CryptoExchange.Net.Objects;
using System;

namespace CryptoExchange.Net
{
    /// <summary>
    /// General helpers functions
    /// </summary>
    public static class ExchangeHelpers
    {
        /// <summary>
        /// Clamp a value between a min and max
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static decimal ClampValue(decimal min, decimal max, decimal value)
        {
            value = Math.Min(max, value);
            value = Math.Max(min, value);
            return value;
        }

        /// <summary>
        /// Adjust a value to be between the min and max parameters and rounded to the closest step.
        /// </summary>
        /// <param name="min">The min value</param>
        /// <param name="max">The max value</param>
        /// <param name="step">The step size the value should be floored to. For example, value 2.548 with a step size of 0.01 will output 2.54</param>
        /// <param name="roundingType">How to round</param>
        /// <param name="value">The input value</param>
        /// <returns></returns>
        public static decimal AdjustValueStep(decimal min, decimal max, decimal? step, RoundingType roundingType, decimal value)
        {
            if(step == 0)
                throw new ArgumentException($"0 not allowed for parameter {nameof(step)}, pass in null to ignore the step size", nameof(step));

            value = Math.Min(max, value);
            value = Math.Max(min, value);
            if (step == null)
                return value;

            var offset = value % step.Value;
            if(roundingType == RoundingType.Down)
                value -= offset;
            else
            {
                if (offset < step / 2)
                    value -= offset;
                else value += (step.Value - offset);
            }

            value = RoundDown(value, 8);
                
            return value.Normalize();
        }

        /// <summary>
        /// Adjust a value to be between the min and max parameters and rounded to the closest precision.
        /// </summary>
        /// <param name="min">The min value</param>
        /// <param name="max">The max value</param>
        /// <param name="precision">The precision the value should be rounded to. For example, value 2.554215 with a precision of 5 will output 2.5542</param>
        /// <param name="roundingType">How to round</param>
        /// <param name="value">The input value</param>
        /// <returns></returns>
        public static decimal AdjustValuePrecision(decimal min, decimal max, int? precision, RoundingType roundingType, decimal value)
        {
            value = Math.Min(max, value);
            value = Math.Max(min, value);
            if (precision == null)
                return value;

            return RoundToSignificantDigits(value, precision.Value, roundingType);
        }

        /// <summary>
        /// Round a value to have the provided total number of digits. For example, value 253.12332 with 5 digits would be 253.12 
        /// </summary>
        /// <param name="value">The value to round</param>
        /// <param name="digits">The total amount of digits (NOT decimal places) to round to</param>
        /// <param name="roundingType">How to round</param>
        /// <returns></returns>
        public static decimal RoundToSignificantDigits(decimal value, int digits, RoundingType roundingType)
        {
            var val = (double)value;
            if (value == 0)
                return 0;

            double scale = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(val))) + 1);
            if(roundingType == RoundingType.Closest)
                return (decimal)(scale * Math.Round(val / scale, digits));
            else
                return (decimal)(scale * (double)RoundDown((decimal)(val / scale), digits));
        }

        /// <summary>
        /// Rounds a value down to 
        /// </summary>
        /// <param name="i"></param>
        /// <param name="decimalPlaces"></param>
        /// <returns></returns>
        public static decimal RoundDown(decimal i, double decimalPlaces)
        {
            var power = Convert.ToDecimal(Math.Pow(10, decimalPlaces));
            return Math.Floor(i * power) / power;
        }

        /// <summary>
        /// Strips any trailing zero's of a decimal value, useful when converting the value to string.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static decimal Normalize(this decimal value)
        {
            return value / 1.000000000000000000000000000000000m;
        }
    }
}
