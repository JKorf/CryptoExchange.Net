using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Interfaces
{
    public interface IMessageProcessor
    {
        public int Id { get; }
        Task<CallResult> HandleMessageAsync(DataEvent<BaseParsedMessage> message);
        public Type ExpectedMessageType { get; }
    }
}
