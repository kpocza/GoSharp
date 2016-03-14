using System.Collections.Generic;
using System.Linq;
using Xunit;
using Go;

namespace Go.Test
{
    public class SelectNonBlockingTest
    {
        #region Non buffered tests

        #region Recv

        [Fact]
        public void SelectRecvMultiItemSingleSendSingleSelectNonBuffered()
        {
            var channel = Channel<int>.CreateNonBuffered();
            var sender1 = ThreadHelper.CreateSenderThread(channel, 1, 1000, 1);

            sender1.Start();
            var items = new List<int>();
            ThreadHelper.Repeat(() => items.Count == 1000, () =>
            {
                Select
                    .CaseRecv(channel, a => items.Add(a))
                    .Default(() => { });
            });

            Assert.True(Enumerable.Range(1, 1000).SequenceEqual(items));
        }

        [Fact]
        public void SelectRecvMultiItemMultiSendSingleSelectNonBuffered()
        {
            var channel = Channel<int>.CreateNonBuffered();
            var sender1 = ThreadHelper.CreateSenderThread(channel, 1, 1000, 2);
            var sender2 = ThreadHelper.CreateSenderThread(channel, 2, 1000, 2);

            sender1.Start();
            sender2.Start();
            var items = new List<int>();
            ThreadHelper.Repeat(() => items.Count == 1000, () =>
            {
                Select
                    .CaseRecv(channel, a => items.Add(a))
                    .Default(() => { });
            });

            items.Sort();
            Assert.True(Enumerable.Range(1, 1000).SequenceEqual(items));
        }

        [Fact]
        public void SelectRecvMultiItemMultiSendMultiChannelSingleSelectNonBuffered()
        {
            var channel1 = Channel<int>.CreateNonBuffered();
            var channel2 = Channel<int>.CreateNonBuffered();
            var sender1 = ThreadHelper.CreateSenderThread(channel1, 1, 1000, 2);
            var sender2 = ThreadHelper.CreateSenderThread(channel2, 2, 1000, 2);

            sender1.Start();
            sender2.Start();
            var items = new List<int>();
            ThreadHelper.Repeat(() => items.Count == 1000, () =>
            {
                Select
                    .CaseRecv(channel1, a => items.Add(a))
                    .CaseRecv(channel2, a => items.Add(a))
                    .Default(() => { });
            });

            items.Sort();
            Assert.True(Enumerable.Range(1, 1000).SequenceEqual(items));
        }

        #endregion

        #region Send

        [Fact]
        public void SelectSendMultiItemSingleSendSingleSelectNonBuffered()
        {
            var channel = Channel<int>.CreateNonBuffered();

            var items = new List<int>();
            var thread1 = ThreadHelper.CreateReceiverThread(channel, 1000, items);

            ThreadHelper.Start(thread1);

            int i = 1;
            ThreadHelper.Repeat(() => items.Count == 1000, () =>
            {
                Select
                    .CaseSend(channel, i++)
                    .Default(() => { });
            });

            ThreadHelper.Wait(thread1);

            Assert.True(items.Distinct().Count() == items.Count);
        }

        [Fact]
        public void SelectSendMultiItemMultiSendSingleSelectNonBuffered()
        {
            var channel = Channel<int>.CreateNonBuffered();

            var items = new List<int>();
            var thread1 = ThreadHelper.CreateReceiverThread(channel, 500, items);
            var thread2 = ThreadHelper.CreateReceiverThread(channel, 500, items);

            ThreadHelper.Start(thread1, thread2);

            int i = 1;
            ThreadHelper.Repeat(() => items.Count == 1000, () =>
            {
                Select
                    .CaseSend(channel, i++)
                    .Default(() => { });
            });

            ThreadHelper.Wait(thread1, thread2);

            items.Sort();
            Assert.True(items.Distinct().Count() == items.Count);
        }

        [Fact]
        public void SelectSendMultiItemMultiSendMultiChannelSingleSelectNonBuffered()
        {
            var channel1 = Channel<int>.CreateNonBuffered();
            var channel2 = Channel<int>.CreateNonBuffered();

            var items1 = new List<int>();
            var items2 = new List<int>();
            var thread1 = ThreadHelper.CreateReceiverThread(channel1, 500, items1);
            var thread2 = ThreadHelper.CreateReceiverThread(channel2, 500, items2);

            ThreadHelper.Start(thread1, thread2);

            int i = 1;
            ThreadHelper.Repeat(() => items1.Count == 500 && items2.Count == 500, () =>
            {
                Select
                    .CaseSend(channel1, i++)
                    .CaseSend(channel2, i++)
                    .Default(() => { });
            });

            ThreadHelper.Wait(thread1, thread2);

            Assert.Equal(500, items1.Count);
            Assert.Equal(500, items1.Count);
            var items = items1.Union(items2).ToList();
            Assert.True(items.Count == items.Distinct().Count());
        }

