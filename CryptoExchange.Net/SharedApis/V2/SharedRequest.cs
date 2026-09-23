using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request
    /// </summary>
    public record SharedRequest
    {
        /// <summary>
        /// Trading mode
        /// </summary>
        public TradingMode? TradingMode { get; set; }

        /// <summary>
        /// Exchange parameters. Some calls may require exchange specific parameters to execute the request.
        /// </summary>
        public ExchangeParameters? ExchangeParameters { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedRequest(TradingMode? tradingMode, ExchangeParameters? exchangeParameters = null)
        {
            TradingMode = tradingMode;
            ExchangeParameters = exchangeParameters;
        }

        /// <summary>
        /// Get the value of a parameter from this instance or the default values
        /// </summary>
        /// <typeparam name="T">Type of the parameter value</typeparam>
        /// <param name="exchange">Exchange name</param>
        /// <param name="names">Parameter name or names</param>
        public T? GetParamValue<T>(string exchange, params string[] names)
        {
            foreach (var name in names)
            {
                var value = ExchangeParameters.GetValue<T>(ExchangeParameters, exchange, name);
                if (value != null)
                    return value;
            }

            return default;
        }
    }
}
