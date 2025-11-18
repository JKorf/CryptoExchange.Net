using CryptoExchange.Net.Converters.MessageParsing;
using CryptoExchange.Net.Converters.MessageParsing.DynamicConverters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text.Json;

namespace CryptoExchange.Net.Converters.SystemTextJson
{
    /// <summary>
    /// JSON message converter
    /// </summary>
    public abstract class DynamicJsonConverter : IMessageConverter
    {
        /// <summary>
        /// The serializer options to use
        /// </summary>
        public abstract JsonSerializerOptions Options { get; }

        protected abstract MessageEvaluator[] MessageEvaluators { get; }

        private readonly SearchResult _searchResult = new SearchResult();

        /// <inheritdoc />
        public virtual MessageInfo GetMessageInfo(ReadOnlySpan<byte> data, WebSocketMessageType? webSocketMessageType)
        {
            InitializeSearch();

            _searchResult.Reset();
            var reader = new Utf8JsonReader(data);
            while (reader.Read())
            {
                if ((reader.TokenType == JsonTokenType.StartArray
                    || reader.TokenType == JsonTokenType.StartObject)
                    && reader.CurrentDepth == _maxSearchDepth)
                {
                    reader.Skip();
                    continue;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                    continue;

                bool written = false;
                foreach (var field in _searchFields)
                {
                    if (reader.CurrentDepth == field.Field.Level
                        && reader.ValueTextEquals(field.Field.Name))
                    {
                        reader.Read();

                        if (field.Field.Type == typeof(int))
                            _searchResult.WriteInt(field.Field.Name, reader.GetInt32());
                        else
                            _searchResult.WriteString(field.Field.Name, reader.GetString()!);

                        if (field.ForceEvaluator != null)
                        {
                            // Force the immediate return upon encountering this field
                            return field.ForceEvaluator.ProduceMessageInfo(_searchResult);
                        }

                        written = true;
                        break;
                    }
                }

                if (!written)
                    continue;

                if (_topEvaluator.Statisfied(_searchResult))
                    return _topEvaluator.ProduceMessageInfo(_searchResult);

                if (_searchFields.All(x => _searchResult.Contains(x.Field.Name)))
                    break;
            }

            foreach (var evaluator in MessageEvaluators)
            {
                if (evaluator.Statisfied(_searchResult))
                    return evaluator.ProduceMessageInfo(_searchResult);
            }

            return new MessageInfo();
        }

        protected bool _initialized;
        protected List<MessageEvalutorFieldReference> _searchFields;
        protected int _maxSearchDepth;
        protected MessageEvaluator _topEvaluator;

        protected void InitializeSearch()
        {
            if (_initialized)
                return;

            _maxSearchDepth = int.MinValue;
            _searchFields = new List<MessageEvalutorFieldReference>();
            foreach (var evaluator in MessageEvaluators.OrderBy(x => x.Priority))
            {
                _topEvaluator ??= evaluator;
                foreach (var field in evaluator.Fields)
                {
                    var existing = _searchFields.SingleOrDefault(x => x.Field.Name == field.Name && x.Field.Level == field.Level);
                    if (existing != null)
                    {
                        if (evaluator.ForceIfFound)
                        {
                            if (existing.ForceEvaluator != null)
                                throw new Exception("Invalid config");

                            existing.ForceEvaluator = evaluator;
                        }
                    }
                    else
                    {
                        _searchFields.Add(new MessageEvalutorFieldReference
                        {
                            ForceEvaluator = evaluator.ForceIfFound ? evaluator : null,
                            Field = field
                        });
                    }

                    if (field.Level > _maxSearchDepth)
                        _maxSearchDepth = field.Level;
                }
            }

            _initialized = true;
        }

        /// <inheritdoc />
        public virtual object Deserialize(ReadOnlySpan<byte> data, Type type)
        {
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
#pragma warning disable IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
            return JsonSerializer.Deserialize(data, type, Options)!;
#pragma warning restore IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
        }
    }
}
