namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for setting position mode
    /// </summary>
    public class SetPositionModeOptions : EndpointOptions<SetPositionModeRequest>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public SetPositionModeOptions() : base(true)
        {
        }
    }
}
