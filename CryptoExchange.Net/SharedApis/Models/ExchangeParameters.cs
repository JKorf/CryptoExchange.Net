using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models
{
    public class ExchangeParameters
    {
        private readonly Dictionary<string, object> _store = new Dictionary<string, object>();

        public void AddValue(string exchange, string parameter, object value)
        {
            _store[exchange + "." + parameter] = value;
        }

        public bool HasValue(string exchange, string name, Type type)
        {
            if (!_store.TryGetValue(exchange + "." + name, out var val))
                return false;

            return val.GetType() == type;
        }

        public T? GetValue<T>(string exchange, string name)
        {
            if (_store == null)
                return default;

            if (_store.TryGetValue(exchange + "." + name, out var exchangeValue))
            {
                if (exchangeValue is not T)
                    throw new ArgumentException("Incorrect type for parameter, expected " + typeof(T).Name, nameof(name));
                return (T)exchangeValue;
            }

            return default;
        }
    }
}
