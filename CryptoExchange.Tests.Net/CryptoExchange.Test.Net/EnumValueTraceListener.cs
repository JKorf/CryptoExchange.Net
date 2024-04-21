using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoExchange.Test.Net
{
    internal class EnumValueTraceListener : TraceListener
    {
        public override void Write(string message)
        {
            if (message.Contains("Cannot map"))
                throw new Exception("Enum value error: " + message);
        }

        public override void WriteLine(string message)
        {
            if (message.Contains("Cannot map"))
                throw new Exception("Enum value error: " + message);
        }
    }
}
