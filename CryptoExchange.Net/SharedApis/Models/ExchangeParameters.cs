using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models
{
    public class ExchangeParameters
    {
        private readonly List<ExchangeParameter> _parameters;

        public ExchangeParameters(params ExchangeParameter[] parameters)
        {
            _parameters = parameters.ToList();
        }

        public void AddValue(ExchangeParameter exchangeParameter)
        {
            _parameters.Add(exchangeParameter);
        }

        public bool HasValue(string exchange, string name, Type type)
        {
            var val = _parameters.SingleOrDefault(x => x.Exchange == exchange && x.Name == name);
            if (val == null)
                return false;

            return val.Value.GetType() == type;
        }

        public T? GetValue<T>(string exchange, string name)
        {
            var val = _parameters.SingleOrDefault(x => x.Exchange == exchange && x.Name == name);
            if (val == null)
                return default;

            if (val.Value is not T)
                throw new ArgumentException("Incorrect type for parameter, expected " + typeof(T).Name, name);
            return (T)val.Value;
        }
    }

    public class ExchangeParameter
    {
        public string Exchange { get; set; }
        public string Name { get; set; }
        public object Value { get; set; }

        public ExchangeParameter(string exchange, string name, object value)
        {
            Exchange = exchange;
            Name = name;
            Value = value;
        }
    }
}
