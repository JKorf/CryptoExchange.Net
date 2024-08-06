using CryptoExchange.Net.Converters.MessageParsing;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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
            catch (Exception ex)
            {
                var info = $"Unknown exception: {ex.Message}";
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

            if (typeof(T) == typeof(string))
            {
                if (value.Value.ValueKind == JsonValueKind.Number)
                    return (T)(object)value.Value.GetInt64().ToString();
            }

            return value.Value.Deserialize<T>();
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
                    var val = node.Index!.Value;
                    if (currentToken!.Value.ValueKind != JsonValueKind.Array || currentToken.Value.GetArrayLength() <= val)
                        return null;

                    currentToken = currentToken.Value[val];
                }
                else if (node.Type == 1)
                {
                    // String value
                    if (currentToken!.Value.ValueKind != JsonValueKind.Object)
                        return null;

                    if (!currentToken.Value.TryGetProperty(node.Property!, out var token))
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
        public async Task<CallResult> Read(Stream stream, bool bufferStream)
        {
            if (bufferStream && stream is not MemoryStream)
            {
                // We need to be buffer the stream, and it's not currently a seekable stream, so copy it to a new memory stream
                _stream = new MemoryStream();
                stream.CopyTo(_stream);
                _stream.Position = 0;
            }
            else if (bufferStream)
            {
                // We need to buffer the stream, and the current stream is seekable, store as is
                _stream = stream;
            }
            else
            {
                // We don't need to buffer the stream, so don't bother keeping the reference
            }

            try
            {
                _document = await JsonDocument.ParseAsync(_stream ?? stream).ConfigureAwait(false);
                IsJson = true;
                return new CallResult(null);
            }
            catch (Exception ex)
            {
                // Not a json message
                IsJson = false;
                return new CallResult(new ServerError("JsonError: " + ex.Message));
            }
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
            _document?.Dispose();
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
        public CallResult Read(ReadOnlyMemory<byte> data)
        {
            _bytes = data;

            try
            {
                var firstByte = data.Span[0];
                if (firstByte != 0x7b && firstByte != 0x5b)
                {
                    // Value doesn't start with `{` or `[`, prevent deserialization attempt as it's slow
                    IsJson = false;
                    return new CallResult(new ServerError("Not a json value"));
                }

                _document = JsonDocument.Parse(data);
                IsJson = true;
                return new CallResult(null);
            }
            catch (Exception ex)
            {
                // Not a json message
                IsJson = false;
                return new CallResult(new ServerError("JsonError: " + ex.Message));
            }
        }

        /// <inheritdoc />
        public override string GetOriginalString() =>
            // Netstandard 2.0 doesn't support GetString from a ReadonlySpan<byte>, so use ToArray there instead
#if NETSTANDARD2_0
            Encoding.UTF8.GetString(_bytes.ToArray());
#else
            Encoding.UTF8.GetString(_bytes.Span);
#endif

        /// <inheritdoc />
        public override bool OriginalDataAvailable => true;

        /// <inheritdoc />
        public override void Clear()
        {
            _bytes = null;
            _document?.Dispose();
            _document = null;
        }
    }
}