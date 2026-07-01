using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Testing.Comparers;
using CryptoExchange.Net.Testing.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
#pragma warning disable IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.

namespace CryptoExchange.Net.Testing
{
    /// <summary>
    /// Validator for REST requests, comparing path, http method, authentication and response parsing
    /// </summary>
    /// <typeparam name="TClient">The Rest client</typeparam>
    public class RestRequestValidator<TClient> where TClient : BaseRestClient
    {
        private readonly TClient _client;
        private readonly Func<IHttpResult, bool> _isAuthenticated;
        private readonly string _folder;
        private readonly string _baseAddress;
        private readonly string? _nestedPropertyForCompare;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="client">Client to test</param>
        /// <param name="folder">Folder for json test values</param>
        /// <param name="baseAddress">The base address that is expected</param>
        /// <param name="isAuthenticated">Func for checking if the request is authenticated</param>
        /// <param name="nestedPropertyForCompare">Property to use for compare</param>
        public RestRequestValidator(TClient client, string folder, string baseAddress, Func<IHttpResult, bool> isAuthenticated, string? nestedPropertyForCompare = null)
        {
            _client = client;
            _folder = folder;
            _baseAddress = baseAddress;
            _nestedPropertyForCompare = nestedPropertyForCompare;
            _isAuthenticated = isAuthenticated;
        }

        /// <summary>
        /// Validate a request
        /// </summary>
        /// <typeparam name="TResponse">Expected response type</typeparam>
        /// <param name="methodInvoke">Method invocation</param>
        /// <param name="name">Method name for looking up json test values</param>
        /// <param name="nestedJsonProperty">Use nested json property for compare</param>
        /// <param name="ignoreProperties">Ignore certain properties</param>
        /// <param name="ignoreParamValidation">Ignore certain parameter validation</param>
        /// <param name="useSingleArrayItem">Use the first item of an json array response</param>
        /// <param name="skipResponseValidation">Whether to skip the response model validation</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public Task ValidateAsync<TResponse>(
           Func<TClient, Task<HttpResult<TResponse>>> methodInvoke,
           string name,
           string? nestedJsonProperty = null,
           List<string>? ignoreProperties = null,
            List<string>? ignoreParamValidation = null,
           bool useSingleArrayItem = false,
           bool skipResponseValidation = false)
            => ValidateAsync<TResponse, TResponse>(methodInvoke, name, nestedJsonProperty, ignoreProperties, ignoreParamValidation, useSingleArrayItem, skipResponseValidation);

