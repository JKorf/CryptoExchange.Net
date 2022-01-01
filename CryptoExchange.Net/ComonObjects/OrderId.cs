using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.ComonObjects
{
    /// <summary>
    /// Id of an order
    /// </summary>
    public class OrderId: BaseComonObject
    {
        /// <summary>
        /// Id of an order
        /// </summary>
        public string Id { get; set; } = string.Empty;
    }
}
