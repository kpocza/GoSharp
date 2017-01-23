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
//            RunThreadedTests();
            RunTaskTests();
        }

        #region Runners

        private static void RunThreadedTests()
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

        private static void RunTaskTests()
        {
            Console.WriteLine("Task tests:");
            Console.WriteLine("~~~~~~~~~~~");
            Console.WriteLine();

            Console.WriteLine("Non-buffered:");
            SendRecvRunTask(1, 200000, 1, 200000);
            SendRecvRunTask(2, 100000, 1, 200000);
            SendRecvRunTask(1, 200000, 2, 100000);
            SendRecvRunTask(10, 50000, 10, 50000);
            SendRecvRunTask(50, 10000, 50, 10000);
            SendRecvRunTask(100, 10000, 100, 10000);
            SendRecvRunTask(10, 100000, 100, 10000);
            SendRecvRunTask(100, 10000, 10, 100000);
            SendRecvRunTask(500, 1000, 500, 1000);
            SendRecvRunTask(1000, 1000, 1000, 1000);
            SendRecvRunTask(10, 100000, 1000, 1000);
            SendRecvRunTask(1000, 1000, 10, 100000);
            SendRecvRunTask(2000, 1000, 2000, 1000);
            Console.WriteLine();

            Console.WriteLine("Buffered:");
            SendRecvRunTask(1, 200000, 1, 200000, 1000);
            SendRecvRunTask(2, 100000, 1, 200000, 1000);
            SendRecvRunTask(1, 200000, 2, 100000, 1000);
            SendRecvRunTask(10, 50000, 10, 50000, 1000);
            SendRecvRunTask(50, 10000, 50, 10000, 1000);
            SendRecvRunTask(100, 10000, 100, 10000, 1000);
            SendRecvRunTask(10, 100000, 100, 10000, 1000);
            SendRecvRunTask(100, 10000, 10, 100000, 1000);
            SendRecvRunTask(500, 1000, 500, 1000, 500);
            SendRecvRunTask(1000, 1000, 1000, 1000, 500);
            SendRecvRunTask(10, 100000, 1000, 1000, 500);
            SendRecvRunTask(1000, 1000, 10, 100000, 500);
            SendRecvRunTask(2000, 1000, 2000, 1000, 500);
            Console.WriteLine();

            Console.WriteLine("Select non-buffered:");
            SelectSendRecvRunTask(10, 1, 100000, 1, 100000);
            SelectSendRecvRunTask(10, 2, 100000, 1, 200000);
            SelectSendRecvRunTask(10, 1, 200000, 2, 100000);
            SelectSendRecvRunTask(10, 10, 10000, 10, 10000);
            SelectSendRecvRunTask(10, 50, 2000, 50, 2000);
            SelectSendRecvRunTask(10, 100, 1000, 100, 1000);
            SelectSendRecvRunTask(10, 10, 10000, 100, 1000);
            SelectSendRecvRunTask(10, 100, 1000, 10, 10000);
            SelectSendRecvRunTask(10, 500, 250, 500, 250);
            SelectSendRecvRunTask(10, 1000, 100, 1000, 100);
            SelectSendRecvRunTask(10, 10, 10000, 1000, 100);
            SelectSendRecvRunTask(10, 1000, 100, 10, 10000);
            SelectSendRecvRunTask(10, 2000, 100, 2000, 100);
            Console.WriteLine();

            Console.WriteLine("Select buffered:");
            SelectSendRecvRunTask(10, 1, 100000, 1, 100000, 500);
            SelectSendRecvRunTask(10, 2, 100000, 1, 200000, 500);
            SelectSendRecvRunTask(10, 1, 200000, 2, 100000, 500);
            SelectSendRecvRunTask(10, 10, 10000, 10, 10000, 500);
            SelectSendRecvRunTask(10, 50, 2000, 50, 2000, 500);
            SelectSendRecvRunTask(10, 100, 1000, 100, 1000, 500);
            SelectSendRecvRunTask(10, 10, 10000, 100, 1000, 500);
            SelectSendRecvRunTask(10, 100, 1000, 10, 10000, 500);
            SelectSendRecvRunTask(10, 500, 250, 500, 250, 100);
            SelectSendRecvRunTask(10, 1000, 100, 1000, 100, 50);
            SelectSendRecvRunTask(10, 10, 10000, 1000, 100, 50);
            SelectSendRecvRunTask(10, 1000, 100, 10, 10000, 50);
            SelectSendRecvRunTask(10, 2000, 100, 2000, 100, 50);
            Console.WriteLine();
        }

        #endregion

        #region Threaded functions

        private static void SendRecvRunThread(int senderThreadCount, int sendItemCount, int recvThreadCount, int recvItemCount, int? bufferSize = null)
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

            if (cntSend != cntRecv || cntSend!= senderThreadCount * sendItemCount)
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

            if (cntSend != cntRecv || cntSend != senderThreadCount * sendItemCount)
                throw new Exception("channel error");
        }

        private static Thread CreateActionThread(int count, Action<Random> action)
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

        #endregion

        #region Task functions

        private static void SendRecvRunTask(int senderTaskCount, int sendItemCount, int recvTaskCount, int recvItemCount, int? bufferSize = null)
        {
            var stopWatch = new Stopwatch();
            var channel = !bufferSize.HasValue ? Channel<int>.CreateNonBuffered() : Channel<int>.CreateBuffered(bufferSize.Value);
            var senderTasks = new Task[senderTaskCount];
            var recvTasks = new Task[recvTaskCount];
            int cntSend = 0;
            int cntRecv = 0;

            if (senderTaskCount * sendItemCount != recvTaskCount * recvItemCount)
                throw new ArgumentException("Invalid parameterization");

            for (int i = 0; i < recvTaskCount; i++)
            {
                recvTasks[i] = CreateActionTask(recvItemCount, async (r) =>
                {
                    await channel.RecvAsync();
                    Interlocked.Increment(ref cntRecv);
                });
            }

            for (int i = 0; i < senderTaskCount; i++)
            {
                senderTasks[i] = CreateActionTask(sendItemCount, async (r) =>
                {
                    await channel.SendAsync(i);
                    Interlocked.Increment(ref cntSend);
                });
            }
            var tasks = senderTasks.Union(recvTasks).ToArray();

            stopWatch.Start();
            Start(tasks);

            Wait(tasks);

            stopWatch.Stop();
            Console.WriteLine($"Sender tsk: {senderTaskCount}, items/tsk: {sendItemCount}; Recv tsk: {recvTaskCount}, items/tsk: {recvItemCount}: {stopWatch.ElapsedMilliseconds} ms");

            if (cntSend != cntRecv || cntSend != senderTaskCount * sendItemCount)
                throw new Exception("channel error");
        }

        private static void SelectSendRecvRunTask(int channelCount, int senderTaskCount, int sendItemCount, int recvTaskCount, int recvItemCount, int? bufferSize = null)
        {
            var stopWatch = new Stopwatch();
            var channels = new Channel<int>[channelCount];
            var senderTasks = new Task[senderTaskCount];
            var recvTasks = new Task[recvTaskCount];
            int cntSend = 0;
            int cntRecv = 0;

            if (senderTaskCount * sendItemCount != recvTaskCount * recvItemCount)
                throw new ArgumentException("Invalid parameterization");

            for (int i = 0; i < channelCount; i++)
            {
                channels[i] = !bufferSize.HasValue ? Channel<int>.CreateNonBuffered() : Channel<int>.CreateBuffered(bufferSize.Value);
            }

            for (int i = 0; i < recvTaskCount; i++)
            {
                recvTasks[i] = CreateActionTask(recvItemCount, async (r) =>
                {
                    var sel = Select.CaseRecv(channels[0], _ => { });
                    for (int c = 1; c < channelCount; c++)
                    {
                        sel = sel.CaseRecv(channels[c], _ => { });
                    }
                    await sel.GoAsync();
                    Interlocked.Increment(ref cntRecv);
                });
            }

            for (int i = 0; i < senderTaskCount; i++)
            {
                senderTasks[i] = CreateActionTask(sendItemCount, async (rnd) =>
                {
                    var fireChannel = rnd.Next(channelCount);
                    await channels[fireChannel].SendAsync(i);
                    Interlocked.Increment(ref cntSend);
                });
            }
            var tasks = senderTasks.Union(recvTasks).ToArray();

            stopWatch.Start();
            Start(tasks);

            Wait(tasks);

            stopWatch.Stop();
            Console.WriteLine($"Sender tsk: {senderTaskCount}, items/tsk: {sendItemCount}; Recv tsk: {recvTaskCount}, items/tsk: {recvItemCount}: {stopWatch.ElapsedMilliseconds} ms");

            if (cntSend != cntRecv || cntSend != senderTaskCount * sendItemCount)
                throw new Exception("channel error");
        }

        private static Task CreateActionTask(int count, Func<Random, Task> action)
        {
            return new Task(() =>
            {
                Random rnd = new Random();
                for (int i = 0; i < count; i++)
                {
                    action(rnd);
                }
            });
        }

        public static void Start(params Task[] tasks)
        {
            foreach (var task in tasks)
            {
                task.Start();
            }
        }

        public static void Wait(params Task[] tasks)
        {
            Task.WaitAll(tasks);
        }

        #endregion
    }

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


    //Threaded tests:
    //~~~~~~~~~~~~~~~

    //Non-buffered:
    //Sender thr: 1, items/thr: 200000; Recv thr: 1, items/thr: 200000: 981 ms
    //Sender thr: 2, items/thr: 100000; Recv thr: 1, items/thr: 200000: 289 ms
    //Sender thr: 1, items/thr: 200000; Recv thr: 2, items/thr: 100000: 274 ms
    //Sender thr: 10, items/thr: 50000; Recv thr: 10, items/thr: 50000: 767 ms
    //Sender thr: 50, items/thr: 10000; Recv thr: 50, items/thr: 10000: 993 ms
    //Sender thr: 100, items/thr: 10000; Recv thr: 100, items/thr: 10000: 2709 ms
    //Sender thr: 10, items/thr: 100000; Recv thr: 100, items/thr: 10000: 1935 ms
    //Sender thr: 100, items/thr: 10000; Recv thr: 10, items/thr: 100000: 1971 ms
    //Sender thr: 500, items/thr: 1000; Recv thr: 500, items/thr: 1000: 3915 ms
    //Sender thr: 1000, items/thr: 1000; Recv thr: 1000, items/thr: 1000: 15478 ms
    //Sender thr: 10, items/thr: 100000; Recv thr: 1000, items/thr: 1000: 2234 ms
    //Sender thr: 1000, items/thr: 1000; Recv thr: 10, items/thr: 100000: 8166 ms
    //Sender thr: 2000, items/thr: 1000; Recv thr: 2000, items/thr: 1000: 53688 ms

    //Buffered:
    //Sender thr: 1, items/thr: 200000; Recv thr: 1, items/thr: 200000: 125 ms
    //Sender thr: 2, items/thr: 100000; Recv thr: 1, items/thr: 200000: 138 ms
    //Sender thr: 1, items/thr: 200000; Recv thr: 2, items/thr: 100000: 143 ms
    //Sender thr: 10, items/thr: 50000; Recv thr: 10, items/thr: 50000: 354 ms
    //Sender thr: 50, items/thr: 10000; Recv thr: 50, items/thr: 10000: 559 ms
    //Sender thr: 100, items/thr: 10000; Recv thr: 100, items/thr: 10000: 1841 ms
    //Sender thr: 10, items/thr: 100000; Recv thr: 100, items/thr: 10000: 1033 ms
    //Sender thr: 100, items/thr: 10000; Recv thr: 10, items/thr: 100000: 1471 ms
    //Sender thr: 500, items/thr: 1000; Recv thr: 500, items/thr: 1000: 1979 ms
    //Sender thr: 1000, items/thr: 1000; Recv thr: 1000, items/thr: 1000: 6735 ms
    //Sender thr: 10, items/thr: 100000; Recv thr: 1000, items/thr: 1000: 899 ms
    //Sender thr: 1000, items/thr: 1000; Recv thr: 10, items/thr: 100000: 9597 ms
    //Sender thr: 2000, items/thr: 1000; Recv thr: 2000, items/thr: 1000: 49475 ms

    //Select non-buffered:
    //Sender thr: 1, items/thr: 100000; Recv thr: 1, items/thr: 100000: 769 ms
    //Sender thr: 2, items/thr: 100000; Recv thr: 1, items/thr: 200000: 1656 ms
    //Sender thr: 1, items/thr: 200000; Recv thr: 2, items/thr: 100000: 1303 ms
    //Sender thr: 10, items/thr: 10000; Recv thr: 10, items/thr: 10000: 642 ms
    //Sender thr: 50, items/thr: 2000; Recv thr: 50, items/thr: 2000: 931 ms
    //Sender thr: 100, items/thr: 1000; Recv thr: 100, items/thr: 1000: 1571 ms
    //Sender thr: 10, items/thr: 10000; Recv thr: 100, items/thr: 1000: 1568 ms
    //Sender thr: 100, items/thr: 1000; Recv thr: 10, items/thr: 10000: 848 ms
    //Sender thr: 500, items/thr: 250; Recv thr: 500, items/thr: 250: 2992 ms
    //Sender thr: 1000, items/thr: 100; Recv thr: 1000, items/thr: 100: 3127 ms
    //Sender thr: 10, items/thr: 10000; Recv thr: 1000, items/thr: 100: 2070 ms
    //Sender thr: 1000, items/thr: 100; Recv thr: 10, items/thr: 10000: 5355 ms
    //Sender thr: 2000, items/thr: 100; Recv thr: 2000, items/thr: 100: 9105 ms

    //Select buffered:
    //Sender thr: 1, items/thr: 100000; Recv thr: 1, items/thr: 100000: 667 ms
    //Sender thr: 2, items/thr: 100000; Recv thr: 1, items/thr: 200000: 1333 ms
    //Sender thr: 1, items/thr: 200000; Recv thr: 2, items/thr: 100000: 779 ms
    //Sender thr: 10, items/thr: 10000; Recv thr: 10, items/thr: 10000: 392 ms
    //Sender thr: 50, items/thr: 2000; Recv thr: 50, items/thr: 2000: 560 ms
    //Sender thr: 100, items/thr: 1000; Recv thr: 100, items/thr: 1000: 744 ms
    //Sender thr: 10, items/thr: 10000; Recv thr: 100, items/thr: 1000: 429 ms
    //Sender thr: 100, items/thr: 1000; Recv thr: 10, items/thr: 10000: 692 ms
    //Sender thr: 500, items/thr: 250; Recv thr: 500, items/thr: 250: 2632 ms
    //Sender thr: 1000, items/thr: 100; Recv thr: 1000, items/thr: 100: 2954 ms
    //Sender thr: 10, items/thr: 10000; Recv thr: 1000, items/thr: 100: 497 ms
    //Sender thr: 1000, items/thr: 100; Recv thr: 10, items/thr: 10000: 3481 ms
    //Sender thr: 2000, items/thr: 100; Recv thr: 2000, items/thr: 100: 9572 ms


    //Task tests:
    //~~~~~~~~~~~

    //Non-buffered:
    //Sender tsk: 1, items/tsk: 200000; Recv tsk: 1, items/tsk: 200000: 889 ms
    //Sender tsk: 2, items/tsk: 100000; Recv tsk: 1, items/tsk: 200000: 1090 ms
    //Sender tsk: 1, items/tsk: 200000; Recv tsk: 2, items/tsk: 100000: 1441 ms
    //Sender tsk: 10, items/tsk: 50000; Recv tsk: 10, items/tsk: 50000: 2388 ms
    //Sender tsk: 50, items/tsk: 10000; Recv tsk: 50, items/tsk: 10000: 2488 ms
    //Sender tsk: 100, items/tsk: 10000; Recv tsk: 100, items/tsk: 10000: 4330 ms
    //Sender tsk: 10, items/tsk: 100000; Recv tsk: 100, items/tsk: 10000: 4999 ms
    //Sender tsk: 100, items/tsk: 10000; Recv tsk: 10, items/tsk: 100000: 4796 ms
    //Sender tsk: 500, items/tsk: 1000; Recv tsk: 500, items/tsk: 1000: 2779 ms
    //Sender tsk: 1000, items/tsk: 1000; Recv tsk: 1000, items/tsk: 1000: 5392 ms
    //Sender tsk: 10, items/tsk: 100000; Recv tsk: 1000, items/tsk: 1000: 5376 ms
    //Sender tsk: 1000, items/tsk: 1000; Recv tsk: 10, items/tsk: 100000: 4317 ms
    //Sender tsk: 2000, items/tsk: 1000; Recv tsk: 2000, items/tsk: 1000: 10610 ms

    //Buffered:
    //Sender tsk: 1, items/tsk: 200000; Recv tsk: 1, items/tsk: 200000: 1065 ms
    //Sender tsk: 2, items/tsk: 100000; Recv tsk: 1, items/tsk: 200000: 933 ms
    //Sender tsk: 1, items/tsk: 200000; Recv tsk: 2, items/tsk: 100000: 1460 ms
    //Sender tsk: 10, items/tsk: 50000; Recv tsk: 10, items/tsk: 50000: 1908 ms
    //Sender tsk: 50, items/tsk: 10000; Recv tsk: 50, items/tsk: 10000: 2969 ms
    //Sender tsk: 100, items/tsk: 10000; Recv tsk: 100, items/tsk: 10000: 4228 ms
    //Sender tsk: 10, items/tsk: 100000; Recv tsk: 100, items/tsk: 10000: 4980 ms
    //Sender tsk: 100, items/tsk: 10000; Recv tsk: 10, items/tsk: 100000: 5078 ms
    //Sender tsk: 500, items/tsk: 1000; Recv tsk: 500, items/tsk: 1000: 2182 ms
    //Sender tsk: 1000, items/tsk: 1000; Recv tsk: 1000, items/tsk: 1000: 5470 ms
    //Sender tsk: 10, items/tsk: 100000; Recv tsk: 1000, items/tsk: 1000: 5115 ms
    //Sender tsk: 1000, items/tsk: 1000; Recv tsk: 10, items/tsk: 100000: 4448 ms
    //Sender tsk: 2000, items/tsk: 1000; Recv tsk: 2000, items/tsk: 1000: 10098 ms

    //Select non-buffered:
    //Sender tsk: 1, items/tsk: 100000; Recv tsk: 1, items/tsk: 100000: 1102 ms
    //Sender tsk: 2, items/tsk: 100000; Recv tsk: 1, items/tsk: 200000: 2182 ms
    //Sender tsk: 1, items/tsk: 200000; Recv tsk: 2, items/tsk: 100000: 1574 ms
    //Sender tsk: 10, items/tsk: 10000; Recv tsk: 10, items/tsk: 10000: 649 ms
    //Sender tsk: 50, items/tsk: 2000; Recv tsk: 50, items/tsk: 2000: 754 ms
    //Sender tsk: 100, items/tsk: 1000; Recv tsk: 100, items/tsk: 1000: 689 ms
    //Sender tsk: 10, items/tsk: 10000; Recv tsk: 100, items/tsk: 1000: 631 ms
    //Sender tsk: 100, items/tsk: 1000; Recv tsk: 10, items/tsk: 10000: 694 ms
    //Sender tsk: 500, items/tsk: 250; Recv tsk: 500, items/tsk: 250: 791 ms
    //Sender tsk: 1000, items/tsk: 100; Recv tsk: 1000, items/tsk: 100: 639 ms
    //Sender tsk: 10, items/tsk: 10000; Recv tsk: 1000, items/tsk: 100: 653 ms
    //Sender tsk: 1000, items/tsk: 100; Recv tsk: 10, items/tsk: 10000: 622 ms
    //Sender tsk: 2000, items/tsk: 100; Recv tsk: 2000, items/tsk: 100: 1364 ms

    //Select buffered:
    //Sender tsk: 1, items/tsk: 100000; Recv tsk: 1, items/tsk: 100000: 2432 ms
    //Sender tsk: 2, items/tsk: 100000; Recv tsk: 1, items/tsk: 200000: 2777 ms
    //Sender tsk: 1, items/tsk: 200000; Recv tsk: 2, items/tsk: 100000: 1587 ms
    //Sender tsk: 10, items/tsk: 10000; Recv tsk: 10, items/tsk: 10000: 795 ms
    //Sender tsk: 50, items/tsk: 2000; Recv tsk: 50, items/tsk: 2000: 907 ms
    //Sender tsk: 100, items/tsk: 1000; Recv tsk: 100, items/tsk: 1000: 749 ms
    //Sender tsk: 10, items/tsk: 10000; Recv tsk: 100, items/tsk: 1000: 769 ms
    //Sender tsk: 100, items/tsk: 1000; Recv tsk: 10, items/tsk: 10000: 783 ms
    //Sender tsk: 500, items/tsk: 250; Recv tsk: 500, items/tsk: 250: 1030 ms
    //Sender tsk: 1000, items/tsk: 100; Recv tsk: 1000, items/tsk: 100: 684 ms
    //Sender tsk: 10, items/tsk: 10000; Recv tsk: 1000, items/tsk: 100: 895 ms
    //Sender tsk: 1000, items/tsk: 100; Recv tsk: 10, items/tsk: 10000: 789 ms
    //Sender tsk: 2000, items/tsk: 100; Recv tsk: 2000, items/tsk: 100: 1656 ms
}
