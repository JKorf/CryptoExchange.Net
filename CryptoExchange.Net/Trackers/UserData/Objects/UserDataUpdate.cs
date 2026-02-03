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
        /// Data
        /// </summary>
        public T Data { get; set; } = default!;
    }
}
