using System.Collections.Generic;
using GoSharp.Impl;

namespace GoSharp
{
    public class Channel<T> : ChannelBase
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
