using GoSharp;
using System;

namespace NetCore2
{
    class Program
    {
        static void Main(string[] args)
        {
            var chan = Channel<int>.CreateBuffered(1);

            chan.Send(1);

            Console.WriteLine(chan.Recv());
        }
    }
}
