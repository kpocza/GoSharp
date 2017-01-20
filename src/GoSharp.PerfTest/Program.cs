using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GoSharp.PerfTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Non-buffered:");
            SendRecvRunThread(1, 200000, 1, 200000);
            SendRecvRunThread(2, 100000, 1, 200000);
            SendRecvRunThread(1, 200000, 2, 100000);
            SendRecvRunThread(10, 50000, 10, 50000);
            SendRecvRunThread(50, 10000, 50, 10000);
            SendRecvRunThread(100, 10000, 100, 10000);
            SendRecvRunThread(500, 1000, 500, 1000);
            SendRecvRunThread(1000, 1000, 1000, 1000);
            Console.WriteLine();

            Console.WriteLine("Buffered:");
            SendRecvRunThread(1, 200000, 1, 200000, 1000);
            SendRecvRunThread(2, 100000, 1, 200000, 1000);
            SendRecvRunThread(1, 200000, 2, 100000, 1000);
            SendRecvRunThread(10, 50000, 10, 50000, 1000);
            SendRecvRunThread(50, 10000, 50, 10000, 1000);
            SendRecvRunThread(100, 10000, 100, 10000, 1000);
            SendRecvRunThread(500, 1000, 500, 1000, 500);
            SendRecvRunThread(1000, 1000, 1000, 1000, 500);
            Console.WriteLine();

            Console.WriteLine("Select non-buffered:");
            SelectSendRecvRunThread(10, 1, 100000, 1, 100000);
            SelectSendRecvRunThread(10, 2, 100000, 1, 200000);
            SelectSendRecvRunThread(10, 1, 200000, 2, 100000);
            SelectSendRecvRunThread(10, 10, 10000, 10, 10000);
            SelectSendRecvRunThread(10, 50, 2000, 50, 2000);
            SelectSendRecvRunThread(10, 100, 1000, 100, 1000);
            SelectSendRecvRunThread(10, 500, 250, 500, 250);
            SelectSendRecvRunThread(10, 1000, 100, 1000, 100);
            Console.WriteLine();

            Console.WriteLine("Select buffered:");
            SelectSendRecvRunThread(10, 1, 100000, 1, 100000, 500);
            SelectSendRecvRunThread(10, 2, 100000, 1, 200000, 500);
            SelectSendRecvRunThread(10, 1, 200000, 2, 100000, 500);
            SelectSendRecvRunThread(10, 10, 10000, 10, 10000, 500);
            SelectSendRecvRunThread(10, 50, 2000, 50, 2000, 500);
            SelectSendRecvRunThread(10, 100, 1000, 100, 1000, 500);
            SelectSendRecvRunThread(10, 500, 250, 500, 250, 100);
            SelectSendRecvRunThread(10, 1000, 100, 1000, 100, 50);
            Console.WriteLine();
        }

        private static void SendRecvRunThread(int senderThreadCount, int sendItemCount, int recvThreadCount, int recvItemCount, int? bufferSize = null)
        {
            var stopWatch = new Stopwatch();
            var channel = !bufferSize.HasValue ? Channel<int>.CreateNonBuffered() : Channel<int>.CreateBuffered(bufferSize.Value);
            var senderThreads = new Thread[senderThreadCount];
            var recvThreads = new Thread[recvThreadCount];
            int cntSend = 0;
            int cntRecv = 0;

            if(senderThreadCount* sendItemCount!= recvThreadCount*recvItemCount)
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

            if (cntSend != cntRecv)
                throw new Exception("channel error");
        }

        private static void SelectSendRecvRunThread(int channelCount, int senderThreadCount, int sendItemCount, int recvThreadCount, int recvItemCount, int? bufferSize = null)
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

            if (cntSend != cntRecv)
                throw new Exception("channel error");
        }

        private static Thread CreateActionThread(int count, Action<Random> action)
        {
            return new Thread(() =>
            {
                var rnd = new Random();
                for(int i = 0;i < count;i++)
                {
                    action(rnd);
                }
            });
        }

        public static void Start(params Thread[] threads)
        {
            foreach (var thread in threads)
            {
                thread.Start();
            }
        }

        public static void Wait(params Thread[] threads)
        {
            foreach (var thread in threads)
            {
                thread.Join();
            }
        }


        private static Task CreateActionTask(int count, Action<Random> action)
        {
            return new Task(() =>
            {
                var rnd = new Random();
                for (int i = 0; i < count; i++)
                {
                    action(rnd);
                }
            });
        }

        public static void Start(params Task[] threads)
        {
            foreach (var thread in threads)
            {
                thread.Start();
            }
        }

        public static void Wait(params Task[] threads)
        {
            foreach (var thread in threads)
            {
                thread.Wait();
            }
        }
    }

    //Non-buffered:
    //Sender thr: 1, items/thr: 200000; Recv thr: 1, items/thr: 200000: 1143 ms
    //Sender thr: 2, items/thr: 100000; Recv thr: 1, items/thr: 200000: 1030 ms
    //Sender thr: 1, items/thr: 200000; Recv thr: 2, items/thr: 100000: 1051 ms
    //Sender thr: 10, items/thr: 50000; Recv thr: 10, items/thr: 50000: 3986 ms
    //Sender thr: 50, items/thr: 10000; Recv thr: 50, items/thr: 10000: 4289 ms
    //Sender thr: 100, items/thr: 10000; Recv thr: 100, items/thr: 10000: 8773 ms
    //Sender thr: 500, items/thr: 1000; Recv thr: 500, items/thr: 1000: 5231 ms
    //Sender thr: 1000, items/thr: 1000; Recv thr: 1000, items/thr: 1000: 11121 ms
    //
    //Buffered:
    //Sender thr: 1, items/thr: 200000; Recv thr: 1, items/thr: 200000: 99 ms
    //Sender thr: 2, items/thr: 100000; Recv thr: 1, items/thr: 200000: 91 ms
    //Sender thr: 1, items/thr: 200000; Recv thr: 2, items/thr: 100000: 111 ms
    //Sender thr: 10, items/thr: 50000; Recv thr: 10, items/thr: 50000: 702 ms
    //Sender thr: 50, items/thr: 10000; Recv thr: 50, items/thr: 10000: 2121 ms
    //Sender thr: 100, items/thr: 10000; Recv thr: 100, items/thr: 10000: 7516 ms
    //Sender thr: 500, items/thr: 1000; Recv thr: 500, items/thr: 1000: 5183 ms
    //Sender thr: 1000, items/thr: 1000; Recv thr: 1000, items/thr: 1000: 11724 ms
    //
    //Select non-buffered:
    //Sender thr: 1, items/thr: 100000; Recv thr: 1, items/thr: 100000: 1003 ms
    //Sender thr: 2, items/thr: 100000; Recv thr: 1, items/thr: 200000: 2055 ms
    //Sender thr: 1, items/thr: 200000; Recv thr: 2, items/thr: 100000: 1787 ms
    //Sender thr: 10, items/thr: 10000; Recv thr: 10, items/thr: 10000: 4330 ms
    //Sender thr: 50, items/thr: 2000; Recv thr: 50, items/thr: 2000: 5645 ms
    //Sender thr: 100, items/thr: 1000; Recv thr: 100, items/thr: 1000: 6083 ms
    //Sender thr: 500, items/thr: 250; Recv thr: 500, items/thr: 250: 9789 ms
    //Sender thr: 1000, items/thr: 100; Recv thr: 1000, items/thr: 100: 6601 ms
    //
    //Select buffered:
    //Sender thr: 1, items/thr: 100000; Recv thr: 1, items/thr: 100000: 787 ms
    //Sender thr: 2, items/thr: 100000; Recv thr: 1, items/thr: 200000: 1581 ms
    //Sender thr: 1, items/thr: 200000; Recv thr: 2, items/thr: 100000: 917 ms
    //Sender thr: 10, items/thr: 10000; Recv thr: 10, items/thr: 10000: 912 ms
    //Sender thr: 50, items/thr: 2000; Recv thr: 50, items/thr: 2000: 1330 ms
    //Sender thr: 100, items/thr: 1000; Recv thr: 100, items/thr: 1000: 1378 ms
    //Sender thr: 500, items/thr: 250; Recv thr: 500, items/thr: 250: 6197 ms
    //Sender thr: 1000, items/thr: 100; Recv thr: 1000, items/thr: 100: 4735 ms
}
