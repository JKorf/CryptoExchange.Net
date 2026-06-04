//using CryptoExchange.Net.Objects;

//namespace CryptoExchange.Net.SharedApis
//{
//    /// <summary>
//    /// A CallResult from an exchange
//    /// </summary>
//    /// <typeparam name="T"></typeparam>
//    public record ExchangeWebSocketResult<T> : ICallResult<T>
//    {
//        /// <summary>
//        /// The exchange
//        /// </summary>
//        public string Exchange { get; }
//        public IWebSocketResult CallResult { get; }
//        public T Data { get; }

//        public Error? Error => CallResult.Error;
//        public bool Success => Error == null;

//        public ExchangeWebSocketResult(string exchange, Error error)
//        {
//            Exchange = exchange;
//            CallResult = WebSocketResult.Fail(error);
//        }

//        public ExchangeWebSocketResult(string exchange, IWebSocketResult callResult, T data)
//        {
//            Exchange = exchange;
//            CallResult = callResult;
//            Data = data;
//        }
//    }
//}
