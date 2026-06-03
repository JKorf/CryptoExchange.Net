using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.Objects
{
    /// <summary>
    /// Settings for parameter serialization
    /// </summary>
    public class ParameterSerializationSettings
    {
        public static ParameterSerializationSettings Default { get; } = new ParameterSerializationSettings();

        public bool Sort { get; set; } = true;
        public IComparer<string>? SortComparer { get; set; }
        public DecimalSerialization Decimal { get; set; } = DecimalSerialization.Number;
        public DateTimeSerialization DateTimes { get; set; } = DateTimeSerialization.MillisecondsNumber;
        public BoolSerialization Bool { get; set; } = BoolSerialization.Bool;
        public IntegerSerialization Integer { get; set; } = IntegerSerialization.Number;
        public EnumSerialization Enum { get; set; } = EnumSerialization.String;
    }


    /// <summary>
    /// Type of decimal value serialization
    /// </summary>
    public enum DecimalSerialization
    {
        /// <summary>
        /// Decimals should be serialized as numbers
        /// </summary>
        Number,
        /// <summary>
        /// Decimals should be strings
        /// </summary>
        String
    }

    /// <summary>
    /// Type of DateTime value serialization
    /// </summary>
    public enum DateTimeSerialization
    {
        /// <summary>
        /// DateTimes should be serialized as milliseconds number
        /// </summary>
        MillisecondsNumber,
        /// <summary>
        /// DateTimes should be serialized as milliseconds string
        /// </summary>
        MillisecondsString,
        /// <summary>
        /// DateTimes should be serialized as seconds number
        /// </summary>
        SecondsNumber,
        /// <summary>
        /// DateTimes should be serialized as seconds string
        /// </summary>
        SecondsString,
        /// <summary>
        /// DateTimes should be serialized as microseconds number
        /// </summary>
        MicrosecondsNumber,
        /// <summary>
        /// DateTimes should be serialized as microseconds string
        /// </summary>
        MicrosecondsString,
    }

    /// <summary>
    /// Type of boolean value serialization
    /// </summary>
    public enum BoolSerialization
    {
        /// <summary>
        /// Booleans should be serialized as bool values
        /// </summary>
        Bool,
        /// <summary>
        /// Booleans should be serialized as strings
        /// </summary>
        String
    }

    /// <summary>
    /// Type of integer value serialization
    /// </summary>
    public enum IntegerSerialization
    {
        /// <summary>
        /// Integers should be serialized as integer values
        /// </summary>
        Number,
        /// <summary>
        /// Integers should be serialized as strings
        /// </summary>
        String
    }

    /// <summary>
    /// Type of enum value serialization
    /// </summary>
    public enum EnumSerialization
    {
        /// <summary>
        /// Enums should be serialized as integer values
        /// </summary>
        Number,
        /// <summary>
        /// Enums should be serialized as strings
        /// </summary>
        String
    }
}
