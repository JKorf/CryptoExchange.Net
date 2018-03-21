using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace CryptoExchange.Net.Logging
{
    public class Log
    {
        private List<TextWriter> writers;
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

        public Log()
        {
            writers = new List<TextWriter>();
        }

        public void UpdateWriters(List<TextWriter> textWriters)
        {
            writers = textWriters;
        }

        public void Write(LogVerbosity logType, string message)
        {
            foreach (var writer in writers)
            {
                try
                {
                    if ((int) logType >= (int) Level)
                        writer.WriteLine($"{DateTime.Now:yyyy/MM/dd hh:mm:ss:fff} | {logType} | {message}");
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Failed to write log: " + e.Message);
                }
            }
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
