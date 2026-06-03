using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Result of a Shared interface call
    /// </summary>
    public abstract class SharedExecutionResult
    {
        /// <summary>
        /// Error before the call was made, for example due to invalid parameters. If this is set, CallResult will be null
        /// </summary>
        public Error? PreCallError { get; init; }
        /// <summary>
        /// The result of the web request made to the exchange
        /// </summary>
        public IWebCallResult? CallResult { get; init; }
        /// <summary>
        /// Next page request
        /// </summary>
        public PageRequest? NextPageRequest { get; init; }
        /// <summary>
        /// Whether the call was successful
        /// </summary>
        public bool Success => PreCallError == null && CallResult != null && CallResult.Success;

        /// <summary>
        /// Create a success result
        /// </summary>
        /// <param name="callResult">Call result</param>
        /// <param name="data">Result data</param>
        public static SharedExecutionResult<TResult> Ok<TResult>(IWebCallResult callResult, TResult data) => new SharedExecutionResult<TResult>(callResult, data, null);
        /// <summary>
        /// Create a success result
        /// </summary>
        /// <param name="callResult">Call result</param>
        /// <param name="data">Result data</param>
        /// <param name="nextPageRequest">Pagination token</param>
        public static SharedExecutionResult<TResult> Ok<TResult>(IWebCallResult callResult, TResult data, PageRequest? nextPageRequest) => new SharedExecutionResult<TResult>(callResult, data, nextPageRequest);
        /// <summary>
        /// Create an error result
        /// </summary>
        /// <param name="callResult">The failed result</param>
        public static SharedExecutionResult<TResult> Error<TResult>(IWebCallResult callResult) => new SharedExecutionResult<TResult>(callResult, default, default);
        /// <summary>
        /// Create an error result
        /// </summary>
        /// <param name="error">The pre-call result</param>
        public static SharedExecutionResult<TResult> Error<TResult>(Error error) => new SharedExecutionResult<TResult>(error);
    }

    /// <summary>
    /// Result of a Shared interface call
    /// </summary>
    public class SharedExecutionResult<TResult> : SharedExecutionResult
    {        
        /// <summary>
        /// The result data
        /// </summary>
        public TResult? Data { get; }

        internal SharedExecutionResult(Error error)
        {
            PreCallError = error;
        }

        internal SharedExecutionResult(IWebCallResult callResult, TResult? data, PageRequest? nextPageRequest)
        {
            CallResult = callResult;
            Data = data;
            NextPageRequest = nextPageRequest;
        }
    }
}
