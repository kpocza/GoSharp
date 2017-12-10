using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoSharp.Impl
{
    internal static class GoLogic
    {
        internal static void MergeCore<T>(Channel<T>[] channels, Channel<T> output)
        {
            Task.Run(async () =>
            {
                int idx = 0;
                while (true)
                {
                    try
                    {
                        var value = await channels[idx].RecvAsync();
                        await output.SendAsync(value);
                        idx = (idx + 1) % channels.Length;
                    }
                    catch(ChannelClosedException)
                    {
                        break;
                    }
                }
            });
        }

        internal static void ZipCore<T1, T2>(Channel<T1> input1, Channel<T2> input2, Channel<Tuple<T1, T2>> output)
        {
            Task.Run(async () =>
            {
                while(true)
                {
                    try
                    {
                        var value1 = await input1.RecvAsync();
                        var value2 = await input2.RecvAsync();

                        await output.SendAsync(new Tuple<T1, T2>(value1, value2));
                    }
                    catch(ChannelClosedException)
                    {
                        break;
                    }
                }
            });
        }

        internal static void ZipCore<T1, T2, T3>(Channel<T1> input1, Channel<T2> input2, Channel<T3> input3, Channel<Tuple<T1, T2, T3>> output)
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        var value1 = await input1.RecvAsync();
                        var value2 = await input2.RecvAsync();
                        var value3 = await input3.RecvAsync();

                        await output.SendAsync(new Tuple<T1, T2, T3>(value1, value2, value3));
                    }
                    catch (ChannelClosedException)
                    {
                        break;
                    }
                }
            });
        }

        internal static void ZipCore<T1, T2, T3, T4>(Channel<T1> input1, Channel<T2> input2, Channel<T3> input3, Channel<T4> input4, Channel<Tuple<T1, T2, T3, T4>> output)
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        var value1 = await input1.RecvAsync();
                        var value2 = await input2.RecvAsync();
                        var value3 = await input3.RecvAsync();
                        var value4 = await input4.RecvAsync();

                        await output.SendAsync(new Tuple<T1, T2, T3, T4>(value1, value2, value3, value4));
                    }
                    catch (ChannelClosedException)
                    {
                        break;
                    }
                }
            });
        }

        internal static void BufferCore<T>(Channel<T> input, Channel<ICollection<T>> output, int blockSize, TimeSpan? timeout)
        {
            Task.Run(async () =>
            {
                var outputBlock = new List<T>();
                try
                {
                    var recvStart = DateTime.UtcNow;
                    while (true)
                    {
                        bool flush = false;
                        if (timeout == null)
                        {
                            var item = await input.RecvAsync();
                            outputBlock.Add(item);
                        }
                        else
                        {
                            var left = timeout.Value - (DateTime.UtcNow - recvStart);
                            if (left.Ticks > 0)
                            {
                                await Select
                                    .CaseRecv(input, item => outputBlock.Add(item))
                                    .TimeoutAsync(left, () => flush = true);
                            }
                            else
                            {
                                flush = true;
                            }
                        }

                        if (outputBlock.Count == blockSize || flush)
                        {
                            if (outputBlock.Count > 0)
                            {
                                await output.SendAsync(outputBlock);
                                outputBlock = new List<T>();
                            }
                            recvStart = DateTime.UtcNow;
                        }
                    }
                }
                finally
                {
                    if (outputBlock.Count > 0)
                    {
                        await output.SendAsync(outputBlock);
                    }
                }
            });
        }

        internal static void BroadcastCore<T>(Channel<T> input, Channel<T>[] outputs)
        {
            Task.Run(async () =>
            {
                while(true)
                {
                    try
                    {
                        var inp = await input.RecvAsync();
                        foreach (var output in outputs)
                        {
                            await output.SendAsync(inp);
                        }
                    }
                    catch(ChannelClosedException)
                    {
                        break;
                    }
                }
            });
        }
    }
}
