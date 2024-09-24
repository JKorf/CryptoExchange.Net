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
        public ExchangeEvent(string exchange, DataEvent<T> evnt) :
            base(evnt.Data,
                evnt.StreamId,
                evnt.Symbol,
                evnt.OriginalData,
                evnt.Timestamp,
                evnt.UpdateType)
        {
            Exchange = exchange;
        }

        /// <inheritdoc />
        public override string ToString() => $"{Exchange} - " + base.ToString();
    }
}
