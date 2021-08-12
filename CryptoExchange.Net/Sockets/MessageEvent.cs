using Newtonsoft.Json.Linq;
using System;

namespace CryptoExchange.Net.Sockets
{
    /// <summary>
    /// Message received event
    /// </summary>
    public class MessageEvent
    {
        /// <summary>
        /// The connection the message was received on
        /// </summary>
        public SocketConnection Connection { get; set; }
        /// <summary>
        /// The json object of the data
        /// </summary>
        public JToken JsonData { get; set; }
        /// <summary>
        /// The originally received string data
        /// </summary>
        public string? OriginalData { get; set; }
        /// <summary>
        /// The timestamp of when the data was received
        /// </summary>
        public DateTime ReceivedTimestamp { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="jsonData"></param>
        /// <param name="originalData"></param>
        /// <param name="timestamp"></param>
        public MessageEvent(SocketConnection connection, JToken jsonData, string? originalData, DateTime timestamp)
        {
            Connection = connection;
            JsonData = jsonData;
            OriginalData = originalData;
            ReceivedTimestamp = timestamp;
        }
    }
}
