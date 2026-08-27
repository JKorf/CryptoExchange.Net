using CryptoExchange.Net.Objects;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using System;
using System.Collections;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.UnitTests
{
    [TestFixture]
    public class RequestCoalescerTests
    {
        private static readonly TimeSpan _testTimeout = TimeSpan.FromSeconds(5);

        [Test]
        public async Task ExecuteAsync_WithIdenticalRequests_ShouldExecuteRequestOnce()
        {
            var coalescer = CreateCoalescer();
            var key = CreateKey();
            var requestCompletion = CreateCompletionSource<string>();
            var requestStarted = CreateCompletionSource<bool>();
            var requestCount = 0;

            async Task<string> ExecuteRequest(CancellationToken cancellationToken)
            {
                Interlocked.Increment(ref requestCount);
                requestStarted.TrySetResult(true);
                return await requestCompletion.Task;
            }

            var first = coalescer.ExecuteAsync(key, ExecuteRequest, () => "canceled", CancellationToken.None);
            await requestStarted.Task.WaitAsync(_testTimeout);
            var second = coalescer.ExecuteAsync(key, ExecuteRequest, () => "canceled", CancellationToken.None);

            Assert.That(requestCount, Is.EqualTo(1));

            requestCompletion.SetResult("result");

            Assert.That(await first.WaitAsync(_testTimeout), Is.EqualTo("result"));
            Assert.That(await second.WaitAsync(_testTimeout), Is.EqualTo("result"));
            Assert.That(requestCount, Is.EqualTo(1));
        }

        [Test]
        public async Task ExecuteAsync_WithDifferentKeys_ShouldExecuteEachRequest()
        {
            var coalescer = CreateCoalescer();
            var firstCompletion = CreateCompletionSource<string>();
            var secondCompletion = CreateCompletionSource<string>();
            var firstStarted = CreateCompletionSource<bool>();
            var secondStarted = CreateCompletionSource<bool>();
            var requestCount = 0;

            var first = coalescer.ExecuteAsync(
                CreateKey(parameters: "symbol=ETH"),
                async cancellationToken =>
                {
                    Interlocked.Increment(ref requestCount);
                    firstStarted.TrySetResult(true);
                    return await firstCompletion.Task;
                },
                () => "canceled",
                CancellationToken.None);

            var second = coalescer.ExecuteAsync(
                CreateKey(parameters: "symbol=BTC"),
                async cancellationToken =>
                {
                    Interlocked.Increment(ref requestCount);
                    secondStarted.TrySetResult(true);
                    return await secondCompletion.Task;
                },
                () => "canceled",
                CancellationToken.None);

            await Task.WhenAll(firstStarted.Task, secondStarted.Task).WaitAsync(_testTimeout);
            Assert.That(requestCount, Is.EqualTo(2));

            firstCompletion.SetResult("first");
            secondCompletion.SetResult("second");

            Assert.That(await first.WaitAsync(_testTimeout), Is.EqualTo("first"));
            Assert.That(await second.WaitAsync(_testTimeout), Is.EqualTo("second"));
        }

        [Test]
        public async Task ExecuteAsync_WhenOneListenerCancels_ShouldKeepSharedRequestRunning()
        {
            var coalescer = CreateCoalescer();
            var requestCompletion = CreateCompletionSource<string>();
            var requestStarted = CreateCompletionSource<CancellationToken>();
            var requestCount = 0;
            using var firstCancellationSource = new CancellationTokenSource();

            async Task<string> ExecuteRequest(CancellationToken cancellationToken)
            {
                Interlocked.Increment(ref requestCount);
                requestStarted.TrySetResult(cancellationToken);
                return await requestCompletion.Task;
            }

            var first = coalescer.ExecuteAsync(CreateKey(), ExecuteRequest, () => "canceled", firstCancellationSource.Token);
            var requestCancellationToken = await requestStarted.Task.WaitAsync(_testTimeout);
            var second = coalescer.ExecuteAsync(CreateKey(), ExecuteRequest, () => "canceled", CancellationToken.None);

            firstCancellationSource.Cancel();

            Assert.That(await first.WaitAsync(_testTimeout), Is.EqualTo("canceled"));
            Assert.That(requestCancellationToken.IsCancellationRequested, Is.False);
            Assert.That(requestCount, Is.EqualTo(1));

            requestCompletion.SetResult("result");

            Assert.That(await second.WaitAsync(_testTimeout), Is.EqualTo("result"));
        }

        [Test]
        public async Task ExecuteAsync_WhenAllListenersCancel_ShouldCancelSharedRequest()
        {
            var coalescer = CreateCoalescer();
            var requestStarted = CreateCompletionSource<bool>();
            var requestCanceled = CreateCompletionSource<bool>();
            using var firstCancellationSource = new CancellationTokenSource();
            using var secondCancellationSource = new CancellationTokenSource();

            async Task<string> ExecuteRequest(CancellationToken cancellationToken)
            {
                using (cancellationToken.Register(() => requestCanceled.TrySetResult(true)))
                {
                    requestStarted.TrySetResult(true);
                    await requestCanceled.Task;
                    return "request canceled";
                }
            }

            var first = coalescer.ExecuteAsync(CreateKey(), ExecuteRequest, () => "listener canceled", firstCancellationSource.Token);
            await requestStarted.Task.WaitAsync(_testTimeout);
            var second = coalescer.ExecuteAsync(CreateKey(), ExecuteRequest, () => "listener canceled", secondCancellationSource.Token);

            firstCancellationSource.Cancel();
            Assert.That(await first.WaitAsync(_testTimeout), Is.EqualTo("listener canceled"));
            Assert.That(requestCanceled.Task.IsCompleted, Is.False);

            secondCancellationSource.Cancel();

            Assert.That(await second.WaitAsync(_testTimeout), Is.EqualTo("listener canceled"));
            Assert.That(await requestCanceled.Task.WaitAsync(_testTimeout), Is.True);
        }

        [Test]
        public async Task ExecuteAsync_WithAlreadyCanceledListener_ShouldNotStartRequest()
        {
            var coalescer = CreateCoalescer();
            var requestCount = 0;
            using var cancellationSource = new CancellationTokenSource();
            cancellationSource.Cancel();

            var result = await coalescer.ExecuteAsync(
                CreateKey(),
                cancellationToken =>
                {
                    Interlocked.Increment(ref requestCount);
                    return Task.FromResult("result");
                },
                () => "canceled",
                cancellationSource.Token);

            Assert.That(result, Is.EqualTo("canceled"));
            Assert.That(requestCount, Is.Zero);
        }

        [Test]
        public async Task ExecuteAsync_AfterRequestCompletes_ShouldStartNewRequest()
        {
            var coalescer = CreateCoalescer();
            var requestCount = 0;

            Task<int> ExecuteRequest(CancellationToken cancellationToken)
                => Task.FromResult(Interlocked.Increment(ref requestCount));

            var first = await coalescer.ExecuteAsync(CreateKey(typeof(int)), ExecuteRequest, () => -1, CancellationToken.None);
            var second = await coalescer.ExecuteAsync(CreateKey(typeof(int)), ExecuteRequest, () => -1, CancellationToken.None);

            Assert.That(first, Is.EqualTo(1));
            Assert.That(second, Is.EqualTo(2));
            Assert.That(requestCount, Is.EqualTo(2));
        }

        [Test]
        public void ExecuteAsync_AfterRequestFails_ShouldStartNewRequest()
        {
            var coalescer = CreateCoalescer();
            var requestCount = 0;

            Task<string> ExecuteRequest(CancellationToken cancellationToken)
            {
                if (Interlocked.Increment(ref requestCount) == 1)
                    return Task.FromException<string>(new InvalidOperationException("Request failed"));

                return Task.FromResult("result");
            }

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await coalescer.ExecuteAsync(CreateKey(), ExecuteRequest, () => "canceled", CancellationToken.None));

            Assert.That(
                coalescer.ExecuteAsync(CreateKey(), ExecuteRequest, () => "canceled", CancellationToken.None).GetAwaiter().GetResult(),
                Is.EqualTo("result"));
            Assert.That(requestCount, Is.EqualTo(2));
        }

        [Test]
        public async Task ExecuteAsync_WhenAbandonedRequestCompletes_ShouldNotRemoveReplacementRequest()
        {
            var coalescer = CreateCoalescer();
            var key = CreateKey();
            var oldRequestCompletion = CreateCompletionSource<string>();
            var oldRequestStarted = CreateCompletionSource<bool>();
            var replacementCompletion = CreateCompletionSource<string>();
            var replacementStarted = CreateCompletionSource<bool>();
            var requestCount = 0;
            using var cancellationSource = new CancellationTokenSource();

            async Task<string> ExecuteOldRequest(CancellationToken cancellationToken)
            {
                Interlocked.Increment(ref requestCount);
                oldRequestStarted.TrySetResult(true);
                return await oldRequestCompletion.Task;
            }

            async Task<string> ExecuteReplacementRequest(CancellationToken cancellationToken)
            {
                Interlocked.Increment(ref requestCount);
                replacementStarted.TrySetResult(true);
                return await replacementCompletion.Task;
            }

            var abandonedListener = coalescer.ExecuteAsync(key, ExecuteOldRequest, () => "canceled", cancellationSource.Token);
            await oldRequestStarted.Task.WaitAsync(_testTimeout);
            var oldPhysicalRequest = GetPhysicalRequest(coalescer, key);

            cancellationSource.Cancel();
            Assert.That(await abandonedListener.WaitAsync(_testTimeout), Is.EqualTo("canceled"));

            var replacementListener = coalescer.ExecuteAsync(key, ExecuteReplacementRequest, () => "canceled", CancellationToken.None);
            await replacementStarted.Task.WaitAsync(_testTimeout);

            oldRequestCompletion.SetResult("old result");
            await oldPhysicalRequest.WaitAsync(_testTimeout);

            var joinedReplacementListener = coalescer.ExecuteAsync(key, ExecuteReplacementRequest, () => "canceled", CancellationToken.None);
            Assert.That(requestCount, Is.EqualTo(2));

            replacementCompletion.SetResult("replacement result");

            Assert.That(await replacementListener.WaitAsync(_testTimeout), Is.EqualTo("replacement result"));
            Assert.That(await joinedReplacementListener.WaitAsync(_testTimeout), Is.EqualTo("replacement result"));
            Assert.That(requestCount, Is.EqualTo(2));
        }

        private static RequestCoalescer CreateCoalescer()
            => new RequestCoalescer(NullLogger.Instance);

        private static RequestCoalescingKey CreateKey(Type? responseType = null, string? parameters = null)
            => new RequestCoalescingKey("GET", "https://localhost/test", parameters, responseType ?? typeof(string));

        private static TaskCompletionSource<T> CreateCompletionSource<T>()
            => new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);

        private static Task GetPhysicalRequest(RequestCoalescer coalescer, RequestCoalescingKey key)
        {
            var requestsField = typeof(RequestCoalescer).GetField("_requests", BindingFlags.Instance | BindingFlags.NonPublic)!;
            var requests = (IDictionary)requestsField.GetValue(coalescer)!;
            var entry = requests[key]!;
            var requestProperty = entry.GetType().GetProperty("Request")!;
            return ((Lazy<Task<object>>)requestProperty.GetValue(entry)!).Value;
        }
    }
}
