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
        ///// <summary>
        ///// Execute a new Shared interface call, validating the request and converting the result to an ExchangeWebResult
        ///// </summary>
        ///// <typeparam name="TClient"></typeparam>
        ///// <typeparam name="TRequest">Request type</typeparam>
        ///// <typeparam name="TResult">Result type</typeparam>
        ///// <param name="client">Calling client</param>
        ///// <param name="optionsCallback">Options callback</param>
        ///// <param name="request">The request</param>
        ///// <param name="action">The call execution</param>
        //public static async Task<HttpResult<TResult>> ExecuteSharedAsync<TClient, TRequest, TResult>(
        //    TClient client,
        //    Func<TClient, EndpointOptions<TRequest, TClient>> optionsCallback,
        //    TRequest request,
        //    Func<Task<HttpResult<TResult>>> action)
        //    where TRequest : SharedRequest
        //    where TClient : ISharedClient
        //{
        //    var options = optionsCallback(client);
        //    var validationError = options.ValidateRequest(request, client);
        //    if (validationError != null)
        //        return HttpResult.Fail<TResult>(options.Exchange, validationError);

        //    return await action().ConfigureAwait(false);
        //}
    }
}
