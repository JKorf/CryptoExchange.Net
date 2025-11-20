using CryptoExchange.Net.Converters.MessageParsing;
using CryptoExchange.Net.Converters.MessageParsing.DynamicConverters;
using CryptoExchange.Net.Objects;
using LightProto;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;

namespace CryptoExchange.Net.Protobuf.Converters.Protobuf
{
    public abstract class DynamicProtobufConverter<T> : IMessageConverter
    {
        public object Deserialize(ReadOnlySpan<byte> data, Type type)
        {
            var result = Serializer.Deserialize<T>(data);
            return result;
        }

        public abstract string GetMessageIdentifier(ReadOnlySpan<byte> data, WebSocketMessageType? webSocketMessageType);
    }
}
