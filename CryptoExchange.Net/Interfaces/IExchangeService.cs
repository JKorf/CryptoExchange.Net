namespace CryptoExchange.Net.Interfaces
{
    /// <summary>
    /// Service for an exchange
    /// </summary>
    public interface IExchangeService
    {
        /// <summary>
        /// The exchange the service is for
        /// </summary>
        public string ExchangeName { get; }
    }
}
