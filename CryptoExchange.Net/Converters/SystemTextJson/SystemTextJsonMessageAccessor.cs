﻿using CryptoExchange.Net.Converters.MessageParsing;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace CryptoExchange.Net.Converters.SystemTextJson
{
    /// <summary>
    /// System.Text.Json message accessor
    /// </summary>
    public abstract class SystemTextJsonMessageAccessor : IMessageAccessor
    {
        /// <summary>
        /// The JsonDocument loaded
        /// </summary>
        protected JsonDocument? _document;

        private static JsonSerializerOptions _serializerOptions = SerializerOptions.WithConverters;

        /// <inheritdoc />
        public bool IsJson { get; set; }

        /// <inheritdoc />
        public abstract bool OriginalDataAvailable { get; }

        /// <inheritdoc />
        public object? Underlying => throw new NotImplementedException();

        /// <inheritdoc />
        public CallResult<object> Deserialize(Type type, MessagePath? path = null)
        {
            if (!IsJson)
                return new CallResult<object>(GetOriginalString());

            if (_document == null)
                throw new InvalidOperationException("No json document loaded");

            try
            {
                var result = _document.Deserialize(type, _serializerOptions);
                return new CallResult<object>(result!);
            }
            catch (JsonException ex)
            {
                var info = $"Deserialize JsonException: {ex.Message}, Path: {ex.Path}, LineNumber: {ex.LineNumber}, LinePosition: {ex.BytePositionInLine}";
                return new CallResult<object>(new DeserializeError(info, OriginalDataAvailable ? GetOriginalString() : "[Data only available when OutputOriginal = true in client options]"));
            }
        }

        /// <inheritdoc />
        public CallResult<T> Deserialize<T>(MessagePath? path = null)
        {
            if (_document == null)
                throw new InvalidOperationException("No json document loaded");

            try
            {
                var result = _document.Deserialize<T>(_serializerOptions);
                return new CallResult<T>(result!);
            }
            catch (JsonException ex)
            {
                var info = $"Deserialize JsonException: {ex.Message}, Path: {ex.Path}, LineNumber: {ex.LineNumber}, LinePosition: {ex.BytePositionInLine}";
                return new CallResult<T>(new DeserializeError(info, OriginalDataAvailable ? GetOriginalString() : "[Data only available when OutputOriginal = true in client options]"));
            }
        }

        /// <inheritdoc />
        public NodeType? GetNodeType()
        {
            if (!IsJson)
                throw new InvalidOperationException("Can't access json data on non-json message");

            if (_document == null)
                throw new InvalidOperationException("No json document loaded");

            return _document.RootElement.ValueKind switch
            {
                JsonValueKind.Object => NodeType.Object,
                JsonValueKind.Array => NodeType.Array,
                _ => NodeType.Value
            };
        }

        /// <inheritdoc />
        public NodeType? GetNodeType(MessagePath path)
        {
            if (!IsJson)
                throw new InvalidOperationException("Can't access json data on non-json message");

            var node = GetPathNode(path);
            if (!node.HasValue)
                return null;

            return node.Value.ValueKind switch
            {
                JsonValueKind.Object => NodeType.Object,
                JsonValueKind.Array => NodeType.Array,
                _ => NodeType.Value
            };
        }

        /// <inheritdoc />
        public T? GetValue<T>(MessagePath path)
        {
            if (!IsJson)
                throw new InvalidOperationException("Can't access json data on non-json message");

            var value = GetPathNode(path);
            if (value == null)
                return default;

            if (value.Value.ValueKind == JsonValueKind.Object || value.Value.ValueKind == JsonValueKind.Array)
                return default;

            var ttype = typeof(T);
            if (ttype == typeof(string))
                return (T?)(object?)value.Value.GetString();
            if (ttype == typeof(short))
                return (T)(object)value.Value.GetInt16();
            if (ttype == typeof(int))
                return (T)(object)value.Value.GetInt32();
            if (ttype == typeof(long))
                return (T)(object)value.Value.GetInt64();

            return default;
        }

        /// <inheritdoc />
        public List<T?>? GetValues<T>(MessagePath path) => throw new NotImplementedException();

        private JsonElement? GetPathNode(MessagePath path)
        {
            if (!IsJson)
                throw new InvalidOperationException("Can't access json data on non-json message");

            if (_document == null)
                throw new InvalidOperationException("No json document loaded");

            JsonElement? currentToken = _document.RootElement;
            foreach (var node in path)
            {
                if (node.Type == 0)
                {
                    // Int value
                    var val = (int)node.Value!;
                    if (currentToken!.Value.ValueKind != JsonValueKind.Array || currentToken.Value.GetArrayLength() <= val)
                        return null;

                    currentToken = currentToken.Value[val];
                }
                else if (node.Type == 1)
                {
                    // String value
                    if (currentToken!.Value.ValueKind != JsonValueKind.Object)
                        return null;

                    if (!currentToken.Value.TryGetProperty((string)node.Value!, out var token))
                        return null;
                    currentToken = token;
                }
                else
                {
                    // Property name
                    if (currentToken!.Value.ValueKind != JsonValueKind.Object)
                        return null;

                    throw new NotImplementedException();
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
    /// System.Text.Json stream message accessor
    /// </summary>
    public class SystemTextJsonStreamMessageAccessor : SystemTextJsonMessageAccessor, IStreamMessageAccessor
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

            try
            {
                _document = JsonDocument.Parse(_stream);
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
            _document = null;
        }

    }

    /// <summary>
    /// System.Text.Json byte message accessor
    /// </summary>
    public class SystemTextJsonByteMessageAccessor : SystemTextJsonMessageAccessor, IByteMessageAccessor
    {
        private ReadOnlyMemory<byte> _bytes;

        /// <inheritdoc />
        public bool Read(ReadOnlyMemory<byte> data)
        {
            try
            {
                _document = JsonDocument.Parse(data);
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
            _document = null;
        }
    }
}