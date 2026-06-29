using CryptoExchange.Net.Attributes;
using CryptoExchange.Net.Converters.SystemTextJson;
using CryptoExchange.Net.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;

namespace CryptoExchange.Net.Objects
{
    /// <summary>
    /// Set of parameters
    /// </summary>
    public class Parameters : IDictionary<string, object>
    {
        private readonly ParameterSerializationSettings _serializationSettings;
        private IDictionary<string, object> _parameters;
        private object? _value;

        /// <inheritdoc />
        public object? BodyValue => _value;

        /// <inheritdoc />
        public ICollection<string> Keys => _parameters.Keys;

        /// <inheritdoc />
        public ICollection<object> Values => _parameters.Values;

        /// <inheritdoc />
        public int Count => _parameters.Count;

        /// <inheritdoc />
        public bool IsReadOnly => _parameters.IsReadOnly;

        /// <summary>
        /// Whether any parameters are defined
        /// </summary>
        public bool Empty => _parameters.Count == 0 && _value == null;

        /// <inheritdoc />
        public object this[string key] { get => _parameters[key]; set => _parameters[key] = value; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="serializationSettings">Serialization settings</param>
        public Parameters(ParameterSerializationSettings serializationSettings)
        {
            _serializationSettings = serializationSettings;
            if (_serializationSettings.Sort)
                _parameters = new SortedDictionary<string, object>(_serializationSettings.SortComparer);
            else
                _parameters = new Dictionary<string, object>();

        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="serializationSettings">Serialization settings</param>
        /// <param name="value">Body value</param>
        public Parameters(object value, ParameterSerializationSettings serializationSettings)
        {
            _parameters = new Dictionary<string, object>();
            _serializationSettings = serializationSettings;
            _value = value;
        }

        /// <summary>
        /// Add a short value if it is not null
        /// </summary>
        public void Add(string key, short? value, IntegerSerialization? serialization = null)
        {
            if (value == null)
                return;

            Add(key, value.Value, serialization);
        }

        /// <summary>
        /// Add a short value
        /// </summary>
        public void Add(string key, short value, IntegerSerialization? serialization = null)
        {
            var serializationToUse = serialization ?? _serializationSettings.Integer;
            if (serializationToUse == IntegerSerialization.String)
                _parameters.Add(key, value.ToString(CultureInfo.InvariantCulture));
            else if (serializationToUse == IntegerSerialization.Number)
                _parameters.Add(key, value);
            else
                throw new ArgumentException("Unknown Integer serialization setting");
        }

        /// <summary>
        /// Add an int value if it is not null
        /// </summary>
        public void Add(string key, int? value, IntegerSerialization? serialization = null)
        {
            if (value == null)
                return;

            Add(key, value.Value, serialization);
        }

        /// <summary>
        /// Add an int value
        /// </summary>
        public void Add(string key, int value, IntegerSerialization? serialization = null)
        {
            var serializationToUse = serialization ?? _serializationSettings.Integer;
            if (serializationToUse == IntegerSerialization.String)
                _parameters.Add(key, value.ToString(CultureInfo.InvariantCulture));
            else if (serializationToUse == IntegerSerialization.Number)
                _parameters.Add(key, value);
            else
                throw new ArgumentException("Unknown Integer serialization setting");
        }

        /// <summary>
        /// Add a long value if it is not null
        /// </summary>
        public void Add(string key, long? value, IntegerSerialization? serialization = null)
        {
            if (value == null)
                return;

            Add(key, value.Value, serialization);
        }

        /// <summary>
        /// Add a long value
        /// </summary>
        public void Add(string key, long value, IntegerSerialization? serialization = null)
        {
            var serializationToUse = serialization ?? _serializationSettings.Integer;
            if (serializationToUse == IntegerSerialization.String)
                _parameters.Add(key, value.ToString(CultureInfo.InvariantCulture));
            else if (serializationToUse == IntegerSerialization.Number)
                _parameters.Add(key, value);
            else
                throw new ArgumentException("Unknown Integer serialization setting");
        }

        /// <summary>
        /// Add a decimal value if it is not null
        /// </summary>
        public void Add(string key, decimal? value, DecimalSerialization? serialization = null)
        {
            if (value == null)
                return;

            Add(key, value.Value, serialization);
        }

        /// <summary>
        /// Add a decimal value
        /// </summary>
        public void Add(string key, decimal value, DecimalSerialization? serialization = null)
        {
            var serializationToUse = serialization ?? _serializationSettings.Decimal;
            if (serializationToUse == DecimalSerialization.String)
                _parameters.Add(key, value.ToString(CultureInfo.InvariantCulture));
            else if (serializationToUse == DecimalSerialization.Number)
                _parameters.Add(key, value);
            else
                throw new ArgumentException("Unknown Decimal serialization setting");
        }

        /// <summary>
        /// Add a double value if it is not null
        /// </summary>
        public void Add(string key, double? value, DecimalSerialization? serialization = null)
        {
            if (value == null)
                return;

            Add(key, value.Value, serialization);
        }

        /// <summary>
        /// Add a double value
        /// </summary>
        public void Add(string key, double value, DecimalSerialization? serialization = null)
        {
            var serializationToUse = serialization ?? _serializationSettings.Decimal;
            if (serializationToUse == DecimalSerialization.String)
                _parameters.Add(key, value.ToString(CultureInfo.InvariantCulture));
            else if (serializationToUse == DecimalSerialization.Number)
                _parameters.Add(key, value);
            else
                throw new ArgumentException("Unknown Decimal serialization setting");
        }

        /// <summary>
        /// Add a bool value if it is not null
        /// </summary>
        public void Add(string key, bool? value, BoolSerialization? serialization = null)
        {
            if (value == null)
                return;

            Add(key, value.Value, serialization);
        }

        /// <summary>
        /// Add a bool value
        /// </summary>
        public void Add(string key, bool value, BoolSerialization? serialization = null)
        {
            var serializationToUse = serialization ?? _serializationSettings.Bool;
            if (serializationToUse == BoolSerialization.String)
                _parameters.Add(key, value.ToString(CultureInfo.InvariantCulture).ToLowerInvariant());
            else if (serializationToUse == BoolSerialization.Bool)
                _parameters.Add(key, value);
            else
                throw new ArgumentException("Unknown Bool serialization setting");
        }

        /// <summary>
        /// Add key as comma separated values if there are values provided
        /// </summary>
        public void AddCommaSeparated(string key, IEnumerable<string>? values)
        {
            if (values == null || !values.Any())
                return;

            _parameters.Add(key, string.Join(",", values));
        }

        /// <summary>
        /// Add key as comma separated values
        /// </summary>
#if NET5_0_OR_GREATER
        public void AddCommaSeparated<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicFields)] T>(string key, IEnumerable<T> values)
#else
        public void AddCommaSeparated<T>(string key, IEnumerable<T>? values)
#endif
            where T : struct, Enum
        {
            if (values == null || !values.Any())
                return;

            _parameters.Add(key, string.Join(",", values.Select(x => EnumConverter.GetString(x))));
        }

