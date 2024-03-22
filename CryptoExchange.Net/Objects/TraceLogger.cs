using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace CryptoExchange.Net.Objects
{
    /// <summary>
    /// Trace logger provider for creating trace loggers
    /// </summary>
    public class TraceLoggerProvider : ILoggerProvider
    {
        private readonly LogLevel _logLevel;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="logLevel"></param>
        public TraceLoggerProvider(LogLevel? logLevel = null)
        {
            _logLevel = logLevel ?? LogLevel.Trace;
        }

        /// <inheritdoc />
        public ILogger CreateLogger(string categoryName) => new TraceLogger(categoryName, _logLevel);
        /// <inheritdoc />
        public void Dispose() { }
    }

    /// <summary>
    /// Trace logger
    /// </summary>
    public class TraceLogger : ILogger
    {
        private readonly string? _categoryName;
        private readonly LogLevel _logLevel;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="categoryName"></param>
        /// <param name="level"></param>
        public TraceLogger(string? categoryName = null, LogLevel level = LogLevel.Trace)
        {
            _categoryName = categoryName;
            _logLevel = level;
        }

        /// <inheritdoc />
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null!;

        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel) => (int)logLevel >= (int)_logLevel;
        /// <inheritdoc />
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var logMessage = $"{DateTime.Now:yyyy/MM/dd HH:mm:ss:fff} | {logLevel} | {(_categoryName == null ? "" : $"{_categoryName} | ")}{formatter(state, exception)}";
            Trace.WriteLine(logMessage);
        }
    }
}
