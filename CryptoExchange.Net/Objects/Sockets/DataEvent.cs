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
        /// The timestamp of the data in local time. Note that this is an estimation based on average delay from the server.
        /// </summary>
        public DateTime? DataTimeLocal { get; set; }

        /// <summary>
        /// The age of the data. Note that this is an estimation based on average delay from the server.
        /// </summary>
        public TimeSpan? DataAge => DateTime.UtcNow - DataTimeLocal;

        /// <summary>
        /// The stream producing the update
        /// </summary>
        public string? StreamId { get; set; }

        /// <summary>
        /// The symbol the update is for
        /// </summary>
        public string? Symbol { get; set; }

        /// <summary>
        /// The exchange name
        /// </summary>
        public string Exchange { get; set; }

        /// <summary>
        /// The original data that was received, only available when OutputOriginalData is set to true in the client options
        /// </summary>
        public string? OriginalData { get; set; }

        /// <summary>
        /// Type of update
        /// </summary>
        public SocketUpdateType? UpdateType { get; set; }

        /// <summary>
        /// Sequence number of the update
        /// </summary>
        public long? SequenceNumber { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public DataEvent(
            string exchange,
            DateTime receiveTimestamp,
            string? originalData)
        {
            Exchange = exchange;
            OriginalData = originalData;
            ReceiveTime = receiveTimestamp;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{StreamId} - {(Symbol == null ? "" : (Symbol + " - "))}{UpdateType}";
        }
    }

    /// <inheritdoc />
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
            string exchange,
            T data,
            DateTime receiveTimestamp,
            string? originalData): base(exchange, receiveTimestamp, originalData)
        {
            Data = data;
        }

        /// <summary>
        /// Specify the symbol
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public DataEvent<T> WithSymbol(string? symbol)
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
        /// Specify the sequence number of the update
        /// </summary>
        public DataEvent<T> WithSequenceNumber(long? sequenceNumber)
        {
            SequenceNumber = sequenceNumber;
            return this;
        }

        /// <summary>
        /// Specify the data timestamp
        /// </summary>
        public DataEvent<T> WithDataTimestamp(DateTime? timestamp, TimeSpan? offset)
        {
            if (timestamp == null || timestamp == default(DateTime))
                return this;

            DataTime = timestamp;
            if (offset == null)
                return this;

            DataTimeLocal = DataTime + offset;
            return this;
        }

        /// <summary>
        /// Create a new DataEvent of the new type
        /// </summary>
        public DataEvent<TNew> ToType<TNew>(TNew data)
        {
            return new DataEvent<TNew>(Exchange, data, ReceiveTime, OriginalData)
            {
                StreamId = StreamId,
                UpdateType = UpdateType,
                Symbol = Symbol                
            };
        }

        /// <inheritdoc />
        public override string ToString() => base.ToString().TrimEnd(' ', '-') + " - " + Data?.ToString();
    }
}