        /// <summary>
        /// Add an enum value if it is not null
        /// </summary>
        public void Add<
#if NET5_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicFields)]
# endif
            T>(string key, T? value, EnumSerialization? serialization = null)
            where T : struct, Enum

        {
            if (value == null)
                return;

            Add(key, value.Value, serialization);
        }

        /// <summary>
        /// Add a enum value
        /// </summary>
        public void Add<
#if NET5_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicFields)]
#endif
            T>(string key, T value, EnumSerialization? serialization = null)
            where T : struct, Enum
        {
            var serializationToUse = serialization ?? _serializationSettings.Enum;
            if (serializationToUse == EnumSerialization.String)
                _parameters.Add(key, EnumConverter<T>.GetString(value));
            else if (serializationToUse == EnumSerialization.Number)
                _parameters.Add(key, int.Parse(EnumConverter<T>.GetString(value), CultureInfo.InvariantCulture));
            else
                throw new ArgumentException("Unknown Integer serialization setting");
        }

        /// <summary>
        /// Add a DateTime value if it is not null
        /// </summary>
        public void Add(string key, DateTime? value, DateTimeSerialization? serialization = null)
        {
            if (value == null)
                return;

            Add(key, value.Value, serialization);
        }

        /// <summary>
        /// Add a DateTime value
        /// </summary>
        public void Add(string key, DateTime value, DateTimeSerialization? serialization = null)
        {
            var serializationToUse = serialization ?? _serializationSettings.DateTimes;
            if (serializationToUse == DateTimeSerialization.MillisecondsNumber)
                _parameters.Add(key, DateTimeConverter.ConvertToMilliseconds(value));
            else if (serializationToUse == DateTimeSerialization.MillisecondsString)
                _parameters.Add(key, DateTimeConverter.ConvertToMilliseconds(value).Value.ToString(CultureInfo.InvariantCulture));
            else if (serializationToUse == DateTimeSerialization.SecondsNumber)
                _parameters.Add(key, DateTimeConverter.ConvertToSeconds(value));
            else if (serializationToUse == DateTimeSerialization.SecondsString)
                _parameters.Add(key, DateTimeConverter.ConvertToSeconds(value).Value.ToString(CultureInfo.InvariantCulture));
            else if (serializationToUse == DateTimeSerialization.MicrosecondsNumber)
                _parameters.Add(key, DateTimeConverter.ConvertToMicroseconds(value));
            else if (serializationToUse == DateTimeSerialization.MicrosecondsString)
                _parameters.Add(key, DateTimeConverter.ConvertToMicroseconds(value).Value.ToString(CultureInfo.InvariantCulture));
            else if (serializationToUse == DateTimeSerialization.Rfc3339String)
                _parameters.Add(key, value.ToRfc3339String());
            else
                throw new ArgumentException("Unknown DateTime serialization setting");
        }

