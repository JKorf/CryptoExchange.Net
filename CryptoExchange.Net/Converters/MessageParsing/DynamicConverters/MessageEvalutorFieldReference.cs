namespace CryptoExchange.Net.Converters.MessageParsing.DynamicConverters
{
    internal class MessageEvalutorFieldReference
    {
        public bool SkipReading { get; set; }
        public bool OverlappingField { get; set; }
        public MessageFieldReference Field { get; set; }
        public MessageTypeDefinition? ForceEvaluator { get; set; }

        public MessageEvalutorFieldReference(MessageFieldReference field)
        {
            Field = field;
        }
    }
}
