namespace CryptoExchange.Net.Sockets.MessageParsing.Interfaces
{
    /// <summary>
    /// Serializer interface
    /// </summary>
    public interface IMessageSerializer
    {
        /// <summary>
        /// Serialize an object to a string
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        string Serialize(object message);
    }
}
