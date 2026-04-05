using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Errors;
using CryptoExchange.Net.Sockets;
using CryptoExchange.Net.Sockets.Default;
using System;

namespace CryptoExchange.Net.UnitTests.Implementations
{
    internal class TestQuery : Query<TestSocketMessage>
    {
        public TestQuery(TestSocketMessage request, bool authenticated) : base(request, authenticated, 1)
        {
            MessageRouter = MessageRouter.CreateWithoutTopicFilter<TestSocketMessage>(request.Id.ToString(), HandleMessage);
        }

        private CallResult? HandleMessage(SocketConnection connection, DateTime time, string? arg3, TestSocketMessage message)
        {
            if (message.Data != "OK")
                return new CallResult(new ServerError(ErrorInfo.Unknown with { Message = message.Data }));

            return CallResult.SuccessResult;
        }
    }
}
