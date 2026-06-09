using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for an exchange endpoint
    /// </summary>
    public abstract class EndpointOptions
    {
        /// <summary>
        /// Required exchange-specific parameters. These can be provided via the `exchangeParameters` property of the request object.
        /// </summary>
        public List<ParameterDescription> RequiredExchangeParameters { get; set; } = new List<ParameterDescription>();
        /// <summary>
        /// Optional exchange-specific parameters. These can be provided via the `exchangeParameters` property of the request object.
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
        /// Exchange specific additional info
        /// </summary>
        public string? RequestNotes { get; set; }
        /// <summary>
        /// Whether the call requires authentication
        /// </summary>
        public bool NeedsAuthentication { get; set; }
        /// <summary>
        /// Whether the call is supported. If false the exchange API does not support this operation.
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
            if (!Supported)
                return ArgumentError.Invalid("Endpoint", $"Endpoint {Exchange} {EndpointName} is not supported by the API");

            if (tradingMode != null && !supportedTradingModes.Contains(tradingMode.Value))
                return ArgumentError.Invalid("TradingMode", $"TradingMode.{tradingMode} is not supported, supported types: {string.Join(", ", supportedTradingModes)}");

            foreach (var param in RequiredExchangeParameters)
            {                
                if (param.Names!.All(x => ExchangeParameters.HasValue(exchangeParameters, Exchange, x, param.ValueType) != true))
                    return ArgumentError.Invalid(string.Join("/", param.Names!), $"One of exchange parameters `{string.Join(", ", param.Names!)}` for exchange `{Exchange}` should be provided. Example: {param.ExampleValue}");
            }

            return null;
        }
    }

    /// <summary>
    /// Options for an exchange endpoint
    /// </summary>
    /// <typeparam name="TRequest">Type of data</typeparam>
#if NET5_0_OR_GREATER
    public class EndpointOptions<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TRequest> : EndpointOptions 
    where TRequest : SharedRequest
#else
    public abstract class EndpointOptions<TRequest> : EndpointOptions
        where TRequest : SharedRequest
#endif
    {
        /// <summary>
        /// ctor
        /// </summary>
        public EndpointOptions(string exchange, bool needsAuthentication, string requestName) : base(exchange, requestName, needsAuthentication)
        {
        }
    }

    /// <summary>
    /// Options for an exchange endpoint
    /// </summary>
    /// <typeparam name="TRequest">Type of data</typeparam>
    /// <typeparam name="TClient">Type of the client</typeparam>
#if NET5_0_OR_GREATER
    public abstract class EndpointOptions<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TRequest, TClient> : EndpointOptions <TRequest>
    where TRequest : SharedRequest
    where TClient : ISharedClient
#else
    public abstract class EndpointOptions<TRequest, TClient> : EndpointOptions<TRequest>
        where TRequest : SharedRequest
        where TClient : ISharedClient
#endif
    {
        private static PropertyInfo[] _requestProperties = typeof(TRequest).GetProperties();

        /// <summary>
        /// Required optional parameters in the request. These can be provided via the `exchangeParameters` property of the request object.
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
        public EndpointOptions(string exchange, bool needsAuthentication, string requestName) : base(exchange, needsAuthentication, requestName)
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
                if (param.Names!.All(x => _requestProperties.Single(p => p.Name == x).GetValue(request, null) == null))
                    return ArgumentError.Invalid(string.Join("/", param.Names!), $"One of optional parameters `{string.Join(", ", param.Names!)}` for exchange `{Exchange}` should be provided. Example: {param.ExampleValue}");
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
            sb.AppendLine($"{Exchange} {EndpointName}");
            sb.AppendLine($"Needs authentication: {NeedsAuthentication}");
            if (!string.IsNullOrEmpty(RequestNotes))
                sb.AppendLine(RequestNotes);
            if (RequiredOptionalParameters.Any())
            {
                sb.AppendLine($"Required optional parameters:");
                foreach(var param in RequiredOptionalParameters)
                    sb.AppendLine($"    {param}");
            }
            if (RequiredExchangeParameters.Any())
            {
                sb.AppendLine($"Required exchange specific parameters:");
                foreach (var param in RequiredExchangeParameters)
                    sb.AppendLine($"    {param}");
            }
            if (OptionalExchangeParameters.Any())
            {
                sb.AppendLine($"Optional exchange specific parameters:");
                foreach (var param in OptionalExchangeParameters)
                    sb.AppendLine($"    {param}");
            }
            return sb.ToString();
        }
    }

}
