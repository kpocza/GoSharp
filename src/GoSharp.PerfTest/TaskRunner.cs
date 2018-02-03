using System;
using System.Diagnostics;
using System.Threading;

namespace GoSharp.PerfTest
{
    public class TaskRunner
    {
        public void Run()
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

        private void SendRecvRunTask(int senderTaskCount, int sendItemCount, int recvTaskCount, int recvItemCount, int? bufferSize = null)
        {
            var stopWatch = new Stopwatch();
            var channel = !bufferSize.HasValue ? Channel<int>.CreateNonBuffered() : Channel<int>.CreateBuffered(bufferSize.Value);
            int cntSend = 0;
            int cntRecv = 0;

            if (senderTaskCount * sendItemCount != recvTaskCount * recvItemCount)
                throw new ArgumentException("Invalid parameterization");

            var cde = new CountdownEvent(recvTaskCount + senderTaskCount);

            stopWatch.Start();

            for (int i = 0; i < recvTaskCount; i++)
            {
                Go.Run(async () =>
                {
                    for (int j = 0; j < recvItemCount; j++)
                    {
                        await channel.RecvAsync();
                        Interlocked.Increment(ref cntRecv);

                    }
                    cde.Signal();
                });
            }

            for (int i = 0; i < senderTaskCount; i++)
            {
                Go.Run(async () =>
                {
                    for (int j = 0; j < sendItemCount; j++)
                    {
                        await channel.SendAsync(i);
                        Interlocked.Increment(ref cntSend);
                    };
                    cde.Signal();
                });
            }

            cde.Wait();

            stopWatch.Stop();
            Console.WriteLine($"Sender task: {senderTaskCount}, items/task: {sendItemCount}; Recv task: {recvTaskCount}, items/task: {recvItemCount}: {stopWatch.ElapsedMilliseconds} ms");

            if (cntSend != cntRecv || cntSend != senderTaskCount * sendItemCount)
                throw new Exception("channel error");
        }

        private void SelectSendRecvRunTask(int channelCount, int senderTaskCount, int sendItemCount, int recvTaskCount, int recvItemCount, int? bufferSize = null)
        {
            var stopWatch = new Stopwatch();
            var channels = new Channel<int>[channelCount];
            int cntSend = 0;
            int cntRecv = 0;

            if (senderTaskCount * sendItemCount != recvTaskCount * recvItemCount)
                throw new ArgumentException("Invalid parameterization");

            for (int i = 0; i < channelCount; i++)
            {
                channels[i] = !bufferSize.HasValue ? Channel<int>.CreateNonBuffered() : Channel<int>.CreateBuffered(bufferSize.Value);
            }

            var cde = new CountdownEvent(recvTaskCount + senderTaskCount);

            stopWatch.Start();

            for (int i = 0; i < recvTaskCount; i++)
            {
                Go.Run(async () =>
                {
                    for (int j = 0; j < recvItemCount; j++)
                    {
                        var sel = Select.CaseRecv(channels[0], _ => { });
                        for (int c = 1; c < channelCount; c++)
                        {
                            sel = sel.CaseRecv(channels[c], _ => { });
                        }
                        await sel.GoAsync();
                        Interlocked.Increment(ref cntRecv);
                    }
                    cde.Signal();
                });
            }

            for (int i = 0; i < senderTaskCount; i++)
            {
                Go.Run(async () =>
                {
                    var rnd = new Random();
                    for (int j = 0; j < sendItemCount; j++)
                    {
                        var fireChannel = rnd.Next(channelCount);
                        await channels[fireChannel].SendAsync(i);
                        Interlocked.Increment(ref cntSend);
                    }
                    cde.Signal();
                });
            }

            cde.Wait();

            stopWatch.Stop();

            Console.WriteLine($"Sender task: {senderTaskCount}, items/task: {sendItemCount}; Recv task: {recvTaskCount}, items/task: {recvItemCount}: {stopWatch.ElapsedMilliseconds} ms");

            if (cntSend != cntRecv || cntSend != senderTaskCount * sendItemCount)
                throw new Exception("channel error");
        }
    }

    //Task tests:
    //~~~~~~~~~~~

