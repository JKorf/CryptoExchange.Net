namespace CryptoExchange.Net.ComonObjects
{
    /// <summary>
    /// Base class for comon objects
    /// </summary>
    public class BaseComonObject
    {
        /// <summary>
        /// The source object the data is derived from
        /// </summary>
        public object SourceObject { get; set; } = null!;
    }
}
