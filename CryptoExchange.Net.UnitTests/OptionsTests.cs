using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Options;
using CryptoExchange.Net.UnitTests.TestImplementations;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoExchange.Net.UnitTests
{
    [TestFixture()]
    public class OptionsTests
    {
        [TearDown]
        public void Init()
        {
            TestClientOptions.Default = new TestClientOptions
            {
            };
        }

        [TestCase(null, null)]
        [TestCase("", "")]
        [TestCase("test", null)]
        [TestCase("test", "")]
        [TestCase(null, "test")]
        [TestCase("", "test")]
        public void SettingEmptyValuesForAPICredentials_Should_ThrowException(string key, string secret)
        {
            // arrange
            // act
            // assert
            Assert.Throws(typeof(ArgumentException),
                () => new RestExchangeOptions<TestEnvironment, ApiCredentials>() { ApiCredentials = new ApiCredentials(key, secret) });
        }

        [Test]
        public void TestBasicOptionsAreSet()
        {
            // arrange, act
            var options = new TestClientOptions
            {
                ApiCredentials = new ApiCredentials("123", "456"),
                ReceiveWindow = TimeSpan.FromSeconds(10)
            };

            // assert
            Assert.That(options.ReceiveWindow == TimeSpan.FromSeconds(10));
            Assert.That(options.ApiCredentials.Key == "123");
            Assert.That(options.ApiCredentials.Secret == "456");
        }

        [Test]
        public void TestApiOptionsAreSet()
        {
            // arrange, act
            var options = new TestClientOptions();
            options.Api1Options.ApiCredentials = new ApiCredentials("123", "456");
            options.Api2Options.ApiCredentials = new ApiCredentials("789", "101");

            // assert
            Assert.That(options.Api1Options.ApiCredentials.Key == "123");
            Assert.That(options.Api1Options.ApiCredentials.Secret == "456");
            Assert.That(options.Api2Options.ApiCredentials.Key == "789");
            Assert.That(options.Api2Options.ApiCredentials.Secret == "101");
        }

        [Test]
        public void TestClientUsesCorrectOptions()
        {
            var client = new TestRestClient(options => {
                options.Api1Options.ApiCredentials = new ApiCredentials("111", "222");
                options.ApiCredentials = new ApiCredentials("333", "444");
            });

            var authProvider1 = (TestAuthProvider)client.Api1.AuthenticationProvider;
            var authProvider2 = (TestAuthProvider)client.Api2.AuthenticationProvider;
            Assert.That(authProvider1.GetKey() == "111");
            Assert.That(authProvider1.GetSecret() == "222");
            Assert.That(authProvider2.GetKey() == "333");
            Assert.That(authProvider2.GetSecret() == "444");
        }

        [Test]
        public void TestClientUsesCorrectOptionsWithDefault()
        {
            TestClientOptions.Default.ApiCredentials = new ApiCredentials("123", "456");
            TestClientOptions.Default.Api1Options.ApiCredentials = new ApiCredentials("111", "222");

            var client = new TestRestClient();

            var authProvider1 = (TestAuthProvider)client.Api1.AuthenticationProvider;
            var authProvider2 = (TestAuthProvider)client.Api2.AuthenticationProvider;
            Assert.That(authProvider1.GetKey() == "111");
            Assert.That(authProvider1.GetSecret() == "222");
            Assert.That(authProvider2.GetKey() == "123");
            Assert.That(authProvider2.GetSecret() == "456");
        }

        [Test]
        public void TestClientUsesCorrectOptionsWithOverridingDefault()
        {
            TestClientOptions.Default.ApiCredentials = new ApiCredentials("123", "456");
            TestClientOptions.Default.Api1Options.ApiCredentials = new ApiCredentials("111", "222");

            var client = new TestRestClient(options =>
            {
                options.Api1Options.ApiCredentials = new ApiCredentials("333", "444");
                options.Environment = new TestEnvironment("Test", "https://test.test");
            });

            var authProvider1 = (TestAuthProvider)client.Api1.AuthenticationProvider;
            var authProvider2 = (TestAuthProvider)client.Api2.AuthenticationProvider;
            Assert.That(authProvider1.GetKey() == "333");
            Assert.That(authProvider1.GetSecret() == "444");
            Assert.That(authProvider2.GetKey() == "123");
            Assert.That(authProvider2.GetSecret() == "456");
            Assert.That(client.Api2.BaseAddress == "https://localhost:123");
        }
    }

    public class TestClientOptions: RestExchangeOptions<TestEnvironment, ApiCredentials>
    {
        /// <summary>
        /// Default options for the futures client
        /// </summary>
        public static TestClientOptions Default { get; set; } = new TestClientOptions()
        {
            Environment = new TestEnvironment("test", "https://test.com")
        };

        /// <summary>
        /// The default receive window for requests
        /// </summary>
        public TimeSpan ReceiveWindow { get; set; } = TimeSpan.FromSeconds(5);

        public RestApiOptions Api1Options { get; private set; } = new RestApiOptions();

        public RestApiOptions Api2Options { get; set; } = new RestApiOptions();

        internal TestClientOptions Copy()
        {
            var options = Copy<TestClientOptions>();
            options.Api1Options = Api1Options.Copy<RestApiOptions>();
            options.Api2Options = Api2Options.Copy<RestApiOptions>();
            return options;
        }
    }
}
