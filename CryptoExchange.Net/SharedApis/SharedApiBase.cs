using CryptoExchange.Net.Clients;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Shared API base client
    /// </summary>
    public abstract class SharedApiBase : ISharedApi
    {
        private readonly Func<string, string, TradingMode, DateTime?, string> _symbolFormatter;
        private readonly Func<bool> _authDelegate;

        private IReadOnlyCollection<CapabilityOptions> _capabilities = Array.Empty<CapabilityOptions>();
        IReadOnlyCollection<CapabilityOptions> ISharedApi.Capabilities => _capabilities;

        /// <inheritdoc />
        public string Exchange { get; }

        /// <inheritdoc />
        public SharedTransport Transport { get; }

        /// <inheritdoc />
        public TradingMode[] SupportedTradingModes { get; }

        /// <inheritdoc />
        public bool Authenticated => _authDelegate();

        /// <summary>
        /// Shared API base client
        /// </summary>
        public SharedApiBase(
            SharedTransport transport,
            string exchange,
            TradingMode[] supportedTradingModes,
            Func<bool> authenticated,
            Func<string, string, TradingMode, DateTime?, string> formatSymbol)
        {
            Transport = transport;
            Exchange = exchange;
            SupportedTradingModes = supportedTradingModes;
            _authDelegate = authenticated;
            _symbolFormatter = formatSymbol;
        }

        /// <inheritdoc />
        protected void SetCapabilities(params CapabilityOptions[] capabilities)
        {
            _capabilities = Array.AsReadOnly(capabilities);
        }

        /// <inheritdoc />
        public string FormatSymbol(string baseAsset, string quoteAsset, TradingMode tradingMode, DateTime? deliverDate = null)
            => _symbolFormatter(baseAsset, quoteAsset, tradingMode, deliverDate);

        /// <inheritdoc />
        public void SetDefaultExchangeParameter(string name, object value) => ExchangeParameters.SetStaticParameter(Exchange, name, value);

        /// <inheritdoc />
        public void ResetDefaultExchangeParameters() => ExchangeParameters.ResetStaticExchangeParameters(Exchange);

        /// <inheritdoc />
        public abstract SharedClientInfo Discover();
    }
}
