using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Options;
using CryptoExchange.Net.UnitTests.TestImplementations;
using NUnit.Framework;
using System;

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
                () => {
                    var opts = new RestExchangeOptions<TestEnvironment, HMACCredential>()
                    {
                        ApiCredentials = new HMACCredential(key, secret)
                    };
                    opts.ApiCredentials.Validate();
                });
        }

        [Test]
        public void TestBasicOptionsAreSet()
        {
            // arrange, act
            var options = new TestClientOptions
            {
                ApiCredentials = new HMACCredential("123", "456"),
                ReceiveWindow = TimeSpan.FromSeconds(10)
            };

            // assert
            Assert.That(options.ReceiveWindow == TimeSpan.FromSeconds(10));
            Assert.That(options.ApiCredentials.Key == "123");
            Assert.That(options.ApiCredentials.Secret == "456");
        }

        [Test]
        public void TestSetOptionsRest()
        {
            var client = new TestRestClient();
            client.SetOptions(new UpdateOptions
            {
                RequestTimeout = TimeSpan.FromSeconds(2),
                Proxy = new ApiProxy("http://testproxy", 1234)
            });

            Assert.That(client.Api1.ClientOptions.Proxy, Is.Not.Null);
            Assert.That(client.Api1.ClientOptions.Proxy.Host, Is.EqualTo("http://testproxy"));
            Assert.That(client.Api1.ClientOptions.Proxy.Port, Is.EqualTo(1234));
            Assert.That(client.Api1.ClientOptions.RequestTimeout, Is.EqualTo(TimeSpan.FromSeconds(2)));
        }

        [Test]
        public void TestSetOptionsRestWithCredentials()
        {
            var client = new TestRestClient();
            client.SetOptions(new UpdateOptions<HMACCredential>
            {
                ApiCredentials = new HMACCredential("123", "456"),
                RequestTimeout = TimeSpan.FromSeconds(2),
                Proxy = new ApiProxy("http://testproxy", 1234)
            });

            Assert.That(client.Api1.ApiCredentials, Is.Not.Null);
            Assert.That(client.Api1.ApiCredentials.Key, Is.EqualTo("123"));
            Assert.That(client.Api1.ClientOptions.Proxy, Is.Not.Null);
            Assert.That(client.Api1.ClientOptions.Proxy.Host, Is.EqualTo("http://testproxy"));
            Assert.That(client.Api1.ClientOptions.Proxy.Port, Is.EqualTo(1234));
            Assert.That(client.Api1.ClientOptions.RequestTimeout, Is.EqualTo(TimeSpan.FromSeconds(2)));
        }

        [Test]
        public void TestWhenUpdatingSettingsExistingClientsAreNotAffected()
        {
            TestClientOptions.Default = new TestClientOptions
            {
                ApiCredentials = new HMACCredential("111", "222"),
                RequestTimeout = TimeSpan.FromSeconds(1),
            };

            var client1 = new TestRestClient();

            Assert.That(client1.ClientOptions.RequestTimeout, Is.EqualTo(TimeSpan.FromSeconds(1)));
            Assert.That(client1.ClientOptions.ApiCredentials.Key, Is.EqualTo("111"));

            TestClientOptions.Default.ApiCredentials = new HMACCredential("333", "444");
            TestClientOptions.Default.RequestTimeout = TimeSpan.FromSeconds(2);

            var client2 = new TestRestClient();

            Assert.That(client2.ClientOptions.RequestTimeout, Is.EqualTo(TimeSpan.FromSeconds(2)));
            Assert.That(client2.ClientOptions.ApiCredentials.Key, Is.EqualTo("333"));
        }
    }

    public class TestClientOptions: RestExchangeOptions<TestEnvironment, HMACCredential>
    {
        /// <summary>
        /// Default options for the futures client
        /// </summary>
        public static TestClientOptions Default { get; set; } = new TestClientOptions()
        {
            Environment = new TestEnvironment("test", "https://test.com")
        };

        /// <summary>
        /// ctor
        /// </summary>
        public TestClientOptions()
        {
            Default?.Set(this);
        }

        /// <summary>
        /// The default receive window for requests
        /// </summary>
        public TimeSpan ReceiveWindow { get; set; } = TimeSpan.FromSeconds(5);

        public RestApiOptions Api1Options { get; private set; } = new RestApiOptions();

        public RestApiOptions Api2Options { get; set; } = new RestApiOptions();

        internal TestClientOptions Set(TestClientOptions targetOptions)
        {
            targetOptions = base.Set<TestClientOptions>(targetOptions);
            targetOptions.Api1Options = Api1Options.Set(targetOptions.Api1Options);
            targetOptions.Api2Options = Api2Options.Set(targetOptions.Api2Options);
            return targetOptions;
        }
    }
}
