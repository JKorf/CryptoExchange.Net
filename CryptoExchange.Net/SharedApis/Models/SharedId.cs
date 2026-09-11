namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Id
    /// </summary>
    public record SharedId
    {
        /// <summary>
        /// The id, note that this can be null or empty if the API doesn't return an ID.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Id
        /// </summary>
        public SharedId(string? id)
        {
            Id = id;
        }
    }
}
