namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for setting position mode
    /// </summary>
    public class SetPositionModeOptions : EndpointOptions<SetPositionModeRequest, IPositionModeRestClient>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public SetPositionModeOptions(string exchange) : base(exchange, true)
        {
        }
    }
}
