namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for setting position mode
    /// </summary>
    public class SetPositionModeOptions : CapabilityOptions<SetPositionModeRequest, ISetPositionModeRest>
    {
        /// <inheritdoc />
        public override string Description => "Set the futures position mode";

        /// <summary>
        /// ctor
        /// </summary>
        public SetPositionModeOptions(string exchange) : base(exchange, true, nameof(ISetPositionModeRest.SetPositionModeOptions))
        {
        }
    }
}
