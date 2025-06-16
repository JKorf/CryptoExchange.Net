using CryptoExchange.Net.Converters.MessageParsing;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using ProtoBuf;
using ProtoBuf.Meta;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Converters.Protobuf
{
    /// <summary>
    /// System.Text.Json message accessor
    /// </summary>
#if NET5_0_OR_GREATER
    public abstract class ProtobufMessageAccessor<
        [DynamicallyAccessedMembers(
#if NET8_0_OR_GREATER
        DynamicallyAccessedMemberTypes.NonPublicConstructors |
        DynamicallyAccessedMemberTypes.PublicFields |
        DynamicallyAccessedMemberTypes.NonPublicFields |
        DynamicallyAccessedMemberTypes.NonPublicNestedTypes |
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.NonPublicProperties |
#endif
        DynamicallyAccessedMemberTypes.PublicNestedTypes |
        DynamicallyAccessedMemberTypes.NonPublicMethods |
        DynamicallyAccessedMemberTypes.PublicMethods
        )]
    TIntermediateType> : IMessageAccessor
#else
    public abstract class ProtobufMessageAccessor<TIntermediateType> : IMessageAccessor
#endif
    {
        /// <summary>
        /// The intermediate deserialization object
        /// </summary>
        protected TIntermediateType? _intermediateType;

        /// <inheritdoc />
        public bool IsValid { get; set; }

        /// <inheritdoc />
        public abstract bool OriginalDataAvailable { get; }

        /// <inheritdoc />
        public object? Underlying => throw new NotImplementedException();

        /// <summary>
        /// ctor
        /// </summary>
        public ProtobufMessageAccessor()
        {
        }

        /// <inheritdoc />
        public NodeType? GetNodeType()
        {
            throw new Exception("");
        }

        /// <inheritdoc />
        public NodeType? GetNodeType(MessagePath path)
        {
            if (_intermediateType == null)
                throw new InvalidOperationException("Data not read");

            object? value = _intermediateType;
            foreach (var step in path)
            {
                if (value == null)
                    break;

                if (step.Type == 0)
                {
                    // array index
                }
                else if (step.Type == 1)
                {
                    // property value
                    value = value.GetType().GetProperty(step.Property!)?.GetValue(value);
                }
                else
                {
                    // property name
                }
            }

            if (value == null)
                return null;

            var valueType = value.GetType();
            if (valueType.IsArray)
                return NodeType.Array;

            if (IsSimple(valueType))
                return NodeType.Value;

            return NodeType.Object;
        }

        private static bool IsSimple(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // nullable type, check if the nested type is simple.
                return IsSimple(type.GetGenericArguments()[0]);
            }
            return type.IsPrimitive
              || type.IsEnum
              || type == typeof(string)
              || type == typeof(decimal);
        }


        /// <inheritdoc />
        public T? GetValue<T>(MessagePath path)
        {
            if (_intermediateType == null)
                throw new InvalidOperationException("Data not read");

            object? value = _intermediateType;
            foreach(var step in path)
            {
                if (value == null)
                    break;

                if (step.Type == 0)
                {
                    // array index
                }
                else if (step.Type == 1)
                {
                    // property value
                    value = value.GetType().GetProperty(step.Property!)?.GetValue(value);
                }
                else
                {
                    // property name
                }
            }

            return (T?)value;
        }

        /// <inheritdoc />
        public T?[]? GetValues<T>(MessagePath path)
        {
            throw new Exception("");

        }

        /// <inheritdoc />
        public abstract string GetOriginalString();

        /// <inheritdoc />
        public abstract void Clear();

        /// <inheritdoc />
        public abstract CallResult<object> Deserialize(
#if NET5_0_OR_GREATER
            [DynamicallyAccessedMembers(
#if NET8_0_OR_GREATER
            DynamicallyAccessedMemberTypes.NonPublicConstructors |
            DynamicallyAccessedMemberTypes.PublicFields |
            DynamicallyAccessedMemberTypes.NonPublicFields |
            DynamicallyAccessedMemberTypes.NonPublicNestedTypes |
            DynamicallyAccessedMemberTypes.PublicProperties |
            DynamicallyAccessedMemberTypes.NonPublicProperties |
            DynamicallyAccessedMemberTypes.PublicConstructors |
#endif
            DynamicallyAccessedMemberTypes.PublicNestedTypes |
            DynamicallyAccessedMemberTypes.NonPublicMethods |
            DynamicallyAccessedMemberTypes.PublicMethods
            )]
#endif
        Type type, MessagePath? path = null);

        /// <inheritdoc />
        public abstract CallResult<T> Deserialize<

#if NET5_0_OR_GREATER
            [DynamicallyAccessedMembers(
#if NET8_0_OR_GREATER
            DynamicallyAccessedMemberTypes.NonPublicConstructors |
            DynamicallyAccessedMemberTypes.PublicFields |
            DynamicallyAccessedMemberTypes.NonPublicFields |
            DynamicallyAccessedMemberTypes.NonPublicNestedTypes |
            DynamicallyAccessedMemberTypes.PublicProperties |
            DynamicallyAccessedMemberTypes.NonPublicProperties |
            DynamicallyAccessedMemberTypes.PublicConstructors |
#endif
            DynamicallyAccessedMemberTypes.PublicNestedTypes |
            DynamicallyAccessedMemberTypes.NonPublicMethods |
            DynamicallyAccessedMemberTypes.PublicMethods
            )]
#endif
        T>(MessagePath? path = null);
    }

    /// <summary>
    /// System.Text.Json stream message accessor
    /// </summary>
    public class ProtobufStreamMessageAccessor<
