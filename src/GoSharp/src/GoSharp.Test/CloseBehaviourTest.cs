using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoSharp.Test
{
    [TestClass]
    public class CloseBehaviourTest
    {
        [TestMethod]
        public void SendWhileCloseTestNonBufferedTest()
        {
            var channel = Channel<int>.CreateNonBuffered();
            var startedEvent = new AutoResetEvent(false);
            new Thread(() =>
            {
                startedEvent.Set();
                Thread.Sleep(10);
                channel.Close();
            }).Start();
            startedEvent.WaitOne();
            Assert.ThrowsException<ChannelClosedException>(() => channel.Send(3));
        }

        [TestMethod]
        public void RecvWhileCloseTestNonBufferedTest()
        {
            var channel = Channel<int>.CreateNonBuffered();
            var startedEvent = new AutoResetEvent(false);
            new Thread(() =>
            {
                startedEvent.Set();
                Thread.Sleep(10);
                channel.Close();
            }).Start();
            startedEvent.WaitOne();
            Assert.ThrowsException<ChannelClosedException>(() => channel.Recv());
        }

        [TestMethod]
        public void SendWhileCloseTestBufferedTest()
        {
            var channel = Channel<int>.CreateBuffered(2);
            channel.Send(1);
            channel.Send(2);
            var startedEvent = new AutoResetEvent(false);
            new Thread(() =>
            {
                startedEvent.Set();
                Thread.Sleep(10);
                channel.Close();
            }).Start();
            startedEvent.WaitOne();
            Assert.ThrowsException<ChannelClosedException>(() => channel.Send(3));
        }

        [TestMethod]
        public void RecvWhileCloseTestBufferedTest()
        {
            var channel = Channel<int>.CreateBuffered(2);
            var startedEvent = new AutoResetEvent(false);
            new Thread(() =>
            {
                startedEvent.Set();
                Thread.Sleep(10);
                channel.Close();
            }).Start();
            startedEvent.WaitOne();
            Assert.ThrowsException<ChannelClosedException>(() => channel.Recv());
        }
    }
}
