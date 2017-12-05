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
            Console.WriteLine($"Sender tsk: {senderTaskCount}, items/tsk: {sendItemCount}; Recv tsk: {recvTaskCount}, items/tsk: {recvItemCount}: {stopWatch.ElapsedMilliseconds} ms");

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

            Console.WriteLine($"Sender tsk: {senderTaskCount}, items/tsk: {sendItemCount}; Recv tsk: {recvTaskCount}, items/tsk: {recvItemCount}: {stopWatch.ElapsedMilliseconds} ms");

            if (cntSend != cntRecv || cntSend != senderTaskCount * sendItemCount)
                throw new Exception("channel error");
        }
    }

    //Task tests:
    //~~~~~~~~~~~

    //Non-buffered:
    //Sender tsk: 1, items/tsk: 200000; Recv tsk: 1, items/tsk: 200000: 145 ms
    //Sender tsk: 2, items/tsk: 100000; Recv tsk: 1, items/tsk: 200000: 130 ms
    //Sender tsk: 1, items/tsk: 200000; Recv tsk: 2, items/tsk: 100000: 131 ms
    //Sender tsk: 10, items/tsk: 50000; Recv tsk: 10, items/tsk: 50000: 339 ms
    //Sender tsk: 50, items/tsk: 10000; Recv tsk: 50, items/tsk: 10000: 283 ms
    //Sender tsk: 100, items/tsk: 10000; Recv tsk: 100, items/tsk: 10000: 550 ms
    //Sender tsk: 10, items/tsk: 100000; Recv tsk: 100, items/tsk: 10000: 590 ms
    //Sender tsk: 100, items/tsk: 10000; Recv tsk: 10, items/tsk: 100000: 633 ms
    //Sender tsk: 500, items/tsk: 1000; Recv tsk: 500, items/tsk: 1000: 324 ms
    //Sender tsk: 1000, items/tsk: 1000; Recv tsk: 1000, items/tsk: 1000: 644 ms
    //Sender tsk: 10, items/tsk: 100000; Recv tsk: 1000, items/tsk: 1000: 588 ms
    //Sender tsk: 1000, items/tsk: 1000; Recv tsk: 10, items/tsk: 100000: 610 ms
    //Sender tsk: 2000, items/tsk: 1000; Recv tsk: 2000, items/tsk: 1000: 1386 ms

    //Buffered:
    //Sender tsk: 1, items/tsk: 200000; Recv tsk: 1, items/tsk: 200000: 111 ms
    //Sender tsk: 2, items/tsk: 100000; Recv tsk: 1, items/tsk: 200000: 140 ms
    //Sender tsk: 1, items/tsk: 200000; Recv tsk: 2, items/tsk: 100000: 168 ms
    //Sender tsk: 10, items/tsk: 50000; Recv tsk: 10, items/tsk: 50000: 326 ms
    //Sender tsk: 50, items/tsk: 10000; Recv tsk: 50, items/tsk: 10000: 382 ms
    //Sender tsk: 100, items/tsk: 10000; Recv tsk: 100, items/tsk: 10000: 790 ms
    //Sender tsk: 10, items/tsk: 100000; Recv tsk: 100, items/tsk: 10000: 728 ms
    //Sender tsk: 100, items/tsk: 10000; Recv tsk: 10, items/tsk: 100000: 713 ms
    //Sender tsk: 500, items/tsk: 1000; Recv tsk: 500, items/tsk: 1000: 368 ms
    //Sender tsk: 1000, items/tsk: 1000; Recv tsk: 1000, items/tsk: 1000: 719 ms
    //Sender tsk: 10, items/tsk: 100000; Recv tsk: 1000, items/tsk: 1000: 714 ms
    //Sender tsk: 1000, items/tsk: 1000; Recv tsk: 10, items/tsk: 100000: 635 ms
    //Sender tsk: 2000, items/tsk: 1000; Recv tsk: 2000, items/tsk: 1000: 1596 ms

    //Select non-buffered:
    //Sender tsk: 1, items/tsk: 100000; Recv tsk: 1, items/tsk: 100000: 709 ms
    //Sender tsk: 2, items/tsk: 100000; Recv tsk: 1, items/tsk: 200000: 1383 ms
    //Sender tsk: 1, items/tsk: 200000; Recv tsk: 2, items/tsk: 100000: 1504 ms
    //Sender tsk: 10, items/tsk: 10000; Recv tsk: 10, items/tsk: 10000: 528 ms
    //Sender tsk: 50, items/tsk: 2000; Recv tsk: 50, items/tsk: 2000: 543 ms
    //Sender tsk: 100, items/tsk: 1000; Recv tsk: 100, items/tsk: 1000: 472 ms
    //Sender tsk: 10, items/tsk: 10000; Recv tsk: 100, items/tsk: 1000: 486 ms
    //Sender tsk: 100, items/tsk: 1000; Recv tsk: 10, items/tsk: 10000: 403 ms
    //Sender tsk: 500, items/tsk: 250; Recv tsk: 500, items/tsk: 250: 856 ms
    //Sender tsk: 1000, items/tsk: 100; Recv tsk: 1000, items/tsk: 100: 1105 ms
    //Sender tsk: 10, items/tsk: 10000; Recv tsk: 1000, items/tsk: 100: 1089 ms
    //Sender tsk: 1000, items/tsk: 100; Recv tsk: 10, items/tsk: 10000: 441 ms
    //Sender tsk: 2000, items/tsk: 100; Recv tsk: 2000, items/tsk: 100: 3256 ms

    //Select buffered:
    //Sender tsk: 1, items/tsk: 100000; Recv tsk: 1, items/tsk: 100000: 584 ms
    //Sender tsk: 2, items/tsk: 100000; Recv tsk: 1, items/tsk: 200000: 1169 ms
    //Sender tsk: 1, items/tsk: 200000; Recv tsk: 2, items/tsk: 100000: 1563 ms
    //Sender tsk: 10, items/tsk: 10000; Recv tsk: 10, items/tsk: 10000: 521 ms
    //Sender tsk: 50, items/tsk: 2000; Recv tsk: 50, items/tsk: 2000: 687 ms
    //Sender tsk: 100, items/tsk: 1000; Recv tsk: 100, items/tsk: 1000: 573 ms
    //Sender tsk: 10, items/tsk: 10000; Recv tsk: 100, items/tsk: 1000: 594 ms
    //Sender tsk: 100, items/tsk: 1000; Recv tsk: 10, items/tsk: 10000: 518 ms
    //Sender tsk: 500, items/tsk: 250; Recv tsk: 500, items/tsk: 250: 1026 ms
    //Sender tsk: 1000, items/tsk: 100; Recv tsk: 1000, items/tsk: 100: 1217 ms
    //Sender tsk: 10, items/tsk: 10000; Recv tsk: 1000, items/tsk: 100: 1162 ms
    //Sender tsk: 1000, items/tsk: 100; Recv tsk: 10, items/tsk: 10000: 399 ms
    //Sender tsk: 2000, items/tsk: 100; Recv tsk: 2000, items/tsk: 100: 3376 ms
}
