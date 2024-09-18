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
        private static List<ExchangeParameter> _staticParameters = new List<ExchangeParameter>();

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
                val = _staticParameters.SingleOrDefault(x => x.Exchange == exchange && x.Name == name);

            if (val == null)
                return false;

            try
            {
                Convert.ChangeType(val.Value, type);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool HasValue(ExchangeParameters? exchangeParameters, string exchange, string name, Type type)
        {
            if (exchangeParameters?.HasValue(exchange, name, type) == false) return false;

            var val = _staticParameters.SingleOrDefault(x => x.Exchange == exchange && x.Name == name);
            if (val == null)
                return false;

            try
            {
                Convert.ChangeType(val.Value, type);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public T? GetValue<T>(string exchange, string name)
        {
            var val = _parameters.SingleOrDefault(x => x.Exchange == exchange && x.Name == name);            
            if (val == null)
                return default;

            try
            {
                return (T)Convert.ChangeType(val.Value, typeof(T));
            }
            catch
            {
                throw new ArgumentException("Incorrect type for parameter, expected " + typeof(T).Name, name);
            }
        }

        public static T? GetValue<T>(ExchangeParameters? exchangeParameters, string exchange, string name)
        {
            T? value;
            if (exchangeParameters == null) 
            {
                var parameter = _staticParameters.SingleOrDefault(x => x.Exchange == exchange && x.Name == name);
                if (parameter == null)
                    return default;

                try
                {
                    return (T)Convert.ChangeType(parameter.Value, typeof(T));
                }
                catch
                {
                    throw new ArgumentException("Incorrect type for parameter, expected " + typeof(T).Name, name);
                }
            }
            else
                value = exchangeParameters.GetValue<T>(exchange, name);

            return value;
        }

        public static void SetStaticParameter(string exchange, string key, object value)
        {
            var existing = _staticParameters.SingleOrDefault(x => x.Exchange == exchange && x.Name == key);
            if (existing != null)
            {
                existing.Value = value;
                return;
            }    

            _staticParameters.Add(new ExchangeParameter(exchange, key, value));
        }

        public static void ResetStaticParameters()
        {
            _staticParameters.Clear();
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
