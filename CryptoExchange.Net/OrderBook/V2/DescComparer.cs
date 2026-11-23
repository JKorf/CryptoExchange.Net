using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.OrderBook.V2
{

    internal class DescComparer<T> : IComparer<T>
    {
        public int Compare(T? x, T? y)
        {
            return Comparer<T>.Default.Compare(y!, x!);
        }
    }
}
