using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GoSharp.Impl;

namespace GoSharp
{
    public sealed class Channel<T> : ChannelBase, ISendChannel<T>, IRecvChannel<T>
    {
        private Channel(int queueSize) : base(queueSize)
        {
        } 

        public static Channel<T> CreateNonBuffered()
        {
            return new Channel<T>(0);
        }

        public static Channel<T> CreateBuffered(int queueSize)
        {
            if(queueSize < 0 || queueSize > 10000)
                throw new ArgumentOutOfRangeException(nameof(queueSize));

            return new Channel<T>(queueSize);
        }

        public bool Send(T msg)
        {
            return SendCore(msg);
        }

        public T Recv()
        {
            return (T) RecvCore();
        }

        public Task<bool> SendAsync(T msg)
        {
            return SendCoreAsync(msg);
        }

        public async Task<T> RecvAsync()
        {
            return (T)await RecvCoreAsync();
        }

        public IEnumerable<T> Range
        {
            get
            {
                foreach (var item in RangeCore())
                {
                    yield return (T) item;
                }
            }
        }

        public void Close()
        {
            CloseCore();
        }

        public static Channel<T> operator +(Channel<T> channel, T message)
        {
            channel.Send(message);
            return channel;
        }

        public static T operator -(Channel<T> channel)
        {
            return channel.Recv();
        }
    }
}
