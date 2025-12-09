using System;

namespace CryptoExchange.Net.Exceptions
{
    /// <summary>
    /// Exception during deserialization
    /// </summary>
    public class CeDeserializationException : Exception
    {
        /// <summary>
        /// ctor
        /// </summary>
        public CeDeserializationException(string message) : base(message)
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        public CeDeserializationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
