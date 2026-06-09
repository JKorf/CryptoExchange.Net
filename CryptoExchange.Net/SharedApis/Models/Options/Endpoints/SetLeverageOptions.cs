namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for setting leverage
    /// </summary>
    public class SetLeverageOptions : EndpointOptions<SetLeverageRequest, ILeverageRestClient>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public SetLeverageOptions(string exchange) : base(exchange, true, nameof(ILeverageRestClient.SetLeverageAsync))
        {
        }
    }
}
