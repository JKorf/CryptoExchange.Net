using System;
using System.Collections.Generic;

namespace CryptoExchange.Net.Objects
{
    /// <summary>
    /// Comparer for byte order
    /// </summary>
    public class ByteOrderComparer : IComparer<byte[]>
    {
        /// <summary>
        /// Compare function
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int Compare(byte[] x, byte[] y)
        {
            // Shortcuts: If both are null, they are the same.
            if (x == null && y == null) return 0;

            // If one is null and the other isn't, then the
            // one that is null is "lesser".
            if (x == null) return -1;
            if (y == null) return 1;

            // Both arrays are non-null.  Find the shorter
            // of the two lengths.
            var bytesToCompare = Math.Min(x.Length, y.Length);

            // Compare the bytes.
            for (var index = 0; index < bytesToCompare; ++index)
            {
                // The x and y bytes.
                var xByte = x[index];
                var yByte = y[index];

                // Compare result.
                var compareResult = Comparer<byte>.Default.Compare(xByte, yByte);

                // If not the same, then return the result of the
                // comparison of the bytes, as they were the same
                // up until now.
                if (compareResult != 0) return compareResult;

                // They are the same, continue.
            }

            // The first n bytes are the same.  Compare lengths.
            // If the lengths are the same, the arrays
            // are the same.
            if (x.Length == y.Length) return 0;

            // Compare lengths.
            return x.Length < y.Length ? -1 : 1;
        }
    }
}
