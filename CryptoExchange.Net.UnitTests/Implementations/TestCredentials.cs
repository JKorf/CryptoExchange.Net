using CryptoExchange.Net.Authentication;
using System;

namespace CryptoExchange.Net.UnitTests.Implementations
{
    internal class TestCredentials : HMACCredential
    {
        public TestCredentials() { }

        public TestCredentials(string key, string secret) : base(key, secret)
        {
        }

        public TestCredentials(HMACCredential credential) : base(credential.Key, credential.Secret)
        {
        }

        public TestCredentials WithHMAC(string key, string secret)
        {
            if (!string.IsNullOrEmpty(Key)) throw new InvalidOperationException("Credentials already set");

            Key = key;
            Secret = secret;
            return this;
        }

        /// <inheritdoc />
        public override ApiCredentials Copy() => new TestCredentials(this);
    }
}
