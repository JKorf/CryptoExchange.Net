using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.FilterOptions
{

    public record EndpointOptions
    {
        // parameters which aren't defined in the request, but required for this exchange
        public List<ParameterDescription> RequiredExchangeParameters { get; set; } = new List<ParameterDescription>();
        public string EndpointName { get; set; }
        // Exchange specific request info
        public string? ExchangeRequestInfo { get; set; }
        public bool NeedsAuthentication { get; set; }

        public EndpointOptions(string endpointName, bool needAuthentication)
        {
            NeedsAuthentication = needAuthentication;
        }

        public virtual Error? ValidateRequest(string exchange, ExchangeParameters? exchangeParameters, ApiType apiType, ApiType[] supportedApiTypes)
        {
            if (!supportedApiTypes.Contains(apiType))
                return new ArgumentError($"ApiType.{apiType} is not supported, supported types: {string.Join(", ", supportedApiTypes)}");

            foreach (var param in RequiredExchangeParameters)
            {
                if (!string.IsNullOrEmpty(param.Name))
                {
                    if (exchangeParameters?.HasValue(exchange, param.Name, param.ValueType) != true)
                        return new ArgumentError($"Required exchange parameter `{param.Name}` for exchange `{exchange}` is missing or has incorrect type. Expected type is {param.ValueType.Name}. Example: {param.ExampleValue}");
                }
                else
                {
                    if (param.Names.All(x => exchangeParameters?.HasValue(exchange, x, param.ValueType) != true))
                        return new ArgumentError($"One of exchange parameters `{string.Join(", ", param.Names)}` for exchange `{exchange}` should be provided. Example: {param.ExampleValue}");
                }
            }

            return null;
        }
        public string ToString(string exchange)
        {
            var sb = new StringBuilder();
            sb.Append($"Exchange {exchange} {EndpointName} endpoint");
            sb.Append($"Required exchange specific parameters: {string.Join(", ", RequiredExchangeParameters.Select(x => x.ToString()))}");
            return sb.ToString();
        }
    }

    public record EndpointOptions<T> : EndpointOptions
    {
        // parameters which are optional in the request, but required for this exchange
        public List<ParameterDescription> RequiredOptionalParameters { get; set; } = new List<ParameterDescription>();

        public EndpointOptions(bool needsAuthentication) : base(typeof(T).Name, needsAuthentication)
        {
        }

        public virtual Error? ValidateRequest(string exchange, T request, ExchangeParameters? exchangeParameters, ApiType apiType, ApiType[] supportedApiTypes)
        {
            foreach (var param in RequiredOptionalParameters)
            {
#warning do we want to use reflection here? Maybe use it optionally?
                if (!string.IsNullOrEmpty(param.Name))
                {
                    if (typeof(T).GetProperty(param.Name).GetValue(request, null) == null)
                        return new ArgumentError($"Required optional parameter `{param.Name}` for exchange `{exchange}` is missing. Example: {param.ExampleValue}");
                }
                else
                {
                    if (param.Names.All(x => typeof(T).GetProperty(param.Name).GetValue(request, null) == null))
                        return new ArgumentError($"One of optional parameters `{string.Join(", ", param.Names)}` for exchange `{exchange}` should be provided. Example: {param.ExampleValue}");
                }

            }

            return ValidateRequest(exchange, exchangeParameters, apiType, supportedApiTypes);
        }

        public string ToString(string exchange)
        {
            var sb = new StringBuilder();
            sb.Append($"Exchange {exchange} {typeof(T).Name} endpoint");
            sb.Append($"Required optional parameters: {string.Join(", ", RequiredOptionalParameters.Select(x => x.ToString()))}");
            sb.Append($"Required exchange specific parameters: {string.Join(", ", RequiredExchangeParameters.Select(x => x.ToString()))}");
            return sb.ToString();
        }
    }

    public record PaginatedEndpointOptions<T> : EndpointOptions<T>
    {
        public bool PaginationSupport { get; }

        public PaginatedEndpointOptions(bool paginationSupported, bool needsAuthentication) : base(needsAuthentication)
        {
            PaginationSupport = paginationSupported;
        }
    }

    public record PaginatedEndpointOptions : EndpointOptions
    {
        public bool PaginationSupport { get; }

        public PaginatedEndpointOptions(string endpointName, bool paginationSupported, bool needsAuthentication) : base(endpointName, needsAuthentication)
        {
            PaginationSupport = paginationSupported;
        }
    }

    public record ParameterDescription
    {
        public string? Name { get; set; }
        public string[]? Names { get; set; }
        public Type ValueType { get; set; }
        public string Description { get; set; }
        public object ExampleValue { get; set; }

        public ParameterDescription(string parameterName, Type valueType, string description, object exampleValue)
        {
            Name = parameterName;
            ValueType = valueType;
            Description = description;
            ExampleValue = exampleValue;
        }
        public ParameterDescription(string[] parameterNames, Type valueType, string description, object exampleValue)
        {
            Names = parameterNames;
            ValueType = valueType;
            Description = description;
            ExampleValue = exampleValue;
        }

        public override string ToString()
        {
            if (Name != null)
                return $"[{ValueType.Name}] {Name}: {Description} | example: {ExampleValue}";
            return $"[{ValueType.Name}] {string.Join(" / ", Names)}: {Description} | example: {ExampleValue}";
        }
    }
}
