using System.Threading;
using Xunit;

namespace Go.Test
{
    public class CloseBehaviourTest
    {
        [Fact]
        public void SendWhileCloseTestNonBufferedTest()
        {
            var channel = Channel<int>.CreateNonBuffered();
            var startedEvent = new AutoResetEvent(false);
            new Thread(() =>
            {
                startedEvent.Set();
                Thread.Sleep(100);
                channel.Close();
            }).Start();
            startedEvent.WaitOne();
            Assert.Throws<ChannelClosedException>(() => channel.Send(3));
        }

        [Fact]
        public void RecvWhileCloseTestNonBufferedTest()
        {
            var channel = Channel<int>.CreateNonBuffered();
            var startedEvent = new AutoResetEvent(false);
            new Thread(() =>
            {
                startedEvent.Set();
                Thread.Sleep(100);
                channel.Close();
            }).Start();
            startedEvent.WaitOne();
            Assert.Throws<ChannelClosedException>(() => channel.Recv());
        }

        [Fact]
        public void SendWhileCloseTestBufferedTest()
        {
            var channel = Channel<int>.CreateBuffered(2);
            channel.Send(1);
            channel.Send(2);
            var startedEvent = new AutoResetEvent(false);
            new Thread(() =>
            {
                startedEvent.Set();
                Thread.Sleep(100);
                channel.Close();
            }).Start();
            startedEvent.WaitOne();
            Assert.Throws<ChannelClosedException>(() => channel.Send(3));
        }

        [Fact]
        public void RecvWhileCloseTestBufferedTest()
        {
            var channel = Channel<int>.CreateBuffered(2);
            var startedEvent = new AutoResetEvent(false);
            new Thread(() =>
            {
                startedEvent.Set();
                Thread.Sleep(100);
                channel.Close();
            }).Start();
            startedEvent.WaitOne();
            Assert.Throws<ChannelClosedException>(() => channel.Recv());
        }
    }
}
