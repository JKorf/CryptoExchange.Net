using CryptoExchange.Net.Objects;
using CryptoExchange.Net.TokenManagement;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace CryptoExchange.Net.UnitTests
{
    [TestFixture]
    public class TokenManagementTests
    {
        [Test]
        public async Task RetainedTokenIsReusedAfterLeaseRelease()
        {
            var starts = 0;
            var manager = new TokenManager(
                Guid.NewGuid().ToString(),
                null,
                TimeSpan.FromMinutes(1),
                TimeSpan.FromMinutes(1),
                (_, _) => Task.FromResult(CallResult.Ok("token-" + ++starts)),
                retentionPolicy: TokenRetentionPolicy.RetainUntilExpired);
            var scope = new TokenScope("Test", "Test", "Test", "apKey");

            var first = await manager.AcquireAsync(scope);
            await first.Data.ReleaseAsync();
            var second = await manager.AcquireAsync(scope);

            Assert.That(second.Data.Token.Token, Is.EqualTo(first.Data.Token.Token));
            Assert.That(starts, Is.EqualTo(1));
            await second.Data.ReleaseAsync();
        }

        [Test]
        public async Task RemoveWhenUnusedStartsNewTokenAfterLeaseRelease()
        {
            var starts = 0;
            var stops = 0;
            var manager = new TokenManager(
                Guid.NewGuid().ToString(),
                null,
                TimeSpan.FromMinutes(1),
                TimeSpan.FromMinutes(1),
                (_, _) => Task.FromResult(CallResult.Ok("token-" + ++starts)),
                stopToken: (_, _) =>
                {
                    stops++;
                    return Task.FromResult(CallResult.Ok());
                });
            var scope = new TokenScope("Test", "Test", "Test", "apKey");

            var first = await manager.AcquireAsync(scope);
            await first.Data.ReleaseAsync();
            var second = await manager.AcquireAsync(scope);

            Assert.That(second.Data.Token.Token, Is.Not.EqualTo(first.Data.Token.Token));
            Assert.That(starts, Is.EqualTo(2));
            Assert.That(stops, Is.EqualTo(1));
            await second.Data.ReleaseAsync();
        }

        [Test]
        public async Task ExpiredRetainedTokenIsNotReused()
        {
            var starts = 0;
            var manager = new TokenManager(
                Guid.NewGuid().ToString(),
                null,
                TimeSpan.FromMinutes(1),
                TimeSpan.FromMilliseconds(20),
                (_, _) => Task.FromResult(CallResult.Ok("token-" + ++starts)),
                retentionPolicy: TokenRetentionPolicy.RetainUntilExpired);
            var scope = new TokenScope("Test", "Test", "Test", "apKey");

            var first = await manager.AcquireAsync(scope);
            await first.Data.ReleaseAsync();
            await Task.Delay(50);
            var second = await manager.AcquireAsync(scope);

            Assert.That(second.Data.Token.Token, Is.Not.EqualTo(first.Data.Token.Token));
            Assert.That(starts, Is.EqualTo(2));
            await second.Data.ReleaseAsync();
        }
    }
}
