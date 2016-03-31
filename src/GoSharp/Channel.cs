using System;
using System.Collections.Generic;
using System.Threading;
using Go.Impl;

namespace Go
{
    public class Channel<T> : ISelectableChannel
    {
        #region Creation

        private Channel(IChannelImpl<T> impl)
        {
            _impl = impl;
            Closed = false;
        }

        public static Channel<T> CreateNonBuffered()
        {
            return new Channel<T>(new NonBufferedChannel<T>());
        }

        public static Channel<T> CreateBuffered(int size)
        {
            if (size < 1)
                throw new ArgumentOutOfRangeException(nameof(size));

            return new Channel<T>(new BufferedChannel<T>(size));
        }

        #endregion

        #region Basic operations

        public void Send(T message)
        {
            EnsureNotClosed();

            _impl.Send(message);
        }

        public T Recv()
        {
            EnsureNotClosed();

            return _impl.Recv();
        }

        public void Close()
        {
            Closed = true;
            _impl.Close();
        }

        public IEnumerable<T> Range
        {
            get
            {
                while (!Closed)
                {
                    T item;
                    try
                    {
                        item = Recv();
                    }
                    catch (ChannelClosedException)
                    {
                        yield break;
                    }
                    yield return item;
                }
            }
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

        public bool Closed { get; private set; }

        #endregion

        #region ISelectableChannel implementation

        object ISelectableChannel.SelectRecvPrepare()
        {
            return _impl.SelectRecvPrepare();
        }

        object ISelectableChannel.SelectRecvChosen(object token)
        {
            return _impl.SelectRecvChosen(token);
        }

        void ISelectableChannel.SelectRecvNotChosen(object token)
        {
            _impl.SelectRecvNotChosen(token);
        }

        WaitHandle ISelectableChannel.SelectRecvGetItemAvailable(object token)
        {
            return _impl.SelectRecvGetItemAvailable(token);
        }

        object ISelectableChannel.SelectSendPrepare()
        {
            return _impl.SelectSendPrepare();
        }

        void ISelectableChannel.SelectSendChosen(object token, object item)
        {
            _impl.SelectSendChosen(token, (T)item);
        }

        void ISelectableChannel.SelectSendNotChosen(object token)
        {
            _impl.SelectSendNotChosen(token);
        }

        WaitHandle ISelectableChannel.SelectSendGetItemAvailable(object token)
        {
            return _impl.SelectSendGetItemAvailable(token);
        }

        #endregion

        #region Internal and private members

        private readonly IChannelImpl<T> _impl;

        internal void EnsureNotClosed()
        {
            if (Closed)
                throw new ChannelClosedException();
        }

        #endregion
    }
}
