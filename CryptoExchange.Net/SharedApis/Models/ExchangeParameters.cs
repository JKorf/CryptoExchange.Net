using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Exchange parameters
    /// </summary>
    public class ExchangeParameters
    {
        private readonly List<ExchangeParameter> _parameters;
        private static List<ExchangeParameter> _staticParameters = new List<ExchangeParameter>();

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="parameters">The parameters to add</param>
        public ExchangeParameters(params ExchangeParameter[] parameters)
        {
            _parameters = parameters.ToList();
        }

        /// <summary>
        /// Add a new parameter value
        /// </summary>
        /// <param name="exchangeParameter"></param>
        public void AddValue(ExchangeParameter exchangeParameter)
        {
            _parameters.Add(exchangeParameter);
        }

        /// <summary>
        /// Check whether a specific parameter is provided in this specific instance
        /// </summary>
        /// <param name="exchange">The exchange name</param>
        /// <param name="name">Parameter name</param>
        /// <param name="type">Type of the parameter value</param>
        /// <returns></returns>
        public bool HasValue(string exchange, string name, Type type)
        {
            var val = _parameters.SingleOrDefault(x => x.Exchange == exchange && x.Name == name);
            val ??= _staticParameters.SingleOrDefault(x => x.Exchange == exchange && x.Name == name);

            if (val == null)
                return false;

            try
            {
                Type t = Nullable.GetUnderlyingType(type) ?? type;
                Convert.ChangeType(val.Value, t);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check whether a specific parameter is provided in the default parameters or the provided instance
        /// </summary>
        /// <param name="exchangeParameters">The provided exchange parameter in the request</param>
        /// <param name="exchange">The exchange name</param>
        /// <param name="name">Parameter name</param>
        /// <param name="type">Type of the parameter value</param>
        /// <returns></returns>
        public static bool HasValue(ExchangeParameters? exchangeParameters, string exchange, string name, Type type)
        {
            var provided = exchangeParameters?.HasValue(exchange, name, type);
            if (provided == true)
                return true;

            var val = _staticParameters.SingleOrDefault(x => x.Exchange == exchange && x.Name == name);
            if (val == null)
                return false;

            try
            {
                Type t = Nullable.GetUnderlyingType(type) ?? type;
                Convert.ChangeType(val.Value, t);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get the value of a parameter from this instance
        /// </summary>
        /// <typeparam name="T">Type of the parameter value</typeparam>
        /// <param name="exchange">Exchange name</param>
        /// <param name="name">Parameter name</param>
        public T? GetValue<T>(string exchange, string name)
        {
            var val = _parameters.SingleOrDefault(x => x.Exchange == exchange && x.Name == name);            
            if (val == null)
                return default;

            try
            {
                Type t = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
                return (T)Convert.ChangeType(val.Value, t);
            }
            catch
            {
                throw new ArgumentException("Incorrect type for parameter, expected " + typeof(T).Name, name);
            }
        }

        /// <summary>
        /// Get the value of a parameter from this instance or the default values
        /// </summary>
        /// <typeparam name="T">Type of the parameter value</typeparam>
        /// <param name="exchangeParameters">The request parameters</param>
        /// <param name="exchange">Exchange name</param>
        /// <param name="name">Parameter name</param>
        public static T? GetValue<T>(ExchangeParameters? exchangeParameters, string exchange, string name)
        {
            T? value;
            if (exchangeParameters == null)
            {
                var parameter = _staticParameters.SingleOrDefault(x => x.Exchange == exchange && x.Name == name);
                if (parameter == null)
                    return default;

                if (parameter.Value is T val)
                    return val;

                try
                {
                    Type t = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
                    return (T)Convert.ChangeType(parameter.Value, t);
                }
                catch
                {
                    throw new ArgumentException("Incorrect type for parameter, expected " + typeof(T).Name, name);
                }
            }
            else
            {
                value = exchangeParameters.GetValue<T>(exchange, name);
            }

            return value;
        }

        /// <summary>
        /// Set static parameters
        /// </summary>
        /// <param name="exchange">Exchange name</param>
        /// <param name="key">Parameter name</param>
        /// <param name="value">Parameter value</param>
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

        /// <summary>
        /// Reset the static parameters, clears all parameters for all exchanges
        /// </summary>
        public static void ResetStaticParameters()
        {
            _staticParameters.Clear();
        }
    }
}
