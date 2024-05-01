using CryptoExchange.Net.Interfaces;
using System;

namespace CryptoExchange.Net.Testing.Implementations
{
    internal class TestAuthTimeProvider : IAuthTimeProvider
    {
        private readonly DateTime _timestamp;

        public TestAuthTimeProvider(DateTime timestamp)
        {
            _timestamp = timestamp;
        }

        public DateTime GetTime() => _timestamp;
    }
}
