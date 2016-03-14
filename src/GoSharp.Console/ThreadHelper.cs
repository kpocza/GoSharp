using System;
using System.Collections.Generic;
using System.Threading;

namespace Go.Console
{
    public class ThreadHelper
    {
        public static Thread CreateSenderThread(Channel<int> channel, int start, int end, int step)
        {
            return new Thread(() =>
            {
                for (int i = start; i <= end; i+=step)
                {
                    channel.Send(i);
                }
            });
        }

        public static Thread CreateReceiverThread(Channel<int> channel, int count, List<int> resultItems)
        {
            return new Thread(() =>
            {
                var items = new List<int>();
                for (int i = 0; i < count; i ++)
                {
                    var item = channel.Recv();
                    items.Add(item);
                }
                lock (resultItems)
                {
                    resultItems.AddRange(items);
                }
            });
        }

        public static void StartAndWait(params Thread[] threads)
        {
            foreach (var thread in threads)
            {
                thread.Start();
            }
            foreach (var thread in threads)
            {
                thread.Join();
            }
        }

        public static void Repeat(int count, Action action)
        {
            for (int i = 0; i < count; i++)
            {
                action();
            }
        }
    }
}