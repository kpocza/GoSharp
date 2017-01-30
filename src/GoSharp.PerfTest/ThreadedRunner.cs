using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace GoSharp.PerfTest
{
    public class ThreadedRunner
    {
        public void Run()
        {
            Console.WriteLine("Threaded tests:");
            Console.WriteLine("~~~~~~~~~~~~~~~");
            Console.WriteLine();

            Console.WriteLine("Non-buffered:");
            SendRecvRunThread(1, 200000, 1, 200000);
            SendRecvRunThread(2, 100000, 1, 200000);
            SendRecvRunThread(1, 200000, 2, 100000);
            SendRecvRunThread(10, 50000, 10, 50000);
            SendRecvRunThread(50, 10000, 50, 10000);
            SendRecvRunThread(100, 10000, 100, 10000);
            SendRecvRunThread(10, 100000, 100, 10000);
            SendRecvRunThread(100, 10000, 10, 100000);
            SendRecvRunThread(500, 1000, 500, 1000);
            SendRecvRunThread(1000, 1000, 1000, 1000);
            SendRecvRunThread(10, 100000, 1000, 1000);
            SendRecvRunThread(1000, 1000, 10, 100000);
            SendRecvRunThread(2000, 1000, 2000, 1000);
            Console.WriteLine();

            Console.WriteLine("Buffered:");
            SendRecvRunThread(1, 200000, 1, 200000, 1000);
            SendRecvRunThread(2, 100000, 1, 200000, 1000);
            SendRecvRunThread(1, 200000, 2, 100000, 1000);
            SendRecvRunThread(10, 50000, 10, 50000, 1000);
            SendRecvRunThread(50, 10000, 50, 10000, 1000);
            SendRecvRunThread(100, 10000, 100, 10000, 1000);
            SendRecvRunThread(10, 100000, 100, 10000, 1000);
            SendRecvRunThread(100, 10000, 10, 100000, 1000);
            SendRecvRunThread(500, 1000, 500, 1000, 500);
            SendRecvRunThread(1000, 1000, 1000, 1000, 500);
            SendRecvRunThread(10, 100000, 1000, 1000, 500);
            SendRecvRunThread(1000, 1000, 10, 100000, 500);
            SendRecvRunThread(2000, 1000, 2000, 1000, 500);
            Console.WriteLine();

            Console.WriteLine("Select non-buffered:");
            SelectSendRecvRunThread(10, 1, 100000, 1, 100000);
            SelectSendRecvRunThread(10, 2, 100000, 1, 200000);
            SelectSendRecvRunThread(10, 1, 200000, 2, 100000);
            SelectSendRecvRunThread(10, 10, 10000, 10, 10000);
            SelectSendRecvRunThread(10, 50, 2000, 50, 2000);
            SelectSendRecvRunThread(10, 100, 1000, 100, 1000);
            SelectSendRecvRunThread(10, 10, 10000, 100, 1000);
            SelectSendRecvRunThread(10, 100, 1000, 10, 10000);
            SelectSendRecvRunThread(10, 500, 250, 500, 250);
            SelectSendRecvRunThread(10, 1000, 100, 1000, 100);
            SelectSendRecvRunThread(10, 10, 10000, 1000, 100);
            SelectSendRecvRunThread(10, 1000, 100, 10, 10000);
            SelectSendRecvRunThread(10, 2000, 100, 2000, 100);
            Console.WriteLine();

            Console.WriteLine("Select buffered:");
            SelectSendRecvRunThread(10, 1, 100000, 1, 100000, 500);
            SelectSendRecvRunThread(10, 2, 100000, 1, 200000, 500);
            SelectSendRecvRunThread(10, 1, 200000, 2, 100000, 500);
            SelectSendRecvRunThread(10, 10, 10000, 10, 10000, 500);
            SelectSendRecvRunThread(10, 50, 2000, 50, 2000, 500);
            SelectSendRecvRunThread(10, 100, 1000, 100, 1000, 500);
            SelectSendRecvRunThread(10, 10, 10000, 100, 1000, 500);
            SelectSendRecvRunThread(10, 100, 1000, 10, 10000, 500);
            SelectSendRecvRunThread(10, 500, 250, 500, 250, 100);
            SelectSendRecvRunThread(10, 1000, 100, 1000, 100, 50);
            SelectSendRecvRunThread(10, 10, 10000, 1000, 100, 50);
            SelectSendRecvRunThread(10, 1000, 100, 10, 10000, 50);
            SelectSendRecvRunThread(10, 2000, 100, 2000, 100, 50);
            Console.WriteLine();
        }

        private void SendRecvRunThread(int senderThreadCount, int sendItemCount, int recvThreadCount, int recvItemCount, int? bufferSize = null)
        {
            var stopWatch = new Stopwatch();
            var channel = !bufferSize.HasValue ? Channel<int>.CreateNonBuffered() : Channel<int>.CreateBuffered(bufferSize.Value);
            var senderThreads = new Thread[senderThreadCount];
            var recvThreads = new Thread[recvThreadCount];
            int cntSend = 0;
            int cntRecv = 0;

            if (senderThreadCount * sendItemCount != recvThreadCount * recvItemCount)
                throw new ArgumentException("Invalid parameterization");

            for (int i = 0; i < recvThreadCount; i++)
            {
                recvThreads[i] = CreateActionThread(recvItemCount, (r) =>
                {
                    channel.Recv();
                    Interlocked.Increment(ref cntRecv);
                });
            }

            for (int i = 0; i < senderThreadCount; i++)
            {
                senderThreads[i] = CreateActionThread(sendItemCount, (r) =>
                {
                    channel.Send(i);
                    Interlocked.Increment(ref cntSend);
                });
            }
            var threads = senderThreads.Union(recvThreads).ToArray();

            stopWatch.Start();
            Start(threads);

            Wait(threads);

            stopWatch.Stop();
            Console.WriteLine($"Sender thr: {senderThreadCount}, items/thr: {sendItemCount}; Recv thr: {recvThreadCount}, items/thr: {recvItemCount}: {stopWatch.ElapsedMilliseconds} ms");

            if (cntSend != cntRecv || cntSend != senderThreadCount * sendItemCount)
                throw new Exception("channel error");
        }

        private void SelectSendRecvRunThread(int channelCount, int senderThreadCount, int sendItemCount, int recvThreadCount, int recvItemCount, int? bufferSize = null)
        {
            var stopWatch = new Stopwatch();
            var channels = new Channel<int>[channelCount];
            var senderThreads = new Thread[senderThreadCount];
            var recvThreads = new Thread[recvThreadCount];
            int cntSend = 0;
            int cntRecv = 0;

            if (senderThreadCount * sendItemCount != recvThreadCount * recvItemCount)
                throw new ArgumentException("Invalid parameterization");

            for (int i = 0; i < channelCount; i++)
            {
                channels[i] = !bufferSize.HasValue ? Channel<int>.CreateNonBuffered() : Channel<int>.CreateBuffered(bufferSize.Value);
            }

            for (int i = 0; i < recvThreadCount; i++)
            {
                recvThreads[i] = CreateActionThread(recvItemCount, (r) =>
                {
                    var sel = Select.CaseRecv(channels[0], _ => { });
                    for (int c = 1; c < channelCount; c++)
                    {
                        sel = sel.CaseRecv(channels[c], _ => { });
                    }
                    sel.Go();
                    Interlocked.Increment(ref cntRecv);
                });
            }

            for (int i = 0; i < senderThreadCount; i++)
            {
                senderThreads[i] = CreateActionThread(sendItemCount, (rnd) =>
                {
                    var fireChannel = rnd.Next(channelCount);
                    channels[fireChannel].Send(i);
                    Interlocked.Increment(ref cntSend);
                });
            }
            var threads = senderThreads.Union(recvThreads).ToArray();

            stopWatch.Start();
            Start(threads);

            Wait(threads);

            stopWatch.Stop();
            Console.WriteLine($"Sender thr: {senderThreadCount}, items/thr: {sendItemCount}; Recv thr: {recvThreadCount}, items/thr: {recvItemCount}: {stopWatch.ElapsedMilliseconds} ms");

            if (cntSend != cntRecv || cntSend != senderThreadCount * sendItemCount)
                throw new Exception("channel error");
        }

        private Thread CreateActionThread(int count, Action<Random> action)
        {
            return new Thread(() =>
            {
                var rnd = new Random();
                for (int i = 0; i < count; i++)
                {
                    action(rnd);
                }
            });
        }

        private void Start(params Thread[] threads)
        {
            foreach (var thread in threads)
            {
                thread.Start();
            }
        }

        private void Wait(params Thread[] threads)
        {
            foreach (var thread in threads)
            {
                thread.Join();
            }
        }
    }

    //Threaded tests:
    //~~~~~~~~~~~~~~~

    //Non-buffered:
    //Sender thr: 1, items/thr: 200000; Recv thr: 1, items/thr: 200000: 521 ms
    //Sender thr: 2, items/thr: 100000; Recv thr: 1, items/thr: 200000: 221 ms
    //Sender thr: 1, items/thr: 200000; Recv thr: 2, items/thr: 100000: 216 ms
    //Sender thr: 10, items/thr: 50000; Recv thr: 10, items/thr: 50000: 566 ms
    //Sender thr: 50, items/thr: 10000; Recv thr: 50, items/thr: 10000: 682 ms
    //Sender thr: 100, items/thr: 10000; Recv thr: 100, items/thr: 10000: 1669 ms
    //Sender thr: 10, items/thr: 100000; Recv thr: 100, items/thr: 10000: 1320 ms
    //Sender thr: 100, items/thr: 10000; Recv thr: 10, items/thr: 100000: 1359 ms
    //Sender thr: 500, items/thr: 1000; Recv thr: 500, items/thr: 1000: 3642 ms
    //Sender thr: 1000, items/thr: 1000; Recv thr: 1000, items/thr: 1000: 14866 ms
    //Sender thr: 10, items/thr: 100000; Recv thr: 1000, items/thr: 1000: 1532 ms
    //Sender thr: 1000, items/thr: 1000; Recv thr: 10, items/thr: 100000: 7568 ms
    //Sender thr: 2000, items/thr: 1000; Recv thr: 2000, items/thr: 1000: 54810 ms

    //Buffered:
    //Sender thr: 1, items/thr: 200000; Recv thr: 1, items/thr: 200000: 141 ms
    //Sender thr: 2, items/thr: 100000; Recv thr: 1, items/thr: 200000: 172 ms
    //Sender thr: 1, items/thr: 200000; Recv thr: 2, items/thr: 100000: 129 ms
    //Sender thr: 10, items/thr: 50000; Recv thr: 10, items/thr: 50000: 321 ms
    //Sender thr: 50, items/thr: 10000; Recv thr: 50, items/thr: 10000: 490 ms
    //Sender thr: 100, items/thr: 10000; Recv thr: 100, items/thr: 10000: 1234 ms
    //Sender thr: 10, items/thr: 100000; Recv thr: 100, items/thr: 10000: 857 ms
    //Sender thr: 100, items/thr: 10000; Recv thr: 10, items/thr: 100000: 1255 ms
    //Sender thr: 500, items/thr: 1000; Recv thr: 500, items/thr: 1000: 3918 ms
    //Sender thr: 1000, items/thr: 1000; Recv thr: 1000, items/thr: 1000: 14220 ms
    //Sender thr: 10, items/thr: 100000; Recv thr: 1000, items/thr: 1000: 756 ms
    //Sender thr: 1000, items/thr: 1000; Recv thr: 10, items/thr: 100000: 10589 ms
    //Sender thr: 2000, items/thr: 1000; Recv thr: 2000, items/thr: 1000: 48862 ms

    //Select non-buffered:
    //Sender thr: 1, items/thr: 100000; Recv thr: 1, items/thr: 100000: 711 ms
    //Sender thr: 2, items/thr: 100000; Recv thr: 1, items/thr: 200000: 1477 ms
    //Sender thr: 1, items/thr: 200000; Recv thr: 2, items/thr: 100000: 1051 ms
    //Sender thr: 10, items/thr: 10000; Recv thr: 10, items/thr: 10000: 537 ms
    //Sender thr: 50, items/thr: 2000; Recv thr: 50, items/thr: 2000: 801 ms
    //Sender thr: 100, items/thr: 1000; Recv thr: 100, items/thr: 1000: 1289 ms
    //Sender thr: 10, items/thr: 10000; Recv thr: 100, items/thr: 1000: 1301 ms
    //Sender thr: 100, items/thr: 1000; Recv thr: 10, items/thr: 10000: 796 ms
    //Sender thr: 500, items/thr: 250; Recv thr: 500, items/thr: 250: 2961 ms
    //Sender thr: 1000, items/thr: 100; Recv thr: 1000, items/thr: 100: 3104 ms
    //Sender thr: 10, items/thr: 10000; Recv thr: 1000, items/thr: 100: 1579 ms
    //Sender thr: 1000, items/thr: 100; Recv thr: 10, items/thr: 10000: 4551 ms
    //Sender thr: 2000, items/thr: 100; Recv thr: 2000, items/thr: 100: 8885 ms

    //Select buffered:
    //Sender thr: 1, items/thr: 100000; Recv thr: 1, items/thr: 100000: 618 ms
    //Sender thr: 2, items/thr: 100000; Recv thr: 1, items/thr: 200000: 1268 ms
    //Sender thr: 1, items/thr: 200000; Recv thr: 2, items/thr: 100000: 763 ms
    //Sender thr: 10, items/thr: 10000; Recv thr: 10, items/thr: 10000: 380 ms
    //Sender thr: 50, items/thr: 2000; Recv thr: 50, items/thr: 2000: 548 ms
    //Sender thr: 100, items/thr: 1000; Recv thr: 100, items/thr: 1000: 727 ms
    //Sender thr: 10, items/thr: 10000; Recv thr: 100, items/thr: 1000: 381 ms
    //Sender thr: 100, items/thr: 1000; Recv thr: 10, items/thr: 10000: 650 ms
    //Sender thr: 500, items/thr: 250; Recv thr: 500, items/thr: 250: 2750 ms
    //Sender thr: 1000, items/thr: 100; Recv thr: 1000, items/thr: 100: 2956 ms
    //Sender thr: 10, items/thr: 10000; Recv thr: 1000, items/thr: 100: 486 ms
    //Sender thr: 1000, items/thr: 100; Recv thr: 10, items/thr: 10000: 3370 ms
    //Sender thr: 2000, items/thr: 100; Recv thr: 2000, items/thr: 100: 8638 ms

    #region deprecated
    // Full threaded version (deprecated in favour of tasks):
    //Non-buffered:
    //Sender thr: 1, items/thr: 200000; Recv thr: 1, items/thr: 200000: 1201 ms
    //Sender thr: 2, items/thr: 100000; Recv thr: 1, items/thr: 200000: 1060 ms
    //Sender thr: 1, items/thr: 200000; Recv thr: 2, items/thr: 100000: 1063 ms
    //Sender thr: 10, items/thr: 50000; Recv thr: 10, items/thr: 50000: 4115 ms
    //Sender thr: 50, items/thr: 10000; Recv thr: 50, items/thr: 10000: 4277 ms
    //Sender thr: 100, items/thr: 10000; Recv thr: 100, items/thr: 10000: 8748 ms
    //Sender thr: 10, items/thr: 100000; Recv thr: 100, items/thr: 10000: 8792 ms
    //Sender thr: 100, items/thr: 10000; Recv thr: 10, items/thr: 100000: 8792 ms
    //Sender thr: 500, items/thr: 1000; Recv thr: 500, items/thr: 1000: 5180 ms
    //Sender thr: 1000, items/thr: 1000; Recv thr: 1000, items/thr: 1000: 11515 ms
    //Sender thr: 10, items/thr: 100000; Recv thr: 1000, items/thr: 1000: 8311 ms
    //Sender thr: 1000, items/thr: 1000; Recv thr: 10, items/thr: 100000: 11538 ms

    //Buffered:
    //Sender thr: 1, items/thr: 200000; Recv thr: 1, items/thr: 200000: 98 ms
    //Sender thr: 2, items/thr: 100000; Recv thr: 1, items/thr: 200000: 89 ms
    //Sender thr: 1, items/thr: 200000; Recv thr: 2, items/thr: 100000: 114 ms
    //Sender thr: 10, items/thr: 50000; Recv thr: 10, items/thr: 50000: 801 ms
    //Sender thr: 50, items/thr: 10000; Recv thr: 50, items/thr: 10000: 1431 ms
    //Sender thr: 100, items/thr: 10000; Recv thr: 100, items/thr: 10000: 6076 ms
    //Sender thr: 10, items/thr: 100000; Recv thr: 100, items/thr: 10000: 7447 ms
    //Sender thr: 100, items/thr: 10000; Recv thr: 10, items/thr: 100000: 8639 ms
    //Sender thr: 500, items/thr: 1000; Recv thr: 500, items/thr: 1000: 5093 ms
    //Sender thr: 1000, items/thr: 1000; Recv thr: 1000, items/thr: 1000: 11542 ms
    //Sender thr: 10, items/thr: 100000; Recv thr: 1000, items/thr: 1000: 3904 ms
    //Sender thr: 1000, items/thr: 1000; Recv thr: 10, items/thr: 100000: 11625 ms

    //Select non-buffered:
    //Sender thr: 1, items/thr: 100000; Recv thr: 1, items/thr: 100000: 968 ms
    //Sender thr: 2, items/thr: 100000; Recv thr: 1, items/thr: 200000: 1994 ms
    //Sender thr: 1, items/thr: 200000; Recv thr: 2, items/thr: 100000: 1801 ms
    //Sender thr: 10, items/thr: 10000; Recv thr: 10, items/thr: 10000: 4339 ms
    //Sender thr: 50, items/thr: 2000; Recv thr: 50, items/thr: 2000: 5724 ms
    //Sender thr: 100, items/thr: 1000; Recv thr: 100, items/thr: 1000: 6125 ms
    //Sender thr: 10, items/thr: 10000; Recv thr: 100, items/thr: 1000: 7605 ms
    //Sender thr: 100, items/thr: 1000; Recv thr: 10, items/thr: 10000: 4255 ms
    //Sender thr: 500, items/thr: 250; Recv thr: 500, items/thr: 250: 7237 ms
    //Sender thr: 1000, items/thr: 100; Recv thr: 1000, items/thr: 100: 4617 ms
    //Sender thr: 10, items/thr: 10000; Recv thr: 1000, items/thr: 100: 4033 ms
    //Sender thr: 1000, items/thr: 100; Recv thr: 10, items/thr: 10000: 5012 ms

    //Select buffered:
    //Sender thr: 1, items/thr: 100000; Recv thr: 1, items/thr: 100000: 762 ms
    //Sender thr: 2, items/thr: 100000; Recv thr: 1, items/thr: 200000: 1538 ms
    //Sender thr: 1, items/thr: 200000; Recv thr: 2, items/thr: 100000: 876 ms
    //Sender thr: 10, items/thr: 10000; Recv thr: 10, items/thr: 10000: 860 ms
    //Sender thr: 50, items/thr: 2000; Recv thr: 50, items/thr: 2000: 1141 ms
    //Sender thr: 100, items/thr: 1000; Recv thr: 100, items/thr: 1000: 1233 ms
    //Sender thr: 10, items/thr: 10000; Recv thr: 100, items/thr: 1000: 646 ms
    //Sender thr: 100, items/thr: 1000; Recv thr: 10, items/thr: 10000: 1125 ms
    //Sender thr: 500, items/thr: 250; Recv thr: 500, items/thr: 250: 3707 ms
    //Sender thr: 1000, items/thr: 100; Recv thr: 1000, items/thr: 100: 3795 ms
    //Sender thr: 10, items/thr: 10000; Recv thr: 1000, items/thr: 100: 963 ms
    //Sender thr: 1000, items/thr: 100; Recv thr: 10, items/thr: 10000: 4397 ms

    #endregion
}
