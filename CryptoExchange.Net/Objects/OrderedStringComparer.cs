using System.Collections.Generic;

namespace CryptoExchange.Net.Objects
{
    /// <summary>
    /// Order string comparer, sorts by alphabetical order
    /// </summary>
    public class OrderedStringComparer : IComparer<string>
    {
        /// <summary>
        /// Compare function
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int Compare(string x, string y)
        {
            // Shortcuts: If both are null, they are the same.
            if (x == null && y == null) return 0;

            // If one is null and the other isn't, then the
            // one that is null is "lesser".
            if (x == null) return -1;
            if (y == null) return 1;

            return x.CompareTo(y);
        }
    }
}
