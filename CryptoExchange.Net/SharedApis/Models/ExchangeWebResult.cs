using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models
{
    /// <summary>
    /// A WebCallResult from an exchange
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ExchangeWebResult<T> : WebCallResult<T>
    {
        /// <summary>
        /// The exchange
        /// </summary>
        public string Exchange { get; }

        public string? NextPageToken { get; }

        /// <summary>
        /// ctor
        /// </summary>
        public ExchangeWebResult(
            string exchange,
            Error error) :
            base(error)
        {
            Exchange = exchange;
        }

        /// <summary>
        /// ctor
        /// </summary>
        public ExchangeWebResult(
            string exchange,
            WebCallResult<T> result,
            string? nextPageToken = null) :
            base(result.ResponseStatusCode,
                result.ResponseHeaders,
                result.ResponseTime,
                result.ResponseLength,
                result.OriginalData,
                result.RequestId,
                result.RequestUrl,
                result.RequestBody,
                result.RequestMethod,
                result.RequestHeaders,
                result.DataSource,
                result.Data,
                result.Error)
        {
            Exchange = exchange;
            NextPageToken = nextPageToken;
        }

        /// <inheritdoc />
        public override string ToString() => $"{Exchange} - " + base.ToString();
    }
}
