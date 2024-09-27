using System;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Parameter description
    /// </summary>
    public class ParameterDescription
    {
        /// <summary>
        /// Name of the parameter
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// Names of the parameters
        /// </summary>
        public string[]? Names { get; set; }
        /// <summary>
        /// Type of the value
        /// </summary>
        public Type ValueType { get; set; }
        /// <summary>
        /// Parameter description
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Example value
        /// </summary>
        public object ExampleValue { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public ParameterDescription(string parameterName, Type valueType, string description, object exampleValue)
        {
            Name = parameterName;
            ValueType = valueType;
            Description = description;
            ExampleValue = exampleValue;
        }

        /// <summary>
        /// ctor
        /// </summary>
        public ParameterDescription(string[] parameterNames, Type valueType, string description, object exampleValue)
        {
            Names = parameterNames;
            ValueType = valueType;
            Description = description;
            ExampleValue = exampleValue;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            if (Name != null)
                return $"[{ValueType.Name}] {Name}: {Description} | example: {ExampleValue}";
            return $"[{ValueType.Name}] {string.Join(" / ", Names)}: {Description} | example: {ExampleValue}";
        }
    }
}
