using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.FilterOptions
{

    public record SubscriptionOptions
    {
        public List<ParameterDescription> RequiredExchangeParameters { get; set; } = new List<ParameterDescription>();
        public string SubscriptionName { get; set; }
        // Exchange specific request info
        public string? ExchangeSubscriptionInfo { get; set; }
        public bool NeedsAuthentication { get; set; }

        public SubscriptionOptions(string endpointName, bool needAuthentication)
        {
            NeedsAuthentication = needAuthentication;
        }

        public virtual Error? ValidateRequest(string exchange, ExchangeParameters? exchangeParameters, ApiType? apiType, ApiType[] supportedApiTypes)
        {
            if (apiType.HasValue && !supportedApiTypes.Contains(apiType.Value))
                return new ArgumentError($"ApiType.{apiType} is not supported, supported types: {string.Join(", ", supportedApiTypes)}");


            foreach (var param in RequiredExchangeParameters)
            {
                if (!string.IsNullOrEmpty(param.Name))
                {
                    if (ExchangeParameters.HasValue(exchangeParameters, exchange, param.Name, param.ValueType) != true)
                        return new ArgumentError($"Required exchange parameter `{param.Name}` for exchange `{exchange}` is missing or has incorrect type. Expected type is {param.ValueType.Name}. Example: {param.ExampleValue}");
                }
                else
                {
                    if (param.Names.All(x => ExchangeParameters.HasValue(exchangeParameters, exchange, x, param.ValueType) != true))
                        return new ArgumentError($"One of exchange parameters `{string.Join(", ", param.Names)}` for exchange `{exchange}` should be provided. Example: {param.ExampleValue}");
                }
            }

            return null;
        }
        public string ToString(string exchange)
        {
            var sb = new StringBuilder();
            sb.Append($"Exchange {exchange} {SubscriptionName} endpoint");
            return sb.ToString();
        }
    }

    public record SubscriptionOptions<T> : SubscriptionOptions where T : SharedRequest
    {
        // parameters which are optional in the request, but required for this exchange
        public List<ParameterDescription> RequiredOptionalParameters { get; set; } = new List<ParameterDescription>();

        public SubscriptionOptions(bool needsAuthentication) : base(typeof(T).Name, needsAuthentication)
        {
        }

        public virtual Error? ValidateRequest(string exchange, T request, ApiType? apiType, ApiType[] supportedApiTypes)
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

            return base.ValidateRequest(exchange, request.ExchangeParameters, apiType, supportedApiTypes);
        }

        public string ToString(string exchange)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{exchange} {typeof(T).Name}");
            sb.Append($"Needs authentication: {NeedsAuthentication}");
            if (!string.IsNullOrEmpty(ExchangeSubscriptionInfo))
                sb.AppendLine(ExchangeSubscriptionInfo);
            if (RequiredOptionalParameters.Any())
                sb.AppendLine($"Required optional parameters: {string.Join(", ", RequiredOptionalParameters.Select(x => x.ToString()))}");
            if (RequiredExchangeParameters.Any())
                sb.AppendLine($"Required exchange specific parameters: {string.Join(", ", RequiredExchangeParameters.Select(x => x.ToString()))}");
            return sb.ToString();
        }
    }
}
