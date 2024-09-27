using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// A CallResult from an exchange
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ExchangeResult<T> : CallResult<T>
    {
        /// <summary>
        /// The exchange
        /// </summary>
        public string Exchange { get; }

        /// <summary>
        /// ctor
        /// </summary>
        public ExchangeResult(
            string exchange,
            Error error) :
            base(error)
        {
            Exchange = exchange;
        }

        /// <summary>
        /// ctor
        /// </summary>
        public ExchangeResult(
            string exchange,
            CallResult<T> result) :
            base(
                result.Data,
                result.OriginalData,
                result.Error)
        {
            Exchange = exchange;
        }

        /// <inheritdoc />
        public override string ToString() => $"{Exchange} - " + base.ToString();
    }
}
