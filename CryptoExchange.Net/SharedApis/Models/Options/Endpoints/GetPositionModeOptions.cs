namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting current position mode
    /// </summary>
    public class GetPositionModeOptions : EndpointOptions<GetPositionModeRequest, IPositionModeRestClient>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public GetPositionModeOptions(string exchange) : base(exchange, true)
        {
        }
    }
}
