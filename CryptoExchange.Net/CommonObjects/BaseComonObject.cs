namespace CryptoExchange.Net.CommonObjects
{
    /// <summary>
    /// Base class for common objects
    /// </summary>
    public class BaseCommonObject
    {
        /// <summary>
        /// The source object the data is derived from
        /// </summary>
        public object SourceObject { get; set; } = null!;
    }
}
