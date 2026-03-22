namespace CryptoExchange.Net.Objects.Options
{
    /// <summary>
    /// Options for API usage
    /// </summary>
    public class ApiOptions
    {
        /// <summary>
        /// Whether or not to automatically sync the local time with the server time
        /// </summary>
        public bool? AutoTimestamp { get; set; }

        /// <summary>
        /// If true, the CallResult and DataEvent objects will also include the originally received string data in the OriginalData property.
        /// Note that this comes at a performance cost
        /// </summary>
        public bool? OutputOriginalData { get; set; }
    }
}
