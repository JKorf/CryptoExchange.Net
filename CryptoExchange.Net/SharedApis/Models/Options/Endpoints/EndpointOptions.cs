using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        /// Optional exchange-specific parameters
        /// </summary>
        public List<ParameterDescription> OptionalExchangeParameters { get; set; } = new List<ParameterDescription>();
        /// <summary>
        /// Exchange
        /// </summary>
        public string Exchange { get; set; }
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
        /// Whether the call is supported by the exchange
        /// </summary>
        public bool Supported { get; set; } = true;

        /// <summary>
        /// ctor
        /// </summary>
        public EndpointOptions(string exchange, string endpointName, bool needAuthentication)
        {
            Exchange = exchange;
            EndpointName = endpointName;
            NeedsAuthentication = needAuthentication;
        }

        /// <summary>
        /// Validate a request
        /// </summary>
        /// <param name="exchangeParameters">Provided exchange parameters</param>
        /// <param name="tradingMode">Request trading mode</param>
        /// <param name="supportedTradingModes">Supported trading modes</param>
        /// <returns></returns>
        public virtual Error? ValidateRequest(ExchangeParameters? exchangeParameters, TradingMode? tradingMode, TradingMode[] supportedTradingModes)
        {
            if (tradingMode != null && !supportedTradingModes.Contains(tradingMode.Value))
                return ArgumentError.Invalid("TradingMode", $"TradingMode.{tradingMode} is not supported, supported types: {string.Join(", ", supportedTradingModes)}");

            foreach (var param in RequiredExchangeParameters)
            {
                if (!string.IsNullOrEmpty(param.Name))
                {
                    if (ExchangeParameters.HasValue(exchangeParameters, Exchange, param.Name!, param.ValueType) != true)
                        return ArgumentError.Invalid(param.Name!, $"Required exchange parameter `{param.Name}` for exchange `{Exchange}` is missing or has incorrect type. Expected type is {param.ValueType.Name}. Example: {param.ExampleValue}");
                }
                else
                {
                    if (param.Names!.All(x => ExchangeParameters.HasValue(exchangeParameters, Exchange, x, param.ValueType) != true))
                        return ArgumentError.Invalid(string.Join("/", param.Names!), $"One of exchange parameters `{string.Join(", ", param.Names!)}` for exchange `{Exchange}` should be provided. Example: {param.ExampleValue}");
                }
            }

            return null;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            if (!Supported)
                return $"{Exchange} {EndpointName} NOT SUPPORTED";

            var sb = new StringBuilder();
            sb.AppendLine($"{Exchange} {EndpointName}");
            if (!string.IsNullOrEmpty(RequestNotes))
                sb.AppendLine(RequestNotes);
            sb.AppendLine($"Needs authentication: {NeedsAuthentication}");
            sb.AppendLine($"Required exchange specific parameters: {string.Join(", ", RequiredExchangeParameters.Select(x => x.ToString()))}");
            sb.AppendLine($"Optional exchange specific parameters: {string.Join(", ", OptionalExchangeParameters.Select(x => x.ToString()))}");
            return sb.ToString();
        }
    }

    /// <summary>
    /// Options for an exchange endpoint
    /// </summary>
    /// <typeparam name="TRequest">Type of data</typeparam>
    /// <typeparam name="TClient">Type of the client</typeparam>
#if NET5_0_OR_GREATER
    public class EndpointOptions<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TRequest, TClient> : EndpointOptions 
    where TRequest : SharedRequest
    where TClient : ISharedClient
#else
    public class EndpointOptions<TRequest, TClient> : EndpointOptions
        where TRequest : SharedRequest
        where TClient : ISharedClient
#endif
    {
        /// <summary>
        /// Required optional parameters in the request
        /// </summary>
        public List<ParameterDescription> RequiredOptionalParameters { get; set; } = new List<ParameterDescription>();

        /// <summary>
        /// Whether this accepts multiple symbols (Only applicable to request requiring symbol parameters)
        /// </summary>
        public bool SupportsMultipleSymbols { get; set; } = false;
        /// <summary>
        /// The max number of symbols which can be passed in a call (Only applicable to request requiring symbol parameters)
        /// </summary>
        public int? MaxSymbolCount { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public EndpointOptions(string exchange, bool needsAuthentication) : base(exchange, typeof(TRequest).Name, needsAuthentication)
        {
        }

        /// <summary>
        /// Validate a request
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="client">Containing client</param>
        /// <returns></returns>
        public virtual Error? ValidateRequest(TRequest request, TClient client)
        {
            foreach (var param in RequiredOptionalParameters)
            {
                if (!string.IsNullOrEmpty(param.Name))
                {
                    if (typeof(TRequest).GetProperty(param.Name)!.GetValue(request, null) == null)
                        return ArgumentError.Invalid(param.Name!, $"Required optional parameter `{param.Name}` for exchange `{Exchange}` is missing. Example: {param.ExampleValue}");
                }
                else
                {
                    if (param.Names!.All(x => typeof(TRequest).GetProperty(x)!.GetValue(request, null) == null))
                        return ArgumentError.Invalid(string.Join("/", param.Names!), $"One of optional parameters `{string.Join(", ", param.Names!)}` for exchange `{Exchange}` should be provided. Example: {param.ExampleValue}");
                }

            }

            if (request is SharedSymbolRequest symbolsRequest)
            {
                if (symbolsRequest.Symbols != null) 
                {
                    if (!SupportsMultipleSymbols)
                        return ArgumentError.Invalid(nameof(SharedSymbolRequest.Symbols), $"Only a single symbol parameter is allowed, multiple symbols are not supported");

                    if (symbolsRequest.Symbols.Length > MaxSymbolCount)
                        return ArgumentError.Invalid(nameof(SharedSymbolRequest.Symbols), $"Max number of symbols is {MaxSymbolCount} but {symbolsRequest.Symbols.Length} were passed");
                }

            }

            return ValidateRequest(request.ExchangeParameters, request.TradingMode, client.SupportedTradingModes);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            if (!Supported)
                return $"{Exchange} {EndpointName} NOT SUPPORTED";

            var sb = new StringBuilder();
            sb.AppendLine($"{Exchange} {typeof(TRequest).Name}");
            sb.AppendLine($"Needs authentication: {NeedsAuthentication}");
            if (!string.IsNullOrEmpty(RequestNotes))
                sb.AppendLine(RequestNotes);
            if (RequiredOptionalParameters.Any())
                sb.AppendLine($"Required optional parameters: {string.Join(", ", RequiredOptionalParameters.Select(x => x.ToString()))}");
            if (RequiredExchangeParameters.Any())
                sb.AppendLine($"Required exchange specific parameters: {string.Join(", ", RequiredExchangeParameters.Select(x => x.ToString()))}");
            if (OptionalExchangeParameters.Any())
                sb.AppendLine($"Optional exchange specific parameters: {string.Join(", ", RequiredExchangeParameters.Select(x => x.ToString()))}");
            return sb.ToString();
        }
    }

}
