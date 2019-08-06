namespace CryptoExchange.Net.Interfaces
{
    /// <summary>
    /// Request factory interface
    /// </summary>
    public interface IRequestFactory
    {
        /// <summary>
        /// Create a request for an uri
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        IRequest Create(string uri);
    }
}
