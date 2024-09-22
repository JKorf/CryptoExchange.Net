namespace CryptoExchange.Net.SharedApis.Models
{
    /// <summary>
    /// Request
    /// </summary>
    public record SharedRequest
    {
        /// <summary>
        /// Exchange parameters. Some calls may require exchange specific parameters to execute the request.
        /// </summary>
        public ExchangeParameters? ExchangeParameters { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedRequest(ExchangeParameters? exchangeParameters = null)
        {
            ExchangeParameters = exchangeParameters;
        }
    }
}
