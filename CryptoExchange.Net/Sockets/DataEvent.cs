using System;

namespace CryptoExchange.Net.Sockets
{
    /// <summary>
    /// An update received from a socket update subscription
    /// </summary>
    /// <typeparam name="T">The type of the data</typeparam>
    public class DataEvent<T>
    {
        /// <summary>
        /// The timestamp the data was received
        /// </summary>
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// The topic of the update, what symbol/asset etc..
        /// </summary>
        public string? Topic { get; set; }
        /// <summary>
        /// The original data that was received, only available when OutputOriginalData is set to true in the client options
        /// </summary>
        public string? OriginalData { get; set; }
        /// <summary>
        /// The received data deserialized into an object
        /// </summary>
        public T Data { get; set; }

        internal DataEvent(T data, DateTime timestamp)
        {
            Data = data;
            Timestamp = timestamp;
        }

        internal DataEvent(T data, string? topic, DateTime timestamp)
        {
            Data = data;
            Topic = topic;
            Timestamp = timestamp;
        }

        internal DataEvent(T data, string? topic, string? originalData, DateTime timestamp)
        {
            Data = data;
            Topic = topic;
            OriginalData = originalData;
            Timestamp = timestamp;
        }

        /// <summary>
        /// Create a new DataEvent with data in the from of type K based on the current DataEvent. Topic, OriginalData and Timestamp will be copied over
        /// </summary>
        /// <typeparam name="K">The type of the new data</typeparam>
        /// <param name="data">The new data</param>
        /// <returns></returns>
        public DataEvent<K> As<K>(K data)
        {
            return new DataEvent<K>(data, Topic, OriginalData, Timestamp);
        }

        /// <summary>
        /// Create a new DataEvent with data in the from of type K based on the current DataEvent. OriginalData and Timestamp will be copied over
        /// </summary>
        /// <typeparam name="K">The type of the new data</typeparam>
        /// <param name="data">The new data</param>
        /// <param name="topic">The new topic</param>
        /// <returns></returns>
        public DataEvent<K> As<K>(K data, string? topic)
        {
            return new DataEvent<K>(data, topic, OriginalData, Timestamp);
        }
    }
}
