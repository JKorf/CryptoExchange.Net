using CryptoExchange.Net.Converters.MessageParsing.DynamicConverters;
using CryptoExchange.Net.Converters.SystemTextJson;
using CryptoExchange.Net.Converters.SystemTextJson.MessageHandlers;
using System.Text.Json;

namespace CryptoExchange.Net.UnitTests.Implementations
{
    internal class TestSocketMessageHandler : JsonSocketMessageHandler
    {
        public override JsonSerializerOptions Options { get; } = SerializerOptions.WithConverters(new TestSerializerContext());

        public TestSocketMessageHandler()
        {
        }

        protected override MessageTypeDefinition[] TypeEvaluators { get; } = [

             new MessageTypeDefinition {
                ForceIfFound = true,
                Fields = [
                    new PropertyFieldReference("id")
                ],
                TypeIdentifierCallback = (doc) => doc.FieldValue("id")!
            },

            new MessageTypeDefinition {
                Fields = [
                ],
                StaticIdentifier = "test"
            },
        ];
    }
}