        /// <summary>
        /// Add a string value if it is not null
        /// </summary>
        public void Add(string key, string? value)
        {
            if (value == null)
                return;

            _parameters.Add(key, value);
        }

        /// <summary>
        /// Add an array of values if there are values provided
        /// </summary>
        public void AddArray<T>(string key, IEnumerable<T>? values)
        {
            if (values == null || !values.Any())
                return;

            _parameters.Add(key, values is T[] arr ? arr : values.ToArray());
        }

        /// <summary>
        /// Add a raw object value if it is not null
        /// </summary>
        public void AddRaw(string key, object? value)
        {
            if (value == null)
                return;

            _parameters.Add(key, value);
        }

        /// <inheritdoc />
        public void Add(string key, object value) => _parameters.Add(key, value);
        /// <inheritdoc />
        public bool ContainsKey(string key) => _parameters.ContainsKey(key);
        /// <inheritdoc />
        public bool Remove(string key) => _parameters.Remove(key);
        /// <inheritdoc />
        public bool TryGetValue(string key, out object value) => _parameters.TryGetValue(key, out value!);
        /// <inheritdoc />
        public void Add(KeyValuePair<string, object> item) => _parameters.Add(item.Key, item.Value);
        /// <inheritdoc />
        public void Clear() => _parameters.Clear();
        /// <inheritdoc />
        public bool Contains(KeyValuePair<string, object> item) => _parameters.ContainsKey(item.Key) && _parameters[item.Key] == item.Value;
        /// <inheritdoc />
        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) => _parameters.CopyTo(array, arrayIndex);
        /// <inheritdoc />
        public bool Remove(KeyValuePair<string, object> item) => _parameters.Remove(item.Key);
        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _parameters.GetEnumerator();
        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
