using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Exchange parameters
    /// </summary>
    public class ExchangeParameters
    {
        private readonly static Dictionary<string, Parameters> _staticProcessParameters = new Dictionary<string, Parameters>();
        private readonly Dictionary<string, Parameters> _processParameters;
        private readonly Dictionary<string, Parameters> _rawParameters;

        public ExchangeParameters(params ExchangeParameter[] parameters)
        {
            _processParameters = new Dictionary<string, Parameters>();
            _rawParameters = new Dictionary<string, Parameters>();

            foreach (var parameter in parameters)
                AddProcessValue(parameter.Exchange, parameter.Name, parameter.Value);
        }

        public Parameters? GetRawParameters(string exchange) => _rawParameters.TryGetValue(exchange, out var value) ? value : null;

        public void AddProcessParameter(ExchangeParameter exchangeParameter)
        {
            AddProcessValue(exchangeParameter.Exchange, exchangeParameter.Name, exchangeParameter.Value);
        }

        public void AddRawParameter(ExchangeParameter exchangeParameter)
        {
            AddRawValue(exchangeParameter.Exchange, exchangeParameter.Name, exchangeParameter.Value);
        }

        public void AddProcessValue(string exchange, string key, object value)
        {
            if (!_processParameters.TryGetValue(exchange, out var exchangeParameters))
            {
                exchangeParameters = new Parameters(ParameterSerializationSettings.Default);
                _processParameters[exchange] = exchangeParameters;
            }

            exchangeParameters.AddRaw(key, value);
        }

        public void AddRawValue(string exchange, string key, object value)
        {
            if (!_rawParameters.TryGetValue(exchange, out var exchangeParameters))
            {
                exchangeParameters = new Parameters(ParameterSerializationSettings.Default);
                _rawParameters[exchange] = exchangeParameters;
            }

            exchangeParameters.AddRaw(key, value);
        }

        private static object? TryGetValue(Dictionary<string, Parameters> list, string exchange, string key)
        {
            if (!list.TryGetValue(exchange, out var exchangeParams))
                return null;

            return exchangeParams.SingleOrDefault(x => x.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase)).Value;
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
            var val = TryGetValue(_processParameters, exchange, name);
            val ??= TryGetValue(_staticProcessParameters, exchange, name);

            if (val == null)
                return false;

            try
            {
                Type t = Nullable.GetUnderlyingType(type) ?? type;
                Convert.ChangeType(val, t);
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

            var val = TryGetValue(_staticProcessParameters, exchange, name);
            if (val == null)
                return false;

            try
            {
                Type t = Nullable.GetUnderlyingType(type) ?? type;
                Convert.ChangeType(val, t);
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
        public T? GetProcessValue<T>(string exchange, string name)
        {
            var val = TryGetValue(_processParameters, exchange, name);
            val ??= TryGetValue(_staticProcessParameters, exchange, name);
            if (val == null)
                return default;

            if (val is T typeVal)
                return typeVal;

            try
            {
                Type t = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
                return (T)Convert.ChangeType(val, t);
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
        public static T? GetProcessValue<T>(ExchangeParameters? exchangeParameters, string exchange, string name)
        {
            if (exchangeParameters != null) {

                var provided = exchangeParameters.GetProcessValue<T>(exchange, name);
                if (provided != null)
                    return provided;
            }

            var val = TryGetValue(_staticProcessParameters, exchange, name);
            if (val == null)
                return default;

            try
            {
                var type = typeof(T);
                Type t = Nullable.GetUnderlyingType(type) ?? type;
                var result = Convert.ChangeType(val, t);
                return (T)result;
            }
            catch
            {
                throw new ArgumentException("Incorrect type for parameter, expected " + typeof(T).Name, name);
            }
        }

        /// <summary>
        /// Set static parameters
        /// </summary>
        /// <param name="exchange">Exchange name</param>
        /// <param name="key">Parameter name</param>
        /// <param name="value">Parameter value</param>
        public static void SetStaticParameter(string exchange, string key, object value)
        {
            // TODO
        }

        /// <summary>
        /// Reset the static parameters, clears all parameters for all exchanges
        /// </summary>
        public static void ResetStaticParameters()
        {
            _staticProcessParameters.Clear();
        }

        /// <summary>
        /// Reset the static parameters, clears all parameters for an exchange exchanges
        /// </summary>
        public static void ResetStaticExchangeParameters(string exchange)
        {
            _staticProcessParameters.Remove(exchange);
        }
    }
}