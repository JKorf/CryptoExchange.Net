using CryptoExchange.Net.Interfaces;
using ProtoBuf.Meta;
using System.IO;
using System.Reflection;

namespace CryptoExchange.Net.Converters.Protobuf
{
    /// <inheritdoc />
    public class ProtobufMessageSerializer : IByteMessageSerializer
    {
        private readonly RuntimeTypeModel _model = RuntimeTypeModel.Create("CryptoExchange");

        /// <summary>
        /// ctor
        /// </summary>
        public ProtobufMessageSerializer()
        {
            _model.UseImplicitZeroDefaults = false;
        }

        /// <inheritdoc />
        public byte[] Serialize<T>(T message)
        {
            using var memoryStream = new MemoryStream();
            _model.Serialize(memoryStream, message);
            return memoryStream.ToArray();
        }
    }
}
