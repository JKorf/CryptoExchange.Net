using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.FilterOptions
{

    public record EndpointOptions
    {
        // parameters which aren't defined in the request, but required for this exchange
        public List<ParameterDescription> RequiredExchangeParameters { get; } = new List<ParameterDescription>();
        // Exchange specific request info
        public string? ExchangeRequestInfo { get; set; }
        public bool NeedsAuthentication { get; set; }

        public EndpointOptions(bool needAuthentication)
        {
            NeedsAuthentication = needAuthentication;
        }

        public virtual Error? ValidateRequest(string exchange, ExchangeParameters? exchangeParameters)
        {
            foreach(var param in RequiredExchangeParameters)
            {
                if (exchangeParameters?.HasValue(exchange, param.Name, param.ValueType) != true)
                    return new ArgumentError($"Required parameter `{param.Name}` for exchange `{exchange}` is missing or has incorrect type. Excpected type is {param.ValueType.Name}.");
            }

            return null;
        }
    }

    public record EndpointOptions<T> : EndpointOptions
    {

        // parameters which are optional in the request, but required for this exchange
        public List<ParameterDescription> RequiredOptionalParameters { get; } = new List<ParameterDescription>();

        public EndpointOptions(bool needsAuthentication) : base(needsAuthentication)
        {
        }

        public virtual Error? ValidateRequest(string exchange, T request, ExchangeParameters? exchangeParameters)
        {
            var exchangeParametersError = ValidateRequest(exchange, exchangeParameters);
            if (exchangeParametersError != null)
                return exchangeParametersError;

            return null;
        }
    }

    public record PaginatedEndpointOptions : EndpointOptions
    {
        public bool PaginationSupport { get; }

        public PaginatedEndpointOptions(bool paginationSupported, bool needsAuthentication) : base(needsAuthentication)
        {
            PaginationSupport = paginationSupported;
        }
    }

    public record ParameterDescription
    {
        public string Name { get; set; }
        public Type ValueType { get; set; }
        public string Description { get; set; }
        public string ExampleValue { get; set; }
    }
}
