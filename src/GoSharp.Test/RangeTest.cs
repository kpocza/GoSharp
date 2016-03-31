using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;

namespace Go.Test
{
    public class RangeTest
    {
        [Fact]
        public void RangeNonBuffered()
        {
            var channel = Channel<int>.CreateNonBuffered();

            var thread = new Thread(() =>
            {
                for (int i = 1; i <= 1000; i++)
                {
                    channel.Send(i);
                }
                Thread.Sleep(100);
                channel.Close();
            });

            thread.Start();
            var items = new List<int>();

            foreach (var item in channel.Range)
            {
                items.Add(item);
            }

            Assert.Equal(Enumerable.Range(1, 1000), items);
        }

        [Fact]
        public void RangeBuffered()
        {
            var channel = Channel<int>.CreateBuffered(2);

            var thread = new Thread(() =>
            {
                for (int i = 1; i <= 1000; i++)
                {
                    channel.Send(i);
                }
                Thread.Sleep(100);
                channel.Close();
            });

            thread.Start();
            var items = new List<int>();

            foreach (var item in channel.Range)
            {
                items.Add(item);
            }

            Assert.Equal(Enumerable.Range(1, 1000), items);
        }
    }
}
