using System;
using System.Diagnostics;

namespace CryptoExchange.Net.Testing
{
    internal class EnumValueTraceListener : TraceListener
    {
        public override void Write(string message)
        {
            if (message.Contains("Cannot map"))
                throw new Exception("Enum value error: " + message);

            if (message.Contains("Received null enum value"))
                throw new Exception("Enum null error: " + message);
        }

        public override void WriteLine(string message)
        {
            if (message.Contains("Cannot map"))
                throw new Exception("Enum value error: " + message);

            if (message.Contains("Received null enum value"))
                throw new Exception("Enum null error: " + message);
        }
    }
}
