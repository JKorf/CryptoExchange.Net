using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Errors;
using CryptoExchange.Net.Sockets;
using CryptoExchange.Net.Sockets.Default;
using CryptoExchange.Net.TokenManagement;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace CryptoExchange.Net.UnitTests
{
    [TestFixture]
    public class TokenManagementTests
    {
        private static readonly TimeSpan TestMaintenanceInterval = TimeSpan.FromMilliseconds(5);

        [Test]
        public async Task AcquireWithoutApiKeyReturnsCredentialsError()
        {
            var starts = 0;
            var manager = CreateManager(
                (_, _) =>
                {
                    starts++;
                    return Task.FromResult(CallResult.Ok("token"));
                });

            var result = await manager.AcquireAsync(new TokenScope("Test", "Test", "Test", ""));

            Assert.That(result.Success, Is.False);
            Assert.That(result.Error, Is.TypeOf<NoApiCredentialsError>());
            Assert.That(starts, Is.EqualTo(0));
        }

        [Test]
        public async Task StartTokenFailureIsReturned()
        {
            var error = new ServerError(ErrorType.Unknown, "start failed");
            var manager = CreateManager((_, _) => Task.FromResult(CallResult.Fail<string>(error)));

            var result = await manager.AcquireAsync(CreateScope());

            Assert.That(result.Success, Is.False);
            Assert.That(result.Error, Is.SameAs(error));
        }

        [Test]
        public async Task ActiveTokenIsSharedWhileLeasedAndStoppedAfterLastRelease()
        {
            var starts = 0;
            var stops = 0;
            var manager = CreateManager(
                (_, _) => Task.FromResult(CallResult.Ok("token-" + ++starts)),
                stopToken: (_, _) =>
                {
                    stops++;
                    return Task.FromResult(CallResult.Ok());
                });
            var scope = CreateScope();

            var first = await manager.AcquireAsync(scope);
            var second = await manager.AcquireAsync(scope);
            AssertSuccess(first);
            AssertSuccess(second);

            Assert.That(second.Data!.Token.Token, Is.EqualTo(first.Data!.Token.Token));
            Assert.That(starts, Is.EqualTo(1));

            await first.Data!.ReleaseAsync();
            Assert.That(stops, Is.EqualTo(0));

            await second.Data!.ReleaseAsync();
            Assert.That(stops, Is.EqualTo(1));
        }

        [Test]
        public async Task ActiveTokenStartsNewTokenAfterLeaseRelease()
        {
            var starts = 0;
            var stops = 0;
            var manager = CreateManager(
                (_, _) => Task.FromResult(CallResult.Ok("token-" + ++starts)),
                stopToken: (_, _) =>
                {
                    stops++;
                    return Task.FromResult(CallResult.Ok());
                });
            var scope = CreateScope();

            var first = await manager.AcquireAsync(scope);
            AssertSuccess(first);
            await first.Data!.ReleaseAsync();
            var second = await manager.AcquireAsync(scope);
            AssertSuccess(second);

            Assert.That(second.Data!.Token.Token, Is.Not.EqualTo(first.Data!.Token.Token));
            Assert.That(starts, Is.EqualTo(2));
            Assert.That(stops, Is.EqualTo(1));
            await second.Data!.ReleaseAsync();
        }

        [Test]
        public async Task ReleasingLeaseTwiceOnlyStopsActiveTokenOnce()
        {
            var stops = 0;
            var manager = CreateManager(
                (_, _) => Task.FromResult(CallResult.Ok("token")),
                stopToken: (_, _) =>
                {
                    stops++;
                    return Task.FromResult(CallResult.Ok());
                });

            var leaseResult = await manager.AcquireAsync(CreateScope());
            AssertSuccess(leaseResult);

            await leaseResult.Data!.ReleaseAsync();
            await leaseResult.Data!.ReleaseAsync();

            Assert.That(stops, Is.EqualTo(1));
        }

        [Test]
        public async Task CachedTokenIsReusedAfterLeaseRelease()
        {
            var starts = 0;
            var manager = CreateManager(
                (_, _) => Task.FromResult(CallResult.Ok("token-" + ++starts)),
                managementType: TokenManagementType.Cached);
            var scope = CreateScope();

            var first = await manager.AcquireAsync(scope);
            AssertSuccess(first);
            await first.Data!.ReleaseAsync();
            var second = await manager.AcquireAsync(scope);
            AssertSuccess(second);

            Assert.That(second.Data!.Token.Token, Is.EqualTo(first.Data!.Token.Token));
            Assert.That(starts, Is.EqualTo(1));
            await second.Data!.ReleaseAsync();
        }

        [Test]
        public async Task CachedTokensAreScopedIndependently()
        {
            var starts = 0;
            var manager = CreateManager(
                (_, _) => Task.FromResult(CallResult.Ok("token-" + ++starts)),
                managementType: TokenManagementType.Cached);

            var firstScope = CreateScope(additionalIdentifier: "one");
            var secondScope = CreateScope(additionalIdentifier: "two");

            var first = await manager.AcquireAsync(firstScope);
            var second = await manager.AcquireAsync(secondScope);
            AssertSuccess(first);
            AssertSuccess(second);
            await first.Data!.ReleaseAsync();
            await second.Data!.ReleaseAsync();

            var firstAgain = await manager.AcquireAsync(firstScope);
            AssertSuccess(firstAgain);

            Assert.That(firstAgain.Data!.Token.Token, Is.EqualTo(first.Data!.Token.Token));
            Assert.That(second.Data!.Token.Token, Is.Not.EqualTo(first.Data!.Token.Token));
            Assert.That(starts, Is.EqualTo(2));
            await firstAgain.Data!.ReleaseAsync();
        }

        [Test]
        public async Task ExpiredCachedTokenIsNotReused()
        {
            var starts = 0;
            var manager = CreateManager(
                (_, _) => Task.FromResult(CallResult.Ok("token-" + ++starts)),
                timeValid: TimeSpan.FromMilliseconds(20),
                managementType: TokenManagementType.Cached);
            var scope = CreateScope();

            var first = await manager.AcquireAsync(scope);
            AssertSuccess(first);
            await first.Data!.ReleaseAsync();
            await Task.Delay(50);
            var second = await manager.AcquireAsync(scope);
            AssertSuccess(second);

            Assert.That(first.Data!.Token.Status, Is.EqualTo(TokenStatus.Expired));
            Assert.That(second.Data!.Token.Token, Is.Not.EqualTo(first.Data!.Token.Token));
            Assert.That(starts, Is.EqualTo(2));
            await second.Data!.ReleaseAsync();
        }

        [Test]
        public async Task CachedTokenDoesNotRunKeepAliveLoop()
        {
            var keepAlives = 0;
            var manager = CreateManager(
                (_, _) => Task.FromResult(CallResult.Ok("token")),
                refreshInterval: TimeSpan.FromMilliseconds(1),
                keepAliveToken: (_, _) =>
                {
                    keepAlives++;
                    return Task.FromResult(CallResult.Ok());
                },
                managementType: TokenManagementType.Cached);

            var leaseResult = await manager.AcquireAsync(CreateScope());
            AssertSuccess(leaseResult);
            await Task.Delay(50);

            Assert.That(keepAlives, Is.EqualTo(0));
            await leaseResult.Data!.ReleaseAsync();
        }

        [Test]
        public async Task ActiveTokenKeepAliveRefreshesValidity()
        {
            var keepAlives = 0;
            var manager = CreateManager(
                (_, _) => Task.FromResult(CallResult.Ok("token")),
                refreshInterval: TimeSpan.FromMilliseconds(1),
                timeValid: TimeSpan.FromSeconds(1),
                keepAliveToken: (_, _) =>
                {
                    keepAlives++;
                    return Task.FromResult(CallResult.Ok());
                });

            var leaseResult = await manager.AcquireAsync(CreateScope());
            AssertSuccess(leaseResult);
            var originalValidUntil = leaseResult.Data!.Token.ValidUntil;

            await WaitUntilAsync(() => keepAlives > 0);

            Assert.That(leaseResult.Data!.Token.ValidUntil, Is.GreaterThan(originalValidUntil));
            await leaseResult.Data!.ReleaseAsync();
        }

        [Test]
        public async Task ActiveTokenKeepAliveFailureExpiresTokenWhenValidityPassed()
        {
            var starts = 0;
            var expired = false;
            var manager = CreateManager(
                (_, _) => Task.FromResult(CallResult.Ok("token-" + ++starts)),
                refreshInterval: TimeSpan.FromMilliseconds(1),
                timeValid: TimeSpan.FromMilliseconds(25),
                keepAliveToken: (_, _) => Task.FromResult(CallResult.Fail(new ServerError(ErrorType.Unknown, "keep alive failed"))));

            var leaseResult = await manager.AcquireAsync(CreateScope());
            AssertSuccess(leaseResult);
            leaseResult.Data!.Token.Expired += _ => expired = true;

            await WaitUntilAsync(() => expired);

            Assert.That(leaseResult.Data!.Token.Status, Is.EqualTo(TokenStatus.Expired));

            var nextLease = await manager.AcquireAsync(CreateScope());
            AssertSuccess(nextLease);
            Assert.That(nextLease.Data!.Token.Token, Is.Not.EqualTo(leaseResult.Data!.Token.Token));
            Assert.That(starts, Is.EqualTo(2));

            await leaseResult.Data!.ReleaseAsync();
            await nextLease.Data!.ReleaseAsync();
        }

        [Test]
        public async Task AcquireAndReplaceReleasesPreviousSubscriptionLease()
        {
            var starts = 0;
            var stops = 0;
            var manager = CreateManager(
                (_, _) => Task.FromResult(CallResult.Ok("token-" + ++starts)),
                stopToken: (_, _) =>
                {
                    stops++;
                    return Task.FromResult(CallResult.Ok());
                });
            var subscription = new TestSubscription();

            var first = await manager.AcquireAndReplaceAsync(subscription, CreateScope(additionalIdentifier: "one"));
            AssertSuccess(first);
            var second = await manager.AcquireAndReplaceAsync(subscription, CreateScope(additionalIdentifier: "two"));
            AssertSuccess(second);

            Assert.That(subscription.TokenLease, Is.SameAs(second.Data));
            Assert.That(second.Data!.Token.Token, Is.Not.EqualTo(first.Data!.Token.Token));
            Assert.That(stops, Is.EqualTo(1));

            await subscription.TokenLease!.ReleaseAsync();
        }

        private static TokenManager CreateManager(
            Func<TokenScope, System.Threading.CancellationToken, Task<CallResult<string>>> startToken,
            TimeSpan? refreshInterval = null,
            TimeSpan? timeValid = null,
            Func<TokenInfo, System.Threading.CancellationToken, Task<CallResult>>? keepAliveToken = null,
            Func<TokenInfo, System.Threading.CancellationToken, Task<CallResult>>? stopToken = null,
            TokenManagementType managementType = TokenManagementType.Active)
        {
            return new TokenManager(
                Guid.NewGuid().ToString(),
                null,
                refreshInterval ?? TimeSpan.FromMinutes(1),
                timeValid ?? TimeSpan.FromMinutes(1),
                startToken,
                keepAliveToken,
                stopToken,
                managementType,
                TestMaintenanceInterval);
        }

        private static TokenScope CreateScope(string apiKey = "apiKey", string? additionalIdentifier = null)
            => new TokenScope("Test", "Test", "Test", apiKey, additionalIdentifier);

        private static void AssertSuccess(CallResult<TokenLease> result)
        {
            Assert.That(result.Success, Is.True, result.Error?.ToString());
            Assert.That(result.Data, Is.Not.Null);
        }

        private static async Task WaitUntilAsync(Func<bool> condition)
        {
            var timeout = DateTime.UtcNow.AddSeconds(2);
            while (!condition())
            {
                if (DateTime.UtcNow > timeout)
                    Assert.Fail("Condition was not met within the timeout");

                await Task.Delay(10);
            }
        }

        private sealed class TestSubscription : Subscription
        {
            public TestSubscription() : base(NullLogger.Instance, true)
            {
            }

            protected override Query? GetSubQuery(SocketConnection connection) => null;

            protected override Query? GetUnsubQuery(SocketConnection connection) => null;
        }
    }
}
