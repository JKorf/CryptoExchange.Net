using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Testing.Comparers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Testing
{
    /// <summary>
    /// Validator for REST requests, comparing path, http method, authentication and response parsing
    /// </summary>
    /// <typeparam name="TClient">The Rest client</typeparam>
    public class RestRequestValidator<TClient> where TClient : BaseRestClient
    {
        private readonly TClient _client;
        private readonly Func<WebCallResult, bool> _isAuthenticated;
        private readonly string _folder;
        private readonly string _baseAddress;
        private readonly string? _nestedPropertyForCompare;
        private readonly bool _stjCompare;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="client">Client to test</param>
        /// <param name="folder">Folder for json test values</param>
        /// <param name="baseAddress">The base address that is expected</param>
        /// <param name="isAuthenticated">Func for checking if the request is authenticated</param>
        /// <param name="nestedPropertyForCompare">Property to use for compare</param>
        /// <param name="stjCompare">Use System.Text.Json for comparing</param>
        public RestRequestValidator(TClient client, string folder, string baseAddress, Func<WebCallResult, bool> isAuthenticated, string? nestedPropertyForCompare = null, bool stjCompare = true)
        {
            _client = client;
            _folder = folder;
            _baseAddress = baseAddress;
            _nestedPropertyForCompare = nestedPropertyForCompare;
            _isAuthenticated = isAuthenticated;
            _stjCompare = stjCompare;
        }

        /// <summary>
        /// Validate a request
        /// </summary>
        /// <typeparam name="TResponse">Expected response type</typeparam>
        /// <param name="methodInvoke">Method invocation</param>
        /// <param name="name">Method name for looking up json test values</param>
        /// <param name="nestedJsonProperty">Use nested json property for compare</param>
        /// <param name="ignoreProperties">Ignore certain properties</param>
        /// <param name="useSingleArrayItem">Use the first item of an json array response</param>
        /// <param name="skipResponseValidation">Whether to skip the response model validation</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public Task ValidateAsync<TResponse>(
           Func<TClient, Task<WebCallResult<TResponse>>> methodInvoke,
           string name,
           string? nestedJsonProperty = null,
           List<string>? ignoreProperties = null,
           bool useSingleArrayItem = false,
            bool skipResponseValidation = false)
            => ValidateAsync<TResponse, TResponse>(methodInvoke, name, nestedJsonProperty, ignoreProperties, useSingleArrayItem, skipResponseValidation);

        /// <summary>
        /// Validate a request
        /// </summary>
        /// <typeparam name="TResponse">Expected response type</typeparam>
        /// <typeparam name="TActualResponse">The concrete response type</typeparam>
        /// <param name="methodInvoke">Method invocation</param>
        /// <param name="name">Method name for looking up json test values</param>
        /// <param name="nestedJsonProperty">Use nested json property for compare</param>
        /// <param name="ignoreProperties">Ignore certain properties</param>
        /// <param name="useSingleArrayItem">Use the first item of an json array response</param>
        /// <param name="skipResponseValidation">Whether to skip the response model validation</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task ValidateAsync<TResponse, TActualResponse>(
            Func<TClient, Task<WebCallResult<TResponse>>> methodInvoke,
            string name,
            string? nestedJsonProperty = null,
            List<string>? ignoreProperties = null,
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
            var response = reader.ReadToEnd();

            TestHelpers.ConfigureRestClient(_client, response, System.Net.HttpStatusCode.OK);
            var result = await methodInvoke(_client).ConfigureAwait(false);

            // Check request/response properties
            if (result.Error != null)
                throw new Exception(name + " returned error " + result.Error);
            if (_isAuthenticated(result.AsDataless()) != expectedAuth)
                throw new Exception(name + $" authentication not matched. Expected: {expectedAuth}, Actual: {_isAuthenticated(result.AsDataless())}");
            if (result.RequestMethod != new HttpMethod(expectedMethod!))
                throw new Exception(name + $" http method not matched. Expected {expectedMethod}, Actual: {result.RequestMethod}");
            if (expectedPath != result.RequestUrl!.Replace(_baseAddress, "").Split(new char[] { '?' })[0])
                throw new Exception(name + $" path not matched. Expected: {expectedPath}, Actual: {result.RequestUrl!.Replace(_baseAddress, "").Split(new char[] { '?' })[0]}");

            if (!skipResponseValidation)
            {
                // Check response data
                object responseData = (TActualResponse)result.Data!;
                if (_stjCompare == true)
                    SystemTextJsonComparer.CompareData(name, responseData, response, nestedJsonProperty ?? _nestedPropertyForCompare, ignoreProperties, useSingleArrayItem);
                else
                    JsonNetComparer.CompareData(name, responseData, response, nestedJsonProperty ?? _nestedPropertyForCompare, ignoreProperties, useSingleArrayItem);
            }

            Trace.Listeners.Remove(listener);
        }

        /// <summary>
        /// Validate a request
        /// </summary>
        /// <param name="methodInvoke">Method invocation</param>
        /// <param name="name">Method name for looking up json test values</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task ValidateAsync(
            Func<TClient, Task<WebCallResult>> methodInvoke,
            string name)
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
            var response = reader.ReadToEnd();

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

            Trace.Listeners.Remove(listener);
        }
    }
}
