using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CryptoExchange.Net.Logging
{
    /// <summary>
    /// Log implementation
    /// </summary>
    public class Log
    {
        /// <summary>
        /// List of ILogger implementations to forward the message to
        /// </summary>
        private List<ILogger> writers;

        /// <summary>
        /// The verbosity of the logging, anything more verbose will not be forwarded to the writers
        /// </summary>
        public LogLevel? Level { get; set; } = LogLevel.Information;

        /// <summary>
        /// Client name
        /// </summary>
        public string ClientName { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="clientName">The name of the client the logging is used in</param>
        public Log(string clientName)
        {
            ClientName = clientName;
            writers = new List<ILogger>();
        }

        /// <summary>
        /// Set the writers
        /// </summary>
        /// <param name="textWriters"></param>
        public void UpdateWriters(List<ILogger> textWriters)
        {
            writers = textWriters;
        }

        /// <summary>
        /// Write a log entry
        /// </summary>
        /// <param name="logLevel">The verbosity of the message</param>
        /// <param name="message">The message to log</param>
        public void Write(LogLevel logLevel, string message)
        {
            if (Level != null && (int)logLevel < (int)Level)
                return;

            var logMessage = $"{ClientName,-10} | {message}";
            foreach (var writer in writers.ToList())
            {
                try
                {
                    writer.Log(logLevel, logMessage);
                }
                catch (Exception e)
                {
                    // Can't write to the logging so where else to output..
                    Debug.WriteLine($"Failed to write log to writer {writer.GetType()}: " + e.ToLogString());
                }
            }
        }
    }
}
