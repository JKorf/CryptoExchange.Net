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
        private ErrorMapping _errorMapping = new ErrorMapping([]);
        public override JsonSerializerOptions Options => new JsonSerializerOptions();

        public override async ValueTask<Error> ParseErrorResponse(int httpStatusCode, HttpResponseHeaders responseHeaders, Stream responseStream)
        {
            var result = await GetJsonDocument(responseStream).ConfigureAwait(false);
            if (result.Item1 != null)
                return result.Item1;

            var errorData = result.Item2.Deserialize<TestError>();
            return new ServerError(errorData.ErrorCode, _errorMapping.GetErrorInfo(errorData.ErrorCode.ToString(), errorData.ErrorMessage));
        }
    }
}
