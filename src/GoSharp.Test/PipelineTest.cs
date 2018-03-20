using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace GoSharp.Test
{
    [TestClass]
    public class PipelineTest
    {
        [TestMethod]
        public void StartStop()
        {
            var pipeline = new Pipeline();

            pipeline.Chain(async (int a) => await Task.Run(() => a.ToString()));
            pipeline.Chain((string a) => int.Parse(a));

            pipeline.Start();
            pipeline.Stop();
        }

        [TestMethod]
        public void SimpleProc()
        {
            var list = new List<int>();
            var pipeline = new Pipeline();

            pipeline.Chain(async (int a) => await Task.Run(() => a.ToString()));
            pipeline.Chain((string a) => int.Parse(a));

            var chIn = pipeline.GetInChannel<int>();
            var chOut = pipeline.GetOutChannel<int>();

            pipeline.Start();

            Task.Run(() => { for (int i = 0; i < 3; i++) list.Add(chOut.Recv()); });

            chIn.Send(1);
            chIn.Send(2);
            chIn.Send(3);

            Thread.Sleep(100);

            pipeline.Stop();

            Assert.IsTrue(new[] { 1, 2, 3 }.SequenceEqual(list));
        }

        [TestMethod]
        public void MultiProc()
        {
            var list = new List<int>();
            var pipeline = new Pipeline();

            pipeline.ChainMulti<int, int>(async (int a) => await Task.Run(() => new[] { a, a * a }));
            pipeline.ChainMulti((int a) => new[] { a, a * a });

            var chIn = pipeline.GetInChannel<int>();
            var chOut = pipeline.GetOutChannel<int>();

            pipeline.Start();

            Task.Run(() => { for (int i = 0; i < 12; i++) list.Add(chOut.Recv()); });

            chIn.Send(1);
            chIn.Send(2);
            chIn.Send(3);

            Thread.Sleep(100);

            pipeline.Stop();

            Assert.IsTrue(new[] { 1, 1, 1, 1, 2, 4, 4, 16, 3, 9, 9, 81}.SequenceEqual(list));
        }
    }
}
