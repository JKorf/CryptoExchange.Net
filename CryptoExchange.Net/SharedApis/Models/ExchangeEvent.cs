using CryptoExchange.Net.Objects.Sockets;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models
{
    public class ExchangeEvent<T> : DataEvent<T>
    {
        /// <summary>
        /// The exchange
        /// </summary>
        public string Exchange { get; }

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
