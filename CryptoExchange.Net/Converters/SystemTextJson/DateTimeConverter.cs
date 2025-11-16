using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CryptoExchange.Net.Converters.SystemTextJson
{
    /// <summary>
    /// Date time converter
    /// </summary>
    public class DateTimeConverter : JsonConverterFactory
    {
        private static readonly DateTime _epoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private const long _ticksPerSecond = TimeSpan.TicksPerMillisecond * 1000;
        private const decimal _ticksPerMicrosecond = TimeSpan.TicksPerMillisecond / 1000m;
        private const decimal _ticksPerNanosecond = TimeSpan.TicksPerMillisecond / 1000m / 1000;

        /// <inheritdoc />
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(DateTime) || typeToConvert == typeof(DateTime?);
        }

        /// <inheritdoc />
        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            return typeToConvert == typeof(DateTime) ? new DateTimeConverterInner() : new NullableDateTimeConverterInner();
        }

        private class NullableDateTimeConverterInner : JsonConverter<DateTime?>
        {
            public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
                => ReadDateTime(ref reader, typeToConvert, options);

            public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
            {
                if (value == null)
                {
                    writer.WriteNullValue();
                    return;
                }

                if (value.Value == default)
                    writer.WriteStringValue(default(DateTime));
                else
                    writer.WriteNumberValue((long)Math.Round((value.Value - new DateTime(1970, 1, 1)).TotalMilliseconds));
            }
        }

        private class DateTimeConverterInner : JsonConverter<DateTime>
        {
            public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
                => ReadDateTime(ref reader, typeToConvert, options) ?? default;            

            public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
            {
                var dtValue = (DateTime)(object)value;
                if (dtValue == default)
                    writer.WriteStringValue(default(DateTime));
                else
                    writer.WriteNumberValue((long)Math.Round((dtValue - new DateTime(1970, 1, 1)).TotalMilliseconds));
            }
        }

        private static DateTime? ReadDateTime(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                if (typeToConvert == typeof(DateTime))
                    LibraryHelpers.StaticLogger?.LogWarning("DateTime value of null, but property is not nullable. Resolver: {Resolver}", options.TypeInfoResolver?.GetType()?.Name);
                return default;
            }

            if (reader.TokenType is JsonTokenType.Number)
            {
                var decValue = reader.GetDecimal();
                if (decValue == 0 || decValue < 0)
                    return default;

                return ParseFromDecimal(decValue);
            }
            else if (reader.TokenType is JsonTokenType.String)
            {
                var stringValue = reader.GetString();
                if (string.IsNullOrWhiteSpace(stringValue)
                    || stringValue == "-1"
                    || stringValue == "0001-01-01T00:00:00Z"
                    || decimal.TryParse(stringValue, out var decVal) && decVal == 0)
                {
                    return default;
                }

                return ParseFromString(stringValue!, options.TypeInfoResolver?.GetType()?.Name);
            }
            else
            {
                return reader.GetDateTime();
            }
        }

        /// <summary>
        /// Parse a double value to datetime
        /// </summary>
        public static DateTime ParseFromDouble(double value)
            => ParseFromDecimal((decimal)value);

        /// <summary>
        /// Parse a decimal value to datetime
        /// </summary>
        public static DateTime ParseFromDecimal(decimal value)
        {
            if (value < 19999999999)
                return ConvertFromSeconds(value);
            if (value < 19999999999999)
                return ConvertFromMilliseconds(value);
            if (value < 19999999999999999)
                return ConvertFromMicroseconds(value);

            return ConvertFromNanoseconds(value);
        }

        /// <summary>
        /// Parse a string value to datetime
        /// </summary>
        public static DateTime ParseFromString(string stringValue, string? resolverName)
        {
            if (stringValue!.Length == 12 && stringValue.StartsWith("202"))
            {
                // Parse 202303261200 format
                if (!int.TryParse(stringValue.Substring(0, 4), out var year)
                    || !int.TryParse(stringValue.Substring(4, 2), out var month)
                    || !int.TryParse(stringValue.Substring(6, 2), out var day)
                    || !int.TryParse(stringValue.Substring(8, 2), out var hour)
                    || !int.TryParse(stringValue.Substring(10, 2), out var minute))
                {
                    LibraryHelpers.StaticLogger?.LogWarning("Unknown DateTime format: {Value}. Resolver: {Resolver}", stringValue, resolverName);
                    return default;
                }
                return new DateTime(year, month, day, hour, minute, 0, DateTimeKind.Utc);
            }

            if (stringValue.Length == 8)
            {
                // Parse 20211103 format
                if (!int.TryParse(stringValue.Substring(0, 4), out var year)
                    || !int.TryParse(stringValue.Substring(4, 2), out var month)
                    || !int.TryParse(stringValue.Substring(6, 2), out var day))
                {
                    LibraryHelpers.StaticLogger?.LogWarning("Unknown DateTime format: {Value}. Resolver: {Resolver}", stringValue, resolverName);
                    return default;
                }
                return new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
            }

            if (stringValue.Length == 6)
            {
                // Parse 211103 format
                if (!int.TryParse(stringValue.Substring(0, 2), out var year)
                    || !int.TryParse(stringValue.Substring(2, 2), out var month)
                    || !int.TryParse(stringValue.Substring(4, 2), out var day))
                {
                    LibraryHelpers.StaticLogger?.LogWarning("Unknown DateTime format: {Value}. Resolver: {Resolver}", stringValue, resolverName);
                    return default;
                }
                return new DateTime(year + 2000, month, day, 0, 0, 0, DateTimeKind.Utc);
            }

            if (decimal.TryParse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var decimalValue))
            {
                // Parse 1637745563.000 format
                if (decimalValue <= 0)
                    return default;
                if (decimalValue < 19999999999)
                    return ConvertFromSeconds(decimalValue);
                if (decimalValue < 19999999999999)
                    return ConvertFromMilliseconds(decimalValue);
                if (decimalValue < 19999999999999999)
                    return ConvertFromMicroseconds(decimalValue);

                return ConvertFromNanoseconds(decimalValue);
            }

            if (stringValue.Length == 10)
            {
                // Parse 2021-11-03 format
                var values = stringValue.Split('-');
                if (!int.TryParse(values[0], out var year)
                    || !int.TryParse(values[1], out var month)
                    || !int.TryParse(values[2], out var day))
                {
                    LibraryHelpers.StaticLogger?.LogWarning("Unknown DateTime format: {Value}. Resolver: {Resolver}", stringValue, resolverName);
                    return default;
                }

                return new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
            }

            return DateTime.Parse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
        }

        /// <summary>
        /// Convert a seconds since epoch (01-01-1970) value to DateTime
        /// </summary>
        public static DateTime ConvertFromSeconds(decimal seconds) => _epoch.AddTicks((long)Math.Round(seconds * _ticksPerSecond));
        /// <summary>
        /// Convert a nanoseconds since epoch (01-01-1970) value to DateTime
        /// </summary>
        public static DateTime ConvertFromSeconds(double seconds) => ConvertFromSeconds((decimal)seconds);
        /// <summary>
        /// Convert a nanoseconds since epoch (01-01-1970) value to DateTime
        /// </summary>
        public static DateTime ConvertFromSeconds(long seconds) => ConvertFromSeconds((decimal)seconds);
        /// <summary>
        /// Convert a milliseconds since epoch (01-01-1970) value to DateTime
        /// </summary>
        public static DateTime ConvertFromMilliseconds(decimal milliseconds) => _epoch.AddTicks((long)Math.Round(milliseconds * TimeSpan.TicksPerMillisecond));
        /// <summary>
        /// Convert a nanoseconds since epoch (01-01-1970) value to DateTime
        /// </summary>
        public static DateTime ConvertFromMilliseconds(double milliseconds) => ConvertFromMilliseconds((decimal)milliseconds);
        /// <summary>
        /// Convert a nanoseconds since epoch (01-01-1970) value to DateTime
        /// </summary>
        public static DateTime ConvertFromMilliseconds(long milliseconds) => ConvertFromMilliseconds((decimal)milliseconds);
        /// <summary>
        /// Convert a microseconds since epoch (01-01-1970) value to DateTime
        /// </summary>
        public static DateTime ConvertFromMicroseconds(decimal microseconds) => _epoch.AddTicks((long)Math.Round(microseconds * _ticksPerMicrosecond));
        /// <summary>
        /// Convert a nanoseconds since epoch (01-01-1970) value to DateTime
        /// </summary>
        public static DateTime ConvertFromMicroseconds(double microseconds) => ConvertFromMicroseconds((decimal)microseconds);
        /// <summary>
        /// Convert a nanoseconds since epoch (01-01-1970) value to DateTime
        /// </summary>
        public static DateTime ConvertFromMicroseconds(long microseconds) => ConvertFromMicroseconds((decimal)microseconds);
        /// <summary>
        /// Convert a nanoseconds since epoch (01-01-1970) value to DateTime
        /// </summary>
        public static DateTime ConvertFromNanoseconds(decimal nanoseconds) => _epoch.AddTicks((long)Math.Round(nanoseconds * _ticksPerNanosecond));
        /// <summary>
        /// Convert a nanoseconds since epoch (01-01-1970) value to DateTime
        /// </summary>
        public static DateTime ConvertFromNanoseconds(double nanoseconds) => ConvertFromNanoseconds((decimal)nanoseconds);
        /// <summary>
        /// Convert a nanoseconds since epoch (01-01-1970) value to DateTime
        /// </summary>
        public static DateTime ConvertFromNanoseconds(long nanoseconds) => ConvertFromNanoseconds((decimal)nanoseconds);

        /// <summary>
        /// Convert a DateTime value to seconds since epoch (01-01-1970) value
        /// </summary>
        [return: NotNullIfNotNull("time")]
        public static long? ConvertToSeconds(DateTime? time) => time == null ? null : (long)Math.Round((time.Value - _epoch).TotalSeconds);
        /// <summary>
        /// Convert a DateTime value to milliseconds since epoch (01-01-1970) value
        /// </summary>
        [return: NotNullIfNotNull("time")]
        public static long? ConvertToMilliseconds(DateTime? time) => time == null ? null : (long)Math.Round((time.Value - _epoch).TotalMilliseconds);
        /// <summary>
        /// Convert a DateTime value to microseconds since epoch (01-01-1970) value
        /// </summary>
        [return: NotNullIfNotNull("time")]
        public static long? ConvertToMicroseconds(DateTime? time) => time == null ? null : (long)Math.Round((time.Value - _epoch).Ticks / _ticksPerMicrosecond);
        /// <summary>
        /// Convert a DateTime value to nanoseconds since epoch (01-01-1970) value
        /// </summary>
        [return: NotNullIfNotNull("time")]
        public static long? ConvertToNanoseconds(DateTime? time) => time == null ? null : (long)Math.Round((time.Value - _epoch).Ticks / _ticksPerNanosecond);
    }
}
