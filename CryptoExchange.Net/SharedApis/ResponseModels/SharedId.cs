namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Id
    /// </summary>
    public record SharedId
    {
        /// <summary>
        /// The id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Id
        /// </summary>
        public SharedId(string id)
        {
            Id = id;
        }
    }
}
