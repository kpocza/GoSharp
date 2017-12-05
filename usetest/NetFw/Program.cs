using GoSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetFw
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
