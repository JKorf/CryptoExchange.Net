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
        private const double _ticksPerMicrosecond = TimeSpan.TicksPerMillisecond / 1000d;
        private const double _ticksPerNanosecond = TimeSpan.TicksPerMillisecond / 1000d / 1000;

        /// <inheritdoc />
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(DateTime) || typeToConvert == typeof(DateTime?);
        }

        /// <inheritdoc />
        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            Type converterType = typeof(DateTimeConverterInner<>).MakeGenericType(typeToConvert);
            return (JsonConverter)Activator.CreateInstance(converterType);
        }

        private class DateTimeConverterInner<T> : JsonConverter<T>
        {
            public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
                => (T)((object?)ReadDateTime(ref reader, typeToConvert, options) ?? default(T))!;

            private DateTime? ReadDateTime(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.Null)
                {
                    if (typeToConvert == typeof(DateTime))
                        Trace.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss:fff} | Warning | DateTime value of null, but property is not nullable");
                    return default;
                }

                if (reader.TokenType is JsonTokenType.Number)
                {
                    var longValue = reader.GetDouble();
                    if (longValue == 0 || longValue == -1)
                        return default;

                    return ParseFromDouble(longValue);
                }
                else if (reader.TokenType is JsonTokenType.String)
                {
                    var stringValue = reader.GetString();
                    if (string.IsNullOrWhiteSpace(stringValue)
                        || stringValue == "-1"
                        || double.TryParse(stringValue, out var doubleVal) && doubleVal == 0)
                    {
                        return default;
                    }

                    return ParseFromString(stringValue!);
                }
                else
                {
                    return reader.GetDateTime();
                }
            }

            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            {
                if (value == null)
                    writer.WriteNullValue();
                else
                {
                    var dtValue = (DateTime)(object)value;
                    if (dtValue == default)
                        writer.WriteStringValue(default(DateTime));
                    else
                        writer.WriteNumberValue((long)Math.Round((dtValue - new DateTime(1970, 1, 1)).TotalMilliseconds));
                }
            }
        }

        /// <summary>
        /// Parse a long value to datetime
        /// </summary>
        /// <param name="longValue"></param>
        /// <returns></returns>
        public static DateTime ParseFromDouble(double longValue)
        {
            if (longValue < 19999999999)
                return ConvertFromSeconds(longValue);
            if (longValue < 19999999999999)
                return ConvertFromMilliseconds(longValue);
            if (longValue < 19999999999999999)
                return ConvertFromMicroseconds(longValue);

            return ConvertFromNanoseconds(longValue);
        }

        /// <summary>
        /// Parse a string value to datetime
        /// </summary>
        /// <param name="stringValue"></param>
        /// <returns></returns>
        public static DateTime ParseFromString(string stringValue)
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
                    Trace.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss:fff} | Warning | Unknown DateTime format: " + stringValue);
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
                    Trace.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss:fff} | Warning | Unknown DateTime format: " + stringValue);
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
                    Trace.WriteLine("{DateTime.Now:yyyy/MM/dd HH:mm:ss:fff} | Warning | Unknown DateTime format: " + stringValue);
                    return default;
                }
                return new DateTime(year + 2000, month, day, 0, 0, 0, DateTimeKind.Utc);
            }

            if (double.TryParse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var doubleValue))
            {
                // Parse 1637745563.000 format
                if (doubleValue <= 0)
                    return default;
                if (doubleValue < 19999999999)
                    return ConvertFromSeconds(doubleValue);
                if (doubleValue < 19999999999999)
                    return ConvertFromMilliseconds((long)doubleValue);
                if (doubleValue < 19999999999999999)
                    return ConvertFromMicroseconds((long)doubleValue);

                return ConvertFromNanoseconds((long)doubleValue);
            }

            if (stringValue.Length == 10)
            {
                // Parse 2021-11-03 format
                var values = stringValue.Split('-');
                if (!int.TryParse(values[0], out var year)
                    || !int.TryParse(values[1], out var month)
                    || !int.TryParse(values[2], out var day))
                {
                    Trace.WriteLine("{DateTime.Now:yyyy/MM/dd HH:mm:ss:fff} | Warning | Unknown DateTime format: " + stringValue);
                    return default;
                }

                return new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
            }

            return DateTime.Parse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
        }

        /// <summary>
        /// Convert a seconds since epoch (01-01-1970) value to DateTime
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static DateTime ConvertFromSeconds(double seconds) => _epoch.AddTicks((long)Math.Round(seconds * _ticksPerSecond));
        /// <summary>
        /// Convert a milliseconds since epoch (01-01-1970) value to DateTime
        /// </summary>
        /// <param name="milliseconds"></param>
        /// <returns></returns>
        public static DateTime ConvertFromMilliseconds(double milliseconds) => _epoch.AddTicks((long)Math.Round(milliseconds * TimeSpan.TicksPerMillisecond));
        /// <summary>
        /// Convert a microseconds since epoch (01-01-1970) value to DateTime
        /// </summary>
        /// <param name="microseconds"></param>
        /// <returns></returns>
        public static DateTime ConvertFromMicroseconds(double microseconds) => _epoch.AddTicks((long)Math.Round(microseconds * _ticksPerMicrosecond));
        /// <summary>
        /// Convert a nanoseconds since epoch (01-01-1970) value to DateTime
        /// </summary>
        /// <param name="nanoseconds"></param>
        /// <returns></returns>
        public static DateTime ConvertFromNanoseconds(double nanoseconds) => _epoch.AddTicks((long)Math.Round(nanoseconds * _ticksPerNanosecond));

        /// <summary>
        /// Convert a DateTime value to seconds since epoch (01-01-1970) value
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        [return: NotNullIfNotNull("time")]
        public static long? ConvertToSeconds(DateTime? time) => time == null ? null : (long)Math.Round((time.Value - _epoch).TotalSeconds);
        /// <summary>
        /// Convert a DateTime value to milliseconds since epoch (01-01-1970) value
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        [return: NotNullIfNotNull("time")]
        public static long? ConvertToMilliseconds(DateTime? time) => time == null ? null : (long)Math.Round((time.Value - _epoch).TotalMilliseconds);
        /// <summary>
        /// Convert a DateTime value to microseconds since epoch (01-01-1970) value
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        [return: NotNullIfNotNull("time")]
        public static long? ConvertToMicroseconds(DateTime? time) => time == null ? null : (long)Math.Round((time.Value - _epoch).Ticks / _ticksPerMicrosecond);
        /// <summary>
        /// Convert a DateTime value to nanoseconds since epoch (01-01-1970) value
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        [return: NotNullIfNotNull("time")]
        public static long? ConvertToNanoseconds(DateTime? time) => time == null ? null : (long)Math.Round((time.Value - _epoch).Ticks / _ticksPerNanosecond);
    }
}
