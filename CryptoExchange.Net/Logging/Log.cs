using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace CryptoExchange.Net.Logging
{
    /// <summary>
    /// Log implementation
    /// </summary>
    public class Log
    {
        private List<TextWriter> writers;
        /// <summary>
        /// The verbosity of the logging
        /// </summary>
        public LogVerbosity Level { get; set; } = LogVerbosity.Info;

        /// <summary>
        /// ctor
        /// </summary>
        public Log()
        {
            writers = new List<TextWriter>();
        }

        /// <summary>
        /// Set the writers
        /// </summary>
        /// <param name="textWriters"></param>
        public void UpdateWriters(List<TextWriter> textWriters)
        {
            writers = textWriters;
        }

        /// <summary>
        /// Write a log entry
        /// </summary>
        /// <param name="logType"></param>
        /// <param name="message"></param>
        public void Write(LogVerbosity logType, string message)
        {
            if ((int)logType < (int)Level)
                return;

            var logMessage = $"{DateTime.Now:yyyy/MM/dd HH:mm:ss:fff} | {logType} | {message}";
            foreach (var writer in writers.ToList())
            {
                try
                {
                    writer.WriteLine(logMessage);
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"Failed to write log to writer {writer.GetType()}: " + (e.InnerException?.Message ?? e.Message));
                }
            }
        }
    }

    /// <summary>
    /// The log verbosity
    /// </summary>
    public enum LogVerbosity
    {
        /// <summary>
        /// Debug logging
        /// </summary>
        Debug,
        /// <summary>
        /// Info logging
        /// </summary>
        Info,
        /// <summary>
        /// Warning logging
        /// </summary>
        Warning,
        /// <summary>
        /// Error logging
        /// </summary>
        Error,
        /// <summary>
        /// None, used for disabling logging
        /// </summary>
        None
    }
}
