using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.ExchangeInterfaces
{
    /// <summary>
    /// Common order id
    /// </summary>
    public interface ICommonOrderId
    {
        /// <summary>
        /// Id of the order
        /// </summary>
        public string CommonId { get; }
    }
}
