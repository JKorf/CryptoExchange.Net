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
    public class Parameters : IParameters
    {
        private readonly ParameterSerializationSettings _serializationSettings;
        private SortedDictionary<string, object> _parameters;
        private object? _value;

        /// <inheritdoc />
        public IDictionary<string, object> Dictionary => _parameters;
        /// <inheritdoc />
        public object? BodyValue => _value;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="serializationSettings">Serialization settings</param>
        public Parameters(ParameterSerializationSettings serializationSettings, IComparer<string>? comparer = null)
        {
            _serializationSettings = serializationSettings;
            _parameters = new SortedDictionary<string, object>(comparer);
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="serializationSettings">Serialization settings</param>
        /// <param name="value">Body value</param>
        public Parameters(object value, ParameterSerializationSettings serializationSettings)
        {
            _serializationSettings = serializationSettings;
            _value = value;
        }

        public void Add(string key, short? value)
        {
            if (value == null)
                return;

            Add(key, value.Value);
        }

        /// <inheritdoc />
        public void Add(string key, short value)
        {
            if (_serializationSettings.Integer == IntegerSerialization.String)
                _parameters.Add(key, value.ToString(CultureInfo.InvariantCulture));
            else if (_serializationSettings.Integer == IntegerSerialization.Number)
                _parameters.Add(key, value);
            else
                throw new ArgumentException("Unknown Integer serialization setting");
        }

        public void Add(string key, int? value)
        {
            if (value == null)
                return;

            Add(key, value.Value);
        }

        /// <inheritdoc />
        public void Add(string key, int value)
        {
            if (_serializationSettings.Integer == IntegerSerialization.String)
                _parameters.Add(key, value.ToString(CultureInfo.InvariantCulture));
            else if (_serializationSettings.Integer == IntegerSerialization.Number)
                _parameters.Add(key, value);
            else
                throw new ArgumentException("Unknown Integer serialization setting");
        }


        public void Add(string key, long? value)
        {
            if (value == null)
                return;

            Add(key, value.Value);
        }

        /// <inheritdoc />
        public void Add(string key, long value)
        {
            if (_serializationSettings.Integer == IntegerSerialization.String)
                _parameters.Add(key, value.ToString(CultureInfo.InvariantCulture));
            else if (_serializationSettings.Integer == IntegerSerialization.Number)
                _parameters.Add(key, value);
            else
                throw new ArgumentException("Unknown Integer serialization setting");
        }

        public void Add(string key, decimal? value)
        {
            if (value == null)
                return;

            Add(key, value.Value);
        }

        /// <inheritdoc />
        public void Add(string key, decimal value)
        {
            if (_serializationSettings.Decimal == DecimalSerialization.String)
                _parameters.Add(key, value.ToString(CultureInfo.InvariantCulture));
            else if (_serializationSettings.Decimal == DecimalSerialization.Number)
                _parameters.Add(key, value);
            else
                throw new ArgumentException("Unknown Decimal serialization setting");
        }

        public void Add(string key, double? value)
        {
            if (value == null)
                return;

            Add(key, value.Value);
        }

        /// <inheritdoc />
        public void Add(string key, double value)
        {
            if (_serializationSettings.Decimal == DecimalSerialization.String)
                _parameters.Add(key, value.ToString(CultureInfo.InvariantCulture));
            else if (_serializationSettings.Decimal == DecimalSerialization.Number)
                _parameters.Add(key, value);
            else
                throw new ArgumentException("Unknown Decimal serialization setting");
        }

        public void Add(string key, bool? value)
        {
            if (value == null)
                return;

            Add(key, value.Value);
        }

        /// <inheritdoc />
        public void Add(string key, bool value)
        {
            if (_serializationSettings.Bool == BoolSerialization.String)
                _parameters.Add(key, value.ToString(CultureInfo.InvariantCulture));
            else if (_serializationSettings.Bool == BoolSerialization.Bool)
                _parameters.Add(key, value);
            else
                throw new ArgumentException("Unknown Bool serialization setting");
        }

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

        /// <inheritdoc />
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

        public void Add(string key, DateTime? value)
        {
            if (value == null)
                return;

            Add(key, value.Value);
        }

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

        public void Add(string key, string? value)
        {
            if (value == null)
                return;

            _parameters.Add(key, value);
        }

        public void ApplyOptionalParameters(IDictionary<string, object>? additionalParameters)
        {
            if (additionalParameters == null)
                return;

            foreach (var kvp in additionalParameters)
                _parameters[kvp.Key] = kvp.Value;
        }
    }
}
