using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoSharp.Test
{
    [TestClass]
    public class BufferedChannelTest
    {
        [TestMethod]
        public void TwoItemSendReceiveSeq()
        {
            var channel = Channel<int>.CreateBuffered(2);

            channel.Send(1);
            channel.Send(2);

            var r1 = channel.Recv();
            var r2 = channel.Recv();

            Assert.AreEqual(1, r1);
            Assert.AreEqual(2, r2);
        }

        [TestMethod]
        public void TwoItemSendReceiveSeqOper()
        {
            var channel = Channel<int>.CreateBuffered(2);

            channel+=1;
            channel+=2;

            var r1 = -channel;
            var r2 = -channel;

            Assert.AreEqual(1, r1);
            Assert.AreEqual(2, r2);
        }

        [TestMethod]
        public void SingleItemSendReceive()
        {
            var channel = Channel<int>.CreateBuffered(2);

            var sender1 = ThreadHelper.CreateSenderThread(channel, 1, 1, 1);

            var items = new List<int>();
            var receiver1 = ThreadHelper.CreateReceiverThread(channel, 1, items);
            ThreadHelper.StartAndWait(sender1, receiver1);

            Assert.IsTrue(Enumerable.Range(1, 1).SequenceEqual(items));
        }

        [TestMethod]
        public void MultiItemSendReceive()
        {
            var channel = Channel<int>.CreateBuffered(10);
            var sender1 = ThreadHelper.CreateSenderThread(channel, 1, 1000, 1);

            var items = new List<int>();
            var receiver1 = ThreadHelper.CreateReceiverThread(channel, 1000, items);
            ThreadHelper.StartAndWait(sender1, receiver1);

            Assert.IsTrue(Enumerable.Range(1, 1000).SequenceEqual(items));
        }

        [TestMethod]
        public void MultiItemMultiSendSingleReceive()
        {
            var channel = Channel<int>.CreateBuffered(10);
            var sender1 = ThreadHelper.CreateSenderThread(channel, 1, 1000, 2);
            var sender2 = ThreadHelper.CreateSenderThread(channel, 2, 1000, 2);

            var items = new List<int>();
            var receiver1 = ThreadHelper.CreateReceiverThread(channel, 1000, items);
            ThreadHelper.StartAndWait(sender1, sender2, receiver1);

            items.Sort();
            Assert.IsTrue(Enumerable.Range(1, 1000).SequenceEqual(items));
        }

        [TestMethod]
        public void MultiItemSingleSendMultiReceive()
        {
            var channel = Channel<int>.CreateBuffered(10);
            var sender1 = ThreadHelper.CreateSenderThread(channel, 1, 1000, 1);

            var items = new List<int>();
            var receiver1 = ThreadHelper.CreateReceiverThread(channel, 500, items);
            var receiver2 = ThreadHelper.CreateReceiverThread(channel, 500, items);
            ThreadHelper.StartAndWait(sender1, receiver1, receiver2);

            items.Sort();
            Assert.IsTrue(Enumerable.Range(1, 1000).SequenceEqual(items));
        }

        [TestMethod]
        public void MultiItemMultiSendMultiReceive()
        {
            var channel = Channel<int>.CreateBuffered(10);
            var sender1 = ThreadHelper.CreateSenderThread(channel, 1, 1000, 2);
            var sender2 = ThreadHelper.CreateSenderThread(channel, 2, 1000, 2);

            var items = new List<int>();
            var receiver1 = ThreadHelper.CreateReceiverThread(channel, 500, items);
            var receiver2 = ThreadHelper.CreateReceiverThread(channel, 500, items);
            ThreadHelper.StartAndWait(sender1, sender2, receiver1, receiver2);

            items.Sort();
            Assert.IsTrue(Enumerable.Range(1, 1000).SequenceEqual(items));
        }

        [TestMethod]
        public void ManyMultiItemMultiSendMultiReceive()
        {
            var channel = Channel<int>.CreateBuffered(1000);
            var sender1 = ThreadHelper.CreateSenderThread(channel, 1, 100000, 2);
            var sender2 = ThreadHelper.CreateSenderThread(channel, 2, 100000, 2);

            var items = new List<int>();
            var receiver1 = ThreadHelper.CreateReceiverThread(channel, 50000, items);
            var receiver2 = ThreadHelper.CreateReceiverThread(channel, 50000, items);
            ThreadHelper.StartAndWait(sender1, sender2, receiver1, receiver2);

            items.Sort();
            Assert.IsTrue(Enumerable.Range(1, 100000).SequenceEqual(items));
        }

        [TestMethod]
        public void ChannelNegativeQueueSizeTest()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => Channel<int>.CreateBuffered(-1));
        }

        [TestMethod]
        public void ChannelLargeQueueSizeTest()
        {
            int upperLimit = 10000;
            var channel = Channel<int>.CreateBuffered(upperLimit);

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => Channel<int>.CreateBuffered(upperLimit + 1));
        }
    }
}
