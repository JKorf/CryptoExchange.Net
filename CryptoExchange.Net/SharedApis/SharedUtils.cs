using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Requests;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Shared interfaces utilities
    /// </summary>
    public static class SharedUtils
    {
        /// <summary>
        /// Execute a new Shared interface call, validating the request and converting the result to an ExchangeWebResult
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <typeparam name="TRequest">Request type</typeparam>
        /// <typeparam name="TResult">Result type</typeparam>
        /// <typeparam name="TOptions">Options type</typeparam>
        /// <param name="client">Calling client</param>
        /// <param name="options">Options object</param>
        /// <param name="request">The request</param>
        /// <param name="action">The call execution</param>
        public static async Task<ExchangeWebResult<TResult>> ExecuteSharedAsync<TRequest, TResult, TOptions, TClient> (
            TClient client,
            TOptions options,
            TRequest request,
            Func<Task<SharedExecutionResult<TResult>>> action)
            where TOptions : EndpointOptions<TRequest, TClient>
            where TRequest : SharedRequest
            where TClient : ISharedClient
        {
            var validationError = options.ValidateRequest(request, client);
            if (validationError != null)
                return new ExchangeWebResult<TResult>(options.Exchange, validationError);

            var result = await action().ConfigureAwait(false);
            if (result.CallResult == null)
                return new ExchangeWebResult<TResult>(options.Exchange, result.PreCallError!);

            return new ExchangeWebResult<TResult>(options.Exchange, request.TradingMode == null ? null : [request.TradingMode.Value], result.CallResult, result.Data!);
        }
    }
}
