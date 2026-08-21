namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for setting leverage
    /// </summary>
    public class SetLeverageOptions : EndpointOptions<SetLeverageRequest, ILeverageRestClient>
    {
        /// <inheritdoc />
        public override string Description => "Set the leverage for a futures symbol";

        /// <summary>
        /// ctor
        /// </summary>
        public SetLeverageOptions(string exchange) : base(exchange, true, nameof(ILeverageRestClient.SetLeverageAsync))
        {
        }
    }
}
