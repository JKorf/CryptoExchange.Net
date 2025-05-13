using CryptoExchange.Net.Interfaces;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace CryptoExchange.Net.Converters.SystemTextJson
{
    /// <inheritdoc />
    public class SystemTextJsonMessageSerializer : IMessageSerializer
    {
        private readonly JsonSerializerOptions _options;

        /// <summary>
        /// ctor
        /// </summary>
        public SystemTextJsonMessageSerializer(JsonSerializerOptions options)
        {
            _options = options;
        }

        /// <inheritdoc />
#if NET5_0_OR_GREATER
        [UnconditionalSuppressMessage("AssemblyLoadTrimming", "IL2026:RequiresUnreferencedCode", Justification = "Everything referenced in the loaded assembly is manually preserved, so it's safe")]
        [UnconditionalSuppressMessage("AssemblyLoadTrimming", "IL3050:RequiresUnreferencedCode", Justification = "Everything referenced in the loaded assembly is manually preserved, so it's safe")]
#endif
        public string Serialize<T>(T message) => JsonSerializer.Serialize(message, _options);
    }
}
