﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoSharp.Test
{
    [TestClass]
    public class SelectNonBlockingTest
    {
        #region Non buffered tests

        #region ExecDefault

        [TestMethod]
        public void ExecDefaultNonBuffered()
        {
            var channel = Channel<int>.CreateNonBuffered();

            int a = 0;
            for (int i = 0; i < 1000; i++)
            {
                Select
                    .CaseSend(channel, i)
                    .Default(() => { a++; });
            }

            Assert.AreEqual(1000, a);
        }

        #endregion

        #region Recv

        [TestMethod]
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

            Assert.IsTrue(Enumerable.Range(1, 1000).SequenceEqual(items));
        }

        [TestMethod]
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
            Assert.IsTrue(Enumerable.Range(1, 1000).SequenceEqual(items));
        }

        [TestMethod]
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
            Assert.IsTrue(Enumerable.Range(1, 1000).SequenceEqual(items));
        }

        #endregion

        #region Send

        [TestMethod]
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

            Assert.IsTrue(items.Distinct().Count() == items.Count);
        }

        [TestMethod]
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

            Assert.IsTrue(items.Distinct().Count() == items.Count);
        }

        [TestMethod]
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

            Assert.AreEqual(500, items1.Count);
            Assert.AreEqual(500, items1.Count);
            var items = items1.Union(items2).ToList();
            Assert.IsTrue(items.Count == items.Distinct().Count());
        }

        #endregion

        #endregion

        #region Buffered tests

        #region ExecDefault

        [TestMethod]
        public void ExecDefaultBuffered()
        {
            var channel = Channel<int>.CreateBuffered(500);

            int a = 0;
            for (int i = 0; i < 1000; i++)
            {
                Select
                    .CaseSend(channel, i)
                    .Default(() => { a++; });
            }

            Assert.AreEqual(500, a);
        }

        #endregion

        #region Recv

        [TestMethod]
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

            Assert.IsTrue(Enumerable.Range(1, 1000).SequenceEqual(items));
        }

        [TestMethod]
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
            Assert.IsTrue(Enumerable.Range(1, 1000).SequenceEqual(items));
        }

        [TestMethod]
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
            Assert.IsTrue(Enumerable.Range(1, 1000).SequenceEqual(items));
        }

        #endregion

        #region Send

        [TestMethod]
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

            Assert.IsTrue(items.Distinct().Count() == items.Count);
        }

        [TestMethod]
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

            Assert.IsTrue(items.Distinct().Count() == items.Count);
        }

        [TestMethod]
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

            Assert.AreEqual(500, items1.Count);
            Assert.AreEqual(500, items1.Count);
            var items = items1.Union(items2).ToList();
            Assert.IsTrue(items.Count == items.Distinct().Count());
        }

        #endregion

        #endregion

        #region Input validation tests
        [TestMethod]
        public void SelectNullDefaultActionTest()
        {
            Assert.ThrowsException<ArgumentNullException>(() => Select.CaseSend(Channel<string>.CreateNonBuffered(), "a").Default(null));
        }

        [TestMethod]
        public async Task SelectNullDefaultAsyncActionTest()
        {
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await Select.CaseSend(Channel<string>.CreateNonBuffered(), "a").DefaultAsync(null));
        }

        #endregion
    }
}
