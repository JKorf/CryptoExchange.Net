namespace CryptoExchange.Net.Interfaces
{
    /// <summary>
    /// Serializer interface
    /// </summary>
    public interface IMessageSerializer
    {
    }

    public interface IByteMessageSerializer: IMessageSerializer
    {
        /// <summary>
        /// Serialize an object to a string
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        byte[] Serialize<T>(T message);
    }


    public interface IStringMessageSerializer: IMessageSerializer
    {
        /// <summary>
        /// Serialize an object to a string
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        string Serialize<T>(T message);
    }
}
