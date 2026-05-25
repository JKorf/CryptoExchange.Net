using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Requests;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    public static class SharedUtils
    {
        public static async Task<ExchangeWebResult<TResult>> ExecuteSharedAsync<TRequest, TResult, TOptions> (
            TOptions options,
            TRequest request,
            TradingMode[] supportedTradingModes,
            Func<Task<(IWebCallResult, TResult?)>> action)
            where TOptions : EndpointOptions
            where TRequest : SharedRequest
        {
            var validationError = options.ValidateRequest(request.ExchangeParameters, request.TradingMode, supportedTradingModes);
            if (validationError != null)
                return new ExchangeWebResult<TResult>(options.Exchange, validationError);

            var (callResult, data) = await action().ConfigureAwait(false);
            if (!callResult.Success)
                return new ExchangeWebResult<TResult>(options.Exchange, default, callResult, data);

            return new ExchangeWebResult<TResult>(options.Exchange, request.TradingMode == null ? null : [request.TradingMode.Value], callResult, data!);
        }

        public static async Task<ExchangeWebResult<TResult>> ExecuteSharedAsync<TRequest, TResult, TOptions>(
            TOptions options,
            TRequest request,
            TradingMode[] supportedTradingModes,
            Func<Task<(IWebCallResult, TResult?, PageRequest? pageParams)>> action)
            where TOptions : EndpointOptions
            where TRequest : SharedRequest
        {
            var validationError = options.ValidateRequest(request.ExchangeParameters, request.TradingMode, supportedTradingModes);
            if (validationError != null)
                return new ExchangeWebResult<TResult>(options.Exchange, validationError);

            var (callResult, data, pageRequest) = await action().ConfigureAwait(false);
            if (!callResult.Success)
                return new ExchangeWebResult<TResult>(options.Exchange, default, callResult, data);

            return new ExchangeWebResult<TResult>(options.Exchange, request.TradingMode == null ? null : [request.TradingMode.Value], callResult, data!, pageRequest);
        }
    }
}
