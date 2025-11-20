using CryptoExchange.Net.Converters.MessageParsing.DynamicConverters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text.Json;

namespace CryptoExchange.Net.Converters.SystemTextJson
{
    /// <summary>
    /// JSON message converter, sequentially read the json and looks for specific prefdefined fields to identify the message
    /// </summary>
    public abstract class DynamicJsonConverter : IMessageConverter
    {
        /// <summary>
        /// The serializer options to use
        /// </summary>
        public abstract JsonSerializerOptions Options { get; }

        /// <summary>
        /// Message evaluators
        /// </summary>
        protected abstract MessageEvaluator[] MessageEvaluators { get; }

        private readonly SearchResult _searchResult = new();

        private bool _hasArraySearches;
        private bool _initialized;
        private int _maxSearchDepth;
        private bool _overlappingFields;
        private MessageEvaluator? _topEvaluator;
        private List<MessageEvalutorFieldReference>? _searchFields;

        private void InitializeConverter()
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
                    if (MessageEvaluators.Where(x => x != evaluator).SelectMany(x => x.Fields).Any(otherField =>
                    {
                        if (field is PropertyFieldReference propRef
                            && otherField is PropertyFieldReference otherPropRef)
                        {
                            return field.Depth == otherPropRef.Depth && propRef.PropertyName.SequenceEqual(otherPropRef.PropertyName);
                        }
                        else if (field is ArrayFieldReference arrayRef
                            && otherField is ArrayFieldReference otherArrayPropRef)
                        {
                            return field.Depth == otherArrayPropRef.Depth && arrayRef.ArrayIndex == otherArrayPropRef.ArrayIndex;
                        }

                        return false;
                    }))
                    {
                        _overlappingFields = true;
                    }

                    MessageEvalutorFieldReference? existingSameSearchField = null;
                    if (field is ArrayFieldReference arrayField)
                    {
                        _hasArraySearches = true;
                        existingSameSearchField = _searchFields.SingleOrDefault(x =>
                            x.Field is ArrayFieldReference arrayFieldRef
                            && arrayFieldRef.ArrayIndex == arrayField.ArrayIndex
                            && arrayFieldRef.Depth == arrayField.Depth
                            && (arrayFieldRef.Constraint == null && arrayField.Constraint == null));
                    }
                    else if (field is PropertyFieldReference propField)
                    {
                        existingSameSearchField = _searchFields.SingleOrDefault(x =>
                            x.Field is PropertyFieldReference propFieldRef
                            && propFieldRef.PropertyName.SequenceEqual(propField.PropertyName)
                            && propFieldRef.Depth == propField.Depth
                            && (propFieldRef.Constraint == null && propFieldRef.Constraint == null));
                    }

                    if (existingSameSearchField != null)
                    {
                        if (existingSameSearchField.SkipReading == true
                            && (evaluator.IdentifyMessageCallback != null
                                || field.Constraint != null))
                        {
                            existingSameSearchField.SkipReading = false;
                        }

                        if (evaluator.ForceIfFound)
                        {
                            if (evaluator.Fields.Length > 1 || existingSameSearchField.ForceEvaluator != null)
                                throw new Exception("Invalid config");

                            existingSameSearchField.ForceEvaluator = evaluator;
                        }
                    }

                    _searchFields.Add(new MessageEvalutorFieldReference
                    {
                        SkipReading = evaluator.IdentifyMessageCallback == null && field.Constraint == null,
                        ForceEvaluator = evaluator.ForceIfFound ? evaluator : null,
                        Field = field
                    });                    

                    if (field.Depth > _maxSearchDepth)
                        _maxSearchDepth = field.Depth;
                }
            }

            _initialized = true;
        }

        /// <inheritdoc />
        public virtual string? GetMessageIdentifier(ReadOnlySpan<byte> data, WebSocketMessageType? webSocketMessageType)
        {
            InitializeConverter();

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
                    arrayIndex = -1;
                else if (reader.TokenType == JsonTokenType.EndArray)
                    arrayIndex = null;
                else if (arrayIndex != null)
                    arrayIndex++;

                if (reader.TokenType == JsonTokenType.PropertyName
                    || arrayIndex != null && _hasArraySearches)
                {
                    bool written = false;

                    string? value = null;
                    byte[]? propName = null;
                    foreach (var field in _searchFields!)
                    {
                        if (field.Field.Depth != reader.CurrentDepth)
                            continue;

                        if (field.Field is PropertyFieldReference propFieldRef)
                        {
                            if (propName == null)
                            {
                                if (reader.TokenType != JsonTokenType.PropertyName)
                                    continue;

                                if (!reader.ValueTextEquals(propFieldRef.PropertyName))
                                    continue;

                                propName = propFieldRef.PropertyName;
                                reader.Read();
                            }
                            else if (!propFieldRef.PropertyName.SequenceEqual(propName))
                            {
                                continue;
                            }
                        }
                        else if (field.Field is ArrayFieldReference arrayFieldRef)
                        {
                            if (propName != null)
                                continue;

                            if (reader.TokenType == JsonTokenType.PropertyName)
                                continue;

                            if (arrayFieldRef.ArrayIndex != arrayIndex)
                                continue;
                        }

                        if (!field.SkipReading)
                        {
                            if (value == null)
                            {
                                if (reader.TokenType == JsonTokenType.Number)
                                    value = reader.GetDecimal().ToString();
                                else if (reader.TokenType == JsonTokenType.String)
                                    value = reader.GetString()!;
                                else if (reader.TokenType == JsonTokenType.Null)
                                    value = null;
                                else
                                    continue;
                            }

                            if (field.Field.Constraint != null
                                && !field.Field.Constraint(value))
                            {
                                continue;
                            }
                        }

                        _searchResult.Write(field.Field, value);

                        if (field.ForceEvaluator != null)
                        {
                            if (field.ForceEvaluator.StaticIdentifier != null)
                                return field.ForceEvaluator.StaticIdentifier;

                            // Force the immediate return upon encountering this field
                            return field.ForceEvaluator.IdentifyMessage(_searchResult);
                        }

                        written = true;
                        if (!_overlappingFields)
                            break;
                    }

                    if (!written)
                        continue;

                    if (_topEvaluator!.Statisfied(_searchResult))
                        return _topEvaluator.IdentifyMessage(_searchResult);

                    if (_searchFields.Count == _searchResult.Count)
                        break;
                }
            }

            foreach (var evaluator in MessageEvaluators)
            {
                if (evaluator.Statisfied(_searchResult))
                    return evaluator.IdentifyMessage(_searchResult);
            }

            return null;
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
