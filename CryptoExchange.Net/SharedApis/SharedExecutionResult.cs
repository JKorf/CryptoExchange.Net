using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    public readonly struct SharedExecutionResult<TResult>
    {
        public Error PreCallError { get; }
        public IWebCallResult CallResult { get; }
        public TResult? Data { get; }
        public PageRequest? NextPageRequest { get; }
        public bool Success => PreCallError == null && CallResult != null && CallResult.Success;

        private SharedExecutionResult(Error error)
        {
            PreCallError = error;
        }

        private SharedExecutionResult(IWebCallResult callResult, TResult? data, PageRequest? nextPageRequest)
        {
            CallResult = callResult;
            Data = data;
            NextPageRequest = nextPageRequest;
        }

        public static SharedExecutionResult<TResult> Ok(IWebCallResult result, TResult data) => new SharedExecutionResult<TResult>(result, data, null);
        public static SharedExecutionResult<TResult> Ok(IWebCallResult result, TResult data, PageRequest? nextPageRequest) => new SharedExecutionResult<TResult>(result, data, nextPageRequest);
        public static SharedExecutionResult<TResult> Error(IWebCallResult result) => new SharedExecutionResult<TResult>(result, default, default);
        public static SharedExecutionResult<TResult> Error(Error error) => new SharedExecutionResult<TResult>(error);
    }
}
