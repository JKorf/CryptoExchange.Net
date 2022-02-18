using System;

namespace CryptoExchange.Net.Attributes
{
    /// <summary>
    /// Map a enum entry to string values
    /// </summary>
    public class MapAttribute : Attribute
    {
        /// <summary>
        /// Values mapping to the enum entry
        /// </summary>
        public string[] Values { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="maps"></param>
        public MapAttribute(params string[] maps)
        {
            Values = maps;
        }
    }
}
