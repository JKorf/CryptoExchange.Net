using CryptoExchange.Net.Converters;
using CryptoExchange.Net.Sockets.MessageParsing.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CryptoExchange.Net.Sockets.MessageParsing.JsonNet
{
    /// <summary>
    /// Json.Net message accessor
    /// </summary>
    public class JsonNetMessageAccessor : IMessageAccessor
    {
        private JToken? _token;
        private Stream? _stream;
        private static JsonSerializer _serializer = JsonSerializer.Create(SerializerOptions.WithConverters);

        /// <inheritdoc />
        public bool IsJson { get; private set; }

        /// <inheritdoc />
        public object? Underlying => _token;

        /// <inheritdoc />
        public void Load(Stream stream)
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
        public object Deserialize(Type type, MessagePath? path = null)
        {
            if (!IsJson)
            {
                var sr = new StreamReader(_stream);
                return sr.ReadToEnd();
            }

            var source = _token;
            if (path != null)
                source = GetPathNode(path.Value);

            return source!.ToObject(type, _serializer)!;
        }

        /// <inheritdoc />
        public NodeType? GetNodeType()
        {
            if (!IsJson)
                throw new InvalidOperationException("Can't access json data on non-json message");

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
            if (!IsJson)
                throw new InvalidOperationException("Can't access json data on non-json message");

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
            if (!IsJson)
                throw new InvalidOperationException("Can't access json data on non-json message");

            var value = GetPathNode(path);
            if (value == null)
                return default;

            if (value.Type == JTokenType.Object || value.Type == JTokenType.Array)
                return default;

            return value!.Value<T>();
        }

        /// <inheritdoc />
        public List<T?>? GetValues<T>(MessagePath path)
        {
            if (!IsJson)
                throw new InvalidOperationException("Can't access json data on non-json message");

            var value = GetPathNode(path);
            if (value == null)
                return default;

            if (value.Type == JTokenType.Object)
                return default;

            return value!.Values<T>().ToList();
        }

        private JToken? GetPathNode(MessagePath path)
        {
            if (!IsJson)
                throw new InvalidOperationException("Can't access json data on non-json message");

            var currentToken = _token;
            foreach (var node in path)
            {
                if (node.Type == 0)
                {
                    // Int value
                    var val = (int)node.Value!;
                    if (currentToken!.Type != JTokenType.Array || ((JArray)currentToken).Count <= val)
                        return null;

                    currentToken = currentToken[val];
                }
                else if (node.Type == 1)
                {
                    // String value
                    if (currentToken!.Type != JTokenType.Object)
                        return null;

                    currentToken = currentToken[(string)node.Value!];
                }
                else
                {
                    // Property name
                    if (currentToken!.Type != JTokenType.Object)
                        return null;

                    currentToken = (currentToken.First as JProperty)?.Name;
                }

                if (currentToken == null)
                    return null;
            }

            return currentToken;
        }
    }
}
