using CryptoExchange.Net.Converters;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Sockets;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;

namespace CryptoExchange.Net.Objects.Sockets
{
    public class MessageInterpreterPipeline
    {
        public Func<WebSocketMessageType, Stream, Stream>? PreProcessCallback { get; set; }
        public List<PreInspectCallback> PreInspectCallbacks { get; set; } = new List<PreInspectCallback>();
        public Func<IMessageAccessor, string?> GetIdentity { get; set; }
        public List<object> PostInspectCallbacks { get; set; } = new List<object>();
        public Func<JToken, Type, BaseParsedMessage> ObjectInitializer { get; set; } = SocketConverter.InstantiateMessageObject;
    }

    public class PreInspectCallback
    {
        public Func<Stream, PreInspectResult> Callback { get; set; }
    }

    public class PostInspectCallback
    {
        public List<TypeField> TypeFields { get; set; } = new List<TypeField>();
        public Func<IMessageAccessor, Dictionary<string, Type>, PostInspectResult> Callback { get; set; }
    }

    public class TypeField
    {
        public string Key { get; set; }
        public bool Required { get; set; }

        public TypeField(string key, bool required = true)
        {
            Key = key;
            Required = required;
        }
    }

    public class PostInspectArrayCallback
    {
        public List<int> TypeFields { get; set; } = new List<int>();
        public Func<Dictionary<int, string>, Dictionary<string, Type>, PostInspectResult> Callback { get; set; }
    }

    public class PreInspectResult
    {
        public bool Matched { get; set; }
        public string Identifier { get; set; }
    }

    public class PostInspectResult
    {
        public Type? Type { get; set; }
        public string Identifier { get; set; }
    }
}
