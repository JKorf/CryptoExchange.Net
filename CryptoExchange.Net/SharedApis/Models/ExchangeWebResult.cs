using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// A WebCallResult from an exchange
    /// </summary>
    /// <typeparam name="T">The result type</typeparam>
    public class ExchangeWebResult<T> : WebCallResult<T>
    {
        /// <summary>
        /// The exchange
        /// </summary>
        public string Exchange { get; }

        /// <summary>
        /// The trade modes for which the result data is
        /// </summary>
        public TradingMode[]? DataTradeMode { get; }

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
            TradingMode dataTradeMode,
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
            DataTradeMode = new[] { dataTradeMode };
            Exchange = exchange;
            NextPageToken = nextPageToken;
        }

        /// <summary>
        /// ctor
        /// </summary>
        public ExchangeWebResult(
            string exchange,
            TradingMode[]? dataTradeModes,
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
            DataTradeMode = dataTradeModes;
            Exchange = exchange;
            NextPageToken = nextPageToken;
        }

        /// <summary>
        /// Create a new result
        /// </summary>
        public ExchangeWebResult(
            string exchange,
            TradingMode[]? dataTradeModes,
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
            DataTradeMode = dataTradeModes;
            Exchange = exchange;
            NextPageToken = nextPageToken;
        }


        /// <summary>
        /// Copy the ExchangeWebResult to a new data type
        /// </summary>
        /// <typeparam name="K">The new type</typeparam>
        /// <param name="data">The data of the new type</param>
        /// <returns></returns>
        public new ExchangeWebResult<K> As<K>([AllowNull] K data)
        {
            return new ExchangeWebResult<K>(Exchange, DataTradeMode, ResponseStatusCode, ResponseHeaders, ResponseTime, ResponseLength, OriginalData, RequestId, RequestUrl, RequestBody, RequestMethod, RequestHeaders, DataSource, data, Error, NextPageToken);
        }

        /// <inheritdoc />
        public override string ToString() => $"{Exchange} - " + base.ToString();
    }
}
