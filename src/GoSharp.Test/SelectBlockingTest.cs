using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoSharp.Test
{
    [TestClass]
    public class SelectBlockingTest
    {
        #region Non buffered tests

        #region Recv
        [TestMethod]
        public void SelectRecvSingleNonBuffered()
        {
            var channel1 = Channel<int>.CreateNonBuffered();

            var thread1 = ThreadHelper.CreateSenderThread(channel1, 1, 1, 1);

            thread1.Start();

            int val = 0;
            Select
                .CaseRecv(channel1, a => val = a)
                .Go();

            Assert.AreEqual(1, val);
        }

        [TestMethod]
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

            Assert.IsTrue(val1 == 0 || val2 == 0);
            Assert.IsFalse(val1 == 0 && val2 == 0);

            Select
                .CaseRecv(channel1, a => val1 = a)
                .CaseRecv(channel2, b => val2 = b)
                .Go();

            Assert.AreEqual(1, val1);
            Assert.AreEqual(2, val2);
        }

        [TestMethod]
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
            ThreadHelper.Repeat(1000, () =>
            {
                Select
                    .CaseRecv(channel, a => items.Add(a))
                    .Go();
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
            ThreadHelper.Repeat(1000, () =>
            {
                Select
                    .CaseRecv(channel1, a => items.Add(a))
                    .CaseRecv(channel2, a => items.Add(a))
                    .Go();
            });

            items.Sort();
            Assert.IsTrue(Enumerable.Range(1, 1000).SequenceEqual(items));
        }

        #endregion

        #region Send

        [TestMethod]
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

            Assert.AreEqual(1, items.Single());
        }

        [TestMethod]
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
            Assert.IsTrue(Enumerable.Range(1, 2).SequenceEqual(items));
        }

        [TestMethod]
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

            Assert.IsTrue(Enumerable.Range(1, 1000).SequenceEqual(items));
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
            ThreadHelper.Repeat(1000, () =>
            {
                Select
                    .CaseSend(channel, i++)
                    .Go();
            });

            ThreadHelper.Wait(thread1, thread2);

            items.Sort();
            Assert.IsTrue(Enumerable.Range(1, 1000).SequenceEqual(items));
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
            ThreadHelper.Repeat(1000, () =>
            {
                Select
                    .CaseSend(channel1, i++)
                    .CaseSend(channel2, i++)
                    .Go();
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

        #region Recv

        [TestMethod]
        public void SelectRecvSingleBuffered()
        {
            var channel1 = Channel<int>.CreateBuffered(2);

            var thread1 = ThreadHelper.CreateSenderThread(channel1, 1, 1, 1);

            thread1.Start();

            int val = 0;
            Select
                .CaseRecv(channel1, a => val = a)
                .Go();

            Assert.AreEqual(1, val);
        }

        [TestMethod]
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

            Assert.IsTrue(val1 == 0 || val2 == 0);
            Assert.IsFalse(val1 == 0 && val2 == 0);

            Select
                .CaseRecv(channel1, a => val1 = a)
                .CaseRecv(channel2, b => val2 = b)
                .Go();

            Assert.AreEqual(1, val1);
            Assert.AreEqual(2, val2);
        }

        [TestMethod]
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

            items.Sort();

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
            ThreadHelper.Repeat(1000, () =>
            {
                Select
                    .CaseRecv(channel, a => items.Add(a))
                    .Go();
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
            ThreadHelper.Repeat(1000, () =>
            {
                Select
                    .CaseRecv(channel1, a => items.Add(a))
                    .CaseRecv(channel2, a => items.Add(a))
                    .Go();
            });

            items.Sort();
            Assert.IsTrue(Enumerable.Range(1, 1000).SequenceEqual(items));
        }

        #endregion

        #region Send

        [TestMethod]
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

            Assert.AreEqual(1, items.Single());
        }

        [TestMethod]
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
            Assert.IsTrue(val == 1 || val == 2);

            var thread2 = ThreadHelper.CreateReceiverThread(channel2, 1, items);

            ThreadHelper.Start(thread2);


            Select
                .CaseSend(channel1, 1)
                .CaseSend(channel2, 2)
                .Go();

            items.Sort();

            ThreadHelper.Wait(thread2);

            Assert.IsTrue(Enumerable.Range(1, 2).SequenceEqual(items));
        }

        [TestMethod]
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
            items.Sort();

            Assert.IsTrue(Enumerable.Range(1, 1000).SequenceEqual(items));
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
            ThreadHelper.Repeat(1000+2, () =>
            {
                Select
                    .CaseSend(channel, i++)
                    .Go();
            });

            ThreadHelper.Wait(thread1, thread2);

            items.Sort();
            Assert.IsTrue(Enumerable.Range(1, 1000).SequenceEqual(items));
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
            ThreadHelper.Repeat(1000+3+3, () =>
            {
                Select
                    .CaseSend(channel1, i++)
                    .CaseSend(channel2, i++)
                    .Go();
            });

            ThreadHelper.Wait(thread1, thread2);

            Assert.AreEqual(500, items1.Count);
            Assert.AreEqual(500, items1.Count);
            var items = items1.Union(items2).ToList();
            Assert.IsTrue(items.Count == items.Distinct().Count());
        }

        #endregion

        #endregion

        #region Input validation test
        [TestMethod]
        public void SelectRecvNullChannelTest()
        {
            Assert.ThrowsException<ArgumentNullException>(() => Select.CaseRecv<string>(null, _ => { }));
        }

        [TestMethod]
        public void SelectRecvNullActionTest()
        {
            Assert.ThrowsException<ArgumentNullException>(() => Select.CaseRecv(Channel<string>.CreateNonBuffered(), null));
        }

        [TestMethod]
        public void SelectSendNullChannelTest()
        {
            Assert.ThrowsException<ArgumentNullException>(() => Select.CaseSend(null, "a"));
        }

        [TestMethod]
        public void SelectMultipleRegRecvTest()
        {
            var ch1 = Channel<int>.CreateNonBuffered();

            var sel = Select.CaseRecv(ch1, _ => { });
            Assert.ThrowsException<InvalidOperationException>(() => sel.CaseRecv(ch1, _ => { }));
        }

        [TestMethod]
        public void SelectMultipleRegSendTest()
        {
            var ch1 = Channel<int>.CreateNonBuffered();

            var sel = Select.CaseRecv(ch1, _ => { });
            Assert.ThrowsException<InvalidOperationException>(() => sel.CaseSend(ch1, 1));
        }

        #endregion

        #region Async test
        [TestMethod]
        public async Task SelectTestAsync()
        {
            var channel = Channel<int>.CreateBuffered(1);
            var done = Channel<bool>.CreateNonBuffered();

            int i = 2;
            Go.Run(async () =>
            {
                await Select.CaseRecv(channel, a => i = a).GoAsync();
                await Select.CaseSend(done, true).GoAsync();
            });
            await Select.CaseSend(channel, 1).DefaultAsync(() => i = 3);
            await Select.CaseRecv(done, _ => { }).GoAsync();

            Assert.AreEqual(1, i);
        }

        #endregion
    }
}
