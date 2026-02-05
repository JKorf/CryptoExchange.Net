namespace CryptoExchange.Net.Trackers.UserData.Objects
{
    /// <summary>
    /// User data update
    /// </summary>
    /// <typeparam name="T">Data type</typeparam>
    public class UserDataUpdate<T>
    {
        /// <summary>
        /// Source
        /// </summary>
        public UpdateSource Source { get; set; }
        /// <summary>
        /// Exchange name
        /// </summary>
        public string Exchange { get; set; }
        /// <summary>
        /// Data
        /// </summary>
        public T Data { get; set; } = default!;

        /// <summary>
        /// ctor
        /// </summary>
        public UserDataUpdate(UpdateSource source, string exchange, T data)
        {
            Source = source;
            Exchange = exchange;
            Data = data;
        }
    }
}
