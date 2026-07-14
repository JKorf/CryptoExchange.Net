using CryptoExchange.Net.Caching;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.UnitTests
{
    [TestFixture]
    public class ExchangeCacheTests
    {
        [Test]
        public async Task ConcurrentRetrievalsForSameKey_ShouldOnlyInvokeFactoryOnce()
        {
            var factoryCalls = 0;
            var cache = new ExchangeCache(new CacheItemDefinition<int>
            {
                Key = "key",
                Ttl = TimeSpan.FromMinutes(1),
                ValueFactory = async () =>
                {
                    Interlocked.Increment(ref factoryCalls);
                    await Task.Delay(25);
                    return 42;
                }
            });

            var results = await Task.WhenAll(
                Enumerable.Range(0, 20).Select(_ => cache.GetOrRetrieveAsync<int>("key")));

            Assert.That(results, Is.All.EqualTo(42));
            Assert.That(factoryCalls, Is.EqualTo(1));
        }

        [Test]
        public async Task ConcurrentRetrievalsForDifferentKeys_ShouldRunInParallel()
        {
            var firstStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var secondStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var releaseFactories = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var cache = new ExchangeCache(
                new CacheItemDefinition<int>
                {
                    Key = "first",
                    Ttl = TimeSpan.FromMinutes(1),
                    ValueFactory = async () =>
                    {
                        firstStarted.TrySetResult(true);
                        await releaseFactories.Task;
                        return 1;
                    }
                },
                new CacheItemDefinition<int>
                {
                    Key = "second",
                    Ttl = TimeSpan.FromMinutes(1),
                    ValueFactory = async () =>
                    {
                        secondStarted.TrySetResult(true);
                        await releaseFactories.Task;
                        return 2;
                    }
                });

            var retrievals = Task.WhenAll(
                cache.GetOrRetrieveAsync<int>("first"),
                cache.GetOrRetrieveAsync<int>("second"));
            var bothStarted = Task.WhenAll(firstStarted.Task, secondStarted.Task);
            var startResult = await Task.WhenAny(bothStarted, Task.Delay(TimeSpan.FromSeconds(1)));

            releaseFactories.TrySetResult(true);
            var results = await retrievals;

            Assert.That(startResult, Is.SameAs(bothStarted));
            Assert.That(results, Is.EqualTo(new int?[] { 1, 2 }));
        }
    }
}
