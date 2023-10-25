using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CryptoExchange.Net.Sockets;

namespace CryptoExchange.Net.Objects.Sockets
{
    /// <summary>
    /// A message received from a stream
    /// </summary>
    public class StreamMessage : IDisposable
    {
        /// <summary>
        /// The connection it was received on
        /// </summary>
        public SocketConnection Connection { get; }
        /// <summary>
        /// The data stream
        /// </summary>
        public Stream Stream { get; }
        /// <summary>
        /// Receive timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }

        private Dictionary<Type, object> _casted;

        /// <summary>
        /// Get the data from the memory in specified type using the converter. If this type has been resolved before it will use that instead
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="converter"></param>
        /// <returns></returns>
        public T Get<T>(Func<Stream, T> converter)
        {
            if (_casted.TryGetValue(typeof(T), out var casted))
                return (T)casted;

            var result = converter(Stream);
            _casted.Add(typeof(T), result!);
            Stream.Position = 0;
            return result;
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Stream.Dispose();
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="stream"></param>
        /// <param name="timestamp"></param>
        public StreamMessage(SocketConnection connection, Stream stream, DateTime timestamp)
        {
            Connection = connection;
            Stream = stream;
            Timestamp = timestamp;
            _casted = new Dictionary<Type, object>();
        }
    }
}
