using Microsoft.Extensions.Logging;
using System;

namespace CryptoExchange.Net.Logging
{
    /// <summary>
    /// Log to console
    /// </summary>
    public class ConsoleLogger : ILogger
    {
        /// <inheritdoc />
        public IDisposable BeginScope<TState>(TState state) => null!;

        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel) => true;

        /// <inheritdoc />
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var logMessage = $"{DateTime.Now:yyyy/MM/dd HH:mm:ss:fff} | {logLevel} | {formatter(state, exception)}";
            Console.WriteLine(logMessage);
        }
    }
}
