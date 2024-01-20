using CryptoExchange.Net.Converters;
using CryptoExchange.Net.Sockets.MessageParsing.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;

namespace CryptoExchange.Net.Sockets.MessageParsing
{
    /// <summary>
    /// Json.Net message accessor
    /// </summary>
    public class JsonNetMessageData : IMessageAccessor
    {
        private readonly JToken? _token;
        private readonly Stream _stream;
        private static JsonSerializer _serializer = JsonSerializer.Create(SerializerOptions.WithConverters);

        /// <inheritdoc />
        public bool IsJson { get; private set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="stream"></param>
        public JsonNetMessageData(Stream stream)
        {
            _stream = stream;
            using var reader = new StreamReader(stream, Encoding.UTF8, false, (int)stream.Length, true);
            using var jsonTextReader = new JsonTextReader(reader);

            try
            {
                _token = JToken.Load(jsonTextReader);
                IsJson = true;
            }
            catch (Exception)
            {
                // Not a json message
                IsJson = false;
            }
        }

        /// <inheritdoc />
        public object Deserialize(Type type)
        {
            if (!IsJson)
            {
                var sr = new StreamReader(_stream);
                return sr.ReadToEnd();
            }

            return _token!.ToObject(type, _serializer)!;
        }

        /// <inheritdoc />
        public NodeType? GetNodeType()
        {
            if (_token == null)
                return null;

            if (_token.Type == JTokenType.Object)
                return NodeType.Object;

            if (_token.Type == JTokenType.Array)
                return NodeType.Array;

            return NodeType.Value;
        }

        /// <inheritdoc />
        public NodeType? GetNodeType(MessagePath path)
        {
            var node = GetPathNode(path);
            if (node == null)
                return null;

            if (node.Type == JTokenType.Object)
                return NodeType.Object;

            if (node.Type == JTokenType.Array)
                return NodeType.Array;

            return NodeType.Value;
        }

        /// <inheritdoc />
        public T? GetValue<T>(MessagePath path)
        {
            var value = GetPathNode(path);
            if (value == null)
                return default;

            if (value.Type == JTokenType.Object || value.Type == JTokenType.Array)
                return default;

            return value!.Value<T>();
        }

        private JToken? GetPathNode(MessagePath path)
        {
            var currentToken = _token;
            foreach (var node in path)
            {
                if (node.Type)
                {
                    // Int value
                    var val = (int)node.Value;
                    if (currentToken!.Type != JTokenType.Array || ((JArray)currentToken).Count <= val)
                        return null;

                    currentToken = currentToken[val];
                }
                else
                {
                    // String value
                    if (currentToken!.Type != JTokenType.Object)
                        return null;

                    currentToken = currentToken[(string)node.Value];
                }

                if (currentToken == null)
                    return null;
            }

            return currentToken;
        }
    }
}
