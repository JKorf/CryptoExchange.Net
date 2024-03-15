using CryptoExchange.Net.Objects;
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
    public class AsyncResetEventTests
    {
        [Test]
        public async Task InitialFalseAndResetFalse_Should_BothCompleteAfterSingleSet()
        {
            var evnt = new AsyncResetEvent(false, false);

            var waiter1 = evnt.WaitAsync();
            var waiter2 = evnt.WaitAsync();

            evnt.Set();

            var result1 = await waiter1;
            var result2 = await waiter2;

            Assert.That(result1);
            Assert.That(result2);
        }

        [Test]
        public async Task InitialTrueAndResetFalse_Should_BothCompleteImmediately()
        {
            var evnt = new AsyncResetEvent(true, false);

            var waiter1 = evnt.WaitAsync();
            var waiter2 = evnt.WaitAsync();

            var result1 = await waiter1;
            var result2 = await waiter2;

            Assert.That(result1);
            Assert.That(result2);
        }

        [Test]
        public async Task InitialFalseAndResetTrue_Should_CompleteEachAfterASet()
        {
            var evnt = new AsyncResetEvent(false, true);

            var waiter1 = evnt.WaitAsync();
            var waiter2 = evnt.WaitAsync();

            evnt.Set();

            var result1 = await waiter1;

            Assert.That(result1);
            Assert.That(waiter2.Status != TaskStatus.RanToCompletion);

            evnt.Set();

            var result2 = await waiter2;

            Assert.That(result2);
        }

        [Test]
        public async Task InitialTrueAndResetTrue_Should_CompleteFirstImmediatelyAndSecondAfterSet()
        {
            var evnt = new AsyncResetEvent(true, true);

            var waiter1 = evnt.WaitAsync();
            var waiter2 = evnt.WaitAsync();

            var result1 = await waiter1;

            Assert.That(result1);
            Assert.That(waiter2.Status != TaskStatus.RanToCompletion);
            evnt.Set();

            var result2 = await waiter2;

            Assert.That(result2);
        }

        [Test]
        public async Task Awaiting10TimesOnSameEvent_Should_AllCompleteAfter10Sets()
        {
            var evnt = new AsyncResetEvent(false, true);

            var waiters = new List<Task<bool>>();
            for(var i = 0; i < 10; i++)
            {
                waiters.Add(evnt.WaitAsync());
            }

            List<bool> results = null;
            var resultsWaiter = Task.Run(async () =>
            {
                await Task.WhenAll(waiters);
                results = waiters.Select(w => w.Result).ToList();
            });

            for(var i = 1; i <= 10; i++)
            {
                evnt.Set();
                Assert.That(10 - i == waiters.Count(w => w.Status != TaskStatus.RanToCompletion));
            }

            await resultsWaiter;

            Assert.That(10 == results.Count(r => r));
        }

        [Test]
        public async Task WaitingShorterThanTimeout_Should_ReturnTrue()
        {
            var evnt = new AsyncResetEvent(false, true);

            var waiter1 = evnt.WaitAsync(TimeSpan.FromMilliseconds(100));
            await Task.Delay(50);
            evnt.Set();

            var result1 = await waiter1;

            Assert.That(result1);
        }

        [Test]
        public async Task WaitingLongerThanTimeout_Should_ReturnFalse()
        {
            var evnt = new AsyncResetEvent(false, true);

            var waiter1 = evnt.WaitAsync(TimeSpan.FromMilliseconds(100));

            var result1 = await waiter1;

            ClassicAssert.False(result1);
        }
    }
}
