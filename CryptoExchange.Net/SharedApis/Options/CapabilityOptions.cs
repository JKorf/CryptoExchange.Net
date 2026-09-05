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
        /// <summary>
        /// Request parameter rules for the capability. The rules differ per exchange API and can be used to
        /// dynamically discover and adjust the request parameters for a capability.
        /// </summary>
        public RequestParameterDescription[] RequestParameterRules { get; private set; } = [];
        /// <summary>
        /// Exchange parameter rules for the capability. These are parameters specific to the exchange API and
        /// must be provided in the request via the ExchangeParameters property of the request object.
        /// </summary>
        public ExchangeParameterDescription[] ExchangeParameterRules { get; set; } = [];

        private readonly RequestParameterDescription[] _defaultParameterRules;


        /// <summary>
        /// Deprectaed, use RequestParameterRules instead.
        /// </summary>
        public List<ParameterDescription> RequiredOptionalParameters
        {
            get => RequestParameterRules.Where(x => x.DefaultSupport == RequestParameterSupport.Optional
                                                    && x.Support == RequestParameterSupport.Required)
                                        .Cast<ParameterDescription>()
                                        .ToList();
        }

        /// <summary>
        /// Deprectaed, use ExchangeParameterRules instead.
        /// </summary>
        public List<ParameterDescription> RequiredExchangeParameters
        {
            get => ExchangeParameterRules.Where(x => x.Requirement == ExchangeParameterRequirement.Required).Cast<ParameterDescription>().ToList();
        }
        /// <summary>
        /// Deprectaed, use ExchangeParameterRules instead.
        /// </summary>
        public List<ParameterDescription> OptionalExchangeParameters
        {
            get => ExchangeParameterRules.Where(x => x.Requirement == ExchangeParameterRequirement.Optional).Cast<ParameterDescription>().ToList();
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
        public CapabilityOptions(
            string exchange,
            string operationName,
            bool needAuthentication,
            IEnumerable<RequestParameterDescription> defaultParameterRules)
        {
            Exchange = exchange;
            OperationName = operationName;
            NeedsAuthentication = needAuthentication;

            _defaultParameterRules = defaultParameterRules
                .Select(x => x.WithSupport(x.DefaultSupport))
                .ToArray();

            RequestParameterRules = CombineParameters(
                _defaultParameterRules,
                Array.Empty<RequestParameterRuleOverride>());
        }

        /// <summary>
        /// Set request parameter overrides for this instance
        /// </summary>
        /// <param name="overrides">The parameter overrides</param>
        protected void SetRequestParameters(IEnumerable<RequestParameterRuleOverride> overrides)
        {
            RequestParameterRules = CombineParameters(_defaultParameterRules, overrides);
        }

        private RequestParameterDescription[] CombineParameters(
            RequestParameterDescription[] defaultParameters,
            IEnumerable<RequestParameterRuleOverride> overrideParameters
            )
        {
            var result = defaultParameters
                .Select(x => x.WithSupport(x.DefaultSupport))
                .ToList();

            foreach (var item in overrideParameters)
            {
                var index = result.FindIndex(
                    x => x.Name == item.ParameterName);

                if (index == -1)
                {
                    throw new ArgumentException(
                        $"No default request parameter rule exists for " +
                        $"`{item.ParameterName}`");
                }

                result[index] = result[index].WithOverride(item);
            }

            return result.ToArray();
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
                if (ExchangeParameters.HasValue(exchangeParameters, Exchange, param.Name, param.ValueType) != true
                    && param.Aliases.Any(x => ExchangeParameters.HasValue(exchangeParameters, Exchange, x, param.ValueType) != true))
                {
                    if (param.Aliases.Length == 0)
                        return ArgumentError.Invalid(param.Name, $"Exchange parameter `{param.Name}` for exchange `{Exchange}` should be provided. Example: {param.ExampleValue}");
                    else
                        return ArgumentError.Invalid(string.Join("/", [param.Name, .. param.Aliases]), $"One of exchange parameters `{string.Join(", ", [param.Name, .. param.Aliases])}` for exchange `{Exchange}` should be provided. Example: {param.ExampleValue}");
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
            if (RequestParameterRules.Any())
            {
                sb.AppendLine($"  Request parameters:");
                foreach (var param in RequestParameterRules)
                    sb.AppendLine($"    {param}");
            }
            if (ExchangeParameterRules.Any())
            {
                sb.AppendLine($"  Exchange specific parameters:");
                foreach (var param in ExchangeParameterRules)
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

        /// <inheritdoc />
        public virtual RequestParameterRuleOverride[] ParameterRuleOverwrites
        {
            set => SetRequestParameters(value);
        }

        /// <summary>
        /// ctor
        /// </summary>
        public CapabilityOptions(string exchange, bool needsAuthentication, string requestName, IEnumerable<RequestParameterDescription> defaultParameterRules)
            : base(exchange, requestName, needsAuthentication, defaultParameterRules)
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

            foreach (var param in RequestParameterRules)
            {
                var property = _requestProperties.Single(x => x.Name == param.Name);
                var value = property.GetValue(request);

                if (param.Support == RequestParameterSupport.Required)
                {
                    if (value == null)
                    {
                        return ArgumentError.Invalid(
                            param.Name,
                            $"Request parameter `{param.Name}` for exchange `{Exchange}` is required and should be provided. Example: {param.ExampleValue}");
                    }
                }
                else if (param.Support == RequestParameterSupport.NotSupported)
                {
                    if (value != null)
                    {
                        return ArgumentError.Invalid(
                            param.Name,
                            $"Request parameter `{param.Name}` is not supported by exchange `{Exchange}`");
                    }
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