        /// <summary>
        /// Validate a request
        /// </summary>
        /// <typeparam name="TResponse">Expected response type</typeparam>
        /// <typeparam name="TActualResponse">The concrete response type</typeparam>
        /// <param name="methodInvoke">Method invocation</param>
        /// <param name="name">Method name for looking up json test values</param>
        /// <param name="nestedJsonProperty">Use nested json property for compare</param>
        /// <param name="ignoreProperties">Ignore certain properties</param>
        /// <param name="ignoreParamValidation">Ignore certain parameter validation</param>
        /// <param name="useSingleArrayItem">Use the first item of an json array response</param>
        /// <param name="skipResponseValidation">Whether to skip the response model validation</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task ValidateAsync<TResponse, TActualResponse>(
            Func<TClient, Task<HttpResult<TResponse>>> methodInvoke,
            string name,
            string? nestedJsonProperty = null,
            List<string>? ignoreProperties = null,
            List<string>? ignoreParamValidation = null,
            bool useSingleArrayItem = false,
            bool skipResponseValidation = false) where TActualResponse : TResponse
        {
            var listener = new EnumValueTraceListener();
            Trace.Listeners.Add(listener);
            
            var path = Directory.GetParent(Environment.CurrentDirectory)!.Parent!.Parent!.FullName;
            FileStream file;
            try
            {
                file = File.OpenRead(Path.Combine(path, _folder, $"{name}.txt"));
            }
            catch (FileNotFoundException)
            {
                throw new Exception($"Response file not found for {name}: {path}");
            }

            var buffer = new byte[file.Length];
            await file.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
            file.Close();

            var data = Encoding.UTF8.GetString(buffer);
            using var reader = new StringReader(data);
            var expectedMethod = reader.ReadLine();
            var expectedPath = reader.ReadLine();
            var expectedAuth = bool.Parse(reader.ReadLine()!);
            var paramsAndResponseBody = reader.ReadToEnd();
            var paramsAndResponseBodySplit = paramsAndResponseBody.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            var uriParamsLine = paramsAndResponseBodySplit.FirstOrDefault(x => x.StartsWith("UriParams: "));
            Dictionary<string, object>? expectedUriParams = null;
            var bodyParamsLine = paramsAndResponseBodySplit.FirstOrDefault(x => x.StartsWith("BodyParams: "));
            Dictionary<string, object>? expectedBodyParams = null;
            var response = string.Join("\r\n", paramsAndResponseBodySplit.Where(x => !x.StartsWith("UriParams: ") && !x.StartsWith("BodyParams: ")));

            if (uriParamsLine != null)
            {
                var expectedUriParamsJson = uriParamsLine.Substring(11);
                expectedUriParams = JsonSerializer.Deserialize<Dictionary<string, object>>(expectedUriParamsJson)!;
            }

            if (bodyParamsLine != null)
            {
                var expectedBodyParamsJson = bodyParamsLine.Substring(12);
                expectedBodyParams = JsonSerializer.Deserialize<Dictionary<string, object>>(expectedBodyParamsJson)!;
            }
            TestHelpers.ConfigureRestClient(_client, response, System.Net.HttpStatusCode.OK);
            var result = await methodInvoke(_client).ConfigureAwait(false);

            // Check request/response properties
            if (result.Error != null)
                throw new Exception(name + " returned error " + result.Error);
            if (_isAuthenticated(result) != expectedAuth)
                throw new Exception(name + $" authentication not matched. Expected: {expectedAuth}, Actual: {_isAuthenticated(result)}");
            if (result.RequestMethod != new HttpMethod(expectedMethod!))
                throw new Exception(name + $" http method not matched. Expected {expectedMethod}, Actual: {result.RequestMethod}");
            if (expectedPath != result.RequestUrl!.Replace(_baseAddress, "").Split(new char[] { '?' })[0])
                throw new Exception(name + $" path not matched. Expected: {expectedPath}, Actual: {result.RequestUrl!.Replace(_baseAddress, "").Split(new char[] { '?' })[0]}");

            if (expectedUriParams != null)
            {
                // Validate request parameters
                var urlParamsSplit = result.RequestUrl!.Split(new char[] { '?' });
                var urlParametersString = urlParamsSplit.Length > 1 ? urlParamsSplit[1] : null;
                var urlParameters = (urlParametersString != null
                    ? urlParametersString.Split('&').ToDictionary(x => x.Split('=')[0], x => (object)x.Split('=')[1])
                    : new());

                CompareParameters(expectedUriParams, urlParameters, ignoreParamValidation);
            }

            if (expectedBodyParams != null)
            {
                // Validate request body
                Dictionary<string, object> bodyParameters;
                if (result.RequestBody!.StartsWith("{") || result.RequestBody.StartsWith("["))
                {
                    bodyParameters = JsonSerializer.Deserialize<Dictionary<string, object>>(result.RequestBody!)!;
                }
                else
                {
                    var splitKvp = result.RequestBody.Split('&');
                    bodyParameters = splitKvp.Select(x => x.Split('=')).ToDictionary(x => x[0], x => (object)x[1]);
                }

                CompareParameters(expectedBodyParams, bodyParameters, ignoreParamValidation);
            }

            if (!skipResponseValidation)
            {
                // Check response data
                object responseData = (TActualResponse)result.Data!;
                var issues = SystemTextJsonComparer.CompareData(name, responseData, response, nestedJsonProperty ?? _nestedPropertyForCompare, ignoreProperties, useSingleArrayItem);
                if (issues.Count > 0)
                    throw new AggregateException(issues);
            }

            Trace.Listeners.Remove(listener);
        }

        private void CompareParameters(Dictionary<string, object> expectedUrlParameters, Dictionary<string, object> parameters, List<string>? ignoreParamValidation)
        {
            if (expectedUrlParameters.Count > parameters.Count)
                throw new Exception($"Url parameters count not matched. Expected: {expectedUrlParameters.Count}, Actual: {parameters.Count}");

            foreach (var kvp in expectedUrlParameters)
            {
                if (ignoreParamValidation != null && ignoreParamValidation.Contains(kvp.Key))
                    continue;

                if (!parameters.TryGetValue(kvp.Key, out var value))
                    throw new Exception($"Url parameter {kvp.Key} not found in actual parameters");

                if (Convert.ToString(kvp.Value, CultureInfo.InvariantCulture) != Convert.ToString(value, CultureInfo.InvariantCulture))
                    throw new Exception($"Url parameter {kvp.Key} value not matched. Expected: {kvp.Value}, Actual: {value}");
            }
        }

        /// <summary>
        /// Validate a request
        /// </summary>
        /// <param name="methodInvoke">Method invocation</param>
        /// <param name="name">Method name for looking up json test values</param>
        /// <param name="ignoreParamValidation">Ignore certain parameter validation</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task ValidateAsync(
            Func<TClient, Task<HttpResult>> methodInvoke,
            string name,
            List<string>? ignoreParamValidation = null
            )
        {
            var listener = new EnumValueTraceListener();
            Trace.Listeners.Add(listener);

            var path = Directory.GetParent(Environment.CurrentDirectory)!.Parent!.Parent!.FullName;
            FileStream file;
            try
            {
                file = File.OpenRead(Path.Combine(path, _folder, $"{name}.txt"));
            }
            catch (FileNotFoundException)
            {
                throw new Exception($"Response file not found for {name}: {path}");
            }

            var buffer = new byte[file.Length];
            await file.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
            file.Close();

            var data = Encoding.UTF8.GetString(buffer);
            using var reader = new StringReader(data);
            var expectedMethod = reader.ReadLine();
            var expectedPath = reader.ReadLine();
            var expectedAuth = bool.Parse(reader.ReadLine()!);
            var paramsAndResponseBody = reader.ReadToEnd();
            var paramsAndResponseBodySplit = paramsAndResponseBody.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            var uriParamsLine = paramsAndResponseBodySplit.FirstOrDefault(x => x.StartsWith("UriParams: "));
            Dictionary<string, object>? expectedUriParams = null;
            var bodyParamsLine = paramsAndResponseBodySplit.FirstOrDefault(x => x.StartsWith("BodyParams: "));
            Dictionary<string, object>? expectedBodyParams = null;
            var response = string.Join("\r\n", paramsAndResponseBodySplit.Where(x => !x.StartsWith("UriParams: ") && !x.StartsWith("BodyParams: ")));

            if (uriParamsLine != null)
            {
                var expectedUriParamsJson = uriParamsLine.Substring(11);
                expectedUriParams = JsonSerializer.Deserialize<Dictionary<string, object>>(expectedUriParamsJson)!;
            }

            if (bodyParamsLine != null)
            {
                var expectedBodyParamsJson = bodyParamsLine.Substring(12);
                expectedBodyParams = JsonSerializer.Deserialize<Dictionary<string, object>>(expectedBodyParamsJson)!;
            }

            TestHelpers.ConfigureRestClient(_client, response, System.Net.HttpStatusCode.OK);
            var result = await methodInvoke(_client).ConfigureAwait(false);

            // Check request/response properties
            if (result.Error != null)
                throw new Exception(name + " returned error " + result.Error);
            if (_isAuthenticated(result) != expectedAuth)
                throw new Exception(name + $" authentication not matched. Expected: {expectedAuth}, Actual: {_isAuthenticated(result)}");
            if (result.RequestMethod != new HttpMethod(expectedMethod!))
                throw new Exception(name + $" http method not matched. Expected {expectedMethod}, Actual: {result.RequestMethod}");
            if (expectedPath != result.RequestUrl!.Replace(_baseAddress, "").Split(new char[] { '?' })[0])
                throw new Exception(name + $" path not matched. Expected: {expectedPath}, Actual: {result.RequestUrl!.Replace(_baseAddress, "").Split(new char[] { '?' })[0]}");

            if (expectedUriParams != null)
            {
                // Validate request parameters
                var urlParamsSplit = result.RequestUrl!.Split(new char[] { '?' });
                var urlParametersString = urlParamsSplit.Length > 1 ? urlParamsSplit[1] : null;
                var urlParameters = (urlParametersString != null
                    ? urlParametersString.Split('&').ToDictionary(x => x.Split('=')[0], x => (object)x.Split('=')[1])
                    : new());

                CompareParameters(expectedUriParams, urlParameters, ignoreParamValidation);
            }

            if (expectedBodyParams != null)
            {
                // Validate request body
                Dictionary<string, object> bodyParameters;
                if (result.RequestBody!.StartsWith("{") || result.RequestBody.StartsWith("["))
                {
                    bodyParameters = JsonSerializer.Deserialize<Dictionary<string, object>>(result.RequestBody!)!;
                }
                else
                {
                    var splitKvp = result.RequestBody.Split('&');
                    bodyParameters = splitKvp.Select(x => x.Split('=')).ToDictionary(x => x[0], x => (object)x[1]);
                }

                CompareParameters(expectedBodyParams, bodyParameters, ignoreParamValidation);
            }

            Trace.Listeners.Remove(listener);
        }
    }
}
