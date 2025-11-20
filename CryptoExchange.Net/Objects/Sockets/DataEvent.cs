using CryptoExchange.Net.SharedApis;
using System;

namespace CryptoExchange.Net.Objects.Sockets
{
    /// <summary>
    /// An update received from a socket update subscription
    /// </summary>
    public class DataEvent
    {
        /// <summary>
        /// The timestamp the data was received
        /// </summary>
        public DateTime ReceiveTime { get; set; }

        /// <summary>
        /// The timestamp of the data as specified by the server. Note that the server time and client time might not be 100% in sync so this value might not be fully comparable to local time.
        /// </summary>
        public DateTime? DataTime { get; set; }

        /// <summary>
        /// The stream producing the update
        /// </summary>
        public string? StreamId { get; set; }

        /// <summary>
        /// The symbol the update is for
        /// </summary>
        public string? Symbol { get; set; }

        /// <summary>
        /// The original data that was received, only available when OutputOriginalData is set to true in the client options
        /// </summary>
        public string? OriginalData { get; set; }

        /// <summary>
        /// Type of update
        /// </summary>
        public SocketUpdateType? UpdateType { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public DataEvent(
            DateTime receiveTimestamp,
            string? originalData)
        {
            OriginalData = originalData;
            ReceiveTime = receiveTimestamp;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{StreamId} - {(Symbol == null ? "" : (Symbol + " - "))}{(UpdateType == null ? "" : (UpdateType + " - "))}";
        }
    }

    public class DataEvent<T> : DataEvent
    {

        /// <summary>
        /// The received data deserialized into an object
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public DataEvent(
            T data,
            DateTime receiveTimestamp,
            string? originalData): base(receiveTimestamp, originalData)
        {
            Data = data;
        }

        /// <summary>
        /// Specify the symbol
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public DataEvent<T> WithSymbol(string symbol)
        {
            Symbol = symbol;
            return this;
        }

        /// <summary>
        /// Specify the update type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public DataEvent<T> WithUpdateType(SocketUpdateType type)
        {
            UpdateType = type;
            return this;
        }

        /// <summary>
        /// Specify the stream id
        /// </summary>
        /// <param name="streamId"></param>
        /// <returns></returns>
        public DataEvent<T> WithStreamId(string streamId)
        {
            StreamId = streamId;
            return this;
        }

        /// <summary>
        /// Specify the data timestamp
        /// </summary>
        public DataEvent<T> WithDataTimestamp(DateTime? timestamp)
        {
            DataTime = timestamp;
            return this;
        }

        /// <summary>
        /// Copy the DataEvent to a new data type
        /// </summary>
        /// <param name="exchange">The exchange the result is for</param>
        /// <returns></returns>
        public ExchangeEvent<K> AsExchangeEvent<K>(string exchange, K data)
        {
            return new ExchangeEvent<K>(exchange, this, data)
            {
                DataTime = DataTime
            };
        }
    }
}
