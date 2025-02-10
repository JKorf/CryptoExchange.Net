using CryptoExchange.Net.SharedApis;
using System;

namespace CryptoExchange.Net.Objects.Sockets
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
        /// The received data deserialized into an object
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public DataEvent(T data, string? streamId, string? symbol, string? originalData, DateTime receiveTimestamp, SocketUpdateType? updateType)
        {
            Data = data;
            StreamId = streamId;
            Symbol = symbol;
            OriginalData = originalData;
            ReceiveTime = receiveTimestamp;
            UpdateType = updateType;
        }

        /// <summary>
        /// Create a new DataEvent with data in the from of type K based on the current DataEvent. Topic, OriginalData and ReceivedTimestamp will be copied over
        /// </summary>
        /// <typeparam name="K">The type of the new data</typeparam>
        /// <param name="data">The new data</param>
        /// <returns></returns>
        public DataEvent<K> As<K>(K data)
        {
            return new DataEvent<K>(data, StreamId, Symbol, OriginalData, ReceiveTime, UpdateType)
            {
                DataTime = DataTime
            };
        }

        /// <summary>
        /// Create a new DataEvent with data in the from of type K based on the current DataEvent. OriginalData and ReceivedTimestamp will be copied over
        /// </summary>
        /// <typeparam name="K">The type of the new data</typeparam>
        /// <param name="data">The new data</param>
        /// <param name="symbol">The new symbol</param>
        /// <returns></returns>
        public DataEvent<K> As<K>(K data, string? symbol)
        {
            return new DataEvent<K>(data, StreamId, symbol, OriginalData, ReceiveTime, UpdateType)
            {
                DataTime = DataTime
            };
        }

        /// <summary>
        /// Create a new DataEvent with data in the from of type K based on the current DataEvent. OriginalData and ReceivedTimestamp will be copied over
        /// </summary>
        /// <typeparam name="K">The type of the new data</typeparam>
        /// <param name="data">The new data</param>
        /// <param name="streamId">The new stream id</param>
        /// <param name="symbol">The new symbol</param>
        /// <param name="updateType">The type of update</param>
        /// <returns></returns>
        public DataEvent<K> As<K>(K data, string streamId, string? symbol, SocketUpdateType updateType)
        {
            return new DataEvent<K>(data, streamId, symbol, OriginalData, ReceiveTime, updateType)
            {
                DataTime = DataTime
            };
        }

        /// <summary>
        /// Copy the WebCallResult to a new data type
        /// </summary>
        /// <typeparam name="K">The new type</typeparam>
        /// <param name="exchange">The exchange the result is for</param>
        /// <param name="data">The data</param>
        /// <returns></returns>
        public ExchangeEvent<K> AsExchangeEvent<K>(string exchange, K data)
        {
            return new ExchangeEvent<K>(exchange, this.As<K>(data))
            {
                DataTime = DataTime
            };
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
        /// Create a CallResult from this DataEvent
        /// </summary>
        /// <returns></returns>
        public CallResult<T> ToCallResult()
        {
            return new CallResult<T>(Data, OriginalData, null);
        }

        /// <summary>
        /// Create a CallResult from this DataEvent
        /// </summary>
        /// <returns></returns>
        public CallResult<K> ToCallResult<K>(K data)
        {
            return new CallResult<K>(data, OriginalData, null);
        }

        /// <summary>
        /// Create a CallResult from this DataEvent
        /// </summary>
        /// <returns></returns>
        public CallResult<K> ToCallResult<K>(Error error)
        {
            return new CallResult<K>(default, OriginalData, error);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{StreamId} - {(Symbol == null ? "" : (Symbol + " - "))}{(UpdateType == null ? "" : (UpdateType + " - "))}{Data}";
        }
    }
}
