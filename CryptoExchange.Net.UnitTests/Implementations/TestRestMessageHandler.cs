using CryptoExchange.Net.Converters.SystemTextJson;
using CryptoExchange.Net.Converters.SystemTextJson.MessageHandlers;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Errors;
using System.IO;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace CryptoExchange.Net.UnitTests.Implementations
{
    internal class TestRestMessageHandler : JsonRestMessageHandler
    {
        public override JsonSerializerOptions Options { get; } = SerializerOptions.WithConverters(new TestSerializerContext());

        public override async ValueTask<Error> ParseErrorResponse(int httpStatusCode, HttpResponseHeaders responseHeaders, Stream responseStream)
        {
            var (jsonError, jsonDocument) = await GetJsonDocument(responseStream).ConfigureAwait(false);
            if (jsonError != null)
                return jsonError;

            int? code = jsonDocument!.RootElement.TryGetProperty("errorCode", out var codeProp) ? codeProp.GetInt32() : null;
            var msg = jsonDocument.RootElement.TryGetProperty("errorMessage", out var msgProp) ? msgProp.GetString() : null;
            if (msg == null)
                return new ServerError(ErrorInfo.Unknown);

            if (code == null)
                return new ServerError(ErrorInfo.Unknown with { Message = msg });

            return new ServerError(code.Value, new ErrorInfo(ErrorType.Unknown, false, "Error") with { Message = msg });
        }
    }
}
