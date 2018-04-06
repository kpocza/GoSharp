using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GoSharp.Impl;

namespace GoSharp
{
    public static class Go
    {
        public static void Run(Action action)
        {
            Task.Run(action);
        }

        public static void Merge<T>(Channel<T> input1, Channel<T> input2, Channel<T> output)
        {
            if (input1 == null)
                throw new ArgumentNullException(nameof(input1));
            if (input2 == null)
                throw new ArgumentNullException(nameof(input2));
            if (output == null)
                throw new ArgumentNullException(nameof(output));

            GoLogic.MergeCore(new[] { input1, input2}, output);
        }

        public static void Merge<T>(IEnumerable<Channel<T>> inputs, Channel<T> output)
        {
            if (inputs == null)
                throw new ArgumentNullException(nameof(inputs));
            if (output == null)
                throw new ArgumentNullException(nameof(output));

            foreach (var input in inputs)
            {
                if (input == null)
                    throw new ArgumentException("One or more input channels are null");
            }

            GoLogic.MergeCore(inputs.ToArray(), output);
        }

        public static void Zip<T1, T2>(Channel<T1> input1, Channel<T2> input2, Channel<Tuple<T1, T2>> output)
        {
            if (input1 == null)
                throw new ArgumentNullException(nameof(input1));
            if (input2 == null)
                throw new ArgumentNullException(nameof(input2));
            if (output == null)
                throw new ArgumentNullException(nameof(output));

            GoLogic.ZipCore(input1, input2, output);
        }

        public static void Zip<T1, T2, T3>(Channel<T1> input1, Channel<T2> input2, Channel<T3> input3, Channel<Tuple<T1, T2, T3>> output)
        {
            if (input1 == null)
                throw new ArgumentNullException(nameof(input1));
            if (input2 == null)
                throw new ArgumentNullException(nameof(input2));
            if (input3 == null)
                throw new ArgumentNullException(nameof(input3));
            if (output == null)
                throw new ArgumentNullException(nameof(output));

            GoLogic.ZipCore(input1, input2, input3, output);
        }

        public static void Zip<T1, T2, T3, T4>(Channel<T1> input1, Channel<T2> input2, Channel<T3> input3, Channel<T4> input4, Channel<Tuple<T1, T2, T3, T4>> output)
        {
            if (input1 == null)
                throw new ArgumentNullException(nameof(input1));
            if (input2 == null)
                throw new ArgumentNullException(nameof(input2));
            if (input3 == null)
                throw new ArgumentNullException(nameof(input3));
            if (input4 == null)
                throw new ArgumentNullException(nameof(input4));
            if (output == null)
                throw new ArgumentNullException(nameof(output));

            GoLogic.ZipCore(input1, input2, input3, input4, output);
        }

        public static void Buffer<T>(Channel<T> input, Channel<ICollection<T>> output, int blockSize, TimeSpan? timeout = null)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (output == null)
                throw new ArgumentNullException(nameof(output));
            if (blockSize < 2)
                throw new ArgumentOutOfRangeException(nameof(blockSize));

            GoLogic.BufferCore(input, output, blockSize, timeout);
        }

        public static void Buffer<T>(Channel<T> input, Channel<ICollection<T>> output, TimeSpan timeout)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (output == null)
                throw new ArgumentNullException(nameof(output));
            if (timeout.Ticks < 0)
                throw new ArgumentOutOfRangeException(nameof(timeout));

            GoLogic.BufferCore(input, output, int.MaxValue, timeout);
        }

        public static void Broadcast<T>(Channel<T> input, Channel<T> output1, Channel<T> output2)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (output1 == null)
                throw new ArgumentNullException(nameof(output1));
            if (output2 == null)
                throw new ArgumentNullException(nameof(output2));

            GoLogic.BroadcastCore(input, new[] { output1, output2 });
        }

        public static void Broadcast<T>(Channel<T> input, IEnumerable<Channel<T>> outputs)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (outputs == null)
                throw new ArgumentNullException(nameof(outputs));

            foreach (var output in outputs)
            {
                if (output == null)
                    throw new ArgumentException("One ore more of the output channels are null");
            }

            GoLogic.BroadcastCore(input, outputs.ToArray());
        }
    }
}
