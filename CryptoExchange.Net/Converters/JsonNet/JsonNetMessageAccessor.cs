﻿using CryptoExchange.Net.Converters.MessageParsing;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CryptoExchange.Net.Converters.JsonNet
{
    /// <summary>
    /// Json.Net message accessor
    /// </summary>
    public abstract class JsonNetMessageAccessor : IMessageAccessor
    {
        /// <summary>
        /// The json token loaded
        /// </summary>
        protected JToken? _token;
        private static JsonSerializer _serializer = JsonSerializer.Create(SerializerOptions.WithConverters);

        /// <inheritdoc />
        public bool IsJson { get; protected set; }

        /// <inheritdoc />
        public abstract bool OriginalDataAvailable { get; }

        /// <inheritdoc />
        public object? Underlying => _token;

        /// <inheritdoc />
        public CallResult<object> Deserialize(Type type, MessagePath? path = null)
        {
            if (!IsJson)
                return new CallResult<object>(GetOriginalString());

            var source = _token;
            if (path != null)
                source = GetPathNode(path.Value);

            try
            {
                var result = source!.ToObject(type, _serializer)!;
                return new CallResult<object>(result);
            }
            catch (JsonReaderException jre)
            {
                var info = $"Deserialize JsonReaderException: {jre.Message}, Path: {jre.Path}, LineNumber: {jre.LineNumber}, LinePosition: {jre.LinePosition}";
                return new CallResult<object>(new DeserializeError(info, OriginalDataAvailable ? GetOriginalString() : "[Data only available when OutputOriginal = true in client options]"));
            }
            catch (JsonSerializationException jse)
            {
                var info = $"Deserialize JsonSerializationException: {jse.Message}";
                return new CallResult<object>(new DeserializeError(info, OriginalDataAvailable ? GetOriginalString() : "[Data only available when OutputOriginal = true in client options]"));
            }
            catch (Exception ex)
            {
                var exceptionInfo = ex.ToLogString();
                var info = $"Deserialize Unknown Exception: {exceptionInfo}";
                return new CallResult<object>(new DeserializeError(info, OriginalDataAvailable ? GetOriginalString() : "[Data only available when OutputOriginal = true in client options]"));
            }
        }

        /// <inheritdoc />
        public CallResult<T> Deserialize<T>(MessagePath? path = null)
        {
            var source = _token;
            if (path != null)
                source = GetPathNode(path.Value);

            try
            {
                var result = source!.ToObject<T>(_serializer)!;
                return new CallResult<T>(result);
            }
            catch (JsonReaderException jre)
            {
                var info = $"Deserialize JsonReaderException: {jre.Message}, Path: {jre.Path}, LineNumber: {jre.LineNumber}, LinePosition: {jre.LinePosition}";
                return new CallResult<T>(new DeserializeError(info, OriginalDataAvailable ? GetOriginalString() : "[Data only available when OutputOriginal = true in client options]"));
            }
            catch (JsonSerializationException jse)
            {
                var info = $"Deserialize JsonSerializationException: {jse.Message}";
                return new CallResult<T>(new DeserializeError(info, OriginalDataAvailable ? GetOriginalString() : "[Data only available when OutputOriginal = true in client options]"));
            }
            catch (Exception ex)
            {
                var exceptionInfo = ex.ToLogString();
                var info = $"Deserialize Unknown Exception: {exceptionInfo}";
                return new CallResult<T>(new DeserializeError(info, OriginalDataAvailable ? GetOriginalString() : "[Data only available when OutputOriginal = true in client options]"));
            }
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

        /// <inheritdoc />
        public abstract string GetOriginalString();

        /// <inheritdoc />
        public abstract void Clear();
    }

    /// <summary>
    /// Json.Net stream message accessor
    /// </summary>
    public class JsonNetStreamMessageAccessor : JsonNetMessageAccessor, IStreamMessageAccessor
    {
        private Stream? _stream;

        /// <inheritdoc />
        public override bool OriginalDataAvailable => _stream?.CanSeek == true;

        /// <inheritdoc />
        public bool Read(Stream stream, bool bufferStream)
        {
            if (bufferStream && stream is not MemoryStream)
            {
                _stream = new MemoryStream();
                stream.CopyTo(_stream);
                _stream.Position = 0;
            }
            else
            {
                _stream = stream;
            }

            var length = _stream.CanSeek ? _stream.Length : 4096;
            using var reader = new StreamReader(_stream, Encoding.UTF8, false, (int)Math.Max(2, length), true);
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

            return IsJson;
        }
        /// <inheritdoc />
        public override string GetOriginalString()
        {
            if (_stream is null)
                throw new NullReferenceException("Stream not initialized");

            _stream.Position = 0;
            using var textReader = new StreamReader(_stream, Encoding.UTF8, false, 1024, true);
            return textReader.ReadToEnd();
        }

        /// <inheritdoc />
        public override void Clear()
        {
            _stream?.Dispose();
            _stream = null;
            _token = null;
        }

    }

    /// <summary>
    /// Json.Net byte message accessor
    /// </summary>
    public class JsonNetByteMessageAccessor : JsonNetMessageAccessor, IByteMessageAccessor
    {
        private ReadOnlyMemory<byte> _bytes;

        /// <inheritdoc />
        public bool Read(ReadOnlyMemory<byte> data)
        {
            _bytes = data;
            using var stream = new MemoryStream(data.ToArray());
            using var reader = new StreamReader(stream, Encoding.UTF8, false, (int)Math.Max(2, data.Length), true);
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

            return IsJson;
        }

        /// <inheritdoc />
        public override string GetOriginalString() => Encoding.UTF8.GetString(_bytes.ToArray());

        /// <inheritdoc />
        public override bool OriginalDataAvailable => true;

        /// <inheritdoc />
        public override void Clear()
        {
            _bytes = null;
            _token = null;
        }
    }
}
