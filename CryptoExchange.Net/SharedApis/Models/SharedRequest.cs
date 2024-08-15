using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.RequestModels
{
    public record SharedRequest
    {
        public ApiType? ApiType { get; set; }

        public Dictionary<string, object>? AdditionalParameters { get; set; }

        public T? GetAdditionalParameter<T>(string exchange, string name)
        {
            if (AdditionalParameters == null)
                return default;

            if (AdditionalParameters.TryGetValue(exchange + "." + name, out var exchangeValue))
            {
                if (exchangeValue is not T)
                    throw new ArgumentException("Incorrect type for parameter, expected " + typeof(T).Name, nameof(name));
                return (T)exchangeValue;
            }

            AdditionalParameters.TryGetValue(exchange + "." + name, out var value);
            if (value is not T)
                throw new ArgumentException("Incorrect type for parameter, expected " + typeof(T).Name, nameof(name));
            return (T)value;
        }
    }

    public record SharedSymbolRequest : SharedRequest
    {
        public string? BaseAsset { get; set; }
        public string? QuoteAsset { get; set; }
        public string? Symbol { get; set; }

        public SharedSymbolRequest(string baseAsset, string quoteAsset)
        {
            BaseAsset = baseAsset;
            QuoteAsset = quoteAsset;
        }

        public SharedSymbolRequest(string symbol)
        {
            Symbol = symbol;
        }

        public string GetSymbol(Func<string, string, ApiType?, string> format)
        {
            if (!string.IsNullOrEmpty(Symbol))
                return Symbol;

            return format(BaseAsset, QuoteAsset, ApiType);
        }
    }
}