        [Fact]
        public void SelectSendMultiItemMultiSendMultiChannelSingleSelectNonBufferedLambda()
        {
            var channel1 = Channel<int>.CreateNonBuffered();
            var channel2 = Channel<int>.CreateNonBuffered();

            var items1 = new List<int>();
            var items2 = new List<int>();
            var thread1 = ThreadHelper.CreateReceiverThread(channel1, 500, items1);
            var thread2 = ThreadHelper.CreateReceiverThread(channel2, 500, items2);

            ThreadHelper.Start(thread1, thread2);

            int e = 2-2;
            int o = 1-2;
            ThreadHelper.Repeat(() => items1.Count == 500 && items2.Count == 500, () =>
            {
                Select
                    .CaseSend(channel1, () => e+=2)
                    .CaseSend(channel2, () => o+=2)
                    .Default(() => { });
            });

            ThreadHelper.Wait(thread1, thread2);

            Assert.Equal(500, items1.Count);
            Assert.Equal(500, items1.Count);
            var items = items1.Union(items2).ToList();
            items.Sort();
            Assert.True(Enumerable.Range(1, 1000).SequenceEqual(items));
        }

        #endregion

        #endregion

        #region Buffered tests

        #region Recv

        [Fact]
        public void SelectRecvMultiItemSingleSendSingleSelectBuffered()
        {
            var channel = Channel<int>.CreateBuffered(1);
            var sender1 = ThreadHelper.CreateSenderThread(channel, 1, 1000, 1);

            sender1.Start();
            var items = new List<int>();
            ThreadHelper.Repeat(() => items.Count == 1000, () =>
            {
                Select
                    .CaseRecv(channel, a => items.Add(a))
                    .Default(() => { });
            });

            Assert.True(Enumerable.Range(1, 1000).SequenceEqual(items));
        }

        [Fact]
        public void SelectRecvMultiItemMultiSendSingleSelectBuffered()
        {
            var channel = Channel<int>.CreateBuffered(2);
            var sender1 = ThreadHelper.CreateSenderThread(channel, 1, 1000, 2);
            var sender2 = ThreadHelper.CreateSenderThread(channel, 2, 1000, 2);

            sender1.Start();
            sender2.Start();
            var items = new List<int>();
            ThreadHelper.Repeat(() => items.Count == 1000, () =>
            {
                Select
                    .CaseRecv(channel, a => items.Add(a))
                    .Default(() => { });
            });

            items.Sort();
            Assert.True(Enumerable.Range(1, 1000).SequenceEqual(items));
        }

        [Fact]
        public void SelectRecvMultiItemMultiSendMultiChannelSingleSelectBuffered()
        {
            var channel1 = Channel<int>.CreateBuffered(2);
            var channel2 = Channel<int>.CreateBuffered(2);
            var sender1 = ThreadHelper.CreateSenderThread(channel1, 1, 1000, 2);
            var sender2 = ThreadHelper.CreateSenderThread(channel2, 2, 1000, 2);

            sender1.Start();
            sender2.Start();
            var items = new List<int>();
            ThreadHelper.Repeat(() => items.Count == 1000, () =>
            {
                Select
                    .CaseRecv(channel1, a => items.Add(a))
                    .CaseRecv(channel2, a => items.Add(a))
                    .Default(() => { });
            });

            items.Sort();
            Assert.True(Enumerable.Range(1, 1000).SequenceEqual(items));
        }

        #endregion

        #region Send

        [Fact]
        public void SelectSendMultiItemSingleSendSingleSelectBuffered()
        {
            var channel = Channel<int>.CreateBuffered(3);

            var items = new List<int>();
            var thread1 = ThreadHelper.CreateReceiverThread(channel, 1000, items);

            ThreadHelper.Start(thread1);

            int i = 1;
            ThreadHelper.Repeat(() => items.Count == 1000, () =>
            {
                Select
                    .CaseSend(channel, i++)
                    .Default(() => { });
            });

            ThreadHelper.Wait(thread1);

            Assert.True(items.Distinct().Count() == items.Count);
        }

        [Fact]
        public void SelectSendMultiItemMultiSendSingleSelectBuffered()
        {
            var channel = Channel<int>.CreateBuffered(2);

            var items = new List<int>();
            var thread1 = ThreadHelper.CreateReceiverThread(channel, 500, items);
            var thread2 = ThreadHelper.CreateReceiverThread(channel, 500, items);

            ThreadHelper.Start(thread1, thread2);

            int i = 1;
            ThreadHelper.Repeat(() => items.Count == 1000, () =>
            {
                Select
                    .CaseSend(channel, i++)
                    .Default(() => { });
            });

            ThreadHelper.Wait(thread1, thread2);

            items.Sort();
            Assert.True(items.Distinct().Count() == items.Count);
        }

        [Fact]
        public void SelectSendMultiItemMultiSendMultiChannelSingleSelectBuffered()
        {
            var channel1 = Channel<int>.CreateBuffered(3);
            var channel2 = Channel<int>.CreateBuffered(4);

            var items1 = new List<int>();
            var items2 = new List<int>();
            var thread1 = ThreadHelper.CreateReceiverThread(channel1, 500, items1);
            var thread2 = ThreadHelper.CreateReceiverThread(channel2, 500, items2);

            ThreadHelper.Start(thread1, thread2);

            int i = 1;
            ThreadHelper.Repeat(() => items1.Count == 500 && items2.Count == 500, () =>
            {
                Select
                    .CaseSend(channel1, i++)
                    .CaseSend(channel2, i++)
                    .Default(() => { });
            });

            ThreadHelper.Wait(thread1, thread2);

            Assert.Equal(500, items1.Count);
            Assert.Equal(500, items1.Count);
            var items = items1.Union(items2).ToList();
            Assert.True(items.Count == items.Distinct().Count());
        }

        #endregion

        #endregion
    }
}
