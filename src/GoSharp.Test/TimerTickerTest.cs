using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoSharp.Test
{
    [TestClass]
    public class TimerTickerTest
    {
        #region Timer tests

        [TestMethod]
        public void StartTimerTest()
        {
            var start = DateTime.Now;
            var timer = new Timer(TimeSpan.FromMilliseconds(20));

            timer.Start();

            var now = timer.Recv();
            Assert.IsTrue(IsInDiff(start, now, 18, 22));
        }

        [TestMethod]
        public void CloseTimerTest()
        {
            var timer = new Timer(TimeSpan.FromMilliseconds(20));

            timer.Start();

            timer.Close();

            Assert.ThrowsException<ChannelClosedException>(() => timer.Start());
        }

        [TestMethod]
        public void StartTwiceTimerTest()
        {
            var timer = new Timer(TimeSpan.FromMilliseconds(20));

            timer.Start();
            Assert.ThrowsException<InvalidOperationException>(() => timer.Start());
        }

        [TestMethod]
        public async Task StartTimerTestAsync()
        {
            var start = DateTime.Now;
            var timer = new Timer(TimeSpan.FromMilliseconds(20));

            timer.Start();

            var now = await timer.RecvAsync();
            Assert.IsTrue(IsInDiff(start, now, 18, 22));
        }

        [TestMethod]
        public void StartStopStartTimerTest()
        {
            var timer = new Timer(TimeSpan.FromMilliseconds(20));

            timer.Start();
            Thread.Sleep(30);
            timer.Stop();
            var start = DateTime.Now;
            timer.Start();

            var now = timer.Recv();
            timer.Close();
            Assert.IsTrue(IsInDiff(start, now, 18, 22));
        }

        [TestMethod]
        public void StartStopStartFastTimerTest()
        {
            var timer = new Timer(TimeSpan.FromMilliseconds(20));

            timer.Start();
            timer.Stop();
            var start = DateTime.Now;
            timer.Start();

            var now = timer.Recv();
            timer.Close();
            Assert.IsTrue(IsInDiff(start, now, 18, 22));
        }

        #endregion

        #region Ticker tests

        [TestMethod]
        public void StartTickerTest()
        {
            var start = DateTime.Now;
            var ticker = new Ticker(TimeSpan.FromMilliseconds(20));

            ticker.Start();

            var now = ticker.Recv();
            ticker.Close();
            Assert.IsTrue(IsInDiff(start, now, 18, 22));
        }

        [TestMethod]
        public void MultiTicksTest()
        {
            var start = DateTime.Now;
            var ticker = new Ticker(TimeSpan.FromMilliseconds(20));

            ticker.Start();

            var now = ticker.Recv();
            Assert.IsTrue(IsInDiff(start, now, 10, 35));
            var last = now;
            now = ticker.Recv();
            Assert.IsTrue(IsInDiff(last, now, 10, 35));
            last = now;
            now = ticker.Recv();
            Assert.IsTrue(IsInDiff(last, now, 10, 35));
            last = now;
            now = ticker.Recv();
            ticker.Close();
            Assert.IsTrue(IsInDiff(last, now, 10, 35));
        }

        [TestMethod]
        public void MultiTicksSkipTest()
        {
            var start = DateTime.Now;
            var ticker = new Ticker(TimeSpan.FromMilliseconds(20));

            ticker.Start();

            var now = ticker.Recv();
            Assert.IsTrue(IsInDiff(start, now, 18, 22));
            Thread.Sleep(21);
            Thread.Sleep(21);
            var last = now;
            now = ticker.Recv();
            Assert.IsTrue(IsInDiff(last, now, 18, 32));
            last = now;
            now = ticker.Recv();
            ticker.Close();
            Assert.IsTrue(IsInDiff(last, now, 18, 32));
        }

        [TestMethod]
        public void CloseTickerTest()
        {
            var ticker = new Ticker(TimeSpan.FromMilliseconds(20));

            ticker.Start();

            ticker.Close();

            Assert.ThrowsException<ChannelClosedException>(() => ticker.Start());
        }

        [TestMethod]
        public void StartTwiceTickerTest()
        {
            var ticker = new Ticker(TimeSpan.FromMilliseconds(20));

            ticker.Start();
            Assert.ThrowsException<InvalidOperationException>(() => ticker.Start());
        }

        [TestMethod]
        public async Task StartTckerTestAsync()
        {
            var start = DateTime.Now;
            var ticker = new Ticker(TimeSpan.FromMilliseconds(20));

            ticker.Start();

            var now = await ticker.RecvAsync();
            ticker.Close();
            Assert.IsTrue(start.AddMilliseconds(19) < now);
        }

        [TestMethod]
        public void StartStopStartTickerTest()
        {
            var ticker = new Ticker(TimeSpan.FromMilliseconds(20));

            ticker.Start();
            Thread.Sleep(30);
            ticker.Stop();
            var start = DateTime.Now;
            ticker.Start();

            var now = ticker.Recv();
            ticker.Close();
            Assert.IsTrue(IsInDiff(start, now, 18, 22));
        }

        [TestMethod]
        public void StartStopStartFastTickerTest()
        {
            var ticker = new Ticker(TimeSpan.FromMilliseconds(20));

            ticker.Start();
            ticker.Stop();
            var start = DateTime.Now;
            ticker.Start();

            var now = ticker.Recv();
            ticker.Close();
            Assert.IsTrue(IsInDiff(start, now, 18, 22));
        }

        #endregion

        #region Helper methods

        private bool IsInDiff(DateTime start, DateTime now, int diffMs, int diffDiffMs)
        {
            return start.AddMilliseconds(diffMs) < now && start.AddMilliseconds(diffMs + diffDiffMs) > now;
        }

        #endregion
    }
}
