//using CryptoExchange.Net.Objects;

//namespace CryptoExchange.Net.SharedApis
//{
//    public record HttpResult
//    {
//        public static HttpResult<T> Fail<T>(string exchange, Error error) => new HttpResult<T> { Exchange = exchange, Error = error } ;
//        public static HttpResult<T> Fail<T>(string exchange, IHttpResult result, Error? error = null) => new HttpResult<T>
//        {
//            Exchange = exchange,
//            Error = error ?? result.Error,
//            CallResult = result
//        };
//        public static HttpResult<T> Ok<T>(string exchange, TradingMode[]? tradingMode, IHttpResult result, T data, PageRequest? pageRequest = null) => new HttpResult<T>
//        {
//            Exchange = exchange,
//            DataTradeMode = tradingMode,
//            CallResult = result,
//            NextPageRequest = pageRequest,
//            Data = data
//        };
//    }

//    /// <summary>
//    /// A CallResult from an exchange
//    /// </summary>
//    /// <typeparam name="T"></typeparam>
//    public record HttpResult<T> : ICallResult<T>
//    {
//        /// <summary>
//        /// The exchange
//        /// </summary>
//        public string Exchange { get; init; }

//        public IHttpResult CallResult { get; init; }

//        /// <summary>
//        /// The trade modes for which the result data is
//        /// </summary>
//        public TradingMode[]? DataTradeMode { get; init;  }

//        public PageRequest? NextPageRequest { get; init; }
//        public T? Data { get; init; }
//        public Error? Error { get; set; }
//        public bool Success => Error == null;
//    }
//}
