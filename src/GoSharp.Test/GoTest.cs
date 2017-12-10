using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoSharp.Test
{
    [TestClass]
    public class GoTest
    {
        #region Merge

        [TestMethod]
        public void Merge2Test()
        {
            var input1 = Channel<int>.CreateNonBuffered();
            var input2 = Channel<int>.CreateNonBuffered();
            var output = Channel<int>.CreateNonBuffered();
            var res = new List<int>();
            int cnt = 100;

            Go.Run(async () =>
            {
                for (int i = 0; i < cnt; i++)
                {
                    await input1.SendAsync(i);
                }
            });

            Go.Run(async () =>
            {
                for (int i = 0; i < cnt; i++)
                {
                    await input2.SendAsync(-i);
                }
            });

            Go.Merge(input1, input2, output);

            while (res.Count < cnt * 2)
            {
                res.Add(output.Recv());
            }

            input1.Close();
            input2.Close();
            output.Close();

            Assert.AreEqual(0, res.Sum());
        }

        [TestMethod]
        public void Merge4Test()
        {
            var input1 = Channel<int>.CreateNonBuffered();
            var input2 = Channel<int>.CreateNonBuffered();
            var input3 = Channel<int>.CreateNonBuffered();
            var input4 = Channel<int>.CreateNonBuffered();
            var output = Channel<int>.CreateNonBuffered();
            var res = new List<int>();
            int cnt = 100;

            Go.Run(async () =>
            {
                for (int i = 0; i < cnt; i++)
                {
                    await input1.SendAsync(i);
                }
            });

            Go.Run(async () =>
            {
                for (int i = 0; i < cnt; i++)
                {
                    await input2.SendAsync(-i);
                }
            });

            Go.Run(async () =>
            {
                for (int i = 0; i < cnt; i++)
                {
                    await input3.SendAsync(i * i);
                }
            });

            Go.Run(async () =>
            {
                for (int i = 0; i < cnt; i++)
                {
                    await input4.SendAsync(-(i * i));
                }
            });

            Go.Merge(new[] { input1, input2, input3, input4 }, output);

            while (res.Count < cnt * 4)
            {
                res.Add(output.Recv());
            }

            input1.Close();
            input2.Close();
            input3.Close();
            input4.Close();
            output.Close();

            Assert.AreEqual(0, res.Sum());
        }

        #endregion

        #region Zip

        [TestMethod]
        public void Zip2Test()
        {
            var input1 = Channel<int>.CreateNonBuffered();
            var input2 = Channel<int>.CreateNonBuffered();
            var output = Channel<Tuple<int, int>>.CreateNonBuffered();
            int cnt = 100;

            Go.Run(async () =>
            {
                for (int i = 0; i < cnt; i++)
                {
                    await input1.SendAsync(i);
                }
            });

            Go.Run(async () =>
            {
                for (int i = 0; i < cnt; i++)
                {
                    await input2.SendAsync(-i);
                }
            });

            Go.Zip(input1, input2, output);

            for (int i = 0; i < cnt; i++)
            {
                var item = output.Recv();
                Assert.AreEqual(-item.Item1, item.Item2);
            }

            input1.Close();
            input2.Close();
            output.Close();
        }

        [TestMethod]
        public void Zip3Test()
        {
            var input1 = Channel<int>.CreateNonBuffered();
            var input2 = Channel<int>.CreateNonBuffered();
            var input3 = Channel<int>.CreateNonBuffered();
            var output = Channel<Tuple<int, int, int>>.CreateNonBuffered();
            int cnt = 100;

            Go.Run(async () =>
            {
                for (int i = 0; i < cnt; i++)
                {
                    await input1.SendAsync(i);
                }
            });

            Go.Run(async () =>
            {
                for (int i = 0; i < cnt; i++)
                {
                    await input2.SendAsync(i * i);
                }
            });

            Go.Run(async () =>
            {
                for (int i = 0; i < cnt; i++)
                {
                    await input3.SendAsync(i * i * i);
                }
            });

            Go.Zip(input1, input2, input3, output);

            for (int i = 0; i < cnt; i++)
            {
                var item = output.Recv();
                Assert.AreEqual(item.Item1 * item.Item1, item.Item2);
                Assert.AreEqual(item.Item1 * item.Item1 * item.Item1, item.Item3);
            }

            input1.Close();
            input2.Close();
            input3.Close();
            output.Close();
        }

        [TestMethod]
        public void Zip4Test()
        {
            var input1 = Channel<int>.CreateNonBuffered();
            var input2 = Channel<int>.CreateNonBuffered();
            var input3 = Channel<int>.CreateNonBuffered();
            var input4 = Channel<int>.CreateNonBuffered();
            var output = Channel<Tuple<int, int, int, int>>.CreateNonBuffered();
            int cnt = 100;

            Go.Run(async () =>
            {
                for (int i = 0; i < cnt; i++)
                {
                    await input1.SendAsync(i);
                }
            });

            Go.Run(async () =>
            {
                for (int i = 0; i < cnt; i++)
                {
                    await input2.SendAsync(i * i);
                }
            });

            Go.Run(async () =>
            {
                for (int i = 0; i < cnt; i++)
                {
                    await input3.SendAsync(i * i * i);
                }
            });

            Go.Run(async () =>
            {
                for (int i = 0; i < cnt; i++)
                {
                    await input4.SendAsync(i * i * i * i);
                }
            });
            Go.Zip(input1, input2, input3, input4, output);

            for (int i = 0; i < cnt; i++)
            {
                var item = output.Recv();
                Assert.AreEqual(item.Item1 * item.Item1, item.Item2);
                Assert.AreEqual(item.Item1 * item.Item1 * item.Item1, item.Item3);
                Assert.AreEqual(item.Item1 * item.Item1 * item.Item1 * item.Item1, item.Item4);
            }

            input1.Close();
            input2.Close();
            input3.Close();
            input4.Close();
            output.Close();
        }

        #endregion

        #region Buffer

        [TestMethod]
        public void Buffer1Test()
        {
            var input = Channel<int>.CreateNonBuffered();
            var output = Channel<ICollection<int>>.CreateNonBuffered();

            int cnt = 100;
            int buffer = 5;

            Go.Run(async () =>
            {
                for (int i = 0; i < cnt; i++)
                {
                    await input.SendAsync(i);
                }
            });

            Go.Buffer(input, output, buffer);

            var res = new List<int>();
            for (int i = 0; i < cnt / buffer; i++)
            {
                var part = output.Recv();
                res.AddRange(part);
            }

            input.Close();
            output.Close();

            Assert.IsTrue(Enumerable.SequenceEqual(Enumerable.Range(0, 100), res));
        }

        [TestMethod]
        public void Buffer2Test()
        {
            var input = Channel<int>.CreateNonBuffered();
            var output = Channel<ICollection<int>>.CreateNonBuffered();

            int cnt = 100;
            int buffer = 5;

            Go.Run(async () =>
            {
                for (int i = 0; i < cnt; i++)
                {
                    await Task.Delay(10);
                    await input.SendAsync(i);
                }
            });

            Go.Buffer(input, output, buffer, TimeSpan.FromMilliseconds(30));

            int parts = 0;
            var res = new List<int>();
            while(res.Count < cnt)
            {
                var part = output.Recv();
                res.AddRange(part);
                parts++;
            }

            input.Close();
            output.Close();

            Assert.IsTrue(parts > cnt / buffer);
            Assert.IsTrue(Enumerable.SequenceEqual(Enumerable.Range(0, 100), res));
        }

        [TestMethod]
        public void Buffer3Test()
        {
            var input = Channel<int>.CreateNonBuffered();
            var output = Channel<ICollection<int>>.CreateNonBuffered();

            int cnt = 100;

            Go.Run(async () =>
            {
                for (int i = 0; i < cnt; i++)
                {
                    await Task.Delay(10);
                    await input.SendAsync(i);
                }
            });

            Go.Buffer(input, output, TimeSpan.FromMilliseconds(30));

            int parts = 0;
            var res = new List<int>();
            while (res.Count < cnt)
            {
                var part = output.Recv();
                res.AddRange(part);
                parts++;
            }

            input.Close();
            output.Close();

            Assert.IsTrue(parts >= cnt / 3);
            Assert.IsTrue(Enumerable.SequenceEqual(Enumerable.Range(0, 100), res));
        }

        #endregion

        #region Broadcast

        [TestMethod]
        public void Broadcast2Test()
        {
            var input = Channel<int>.CreateNonBuffered();
            var output1 = Channel<int>.CreateNonBuffered();
            var output2 = Channel<int>.CreateNonBuffered();
            var done = Channel<bool>.CreateNonBuffered();

            int cnt = 100;

            Go.Run(async () =>
            {
                for (int i = 0; i < cnt; i++)
                {
                    await input.SendAsync(i);
                }
            });

            Go.Broadcast(input, output1, output2);

            var res1 = new List<int>();
            var res2 = new List<int>();

            Go.Run(async () =>
            {
                for (int i = 0; i < cnt; i++)
                {
                    res1.Add(await output1.RecvAsync());
                }
                await done.SendAsync(true);
            });

            Go.Run(async () =>
            {
                for (int i = 0; i < cnt; i++)
                {
                    res2.Add(await output2.RecvAsync());
                }
                await done.SendAsync(true);
            });

            done.Recv();
            done.Recv();

            input.Close();
            output1.Close();
            output2.Close();

            Assert.IsTrue(Enumerable.SequenceEqual(Enumerable.Range(0, 100), res1));
            Assert.IsTrue(Enumerable.SequenceEqual(Enumerable.Range(0, 100), res2));
        }

        [TestMethod]
        public void Broadcast4Test()
        {
            var input = Channel<int>.CreateNonBuffered();
            var output1 = Channel<int>.CreateNonBuffered();
            var output2 = Channel<int>.CreateNonBuffered();
            var output3 = Channel<int>.CreateNonBuffered();
            var output4 = Channel<int>.CreateNonBuffered();
            var done = Channel<bool>.CreateNonBuffered();

            int cnt = 100;

            Go.Run(async () =>
            {
                for (int i = 0; i < cnt; i++)
                {
                    await input.SendAsync(i);
                }
            });

            Go.Broadcast(input, new[] { output1, output2, output3, output4 });

            var res1 = new List<int>();
            var res2 = new List<int>();
            var res3 = new List<int>();
            var res4 = new List<int>();

            Go.Run(async () =>
            {
                for (int i = 0; i < cnt; i++)
                {
                    res1.Add(await output1.RecvAsync());
                }
                await done.SendAsync(true);
            });

            Go.Run(async () =>
            {
                for (int i = 0; i < cnt; i++)
                {
                    res2.Add(await output2.RecvAsync());
                }
                await done.SendAsync(true);
            });

            Go.Run(async () =>
            {
                for (int i = 0; i < cnt; i++)
                {
                    res3.Add(await output3.RecvAsync());
                }
                await done.SendAsync(true);
            });

            Go.Run(async () =>
            {
                for (int i = 0; i < cnt; i++)
                {
                    res4.Add(await output4.RecvAsync());
                }
                await done.SendAsync(true);
            });

            done.Recv();
            done.Recv();
            done.Recv();
            done.Recv();

            input.Close();
            output1.Close();
            output2.Close();
            output3.Close();
            output4.Close();

            Assert.IsTrue(Enumerable.SequenceEqual(Enumerable.Range(0, 100), res1));
            Assert.IsTrue(Enumerable.SequenceEqual(Enumerable.Range(0, 100), res2));
            Assert.IsTrue(Enumerable.SequenceEqual(Enumerable.Range(0, 100), res3));
            Assert.IsTrue(Enumerable.SequenceEqual(Enumerable.Range(0, 100), res4));
        }

        #endregion
    }
}
