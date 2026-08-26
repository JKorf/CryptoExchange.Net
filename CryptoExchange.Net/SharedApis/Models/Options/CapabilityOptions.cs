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
    /// Options and description of a Shared API capability.
    /// </summary>
    public abstract class CapabilityOptions
    {
        private List<ParameterDescription> _requiredRequestParameters = new();

        /// <summary>
        /// Request properties that are optional in the shared contract,
        /// but required by this exchange implementation.
        /// </summary>
        public List<ParameterDescription> RequiredRequestParameters
        {
            get => _requiredRequestParameters;
            set => _requiredRequestParameters = value;
        }

        /// <summary>
        /// Use RequiredRequestParameters instead.
        /// </summary>
        [Obsolete("Use RequiredRequestParameters instead")]
        public List<ParameterDescription> RequiredOptionalParameters
        {
            get => _requiredRequestParameters;
            set => _requiredRequestParameters = value;
        }

        /// <summary>
        /// Whether this accepts multiple symbols (Only applicable to request requiring symbol parameters)
        /// </summary>
        public bool SupportsMultipleSymbols { get; set; } = false;
        /// <summary>
        /// The max number of symbols which can be passed in a call (Only applicable to request requiring symbol parameters)
        /// </summary>
        public int? MaxSymbolCount { get; set; }

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
        /// Operation name
        /// </summary>
        public string OperationName { get; set; }
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
        /// Description of the endpoint
        /// </summary>
        public abstract string Description { get; }
        /// <summary>
        /// The type of the capability
        /// </summary>
        public abstract Type CapabilityType { get; }

        /// <summary>
        /// ctor
        /// </summary>
        public CapabilityOptions(string exchange, string operationName, bool needAuthentication)
        {
            Exchange = exchange;
            OperationName = operationName;
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
                return ArgumentError.Invalid("Operation", $"Operation {Exchange} {OperationName} is not supported by the API");

            if (tradingMode != null && !supportedTradingModes.Contains(tradingMode.Value))
                return ArgumentError.Invalid("TradingMode", $"TradingMode.{tradingMode} is not supported, supported types: {string.Join(", ", supportedTradingModes)}");

            foreach (var param in RequiredExchangeParameters)
            {
                if (param.Names!.All(x => ExchangeParameters.HasValue(exchangeParameters, Exchange, x, param.ValueType) != true))
                {
                    if (param.Names.Length == 1)
                        return ArgumentError.Invalid(string.Join("/", param.Names!), $"Exchange parameter `{param.Names[0]}` for exchange `{Exchange}` should be provided. Example: {param.ExampleValue}");
                    else
                        return ArgumentError.Invalid(string.Join("/", param.Names!), $"One of exchange parameters `{string.Join(", ", param.Names!)}` for exchange `{Exchange}` should be provided. Example: {param.ExampleValue}");
                }
            }

            return null;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{OperationName}");
            if (!Supported)
            {
                sb.AppendLine(" NOT SUPPORTED");
                return sb.ToString();
            }

            sb.AppendLine($"{Description}");
            if (!string.IsNullOrEmpty(RequestNotes))
            {
                sb.Append("  Notes:                          ");
                sb.AppendLine(RequestNotes);
            }
            if (RequiredRequestParameters.Any())
            {
                sb.AppendLine($"  Required request parameters:");
                foreach (var param in RequiredRequestParameters)
                    sb.AppendLine($"    {param}");
            }
            if (RequiredExchangeParameters.Any())
            {
                sb.AppendLine($"  Required exchange specific parameters:");
                foreach (var param in RequiredExchangeParameters)
                    sb.AppendLine($"    {param}");
            }
            if (OptionalExchangeParameters.Any())
            {
                sb.AppendLine($"  Optional exchange specific parameters:");
                foreach (var param in OptionalExchangeParameters)
                    sb.AppendLine($"    {param}");
            }
            sb.Append("  Needs authentication:           ");
            sb.AppendLine($"{NeedsAuthentication}");
            return sb.ToString();
        }
    }

    /// <summary>
    /// Options and description of a Shared API capability.
    /// </summary>
    public abstract class CapabilityOptions<
#if NET5_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
#pragma warning disable CS0618 // Type or member is obsolete
    TRequest, TCapability> : EndpointOptions // Derives from endpoint options for compatibility, but can derive from CapabilityOptions in a later release
#pragma warning restore CS0618 // Type or member is obsolete
            where TRequest : SharedRequest
            where TCapability : ISharedApiCapability
    {
        private static PropertyInfo[] _requestProperties = typeof(TRequest).GetProperties();

        /// <inheritdoc />
        public override Type CapabilityType => typeof(TCapability);

        /// <summary>
        /// ctor
        /// </summary>
        public CapabilityOptions(string exchange, bool needsAuthentication, string requestName) : base(exchange, requestName, needsAuthentication)
        {
        }

        /// <summary>
        /// Validate a request
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="client">Containing client</param>
        /// <returns></returns>
        public virtual Error? ValidateRequest(TRequest request, TCapability client)
        {
            if (NeedsAuthentication && !client.Authenticated)
                return new NoApiCredentialsError();

            foreach (var param in RequiredRequestParameters)
            {
                if (param.Names!.All(x => _requestProperties.Single(p => p.Name == x).GetValue(request, null) == null))
                {
                    if (param.Names.Length == 1)
                        return ArgumentError.Invalid(string.Join("/", param.Names!), $"Optional parameter `{param.Names[0]}` for exchange `{Exchange}` should be provided. Example: {param.ExampleValue}");
                    else
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
    }
}
