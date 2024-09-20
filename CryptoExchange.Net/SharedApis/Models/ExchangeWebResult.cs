using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net;
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

        /// <summary>
        /// The trade modes for which the result data is
        /// </summary>
        public TradingMode[]? ResultModes { get; }

        /// <summary>
        /// Token to retrieve the next page with
        /// </summary>
        public INextPageToken? NextPageToken { get; }

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
            TradingMode resultModes,
            WebCallResult<T> result,
            INextPageToken? nextPageToken = null) :
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
            ResultModes = new[] { resultModes };
            Exchange = exchange;
            NextPageToken = nextPageToken;
            if (NextPageToken != null)
                NextPageToken.Exchange = exchange;
        }

        /// <summary>
        /// ctor
        /// </summary>
        public ExchangeWebResult(
            string exchange,
            TradingMode[]? resultModes,
            WebCallResult<T> result,
            INextPageToken? nextPageToken = null) :
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
            ResultModes = resultModes;
            Exchange = exchange;
            NextPageToken = nextPageToken;
            if (NextPageToken != null)
                NextPageToken.Exchange = exchange;
        }

        /// <summary>
        /// Create a new result
        /// </summary>
        public ExchangeWebResult(
            string exchange,
            TradingMode[]? resultMode,
            HttpStatusCode? code,
            IEnumerable<KeyValuePair<string, IEnumerable<string>>>? responseHeaders,
            TimeSpan? responseTime,
            long? responseLength,
            string? originalData,
            int? requestId,
            string? requestUrl,
            string? requestBody,
            HttpMethod? requestMethod,
            IEnumerable<KeyValuePair<string, IEnumerable<string>>>? requestHeaders,
            ResultDataSource dataSource,
            [AllowNull] T data,
            Error? error,
            INextPageToken? nextPageToken = null) : base(
                code,
                responseHeaders,
                responseTime,
                responseLength,
                originalData,
                requestId,
                requestUrl,
                requestBody,
                requestMethod,
                requestHeaders,
                dataSource,
                data,
                error)
        {
            Exchange = exchange;
            NextPageToken = nextPageToken;
            if (NextPageToken != null)
                NextPageToken.Exchange = exchange;
        }


        /// <summary>
        /// Copy the WebCallResult to a new data type
        /// </summary>
        /// <typeparam name="K">The new type</typeparam>
        /// <param name="data">The data of the new type</param>
        /// <returns></returns>
        public new ExchangeWebResult<K> As<K>([AllowNull] K data)
        {
            return new ExchangeWebResult<K>(Exchange, ResultModes, ResponseStatusCode, ResponseHeaders, ResponseTime, ResponseLength, OriginalData, RequestId, RequestUrl, RequestBody, RequestMethod, RequestHeaders, DataSource, data, Error, NextPageToken);
        }

        /// <inheritdoc />
        public override string ToString() => $"{Exchange} - " + base.ToString();
    }
}
