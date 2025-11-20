using CryptoExchange.Net.Objects.Sockets;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// An update event for a specific exchange
    /// </summary>
    /// <typeparam name="T">Type of the data</typeparam>
    public class ExchangeEvent<T> : DataEvent<T>
    {
        /// <summary>
        /// The exchange
        /// </summary>
        public string Exchange { get; }

        /// <summary>
        /// ctor
        /// </summary>
        public ExchangeEvent(string exchange, DataEvent baseEvent, T data) :
            base(data,
                 baseEvent.ReceiveTime,
                 baseEvent.OriginalData)
        {
            StreamId = baseEvent.StreamId;
            Symbol = baseEvent.Symbol;
            UpdateType = baseEvent.UpdateType;
            DataTime = baseEvent.DataTime;
            Exchange = exchange;
        }

        /// <inheritdoc />
        public override string ToString() => $"{Exchange} - " + base.ToString();
    }
}
