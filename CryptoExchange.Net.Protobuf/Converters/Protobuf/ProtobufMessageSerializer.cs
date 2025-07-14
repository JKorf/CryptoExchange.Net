using CryptoExchange.Net.Interfaces;
using ProtoBuf.Meta;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;

namespace CryptoExchange.Net.Converters.Protobuf
{
    /// <inheritdoc />
    public class ProtobufMessageSerializer : IByteMessageSerializer
    {
        private RuntimeTypeModel _model;

        /// <summary>
        /// ctor
        /// </summary>
        public ProtobufMessageSerializer(RuntimeTypeModel model)
        {
            _model = model;
        }

        /// <inheritdoc />
#if NET5_0_OR_GREATER
        [UnconditionalSuppressMessage("AssemblyLoadTrimming", "IL2092:RequiresUnreferencedCode", Justification = "JsonSerializerOptions provided here has TypeInfoResolver set")]
        [UnconditionalSuppressMessage("AssemblyLoadTrimming", "IL2095:RequiresUnreferencedCode", Justification = "JsonSerializerOptions provided here has TypeInfoResolver set")]
#endif
#if NET5_0_OR_GREATER
        public byte[] Serialize<
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
        T>(T message)
#else
        public byte[] Serialize<T>(T message)
#endif
        {
            using var memoryStream = new MemoryStream();
            _model.Serialize(memoryStream, message);
            return memoryStream.ToArray();
        }
    }
}
