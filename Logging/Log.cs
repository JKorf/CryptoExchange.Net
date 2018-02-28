using System;
using System.IO;

namespace CryptoExchange.Net.Logging
{
    public class Log
    {
        public TextWriter TextWriter { get; internal set; } = new DebugTextWriter();
        public LogVerbosity Level { get; internal set; } = LogVerbosity.Warning;

        public void Write(LogVerbosity logType, string message)
        {
            if ((int)logType >= (int)Level)
                TextWriter.WriteLine($"{DateTime.Now:hh:mm:ss:fff} | {logType} | {message}");
        }
    }

    public enum LogVerbosity
    {
        Debug,
        Warning,
        Error,
        None
    }
}
