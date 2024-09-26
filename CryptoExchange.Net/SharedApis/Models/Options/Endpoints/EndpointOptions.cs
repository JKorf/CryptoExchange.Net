using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for an exchange endpoint
    /// </summary>
    public class EndpointOptions
    {
        /// <summary>
        /// Required exchange-specific parameters
        /// </summary>
        public List<ParameterDescription> RequiredExchangeParameters { get; set; } = new List<ParameterDescription>();
        /// <summary>
        /// Endpoint name
        /// </summary>
        public string EndpointName { get; set; }
        /// <summary>
        /// Information on the specific exchange request
        /// </summary>
        public string? RequestNotes { get; set; }
        /// <summary>
        /// Whether the call requires authentication
        /// </summary>
        public bool NeedsAuthentication { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public EndpointOptions(string endpointName, bool needAuthentication)
        {
            EndpointName = endpointName;
            NeedsAuthentication = needAuthentication;
        }

        /// <summary>
        /// Validate a request
        /// </summary>
        /// <param name="exchange">Exchange name</param>
        /// <param name="exchangeParameters">Provided exchange parameters</param>
        /// <param name="tradingMode">Request trading mode</param>
        /// <param name="supportedTradingModes">Supported trading modes</param>
        /// <returns></returns>
        public virtual Error? ValidateRequest(string exchange, ExchangeParameters? exchangeParameters, TradingMode? tradingMode, TradingMode[] supportedTradingModes)
        {
            if (tradingMode != null && !supportedTradingModes.Contains(tradingMode.Value))
                return new ArgumentError($"ApiType.{tradingMode} is not supported, supported types: {string.Join(", ", supportedTradingModes)}");

            foreach (var param in RequiredExchangeParameters)
            {
                if (!string.IsNullOrEmpty(param.Name))
                {
                    if (ExchangeParameters.HasValue(exchangeParameters, exchange, param.Name!, param.ValueType) != true)
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

        /// <inheritdoc />
        public virtual string ToString(string exchange)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{exchange} {EndpointName}");
            if (!string.IsNullOrEmpty(RequestNotes))
                sb.AppendLine(RequestNotes);
            sb.AppendLine($"Needs authentication: {NeedsAuthentication}");
            sb.AppendLine($"Required exchange specific parameters: {string.Join(", ", RequiredExchangeParameters.Select(x => x.ToString()))}");
            return sb.ToString();
        }
    }

    /// <summary>
    /// Options for an exchange endpoint
    /// </summary>
    /// <typeparam name="T">Type of data</typeparam>
    public class EndpointOptions<T> : EndpointOptions where T : SharedRequest
    {
        /// <summary>
        /// Required optional parameters in the request
        /// </summary>
        public List<ParameterDescription> RequiredOptionalParameters { get; set; } = new List<ParameterDescription>();

        /// <summary>
        /// ctor
        /// </summary>
        public EndpointOptions(bool needsAuthentication) : base(typeof(T).Name, needsAuthentication)
        {
        }

        /// <summary>
        /// Validate a request
        /// </summary>
        /// <param name="exchange">Exchange name</param>
        /// <param name="request">The request</param>
        /// <param name="tradingMode">Request trading mode</param>
        /// <param name="supportedTradingModes">Supported trading modes</param>
        /// <returns></returns>
        public virtual Error? ValidateRequest(string exchange, T request, TradingMode? tradingMode, TradingMode[] supportedTradingModes)
        {
            foreach (var param in RequiredOptionalParameters)
            {
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

            return ValidateRequest(exchange, request.ExchangeParameters, tradingMode, supportedTradingModes);
        }

        /// <inheritdoc />
        public override string ToString(string exchange)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{exchange} {typeof(T).Name}");
            sb.AppendLine($"Needs authentication: {NeedsAuthentication}");
            if (!string.IsNullOrEmpty(RequestNotes))
                sb.AppendLine(RequestNotes);
            if (RequiredOptionalParameters.Any())
                sb.AppendLine($"Required optional parameters: {string.Join(", ", RequiredOptionalParameters.Select(x => x.ToString()))}");
            if (RequiredExchangeParameters.Any())
                sb.AppendLine($"Required exchange specific parameters: {string.Join(", ", RequiredExchangeParameters.Select(x => x.ToString()))}");
            return sb.ToString();
        }
    }

}
