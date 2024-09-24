namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting current position mode
    /// </summary>
    public class GetPositionModeOptions : EndpointOptions<GetPositionModeRequest>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public GetPositionModeOptions() : base(true)
        {
        }
    }
}
