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
}
