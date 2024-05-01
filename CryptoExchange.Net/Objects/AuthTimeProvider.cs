using CryptoExchange.Net.Interfaces;
using System;

namespace CryptoExchange.Net.Objects
{
    internal class AuthTimeProvider : IAuthTimeProvider
    {
        public DateTime GetTime() => DateTime.UtcNow;
    }
}
