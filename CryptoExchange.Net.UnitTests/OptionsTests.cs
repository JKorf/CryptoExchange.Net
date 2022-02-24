using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.UnitTests.TestImplementations;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
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
                () => new RestApiClientOptions() { ApiCredentials = new ApiCredentials(key, secret) });
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
            Assert.AreEqual(options.ReceiveWindow, TimeSpan.FromSeconds(10));
            Assert.AreEqual(options.ApiCredentials.Key.GetString(), "123");
            Assert.AreEqual(options.ApiCredentials.Secret.GetString(), "456");
        }

        [Test]
        public void TestApiOptionsAreSet()
        {
            // arrange, act
            var options = new TestClientOptions
            {
                Api1Options = new RestApiClientOptions
                {
                    ApiCredentials = new ApiCredentials("123", "456"),
                    BaseAddress = "http://test1.com"
                },
                Api2Options = new RestApiClientOptions
                {
                    ApiCredentials = new ApiCredentials("789", "101"),
                    BaseAddress = "http://test2.com"
                }
            };

            // assert
            Assert.AreEqual(options.Api1Options.ApiCredentials.Key.GetString(), "123");
            Assert.AreEqual(options.Api1Options.ApiCredentials.Secret.GetString(), "456");
            Assert.AreEqual(options.Api1Options.BaseAddress, "http://test1.com");
            Assert.AreEqual(options.Api2Options.ApiCredentials.Key.GetString(), "789");
            Assert.AreEqual(options.Api2Options.ApiCredentials.Secret.GetString(), "101");
            Assert.AreEqual(options.Api2Options.BaseAddress, "http://test2.com");
        }

        [Test]
        public void TestNotOverridenApiOptionsAreStillDefault()
        {
            // arrange, act
            var options = new TestClientOptions
            {
                Api1Options = new RestApiClientOptions
                {
                    ApiCredentials = new ApiCredentials("123", "456"),
                }
            };

            // assert
            Assert.AreEqual(options.Api1Options.RateLimitingBehaviour, RateLimitingBehaviour.Wait);
            Assert.AreEqual(options.Api1Options.BaseAddress, "https://api1.test.com/");
            Assert.AreEqual(options.Api2Options.BaseAddress, "https://api2.test.com/");
        }

        [Test]
        public void TestSettingDefaultBaseOptionsAreRespected()
        {
            // arrange
            TestClientOptions.Default = new TestClientOptions
            {
                ApiCredentials = new ApiCredentials("123", "456"),
                LogLevel = LogLevel.Trace
            };

            // act
            var options = new TestClientOptions();

            // assert
            Assert.AreEqual(options.LogLevel, LogLevel.Trace);
            Assert.AreEqual(options.ApiCredentials.Key.GetString(), "123");
            Assert.AreEqual(options.ApiCredentials.Secret.GetString(), "456");
        }

        [Test]
        public void TestSettingDefaultApiOptionsAreRespected()
        {
            // arrange
            TestClientOptions.Default = new TestClientOptions
            {
                ApiCredentials = new ApiCredentials("123", "456"),
                LogLevel = LogLevel.Trace,
                Api1Options = new RestApiClientOptions
                {
                    ApiCredentials = new ApiCredentials("456", "789")
                }
            };

            // act
            var options = new TestClientOptions();

            // assert
            Assert.AreEqual(options.ApiCredentials.Key.GetString(), "123");
            Assert.AreEqual(options.ApiCredentials.Secret.GetString(), "456");
            Assert.AreEqual(options.Api1Options.BaseAddress, "https://api1.test.com/");
            Assert.AreEqual(options.Api1Options.ApiCredentials.Key.GetString(), "456");
            Assert.AreEqual(options.Api1Options.ApiCredentials.Secret.GetString(), "789");
        }

        [Test]
        public void TestSettingDefaultApiOptionsWithSomeOverriddenAreRespected()
        {
            // arrange
            TestClientOptions.Default = new TestClientOptions
            {
                ApiCredentials = new ApiCredentials("123", "456"),
                LogLevel = LogLevel.Trace,
                Api1Options = new RestApiClientOptions
                {
                    ApiCredentials = new ApiCredentials("456", "789")
                },
                Api2Options = new RestApiClientOptions
                {
                    ApiCredentials = new ApiCredentials("111", "222")
                }
            };

            // act
            var options = new TestClientOptions
            {
                Api1Options = new RestApiClientOptions
                {
                    ApiCredentials = new ApiCredentials("333", "444")
                }
            };

            // assert
            Assert.AreEqual(options.ApiCredentials.Key.GetString(), "123");
            Assert.AreEqual(options.ApiCredentials.Secret.GetString(), "456");
            Assert.AreEqual(options.Api1Options.ApiCredentials.Key.GetString(), "333");
            Assert.AreEqual(options.Api1Options.ApiCredentials.Secret.GetString(), "444");
            Assert.AreEqual(options.Api2Options.ApiCredentials.Key.GetString(), "111");
            Assert.AreEqual(options.Api2Options.ApiCredentials.Secret.GetString(), "222");
        }

        [Test]
        public void TestClientUsesCorrectOptions()
        {
            var client = new TestRestClient(new TestClientOptions()
            {
                ApiCredentials = new ApiCredentials("123", "456"),
                Api1Options = new RestApiClientOptions
                {
                    ApiCredentials = new ApiCredentials("111", "222")
                }
            });

            Assert.AreEqual(client.Api1.AuthenticationProvider.Credentials.Key.GetString(), "111");
            Assert.AreEqual(client.Api1.AuthenticationProvider.Credentials.Secret.GetString(), "222");
            Assert.AreEqual(client.Api2.AuthenticationProvider.Credentials.Key.GetString(), "123");
            Assert.AreEqual(client.Api2.AuthenticationProvider.Credentials.Secret.GetString(), "456");
        }

        [Test]
        public void TestClientUsesCorrectOptionsWithDefault()
        {
            TestClientOptions.Default = new TestClientOptions()
            {
                ApiCredentials = new ApiCredentials("123", "456"),
                Api1Options = new RestApiClientOptions
                {
                    ApiCredentials = new ApiCredentials("111", "222")
                }
            };

            var client = new TestRestClient();

            Assert.AreEqual(client.Api1.AuthenticationProvider.Credentials.Key.GetString(), "111");
            Assert.AreEqual(client.Api1.AuthenticationProvider.Credentials.Secret.GetString(), "222");
            Assert.AreEqual(client.Api2.AuthenticationProvider.Credentials.Key.GetString(), "123");
            Assert.AreEqual(client.Api2.AuthenticationProvider.Credentials.Secret.GetString(), "456");
        }

        [Test]
        public void TestClientUsesCorrectOptionsWithOverridingDefault()
        {
            TestClientOptions.Default = new TestClientOptions()
            {
                ApiCredentials = new ApiCredentials("123", "456"),
                Api1Options = new RestApiClientOptions
                {
                    ApiCredentials = new ApiCredentials("111", "222")
                }
            };

            var client = new TestRestClient(new TestClientOptions
            {
                Api1Options = new RestApiClientOptions
                {
                    ApiCredentials = new ApiCredentials("333", "444")
                },
                Api2Options = new RestApiClientOptions()
                {
                    BaseAddress = "http://test.com"
                }
            });

            Assert.AreEqual(client.Api1.AuthenticationProvider.Credentials.Key.GetString(), "333");
            Assert.AreEqual(client.Api1.AuthenticationProvider.Credentials.Secret.GetString(), "444");
            Assert.AreEqual(client.Api2.AuthenticationProvider.Credentials.Key.GetString(), "123");
            Assert.AreEqual(client.Api2.AuthenticationProvider.Credentials.Secret.GetString(), "456");
            Assert.AreEqual(client.Api2.BaseAddress, "http://test.com");
        }
    }

    public class TestClientOptions: BaseRestClientOptions
    {
        /// <summary>
        /// Default options for the futures client
        /// </summary>
        public static TestClientOptions Default { get; set; } = new TestClientOptions();

        /// <summary>
        /// The default receive window for requests
        /// </summary>
        public TimeSpan ReceiveWindow { get; set; } = TimeSpan.FromSeconds(5);

        private RestApiClientOptions _api1Options = new RestApiClientOptions("https://api1.test.com/");
        public RestApiClientOptions Api1Options
        {
            get => _api1Options;
            set => _api1Options = new RestApiClientOptions(_api1Options, value);
        }

        private RestApiClientOptions _api2Options = new RestApiClientOptions("https://api2.test.com/");
        public RestApiClientOptions Api2Options
        {
            get => _api2Options;
            set => _api2Options = new RestApiClientOptions(_api2Options, value);
        }

        /// <summary>
        /// ctor
        /// </summary>
        public TestClientOptions(): this(Default)
        {
        }

        public TestClientOptions(TestClientOptions baseOn): base(baseOn)
        {
            if (baseOn == null)
                return;

            ReceiveWindow = baseOn.ReceiveWindow;

            Api1Options = new RestApiClientOptions(baseOn.Api1Options, null);
            Api2Options = new RestApiClientOptions(baseOn.Api2Options, null);
        }
    }
}
