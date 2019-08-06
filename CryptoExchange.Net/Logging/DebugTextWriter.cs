using System.Diagnostics;
using System.IO;
using System.Text;

namespace CryptoExchange.Net.Logging
{
    /// <summary>
    /// Default log writer, writes to debug
    /// </summary>
    public class DebugTextWriter: TextWriter
    {
        /// <inheritdoc />
        public override Encoding Encoding => Encoding.ASCII;

        /// <inheritdoc />
        public override void WriteLine(string value)
        {
            Debug.WriteLine(value);
        }
    }
}
