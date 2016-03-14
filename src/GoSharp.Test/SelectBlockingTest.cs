﻿using System.Collections.Generic;
using System.Linq;
using Xunit;
using Go;

namespace Go.Test
{
    public class SelectBlockingTest
    {
        #region Non buffered tests

        #region Recv
        [Fact]
        public void SelectRecvSingleNonBuffered()
        {
            var channel1 = Channel<int>.CreateNonBuffered();

            var thread1 = ThreadHelper.CreateSenderThread(channel1, 1, 1, 1);

            thread1.Start();

            int val = 0;
            Select
                .CaseRecv(channel1, a => val = a)
                .Go();

            Assert.Equal(1, val);
        }

        [Fact]
        public void SelectRecvMultiNonBuffered()
        {
            var channel1 = Channel<int>.CreateNonBuffered();
            var channel2 = Channel<int>.CreateNonBuffered();

            var thread1 = ThreadHelper.CreateSenderThread(channel1, 1, 1, 1);
            var thread2 = ThreadHelper.CreateSenderThread(channel2, 2, 2, 1);

            thread1.Start();
            thread2.Start();

            int val1 = 0;
            int val2 = 0;
            Select
                .CaseRecv(channel1, a => val1 = a)
                .CaseRecv(channel2, b => val2 = b)
                .Go();

            Assert.True(val1 == 0 || val2 == 0);
            Assert.False(val1 == 0 && val2 == 0);

            Select
                .CaseRecv(channel1, a => val1 = a)
                .CaseRecv(channel2, b => val2 = b)
                .Go();

            Assert.Equal(1, val1);
            Assert.Equal(2, val2);
        }

