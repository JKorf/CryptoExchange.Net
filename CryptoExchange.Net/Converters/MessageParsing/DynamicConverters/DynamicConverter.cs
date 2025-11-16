using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace CryptoExchange.Net.Converters.MessageParsing.DynamicConverters
{

    public ref struct MessageType
    {
        public Type Type { get; set; }
        public string? Identifier { get; set; }
    }

    public interface IMessageConverter
    {
        MessageType GetMessageType(ReadOnlySpan<byte> data, WebSocketMessageType? webSocketMessageType);

        object Deserialize(ReadOnlySpan<byte> data, Type type);
    }

    public abstract class DynamicConverter : IMessageConverter
    {
        public abstract JsonSerializerOptions Options { get; }

        public abstract MessageType GetMessageType(ReadOnlySpan<byte> data, WebSocketMessageType? webSocketMessageType);

        public virtual object Deserialize(ReadOnlySpan<byte> data, Type type)
        {
            return JsonSerializer.Deserialize(data, type, Options);
        }
    }

    public abstract class StaticConverter : IMessageConverter
    {
        public abstract JsonSerializerOptions Options { get; }
        public abstract MessageType GetMessageType(ReadOnlySpan<byte> data, WebSocketMessageType? webSocketMessageType);

        public object? Deserialize(ReadOnlySpan<byte> data, Type type)
        {
            return JsonSerializer.Deserialize(data, type, Options);
        }

    }

    public abstract class StaticConverter<T> : StaticConverter
    {
        public override MessageType GetMessageType(ReadOnlySpan<byte> data,, WebSocketMessageType? webSocketMessageType) =>
            new MessageType { Type = typeof(T), Identifier = GetMessageListenId(data, webSocketMessageType) };

        public abstract string GetMessageListenId(ReadOnlySpan<byte> data, WebSocketMessageType? webSocketMessageType);
    }
}
