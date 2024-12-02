using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net
{
    /// <summary>
    /// Helpers for client libraries
    /// </summary>
    public static class LibraryHelpers
    {
        /// <summary>
        /// Client order id seperator
        /// </summary>
        public const string ClientOrderIdSeperator = "|JK|";

        /// <summary>
        /// Apply broker id to a client order id
        /// </summary>
        /// <param name="clientOrderId"></param>
        /// <param name="brokerId"></param>
        /// <param name="maxLength"></param>
        /// <param name="allowValueAdjustement"></param>
        /// <returns></returns>
        public static string ApplyBrokerId(string? clientOrderId, string brokerId, int maxLength, bool allowValueAdjustement)
        {
            var reservedLength = brokerId.Length + ClientOrderIdSeperator.Length;

            if ((clientOrderId?.Length + reservedLength) > maxLength)
                return clientOrderId!;

            if (!string.IsNullOrEmpty(clientOrderId))
            {
                if (allowValueAdjustement)
                    clientOrderId = brokerId + ClientOrderIdSeperator + clientOrderId;

                return clientOrderId!;
            }
            else
            {
                clientOrderId = ExchangeHelpers.AppendRandomString(brokerId + ClientOrderIdSeperator, maxLength);
            }

            return clientOrderId;
        }
    }
}
