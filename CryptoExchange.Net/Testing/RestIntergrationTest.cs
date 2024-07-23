using CryptoExchange.Net.Objects;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Testing
{
    /// <summary>
    /// Base class for executing REST API integration tests
    /// </summary>
    /// <typeparam name="TClient">Client type</typeparam>
    public abstract class RestIntergrationTest<TClient>
    {
        /// <summary>
        /// Get a client instance
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <returns></returns>
        public abstract TClient GetClient(ILoggerFactory loggerFactory);

        /// <summary>
        /// Whether the test should be run. By default integration tests aren't executed, can be set to true to force execution.
        /// </summary>
        public virtual bool Run { get; set; }

        /// <summary>
        /// Whether API credentials are provided and thus authenticated calls can be executed. Should be set in the GetClient implementation.
        /// </summary>
        public bool Authenticated { get; set; }

        /// <summary>
        /// Create a client
        /// </summary>
        /// <returns></returns>
        protected TClient CreateClient()
        {
            var fact = new LoggerFactory();
            fact.AddProvider(new TraceLoggerProvider());
            return GetClient(fact);
        }

        /// <summary>
        /// Check if integration tests should be executed
        /// </summary>
        /// <returns></returns>
        protected bool ShouldRun()
        {
            var integrationTests = Environment.GetEnvironmentVariable("INTEGRATION");
            if (!Run && integrationTests != "1")
                return false;

            return true;
        }

        /// <summary>
        /// Execute a REST endpoint call and check for any errors or warnings.
        /// </summary>
        /// <typeparam name="T">Type of response</typeparam>
        /// <param name="expression">The call expression</param>
        /// <param name="authRequest">Whether this is an authenticated request</param>
        public async Task RunAndCheckResult<T>(Expression<Func<TClient, Task<WebCallResult<T>>>> expression, bool authRequest)
        {
            if (!ShouldRun())
                return;

            var client = CreateClient();

            var expressionBody = (MethodCallExpression)expression.Body;
            if (authRequest && !Authenticated)
            {
                Debug.WriteLine($"Skipping {expressionBody.Method.Name}, not authenticated");
                return;
            }

            var listener = new EnumValueTraceListener();
            Trace.Listeners.Add(listener);

            WebCallResult<T> result;
            try
            {
                result = await expression.Compile().Invoke(client).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new Exception($"Method {expressionBody.Method.Name} threw an exception: " + ex.ToLogString());
            }
            finally
            {
                Trace.Listeners.Remove(listener);
            }

            if (!result.Success)
                throw new Exception($"Method {expressionBody.Method.Name} returned error: " + result.Error);

            Debug.WriteLine($"{expressionBody.Method.Name} {result}");
        }
    }
}
