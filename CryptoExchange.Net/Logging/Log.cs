using System;
using System.IO;

namespace CryptoExchange.Net.Logging
{
    public class Log
    {
        public TextWriter TextWriter { get; internal set; } = new DebugTextWriter();
        private LogVerbosity level = LogVerbosity.Info;

        public LogVerbosity Level
        {
            get => level;
            set
            {
                if (level != value)
                {
                    Write(LogVerbosity.Info, "Loglevel set to " + value);
                    level = value;
                }
            }
        }

        public void Write(LogVerbosity logType, string message)
        {
            if ((int)logType >= (int)Level)
                TextWriter.WriteLine($"{DateTime.Now:hh:mm:ss:fff} | {logType} | {message}");
        }
    }

    public enum LogVerbosity
    {
        Debug,
        Info,
        Warning,
        Error,
        None
    }
}
