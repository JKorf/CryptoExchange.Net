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
        public void Add(string key, short? value)
        {
            if (value == null)
                return;

            Add(key, value.Value);
        }

        /// <summary>
        /// Add a short value
        /// </summary>
        public void Add(string key, short value)
        {
            if (_serializationSettings.Integer == IntegerSerialization.String)
                _parameters.Add(key, value.ToString(CultureInfo.InvariantCulture));
            else if (_serializationSettings.Integer == IntegerSerialization.Number)
                _parameters.Add(key, value);
            else
                throw new ArgumentException("Unknown Integer serialization setting");
        }

        /// <summary>
        /// Add an int value if it is not null
        /// </summary>
        public void Add(string key, int? value)
        {
            if (value == null)
                return;

            Add(key, value.Value);
        }

        /// <summary>
        /// Add an int value
        /// </summary>
        public void Add(string key, int value)
        {
            if (_serializationSettings.Integer == IntegerSerialization.String)
                _parameters.Add(key, value.ToString(CultureInfo.InvariantCulture));
            else if (_serializationSettings.Integer == IntegerSerialization.Number)
                _parameters.Add(key, value);
            else
                throw new ArgumentException("Unknown Integer serialization setting");
        }


        /// <summary>
        /// Add a long value as string if it is not null
        /// </summary>
        public void AddAsString(string key, long? value)
        {
            if (value == null)
                return;

            AddAsString(key, value.Value);
        }

        /// <summary>
        /// Add a long value as string
        /// </summary>
        public void AddAsString(string key, long value)
        {
            _parameters.Add(key, value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Add a long value if it is not null
        /// </summary>
        public void Add(string key, long? value)
        {
            if (value == null)
                return;

            Add(key, value.Value);
        }

        /// <summary>
        /// Add a long value
        /// </summary>
        public void Add(string key, long value)
        {
            if (_serializationSettings.Integer == IntegerSerialization.String)
                _parameters.Add(key, value.ToString(CultureInfo.InvariantCulture));
            else if (_serializationSettings.Integer == IntegerSerialization.Number)
                _parameters.Add(key, value);
            else
                throw new ArgumentException("Unknown Integer serialization setting");
        }

        /// <summary>
        /// Add a decimal value if it is not null
        /// </summary>
        public void Add(string key, decimal? value)
        {
            if (value == null)
                return;

            Add(key, value.Value);
        }

        /// <summary>
        /// Add a decimal value
        /// </summary>
        public void Add(string key, decimal value)
        {
            if (_serializationSettings.Decimal == DecimalSerialization.String)
                _parameters.Add(key, value.ToString(CultureInfo.InvariantCulture));
            else if (_serializationSettings.Decimal == DecimalSerialization.Number)
                _parameters.Add(key, value);
            else
                throw new ArgumentException("Unknown Decimal serialization setting");
        }

        /// <summary>
        /// Add a decimal value as string if it is not null
        /// </summary>
        public void AddAsString(string key, decimal? value)
        {
            if (value == null)
                return;

            AddAsString(key, value.Value);
        }

        /// <summary>
        /// Add a decimal value as string
        /// </summary>
        public void AddAsString(string key, decimal value)
        {
            _parameters.Add(key, value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Add a double value if it is not null
        /// </summary>
        public void Add(string key, double? value)
        {
            if (value == null)
                return;

            Add(key, value.Value);
        }

        /// <summary>
        /// Add a double value
        /// </summary>
        public void Add(string key, double value)
        {
            if (_serializationSettings.Decimal == DecimalSerialization.String)
                _parameters.Add(key, value.ToString(CultureInfo.InvariantCulture));
            else if (_serializationSettings.Decimal == DecimalSerialization.Number)
                _parameters.Add(key, value);
            else
                throw new ArgumentException("Unknown Decimal serialization setting");
        }

        /// <summary>
        /// Add a bool value if it is not null
        /// </summary>
        public void Add(string key, bool? value)
        {
            if (value == null)
                return;

            Add(key, value.Value);
        }

        /// <summary>
        /// Add a bool value
        /// </summary>
        public void Add(string key, bool value)
        {
            if (_serializationSettings.Bool == BoolSerialization.String)
                _parameters.Add(key, value.ToString(CultureInfo.InvariantCulture));
            else if (_serializationSettings.Bool == BoolSerialization.Bool)
                _parameters.Add(key, value);
            else
                throw new ArgumentException("Unknown Bool serialization setting");
        }

        /// <summary>
        /// Add key as comma separated values if there are values provided
        /// </summary>
        public void AddOptionalCommaSeparated(string key, IEnumerable<string>? values)
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
            T>(string key, T? value)
            where T : struct, Enum

        {
            if (value == null)
                return;

            Add(key, value.Value);
        }

        /// <summary>
        /// Add a enum value
        /// </summary>
        public void Add<
#if NET5_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicFields)]
#endif
            T>(string key, T value)
            where T : struct, Enum
        {
            if (_serializationSettings.Enum == EnumSerialization.String)
                _parameters.Add(key, EnumConverter<T>.GetString(value));
            else if (_serializationSettings.Enum == EnumSerialization.Number)
                _parameters.Add(key, int.Parse(EnumConverter<T>.GetString(value), CultureInfo.InvariantCulture));
            else
                throw new ArgumentException("Unknown Integer serialization setting");
        }

        /// <summary>
        /// Add an enum as int value if it is not null
        /// </summary>
        public void AddAsInt<
#if NET5_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicFields)]
#endif
        T>(string key, T? value)
            where T : struct, Enum

        {
            if (value == null)
                return;

            AddAsInt(key, value.Value);
        }

        /// <summary>
        /// Add an enum as int value
        /// </summary>
        public void AddAsInt<
#if NET5_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicFields)]
#endif
        T>(string key, T value)
            where T : struct, Enum
        {
            _parameters.Add(key, int.Parse(EnumConverter<T>.GetString(value), CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Add a DateTime value if it is not null
        /// </summary>
        public void Add(string key, DateTime? value)
        {
            if (value == null)
                return;

            Add(key, value.Value);
        }

        /// <summary>
        /// Add a DateTime value
        /// </summary>
        public void Add(string key, DateTime value)
        {
            if (_serializationSettings.DateTimes == DateTimeSerialization.MillisecondsNumber)
                _parameters.Add(key, DateTimeConverter.ConvertToMilliseconds(value));
            else if (_serializationSettings.DateTimes == DateTimeSerialization.MillisecondsString)
                _parameters.Add(key, DateTimeConverter.ConvertToMilliseconds(value).Value.ToString(CultureInfo.InvariantCulture));
            else if (_serializationSettings.DateTimes == DateTimeSerialization.SecondsNumber)
                _parameters.Add(key, DateTimeConverter.ConvertToSeconds(value));
            else if (_serializationSettings.DateTimes == DateTimeSerialization.SecondsString)
                _parameters.Add(key, DateTimeConverter.ConvertToSeconds(value).Value.ToString(CultureInfo.InvariantCulture));
            else if (_serializationSettings.DateTimes == DateTimeSerialization.MicrosecondsNumber)
                _parameters.Add(key, DateTimeConverter.ConvertToMicroseconds(value));
            else if (_serializationSettings.DateTimes == DateTimeSerialization.MicrosecondsString)
                _parameters.Add(key, DateTimeConverter.ConvertToMicroseconds(value).Value.ToString(CultureInfo.InvariantCulture));
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
        /// Add a raw object value if it is not null
        /// </summary>
        public void AddRaw(string key, object? value)
        {
            if (value == null)
                return;

            _parameters.Add(key, value);
        }

        /// <summary>
        /// Apply a set of raw parameters, overwriting existing ones with the same key
        /// </summary>
        public void ApplyRawParameters(IDictionary<string, object>? rawParameters)
        {
            if (rawParameters == null)
                return;

            foreach (var kvp in rawParameters)
                _parameters[kvp.Key] = kvp.Value;
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
