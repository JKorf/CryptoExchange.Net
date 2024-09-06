using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.FilterOptions
{

    public record SubscriptionOptions
    {
        public string SubscriptionName { get; set; }
        // Exchange specific request info
        public string? ExchangeSubscriptionInfo { get; set; }
        public bool NeedsAuthentication { get; set; }

        public SubscriptionOptions(string endpointName, bool needAuthentication)
        {
            NeedsAuthentication = needAuthentication;
        }

        public virtual Error? ValidateRequest(string exchange, ApiType apiType, ApiType[] supportedApiTypes)
        {
            if (!supportedApiTypes.Contains(apiType))
                return new ArgumentError($"ApiType.{apiType} is not supported, supported types: {string.Join(", ", supportedApiTypes)}");

            return null;
        }
        public string ToString(string exchange)
        {
            var sb = new StringBuilder();
            sb.Append($"Exchange {exchange} {SubscriptionName} endpoint");
            return sb.ToString();
        }
    }

    public record SubscriptionOptions<T> : SubscriptionOptions
    {
        //// parameters which are optional in the request, but required for this exchange
        //public List<ParameterDescription> RequiredOptionalParameters { get; set; } = new List<ParameterDescription>();

        public SubscriptionOptions(bool needsAuthentication) : base(typeof(T).Name, needsAuthentication)
        {
        }

        public virtual Error? ValidateRequest(string exchange, T request, ApiType apiType, ApiType[] supportedApiTypes)
        {

            return base.ValidateRequest(exchange, apiType, supportedApiTypes);
        }

        //public string ToString(string exchange)
        //{
        //    var sb = new StringBuilder();
        //    sb.Append($"Exchange {exchange} {typeof(T).Name} endpoint");
        //    sb.Append($"Required optional parameters: {string.Join(", ", RequiredOptionalParameters.Select(x => x.ToString()))}");
        //    sb.Append($"Required exchange specific parameters: {string.Join(", ", RequiredExchangeParameters.Select(x => x.ToString()))}");
        //    return sb.ToString();
        //}
    }
}
