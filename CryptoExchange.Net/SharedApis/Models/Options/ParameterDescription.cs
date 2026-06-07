using System;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Parameter description
    /// </summary>
    public class ParameterDescription
    {
        /// <summary>
        /// Possible names for the parameter. One of these names can be used to provide the parameter in the ExchangeParameters of the request.
        /// </summary>
        public string[] Names { get; set; }
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
            Names = [parameterName];
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
            return $"[{ValueType.Name}] {string.Join(" / ", Names.Select(x => $"\"{x}\""))}: {Description}";
        }
    }
}
