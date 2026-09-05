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
        /// Name of the parameter
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Alternative names for the parameter.
        /// </summary>
        public string[] Aliases { get; set; } = [];
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
        /// Names of the parameter
        /// </summary>
        [Obsolete("Use Name and Aliases instead")]
        public string[] Names
        {
            get => [Name, .. Aliases];
        }


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
        public ParameterDescription(string parameterName, string[] aliases, Type valueType, string description, object exampleValue)
        {
            Name = parameterName;
            Aliases = aliases;
            ValueType = valueType;
            Description = description;
            ExampleValue = exampleValue;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{ValueType.Name}] \"{Name}\" {string.Join(" / ", Aliases.Select(x => $"\"{x}\""))}: {Description}";
        }
    }

    /// <summary>
    /// Request parameter description
    /// </summary>
    public class RequestParameterDescription : ParameterDescription
    {
        /// <summary>
        /// Status defined by the shared request contract.
        /// </summary>
        public RequestParameterSupport DefaultSupport { get; }

        /// <summary>
        /// Status after applying exchange overrides.
        /// </summary>
        public RequestParameterSupport Support { get; }

        /// <summary>
        /// ctor
        /// </summary>
        public RequestParameterDescription(
            RequestParameterSupport support,
            string parameterName,
            Type valueType,
            string description,
            object exampleValue)
            : base(parameterName, valueType, description, exampleValue)
        {
            Support = support;
            DefaultSupport = support;
        }

        private RequestParameterDescription(
            RequestParameterSupport defaultSupport,
            RequestParameterSupport support,
            string parameterName,
            Type valueType,
            string description,
            object exampleValue)
            : base(parameterName, valueType, description, exampleValue)
        {
            DefaultSupport = defaultSupport;
            Support = support;
        }

        internal RequestParameterDescription WithSupport(
            RequestParameterSupport support)
            => new(
                DefaultSupport,
                support,
                Name,
                ValueType,
                Description,
                ExampleValue);

        internal RequestParameterDescription WithOverride(
            RequestParameterRuleOverride parameterOverride)
            => new(
                DefaultSupport,
                parameterOverride.Support,
                Name,
                ValueType,
                parameterOverride.Description ?? Description,
                ExampleValue);

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{Support}] [{ValueType.Name}] \"{Name}\" {string.Join(" / ", Aliases.Select(x => $"\"{x}\""))}: {Description}";
        }
    }

    /// <summary>
    /// Exchange-specific parameter description
    /// </summary>
    public class ExchangeParameterDescription : ParameterDescription
    {
        /// <summary>
        /// Requirement for the parameter
        /// </summary>
        public ExchangeParameterRequirement Requirement { get; init; }

        /// <summary>
        /// ctor
        /// </summary>
        public ExchangeParameterDescription(
            ExchangeParameterRequirement requirement,
            string parameterName,
            string[] aliases,
            Type valueType,
            string description,
            object exampleValue)
            : base(parameterName, aliases, valueType, description, exampleValue)
        {
            Requirement = requirement;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{Requirement}] [{ValueType.Name}] \"{Name}\" {string.Join(" / ", Aliases.Select(x => $"\"{x}\""))}: {Description}";
        }
    }
}
