using System;
using System.Threading;
using Xunit;

namespace Go.Test
{
    public class SelectTimeoutTest
    {
        [Fact]
        public void TimeoutExpiredSendNonBufferedTest()
        {
            var channel = Channel<int>.CreateNonBuffered();
            var timeout = false;
            Select
                .CaseSend(channel, 1)
                .Timeout(TimeSpan.FromSeconds(0.1), () => { timeout = true; });

            Assert.True(timeout);
        }

        [Fact]
        public void TimeoutExpiredRecvNonBufferedTest()
        {
            var channel = Channel<int>.CreateNonBuffered();
            var timeout = false;
            Select
                .CaseRecv(channel, (a) => { })
                .Timeout(TimeSpan.FromSeconds(0.1), () => { timeout = true; });

            Assert.True(timeout);
        }

        [Fact]
        public void TimeoutNotExpiredSendNonBufferedTest()
        {
            var channel = Channel<int>.CreateNonBuffered();
            var timeout = false;
            new Thread(() => { channel.Recv(); }).Start();

            Select
                .CaseSend(channel, 1)
                .Timeout(TimeSpan.FromSeconds(0.1), () => { timeout = true; });

            Assert.False(timeout);
        }

        [Fact]
        public void TimeoutNotExpiredRecvNonBufferedTest()
        {
            var channel = Channel<int>.CreateNonBuffered();
            var timeout = false;
            new Thread(() => { channel.Send(1); }).Start();

            Select
                .CaseRecv(channel, (a) => { })
                .Timeout(TimeSpan.FromSeconds(0.1), () => { timeout = true; });

            Assert.False(timeout);
        }
    }
}