    //Non-buffered:
    //Sender task: 1, items/task: 200000; Recv task: 1, items/task: 200000: 148 ms
    //Sender task: 2, items/task: 100000; Recv task: 1, items/task: 200000: 134 ms
    //Sender task: 1, items/task: 200000; Recv task: 2, items/task: 100000: 131 ms
    //Sender task: 10, items/task: 50000; Recv task: 10, items/task: 50000: 339 ms
    //Sender task: 50, items/task: 10000; Recv task: 50, items/task: 10000: 281 ms
    //Sender task: 100, items/task: 10000; Recv task: 100, items/task: 10000: 584 ms
    //Sender task: 10, items/task: 100000; Recv task: 100, items/task: 10000: 554 ms
    //Sender task: 100, items/task: 10000; Recv task: 10, items/task: 100000: 573 ms
    //Sender task: 500, items/task: 1000; Recv task: 500, items/task: 1000: 309 ms
    //Sender task: 1000, items/task: 1000; Recv task: 1000, items/task: 1000: 599 ms
    //Sender task: 10, items/task: 100000; Recv task: 1000, items/task: 1000: 644 ms
    //Sender task: 1000, items/task: 1000; Recv task: 10, items/task: 100000: 627 ms
    //Sender task: 2000, items/task: 1000; Recv task: 2000, items/task: 1000: 1394 ms

    //Buffered:
    //Sender task: 1, items/task: 200000; Recv task: 1, items/task: 200000: 97 ms
    //Sender task: 2, items/task: 100000; Recv task: 1, items/task: 200000: 121 ms
    //Sender task: 1, items/task: 200000; Recv task: 2, items/task: 100000: 152 ms
    //Sender task: 10, items/task: 50000; Recv task: 10, items/task: 50000: 318 ms
    //Sender task: 50, items/task: 10000; Recv task: 50, items/task: 10000: 354 ms
    //Sender task: 100, items/task: 10000; Recv task: 100, items/task: 10000: 666 ms
    //Sender task: 10, items/task: 100000; Recv task: 100, items/task: 10000: 671 ms
    //Sender task: 100, items/task: 10000; Recv task: 10, items/task: 100000: 583 ms
    //Sender task: 500, items/task: 1000; Recv task: 500, items/task: 1000: 370 ms
    //Sender task: 1000, items/task: 1000; Recv task: 1000, items/task: 1000: 705 ms
    //Sender task: 10, items/task: 100000; Recv task: 1000, items/task: 1000: 686 ms
    //Sender task: 1000, items/task: 1000; Recv task: 10, items/task: 100000: 644 ms
    //Sender task: 2000, items/task: 1000; Recv task: 2000, items/task: 1000: 1637 ms

    //Select non-buffered:
    //Sender task: 1, items/task: 100000; Recv task: 1, items/task: 100000: 698 ms
    //Sender task: 2, items/task: 100000; Recv task: 1, items/task: 200000: 1353 ms
    //Sender task: 1, items/task: 200000; Recv task: 2, items/task: 100000: 1465 ms
    //Sender task: 10, items/task: 10000; Recv task: 10, items/task: 10000: 683 ms
    //Sender task: 50, items/task: 2000; Recv task: 50, items/task: 2000: 506 ms
    //Sender task: 100, items/task: 1000; Recv task: 100, items/task: 1000: 528 ms
    //Sender task: 10, items/task: 10000; Recv task: 100, items/task: 1000: 458 ms
    //Sender task: 100, items/task: 1000; Recv task: 10, items/task: 10000: 362 ms
    //Sender task: 500, items/task: 250; Recv task: 500, items/task: 250: 862 ms
    //Sender task: 1000, items/task: 100; Recv task: 1000, items/task: 100: 1035 ms
    //Sender task: 10, items/task: 10000; Recv task: 1000, items/task: 100: 1031 ms
    //Sender task: 1000, items/task: 100; Recv task: 10, items/task: 10000: 402 ms
    //Sender task: 2000, items/task: 100; Recv task: 2000, items/task: 100: 3181 ms

    //Select buffered:
    //Sender task: 1, items/task: 100000; Recv task: 1, items/task: 100000: 572 ms
    //Sender task: 2, items/task: 100000; Recv task: 1, items/task: 200000: 1131 ms
    //Sender task: 1, items/task: 200000; Recv task: 2, items/task: 100000: 1525 ms
    //Sender task: 10, items/task: 10000; Recv task: 10, items/task: 10000: 551 ms
    //Sender task: 50, items/task: 2000; Recv task: 50, items/task: 2000: 649 ms
    //Sender task: 100, items/task: 1000; Recv task: 100, items/task: 1000: 570 ms
    //Sender task: 10, items/task: 10000; Recv task: 100, items/task: 1000: 557 ms
    //Sender task: 100, items/task: 1000; Recv task: 10, items/task: 10000: 526 ms
    //Sender task: 500, items/task: 250; Recv task: 500, items/task: 250: 964 ms
    //Sender task: 1000, items/task: 100; Recv task: 1000, items/task: 100: 1164 ms
    //Sender task: 10, items/task: 10000; Recv task: 1000, items/task: 100: 1194 ms
    //Sender task: 1000, items/task: 100; Recv task: 10, items/task: 10000: 504 ms
    //Sender task: 2000, items/task: 100; Recv task: 2000, items/task: 100: 3410 ms
}
