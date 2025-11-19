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

        private readonly SearchResult _searchResult = new();

        /// <inheritdoc />
        public virtual string? GetMessageIdentifier(ReadOnlySpan<byte> data, WebSocketMessageType? webSocketMessageType)
        {
            InitializeSearch();

            int? arrayIndex = null;

            _searchResult.Clear();
            var reader = new Utf8JsonReader(data);
            while (reader.Read())
            {
                if ((reader.TokenType == JsonTokenType.StartArray
                    || reader.TokenType == JsonTokenType.StartObject)
                    && reader.CurrentDepth == _maxSearchDepth)
                {
                    // There is no field we need to search for on a depth deeper than this, skip
                    reader.Skip();
                    continue;
                }

                if (reader.TokenType == JsonTokenType.StartArray)
                    arrayIndex = 0;
                else if (reader.TokenType == JsonTokenType.EndArray)
                    arrayIndex = null;
                else if (arrayIndex != null)
                    arrayIndex++;

                if (reader.TokenType == JsonTokenType.PropertyName
                    || arrayIndex != null && _hasArraySearches)
                {
                    bool written = false;
                    foreach (var field in _searchFields)
                    {
                        if (field.Field.Depth != reader.CurrentDepth)
                            continue;

                        if (field.Field is PropertyFieldReference propFieldRef)
                        {
                            if (reader.TokenType != JsonTokenType.PropertyName)
                                continue;

                            if (!reader.ValueTextEquals(propFieldRef.PropertyName))
                                continue;

                            reader.Read();
                        }
                        else if(field.Field is ArrayFieldReference arrayFieldRef)
                        {
                            if (reader.TokenType == JsonTokenType.PropertyName)
                                continue;

                            if (arrayFieldRef.ArrayIndex != arrayIndex)
                                continue;
                        }

                        string? value = null;
                        if (reader.TokenType == JsonTokenType.Number)
                            value = reader.GetDecimal().ToString();
                        else if (reader.TokenType == JsonTokenType.String)
                            value = reader.GetString()!;
                        else if (reader.TokenType == JsonTokenType.Null)
                            value = null;
                        else
                            continue;

                        if (field.Field.Constraint != null
                            && !field.Field.Constraint(value))
                        {
                            continue;
                        }

                        _searchResult.Write(field.Field, value);

                        if (field.ForceEvaluator != null)
                        {
                            // Force the immediate return upon encountering this field
                            return field.ForceEvaluator.MessageIdentifier(_searchResult);
                        }

                        written = true;
                        break;
                    }

                    if (!written)
                        continue;

                    if (_topEvaluator.Statisfied(_searchResult))
                        return _topEvaluator.MessageIdentifier(_searchResult);

                    if (_searchFields.Count == _searchResult.Count)
                        break;
                }
            }

            foreach (var evaluator in MessageEvaluators)
            {
                if (evaluator.Statisfied(_searchResult))
                    return evaluator.MessageIdentifier(_searchResult);
            }

            return null;
        }

        protected bool _hasArraySearches;
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
                    MessageEvalutorFieldReference? existing = null;
                    if (field is ArrayFieldReference arrayField)
                    {
                        _hasArraySearches = true;
                        existing = _searchFields.SingleOrDefault(x =>
                            x.Field is ArrayFieldReference arrayFieldRef
                            && arrayFieldRef.ArrayIndex == arrayFieldRef.ArrayIndex
                            && arrayFieldRef.Depth == arrayFieldRef.Depth);
                    }
                    else if(field is PropertyFieldReference propField)
                    {
                        existing = _searchFields.SingleOrDefault(x => 
                            x.Field is PropertyFieldReference propFieldRef 
                            && propFieldRef.PropertyName == propField.PropertyName 
                            && propFieldRef.Depth == propField.Depth);
                    }

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

                    if (field.Depth > _maxSearchDepth)
                        _maxSearchDepth = field.Depth;
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
