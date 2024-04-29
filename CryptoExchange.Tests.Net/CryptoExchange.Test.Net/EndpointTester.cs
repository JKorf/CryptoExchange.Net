using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Converters;
using CryptoExchange.Net.Converters.JsonNet;
using CryptoExchange.Net.Objects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace CryptoExchange.Test.Net
{
    public class EndpointTester<TClient> where TClient : BaseRestClient
    {
        private readonly TClient _client;
        private readonly Func<WebCallResult, bool> _isAuthenticated;
        private readonly string _folder;
        private readonly string _baseAddress;
        private readonly string? _nestedPropertyForCompare;
        private readonly bool _stjCompare;

        public EndpointTester(TClient client, string folder, string baseAddress, Func<WebCallResult, bool> isAuthenticated, string? nestedPropertyForCompare = null, bool stjCompare = true)
        {
            _client = client;
            _folder = folder;
            _baseAddress = baseAddress;
            _nestedPropertyForCompare = nestedPropertyForCompare;
            _isAuthenticated = isAuthenticated;
            _stjCompare = stjCompare;
        }

        public async Task ValidateAsync<TResponse>(
            Func<TClient, Task<WebCallResult<TResponse>>> methodInvoke,
            string name,
            string? nestedJsonProperty = null,
            List<string>? ignoreProperties = null,
            bool userSingleArrayItem = false)
        {
            var listener = new EnumValueTraceListener();
            Trace.Listeners.Add(listener);
            
            var path = Directory.GetParent(Environment.CurrentDirectory)!.Parent!.Parent!.FullName;
            FileStream? file = null;
            try
            {
                file = File.OpenRead(Path.Combine(path, _folder, $"{name}.txt"));
            }
            catch (FileNotFoundException)
            {
                throw new Exception("Response file not found");
            }

            var buffer = new byte[file.Length];
            await file.ReadAsync(buffer, 0, buffer.Length);
            file.Close();

            var data = Encoding.UTF8.GetString(buffer);
            using var reader = new StringReader(data);
            var expectedMethod = reader.ReadLine();
            var expectedPath = reader.ReadLine();
            var expectedAuth = bool.Parse(reader.ReadLine()!);
            var response = reader.ReadToEnd();

            TestHelpers.ConfigureRestClient(_client, response, System.Net.HttpStatusCode.OK);
            var result = await methodInvoke(_client);

            // asset
            Assert.That(result.Error, Is.Null, name + " returned error");
            Assert.That(_isAuthenticated(result.AsDataless()), Is.EqualTo(expectedAuth));
            Assert.That(result.RequestMethod, Is.EqualTo(new HttpMethod(expectedMethod!)));
            Assert.That(expectedPath, Is.EqualTo(result.RequestUrl!.Replace(_baseAddress, "").Split(new char[] { '?' })[0]));

            object responseData = result.Data!;
            if (_stjCompare == true)
                SystemTextJsonComparer.CompareData(name, result.Data!, response, nestedJsonProperty ?? _nestedPropertyForCompare, ignoreProperties, userSingleArrayItem);
            else
                JsonNetComparer.CompareData(name, result.Data!, response, nestedJsonProperty ?? _nestedPropertyForCompare, ignoreProperties, userSingleArrayItem);
           
            Trace.Listeners.Remove(listener);
        }
    }
}
