using CryptoExchange.Net.Objects;
using System;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for editing an open Futures order
    /// </summary>
    public class EditFuturesOrderOptions : CapabilityOptions<EditFuturesOrderRequest, IEditFuturesOrder>
    {
        /// <inheritdoc />
        public override string Description => "Edit an existing Futures order";


        /// <summary>
        /// ctor
        /// </summary>
        public EditFuturesOrderOptions(string exchange) : base(exchange, true, nameof(IEditFuturesOrder.EditFuturesOrderAsync))
        {
        }
    }
}
