using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static System.Collections.Specialized.BitVector32;

namespace CryptoExchange.Net.Objects
{
    public class ConcurrentList<T> : IEnumerable<T>
    {
        private readonly object _lock = new object();
        private readonly List<T> _collection = new List<T>();

        public void Add(T item)
        {
            lock (_lock)
                _collection.Add(item);
        }


        public void Remove(T item)
        {
            lock (_lock)
                _collection.Remove(item);
        }

        public T? SingleOrDefault(Func<T, bool> action)
        {
            lock (_lock)
                return _collection.SingleOrDefault(action);
        }

        public bool All(Func<T, bool> action)
        {
            lock (_lock)
                return _collection.All(action);
        }

        public bool Any(Func<T, bool> action)
        {
            lock (_lock)
                return _collection.Any(action);
        }

        public int Count(Func<T, bool> action)
        {
            lock (_lock)
                return _collection.Count(action);
        }

        public bool Contains(T item)
        {
            lock (_lock)
                return _collection.Contains(item);
        }

        public T[] ToArray(Func<T, bool> predicate)
        {
            lock (_lock)
                return _collection.Where(predicate).ToArray();
        }

        public List<T> ToList()
        {
            lock (_lock)
                return _collection.ToList();
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (_lock)
            {
                foreach (var item in _collection)
                    yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
