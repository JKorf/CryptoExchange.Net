using CryptoExchange.Net.Sockets;

namespace CryptoExchange.Net.UnitTests.TestImplementations.Sockets
{
    internal class TestQuery : Query<object>
    {
        public TestQuery(string identifier, object request, bool authenticated, int weight = 1) : base(request, authenticated, weight)
        {
            MessageMatcher = MessageMatcher.Create<object>(identifier);
        }
    }
}
