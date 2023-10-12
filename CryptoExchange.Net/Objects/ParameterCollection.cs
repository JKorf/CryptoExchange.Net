using CryptoExchange.Net.Attributes;
using CryptoExchange.Net.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace CryptoExchange.Net.Objects
{
    /// <summary>
    /// Parameters collection
    /// </summary>
    public class ParameterCollection : Dictionary<string, object>
    {
        /// <summary>
        /// Add an optional parameter. Not added if value is null
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddOptional(string key, object? value)
        {
            if (value != null)
                Add(key, value);
        }

        /// <summary>
        /// Add a decimal value as string
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddString(string key, decimal value)
        {
            Add(key, value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Add a decimal value as string. Not added if value is null
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddOptionalString(string key, decimal? value)
        {
            if (value != null)
                Add(key, value.Value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Add a int value as string
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddString(string key, int value)
        {
            Add(key, value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Add a int value as string. Not added if value is null
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddOptionalString(string key, int? value)
        {
            if (value != null)
                Add(key, value.Value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Add a long value as string
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddString(string key, long value)
        {
            Add(key, value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Add a long value as string. Not added if value is null
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddOptionalString(string key, long? value)
        {
            if (value != null)
                Add(key, value.Value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Add a datetime value as milliseconds timestamp
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddMilliseconds(string key, DateTime value)
        {
            Add(key, DateTimeConverter.ConvertToMilliseconds(value));
        }

        /// <summary>
        /// Add a datetime value as milliseconds timestamp. Not added if value is null
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddOptionalMilliseconds(string key, DateTime? value)
        {
            if (value != null)
                Add(key, DateTimeConverter.ConvertToMilliseconds(value));
        }

        /// <summary>
        /// Add a datetime value as milliseconds timestamp
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddMillisecondsString(string key, DateTime value)
        {
            Add(key, DateTimeConverter.ConvertToMilliseconds(value).Value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Add a datetime value as milliseconds timestamp. Not added if value is null
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddOptionalMillisecondsString(string key, DateTime? value)
        {
            if (value != null)
                Add(key, DateTimeConverter.ConvertToMilliseconds(value).Value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Add a datetime value as seconds timestamp
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddSeconds(string key, DateTime value)
        {
            Add(key, DateTimeConverter.ConvertToSeconds(value));
        }

        /// <summary>
        /// Add a datetime value as seconds timestamp. Not added if value is null
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddOptionalSeconds(string key, DateTime? value)
        {
            if (value != null)
                Add(key, DateTimeConverter.ConvertToSeconds(value));
        }

        /// <summary>
        /// Add an enum value as the string value as mapped using the <see cref="MapAttribute" />
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddEnum<T>(string key, T value)
        {
            Add(key, EnumConverter.GetString(value)!);
        }

        /// <summary>
        /// Add an enum value as the string value as mapped using the <see cref="MapAttribute" />. Not added if value is null
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddOptionalEnum<T>(string key, T? value)
        {
            if (value != null)
                Add(key, EnumConverter.GetString(value));
        }
    }
}
