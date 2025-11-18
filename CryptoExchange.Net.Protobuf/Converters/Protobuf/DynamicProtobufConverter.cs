using CryptoExchange.Net.Converters.MessageParsing;
using CryptoExchange.Net.Converters.MessageParsing.DynamicConverters;
using CryptoExchange.Net.Objects;
using ProtoBuf.Meta;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;

namespace CryptoExchange.Net.Protobuf.Converters.Protobuf
{
    public abstract class DynamicProtobufConverter : IMessageConverter
    {
        /// <summary>
        /// Runtime type model
        /// </summary>
        protected RuntimeTypeModel _model;

        public DynamicProtobufConverter(RuntimeTypeModel model)
        {
            _model = model;
        }

        public object Deserialize(ReadOnlySpan<byte> data, Type type)
        {
            var result = _model.Deserialize(type, data);
            return result;
        }

        public abstract string GetMessageIdentifier(ReadOnlySpan<byte> data, WebSocketMessageType? webSocketMessageType);
    }
}