#if NET5_0_OR_GREATER
            [DynamicallyAccessedMembers(
#if NET8_0_OR_GREATER
            DynamicallyAccessedMemberTypes.NonPublicConstructors |
            DynamicallyAccessedMemberTypes.PublicFields |
            DynamicallyAccessedMemberTypes.NonPublicFields |
            DynamicallyAccessedMemberTypes.NonPublicNestedTypes |
            DynamicallyAccessedMemberTypes.PublicProperties |
            DynamicallyAccessedMemberTypes.NonPublicProperties |
            DynamicallyAccessedMemberTypes.PublicConstructors |
#endif
            DynamicallyAccessedMemberTypes.PublicNestedTypes |
            DynamicallyAccessedMemberTypes.NonPublicMethods |
            DynamicallyAccessedMemberTypes.PublicMethods
            )]
#endif
    TIntermediate> : ProtobufMessageAccessor<TIntermediate>, IStreamMessageAccessor
    {
        private Stream? _stream;

        /// <inheritdoc />
        public override bool OriginalDataAvailable => _stream?.CanSeek == true;

        /// <summary>
        /// ctor
        /// </summary>
        public ProtobufStreamMessageAccessor(): base()
        {
        }

        /// <inheritdoc />
        public override CallResult<object> Deserialize(
#if NET5_0_OR_GREATER
            [DynamicallyAccessedMembers(
#if NET8_0_OR_GREATER
            DynamicallyAccessedMemberTypes.NonPublicConstructors |
            DynamicallyAccessedMemberTypes.PublicFields |
            DynamicallyAccessedMemberTypes.NonPublicFields |
            DynamicallyAccessedMemberTypes.NonPublicNestedTypes |
            DynamicallyAccessedMemberTypes.PublicProperties |
            DynamicallyAccessedMemberTypes.NonPublicProperties |
            DynamicallyAccessedMemberTypes.PublicConstructors |
#endif
            DynamicallyAccessedMemberTypes.PublicNestedTypes |
            DynamicallyAccessedMemberTypes.NonPublicMethods |
            DynamicallyAccessedMemberTypes.PublicMethods
            )]
#endif
        Type type, MessagePath? path = null)
        {
            try
            {
                var result = Serializer.Deserialize(type, _stream);
                return new CallResult<object>(result);
            }
            catch (Exception ex)
            {
                return new CallResult<object>(new DeserializeError(ex.Message));
            }
        }

        /// <inheritdoc />
        public override CallResult<T> Deserialize<            
#if NET5_0_OR_GREATER
            [DynamicallyAccessedMembers(
#if NET8_0_OR_GREATER
            DynamicallyAccessedMemberTypes.NonPublicConstructors |
            DynamicallyAccessedMemberTypes.PublicFields |
            DynamicallyAccessedMemberTypes.NonPublicFields |
            DynamicallyAccessedMemberTypes.NonPublicNestedTypes |
            DynamicallyAccessedMemberTypes.PublicProperties |
            DynamicallyAccessedMemberTypes.NonPublicProperties |
            DynamicallyAccessedMemberTypes.PublicConstructors |
#endif
            DynamicallyAccessedMemberTypes.PublicNestedTypes |
            DynamicallyAccessedMemberTypes.NonPublicMethods |
            DynamicallyAccessedMemberTypes.PublicMethods
            )]
#endif
        T>(MessagePath? path = null)
        {
            try
            {
                var result = Serializer.Deserialize<T>(_stream);
                return new CallResult<T>(result);
            }
            catch(Exception ex)
            {
                return new CallResult<T>(new DeserializeError(ex.Message));
            }
        }

        /// <inheritdoc />
        public Task<CallResult> Read(Stream stream, bool bufferStream)
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
                _intermediateType = Serializer.Deserialize<TIntermediate>(_stream);
                IsValid = true;
                return Task.FromResult(CallResult.SuccessResult);
            }
            catch (Exception ex)
            {
                // Not a json message
                IsValid = false;
                return Task.FromResult(new CallResult(new DeserializeError("JsonError: " + ex.Message, ex)));
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
        }

    }

    /// <summary>
    /// Protobuf byte message accessor
    /// </summary>
    public class ProtobufByteMessageAccessor<