        [Fact]
        public void SelectRecvMultiItemSingleSendSingleSelectNonBuffered()
        {
            var channel = Channel<int>.CreateNonBuffered();
            var sender1 = ThreadHelper.CreateSenderThread(channel, 1, 1000, 1);

            sender1.Start();
            var items = new List<int>();
            ThreadHelper.Repeat(1000, () =>
            {
                Select
                    .CaseRecv(channel, a => items.Add(a))
                    .Go();
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
            ThreadHelper.Repeat(1000, () =>
            {
                Select
                    .CaseRecv(channel, a => items.Add(a))
                    .Go();
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
            ThreadHelper.Repeat(1000, () =>
            {
                Select
                    .CaseRecv(channel1, a => items.Add(a))
                    .CaseRecv(channel2, a => items.Add(a))
                    .Go();
            });

            items.Sort();
            Assert.True(Enumerable.Range(1, 1000).SequenceEqual(items));
        }

        #endregion

        #region Send

        [Fact]
        public void SelectSendSingleNonBuffered()
        {
            var channel1 = Channel<int>.CreateNonBuffered();

            var items = new List<int>();
            var thread1 = ThreadHelper.CreateReceiverThread(channel1, 1, items);

            ThreadHelper.Start(thread1);

            Select
                .CaseSend(channel1, 1)
                .Go();

            ThreadHelper.Wait(thread1);

            Assert.Equal(1, items.Single());
        }

        [Fact]
        public void SelectSendMultiNonBuffered()
        {
            var channel1 = Channel<int>.CreateNonBuffered();
            var channel2 = Channel<int>.CreateNonBuffered();

            var items = new List<int>();
            var thread1 = ThreadHelper.CreateReceiverThread(channel1, 1, items);
            var thread2 = ThreadHelper.CreateReceiverThread(channel2, 1, items);

            ThreadHelper.Start(thread1, thread2);

            Select
                .CaseSend(channel1, 1)
                .CaseSend(channel2, 2)
                .Go();

            Select
                .CaseSend(channel1, 1)
                .CaseSend(channel2, 2)
                .Go();

            ThreadHelper.Wait(thread1, thread2);

            items.Sort();
            Assert.Equal(Enumerable.Range(1, 2), items);
        }

        [Fact]
        public void SelectSendMultiItemSingleSendSingleSelectNonBuffered()
        {
            var channel = Channel<int>.CreateNonBuffered();

            var items = new List<int>();
            var thread1 = ThreadHelper.CreateReceiverThread(channel, 1000, items);

            ThreadHelper.Start(thread1);

            int i = 1;
            ThreadHelper.Repeat(1000, () =>
            {
                Select
                    .CaseSend(channel, i++)
                    .Go();
            });

            ThreadHelper.Wait(thread1);

            Assert.True(Enumerable.Range(1, 1000).SequenceEqual(items));
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
            ThreadHelper.Repeat(1000, () =>
            {
                Select
                    .CaseSend(channel, i++)
                    .Go();
            });

            ThreadHelper.Wait(thread1, thread2);

            items.Sort();
            Assert.True(Enumerable.Range(1, 1000).SequenceEqual(items));
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
            ThreadHelper.Repeat(1000, () =>
            {
                Select
                    .CaseSend(channel1, i++)
                    .CaseSend(channel2, i++)
                    .Go();
            });

            ThreadHelper.Wait(thread1, thread2);

            Assert.Equal(500, items1.Count);
            Assert.Equal(500, items1.Count);
            var items = items1.Union(items2).ToList();
            Assert.True(items.Count == items.Distinct().Count());
        }

        #endregion

        #endregion

        #region Buffered tests

        #region Recv

        [Fact]
        public void SelectRecvSingleBuffered()
        {
            var channel1 = Channel<int>.CreateBuffered(2);

            var thread1 = ThreadHelper.CreateSenderThread(channel1, 1, 1, 1);

            thread1.Start();

            int val = 0;
            Select
                .CaseRecv(channel1, a => val = a)
                .Go();

            Assert.Equal(1, val);
        }

        [Fact]
        public void SelectRecvMultiBuffered()
        {
            var channel1 = Channel<int>.CreateBuffered(2);
            var channel2 = Channel<int>.CreateBuffered(3);

            var thread1 = ThreadHelper.CreateSenderThread(channel1, 1, 1, 1);
            var thread2 = ThreadHelper.CreateSenderThread(channel2, 2, 2, 1);

            thread1.Start();
            thread2.Start();

            int val1 = 0;
            int val2 = 0;
            Select
                .CaseRecv(channel1, a => val1 = a)
                .CaseRecv(channel2, b => val2 = b)
                .Go();

            Assert.True(val1 == 0 || val2 == 0);
            Assert.False(val1 == 0 && val2 == 0);

            Select
                .CaseRecv(channel1, a => val1 = a)
                .CaseRecv(channel2, b => val2 = b)
                .Go();

            Assert.Equal(1, val1);
            Assert.Equal(2, val2);
        }

        [Fact]
        public void SelectRecvMultiItemSingleSendSingleSelectBuffered()
        {
            var channel = Channel<int>.CreateBuffered(1);
            var sender1 = ThreadHelper.CreateSenderThread(channel, 1, 1000, 1);

            sender1.Start();
            var items = new List<int>();
            ThreadHelper.Repeat(1000, () =>
            {
                Select
                    .CaseRecv(channel, a => items.Add(a))
                    .Go();
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
            ThreadHelper.Repeat(1000, () =>
            {
                Select
                    .CaseRecv(channel, a => items.Add(a))
                    .Go();
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
            ThreadHelper.Repeat(1000, () =>
            {
                Select
                    .CaseRecv(channel1, a => items.Add(a))
                    .CaseRecv(channel2, a => items.Add(a))
                    .Go();
            });

            items.Sort();
            Assert.True(Enumerable.Range(1, 1000).SequenceEqual(items));
        }

        #endregion

        #region Send

        [Fact]
        public void SelectSendSingleBuffered()
        {
            var channel1 = Channel<int>.CreateBuffered(2);

            var items = new List<int>();
            var thread1 = ThreadHelper.CreateReceiverThread(channel1, 1, items);

            ThreadHelper.Start(thread1);

            Select
                .CaseSend(channel1, 1)
                .Go();

            ThreadHelper.Wait(thread1);

            Assert.Equal(1, items.Single());
        }

        [Fact]
        public void SelectSendMultiBuffered()
        {
            var channel1 = Channel<int>.CreateBuffered(2);
            var channel2 = Channel<int>.CreateBuffered(3);

            channel1.Send(1);
            channel1.Send(1);
            channel2.Send(2);
            channel2.Send(2);
            channel2.Send(2);

            var items = new List<int>();
            var thread1 = ThreadHelper.CreateReceiverThread(channel1, 1, items);

            ThreadHelper.Start(thread1);

            Select
                .CaseSend(channel1, 1)
                .CaseSend(channel2, 2)
                .Go();

            ThreadHelper.Wait(thread1);

            var val = items.Single();
            Assert.True(val == 1 || val == 2);

            var thread2 = ThreadHelper.CreateReceiverThread(channel2, 1, items);

            ThreadHelper.Start(thread2);


            Select
                .CaseSend(channel1, 1)
                .CaseSend(channel2, 2)
                .Go();

            items.Sort();

            ThreadHelper.Wait(thread2);

            Assert.Equal(Enumerable.Range(1, 2), items);
        }

        [Fact]
        public void SelectSendMultiItemSingleSendSingleSelectBuffered()
        {
            var channel = Channel<int>.CreateBuffered(3);

            var items = new List<int>();
            var thread1 = ThreadHelper.CreateReceiverThread(channel, 1000, items);

            ThreadHelper.Start(thread1);

            int i = 1;
            ThreadHelper.Repeat(1000, () =>
            {
                Select
                    .CaseSend(channel, i++)
                    .Go();
            });

            ThreadHelper.Wait(thread1);

            Assert.True(Enumerable.Range(1, 1000).SequenceEqual(items));
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
            ThreadHelper.Repeat(1000+2, () =>
            {
                Select
                    .CaseSend(channel, i++)
                    .Go();
            });

            ThreadHelper.Wait(thread1, thread2);

            items.Sort();
            Assert.True(Enumerable.Range(1, 1000).SequenceEqual(items));
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
            ThreadHelper.Repeat(1000+3+3, () =>
            {
                Select
                    .CaseSend(channel1, i++)
                    .CaseSend(channel2, i++)
                    .Go();
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
