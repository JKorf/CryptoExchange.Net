using CryptoExchange.Net.Converters.MessageParsing.DynamicConverters;
using CryptoExchange.Net.Converters.SystemTextJson.MessageHandlers;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Errors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.UnitTests.TestImplementations
{
    internal class TestRestMessageHandler : JsonRestMessageHandler
    {
        private ErrorMapping _errorMapping;
        public override JsonSerializerOptions Options => new JsonSerializerOptions();

        public override ValueTask<Error> ParseErrorResponse(int httpStatusCode, HttpResponseHeaders responseHeaders, Stream responseStream)
        {
            var errorData = JsonSerializer.Deserialize<TestError>(responseStream);

            return new ValueTask<Error>(new ServerError(errorData.ErrorCode, _errorMapping.GetErrorInfo(errorData.ErrorCode.ToString(), errorData.ErrorMessage)));
        }
    }
}
