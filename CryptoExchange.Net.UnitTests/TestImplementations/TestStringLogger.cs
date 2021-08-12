using Microsoft.Extensions.Logging;
using System;
using System.Text;

namespace CryptoExchange.Net.UnitTests.TestImplementations
{
    public class TestStringLogger : ILogger
    {
        StringBuilder _builder = new StringBuilder();

        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            _builder.AppendLine(formatter(state, exception));
        }

        public string GetLogs()
        {
            return _builder.ToString();
        }
    }
}
