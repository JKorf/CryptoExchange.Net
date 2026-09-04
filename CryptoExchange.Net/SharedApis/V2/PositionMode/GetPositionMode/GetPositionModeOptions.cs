namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting current position mode
    /// </summary>
    public class GetPositionModeOptions : CapabilityOptions<GetPositionModeRequest, IGetPositionModeRest>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve the current futures position mode";

        /// <summary>
        /// ctor
        /// </summary>
        public GetPositionModeOptions(string exchange) : base(exchange, true, nameof(IGetPositionModeRest.GetPositionModeAsync))
        {
        }
    }
}