#if NET5_0_OR_GREATER
            [DynamicallyAccessedMembers(
#if NET8_0_OR_GREATER
            DynamicallyAccessedMemberTypes.NonPublicConstructors |
            DynamicallyAccessedMemberTypes.PublicFields |
            DynamicallyAccessedMemberTypes.NonPublicFields |
            DynamicallyAccessedMemberTypes.NonPublicNestedTypes |
            DynamicallyAccessedMemberTypes.PublicProperties |
            DynamicallyAccessedMemberTypes.NonPublicProperties |
            DynamicallyAccessedMemberTypes.PublicConstructors |
#endif
            DynamicallyAccessedMemberTypes.PublicNestedTypes |
            DynamicallyAccessedMemberTypes.NonPublicMethods |
            DynamicallyAccessedMemberTypes.PublicMethods
            )]
#endif
    TIntermediate> : ProtobufMessageAccessor<TIntermediate>, IByteMessageAccessor
    {
        private ReadOnlyMemory<byte> _bytes;

        /// <summary>
        /// ctor
        /// </summary>
        public ProtobufByteMessageAccessor() : base()
        {
        }

        /// <inheritdoc />
        public override CallResult<object> Deserialize(
#if NET5_0_OR_GREATER
            [DynamicallyAccessedMembers(
#if NET8_0_OR_GREATER
            DynamicallyAccessedMemberTypes.NonPublicConstructors |
            DynamicallyAccessedMemberTypes.PublicFields |
            DynamicallyAccessedMemberTypes.NonPublicFields |
            DynamicallyAccessedMemberTypes.NonPublicNestedTypes |
            DynamicallyAccessedMemberTypes.PublicProperties |
            DynamicallyAccessedMemberTypes.NonPublicProperties |
            DynamicallyAccessedMemberTypes.PublicConstructors |
#endif
            DynamicallyAccessedMemberTypes.PublicNestedTypes |
            DynamicallyAccessedMemberTypes.NonPublicMethods |
            DynamicallyAccessedMemberTypes.PublicMethods
            )]
#endif
        Type type, MessagePath? path = null)
        {
            try
            {
                using var stream = new MemoryStream(_bytes.ToArray());
                stream.Position = 0;
                var result = Serializer.Deserialize(type, stream);
                return new CallResult<object>(result);
            }
            catch (Exception ex)
            {
                return new CallResult<object>(new DeserializeError(ex.Message));
            }
        }

        /// <inheritdoc />
#if NET5_0_OR_GREATER
        public override CallResult<T> Deserialize<
            [DynamicallyAccessedMembers(
#if NET8_0_OR_GREATER
            DynamicallyAccessedMemberTypes.NonPublicConstructors |
            DynamicallyAccessedMemberTypes.PublicFields |
            DynamicallyAccessedMemberTypes.NonPublicFields |
            DynamicallyAccessedMemberTypes.NonPublicNestedTypes |
            DynamicallyAccessedMemberTypes.PublicProperties |
            DynamicallyAccessedMemberTypes.NonPublicProperties |
            DynamicallyAccessedMemberTypes.PublicConstructors |
#endif
            DynamicallyAccessedMemberTypes.PublicNestedTypes |
            DynamicallyAccessedMemberTypes.NonPublicMethods |
            DynamicallyAccessedMemberTypes.PublicMethods
            )]
        T>(MessagePath? path = null)
#else
        public override CallResult<T> Deserialize<T>(MessagePath? path = null)
#endif
        {
            try
            {
                var result = Serializer.Deserialize<T>(_bytes);
                return new CallResult<T>(result);
            }
            catch (Exception ex)
            {
                return new CallResult<T>(new DeserializeError(ex.Message));
            }
        }

        /// <inheritdoc />
        public CallResult Read(ReadOnlyMemory<byte> data)
        {
            _bytes = data;

            try
            {
                _intermediateType = Serializer.Deserialize<TIntermediate>(data);
                IsValid = true;
                return CallResult.SuccessResult;
            }
            catch (Exception ex)
            {
                // Not a json message
                IsValid = false;
                return new CallResult(new DeserializeError("JsonError: " + ex.Message, ex));
            }
        }

        /// <inheritdoc />
        public override string GetOriginalString() =>
            // NetStandard 2.0 doesn't support GetString from a ReadonlySpan<byte>, so use ToArray there instead
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
        }
    }
}