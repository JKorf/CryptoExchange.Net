using CryptoExchange.Net.Objects;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

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
        /// 
        /// </summary>
        public PageRequest? NextPageRequest { get; }

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
            PageRequest? nextPageToken = null) :
            base(result.ResponseStatusCode,
                result.HttpVersion,
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
            NextPageRequest = nextPageToken;
        }

        /// <summary>
        /// ctor
        /// </summary>
        public ExchangeWebResult(
            string exchange,
            TradingMode[]? dataTradeModes,
            WebCallResult<T> result,
            PageRequest? nextPageRequest = null) :
            base(result.ResponseStatusCode,
                result.HttpVersion,
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
            NextPageRequest = nextPageRequest;
        }

        /// <summary>
        /// Create a new result
        /// </summary>
        public ExchangeWebResult(
            string exchange,
            TradingMode[]? dataTradeModes,
            HttpStatusCode? code,
            Version? httpVersion,
            HttpResponseHeaders? responseHeaders,
            TimeSpan? responseTime,
            long? responseLength,
            string? originalData,
            int? requestId,
            string? requestUrl,
            string? requestBody,
            HttpMethod? requestMethod,
            HttpRequestHeaders? requestHeaders,
            ResultDataSource dataSource,
            [AllowNull] T data,
            Error? error,
            PageRequest? nextPageToken = null) : base(
                code,
                httpVersion,
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
            NextPageRequest = nextPageToken;
        }

        /// <summary>
        /// Copy the ExchangeWebResult to a new data type
        /// </summary>
        /// <typeparam name="K">The new type</typeparam>
        /// <param name="data">The data of the new type</param>
        /// <returns></returns>
        public new ExchangeWebResult<K> As<K>([AllowNull] K data)
        {
            return new ExchangeWebResult<K>(Exchange, DataTradeMode, ResponseStatusCode, HttpVersion, ResponseHeaders, ResponseTime, ResponseLength, OriginalData, RequestId, RequestUrl, RequestBody, RequestMethod, RequestHeaders, DataSource, data, Error, NextPageRequest);
        }

        /// <inheritdoc />
        public override string ToString() => $"{Exchange} - " + base.ToString();
    }
}
