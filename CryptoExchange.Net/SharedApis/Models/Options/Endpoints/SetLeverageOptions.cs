namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for setting leverage
    /// </summary>
    public class SetLeverageOptions : EndpointOptions<SetLeverageRequest>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public SetLeverageOptions() : base(true)
        {
        }
    }
}
