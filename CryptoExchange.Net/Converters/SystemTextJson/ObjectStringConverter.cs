using System;
using System.Text.Json.Serialization;
using System.Text.Json;
#if NET5_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace CryptoExchange.Net.Converters.SystemTextJson;

/// <summary>
/// Converter for values which contain a nested json value
/// </summary>
public class ObjectStringConverter<T> : JsonConverter<T>
{
    /// <inheritdoc />
#if NET5_0_OR_GREATER
    [UnconditionalSuppressMessage("AssemblyLoadTrimming", "IL3050:RequiresUnreferencedCode", Justification = "JsonSerializerOptions provided here has TypeInfoResolver set")]
    [UnconditionalSuppressMessage("AssemblyLoadTrimming", "IL2026:RequiresUnreferencedCode", Justification = "JsonSerializerOptions provided here has TypeInfoResolver set")]
#endif
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return default;

        var value = reader.GetString();
        if (string.IsNullOrEmpty(value))
            return default;

        return JsonDocument.Parse(value!).Deserialize<T>(options);
    }

    /// <inheritdoc />
#if NET5_0_OR_GREATER
    [UnconditionalSuppressMessage("AssemblyLoadTrimming", "IL3050:RequiresUnreferencedCode", Justification = "JsonSerializerOptions provided here has TypeInfoResolver set")]
    [UnconditionalSuppressMessage("AssemblyLoadTrimming", "IL2026:RequiresUnreferencedCode", Justification = "JsonSerializerOptions provided here has TypeInfoResolver set")]
#endif
    public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
    {
        if (value is null)
            writer.WriteStringValue("");

        writer.WriteStringValue(JsonSerializer.Serialize(value, options));
    }
}
