namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for setting position mode
    /// </summary>
    public class SetPositionModeOptions : EndpointOptions<SetPositionModeRequest, ISetPositionModeEndpoint>
    {
        /// <inheritdoc />
        public override string Description => "Set the futures position mode";

        /// <summary>
        /// ctor
        /// </summary>
        public SetPositionModeOptions(string exchange) : base(exchange, true, nameof(ISetPositionModeEndpoint.SetPositionModeOptions))
        {
        }
    }
}
