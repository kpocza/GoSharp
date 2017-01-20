using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GoSharp.Impl
{
    public class ChannelBase
    {
        private readonly TransferQueue _readerQueue;
        private readonly TransferQueue _writerQueue;
        private readonly object _lockObj;
        private readonly int _queueSize;
        private readonly Queue<object> _msgQueue;
        private static int _globalUidCounter = 0;
        protected readonly int _uid;

        private volatile bool _isClosed;

        internal ChannelBase(int queueSize)
        {
            _uid = Interlocked.Increment(ref _globalUidCounter);

            _readerQueue = new TransferQueue();
            _writerQueue = new TransferQueue();
            _lockObj = new object();

            _queueSize = queueSize;
            if (_queueSize > 0)
            {
                _msgQueue = new Queue<object>(_queueSize);
            }

            _isClosed = false;
        }

        public int Uid => _uid;

        protected bool SendCore(object msg)
        {
            Lock();

            if (_isClosed)
            {
                Unlock();
                throw new ChannelClosedException();
            }

            TransferQueueItem readerQueueItem;
            if (_readerQueue.TryDequeue(out readerQueueItem))
            {
                Unlock();

                ((RecvChannelOperation)readerQueueItem.ChannelOperation).Msg = msg;
                readerQueueItem.ChannelOperation.Notify();
                return true;
            }

            if (IsBuffered && _msgQueue.Count < _queueSize)
            {
                _msgQueue.Enqueue(msg);
                Unlock();
                return true;
            }

            var evt = new AutoResetEvent(false);
            var sendOperation = new SendChannelOperation(this, evt, msg);
            _writerQueue.Enqueue(new TransferQueueItem(sendOperation));

            Unlock();

            evt.WaitOne();

            if (_isClosed)
            {
                throw new ChannelClosedException();
            }

            return true;
        }

        protected object RecvCore()
        {
            Lock();

            if (_isClosed)
            {
                Unlock();
                throw new ChannelClosedException();
            }

            TransferQueueItem writerQueueItem;
            if (_writerQueue.TryDequeue(out writerQueueItem))
            {
                Unlock();
                var msg = ((SendChannelOperation)writerQueueItem.ChannelOperation).Msg;

                writerQueueItem.ChannelOperation.Notify();

                return msg;
            }

            if (IsBuffered && _msgQueue.Any())
            {
                var msg = _msgQueue.Dequeue();
                Unlock();

                return msg;
            }

            var evt = new AutoResetEvent(false);
            var recvOperation = new RecvChannelOperation(this, evt);
            _readerQueue.Enqueue(new TransferQueueItem(recvOperation));

            Unlock();

            evt.WaitOne();

            if (_isClosed)
            {
                throw new ChannelClosedException();
            }

            return recvOperation.Msg;
        }

        protected IEnumerable<object> RangeCore()
        {
            while (!IsClosed)
            {
                object item;
                try
                {
                    item =  RecvCore();
                }
                catch (ChannelClosedException)
                {
                    yield break;
                }
                yield return item;
            }
        }

        protected void CloseCore()
        {
            Lock();
            _isClosed = true;

            TransferQueueItem tqi;
            while (_readerQueue.TryDequeue(out tqi))
            {
                tqi.ChannelOperation.Notify();
            }

            while (_writerQueue.TryDequeue(out tqi))
            {
                tqi.ChannelOperation.Notify();
            }

            Unlock();
        }


        internal bool SendFast(SendChannelOperation sendChannelOperation, Action unlock)
        {
            TransferQueueItem readerQueueItem;
            if (_readerQueue.TryDequeue(out readerQueueItem))
            {
                unlock();

                ((RecvChannelOperation)readerQueueItem.ChannelOperation).Msg = sendChannelOperation.Msg;

                readerQueueItem.ChannelOperation.Notify();
                return true;
            }

            if (IsBuffered && _msgQueue.Count < _queueSize)
            {
                _msgQueue.Enqueue(sendChannelOperation.Msg);
                unlock();

                return true;
            }

            return false;
        }

        internal bool ReceiveFast(RecvChannelOperation recvChannelOperation, Action unlock)
        {
            TransferQueueItem writerQueueItem;
            if (_writerQueue.TryDequeue(out writerQueueItem))
            {
                unlock();
                recvChannelOperation.Msg = ((SendChannelOperation)writerQueueItem.ChannelOperation).Msg;

                writerQueueItem.ChannelOperation.Notify();

                return true;
            }

            if (IsBuffered && _msgQueue.Any())
            {
                var msg = _msgQueue.Dequeue();
                unlock();

                recvChannelOperation.Msg = msg;

                return true;
            }

            return false;
        }

        internal void Enqueue(ChannelOperation channelOperation, SelectFireContext selectFireContext, AutoResetEvent evt)
        {
            if (channelOperation is RecvChannelOperation)
            {
                _readerQueue.Enqueue(new TransferQueueItem(channelOperation, selectFireContext));
            }
            else
            {
                _writerQueue.Enqueue(new TransferQueueItem(channelOperation, selectFireContext));
            }
        }

        internal bool IsBuffered => _queueSize > 0;

        internal bool IsClosed => _isClosed;

        internal void Lock()
        {
            Monitor.Enter(_lockObj);
        }

        internal void Unlock()
        {
            Monitor.Exit(_lockObj);
        }
    }
}
