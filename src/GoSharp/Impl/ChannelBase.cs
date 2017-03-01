using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GoSharp.Impl
{
    public abstract class ChannelBase
    {
        private readonly TransferQueue _readerQueue;
        private readonly TransferQueue _writerQueue;
        private readonly object _lockObj;
        private readonly int _queueSize;
        private readonly Queue<object> _msgQueue;
        private static int _globalIdCounter = 0;
        protected readonly int _id;

        private volatile bool _isClosed;

        internal ChannelBase(int queueSize)
        {
            _id = Interlocked.Increment(ref _globalIdCounter);

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

        internal int Id => _id;

        protected async Task<bool> SendCoreAsync(object msg)
        {
            if (_isClosed)
                throw new ChannelClosedException();

            Lock();

            TransferQueueItem readerQueueItem;
            if (_readerQueue.TryDequeue(out readerQueueItem))
            {
                Unlock();

                readerQueueItem.ChannelOperation.Msg = msg;
                readerQueueItem.ChannelOperation.Notify();
                return true;
            }

            if (IsBuffered && _msgQueue.Count < _queueSize)
            {
                _msgQueue.Enqueue(msg);
                Unlock();
                return true;
            }

            var evt = new CompletionEvent();
            var sendOperation = new SendChannelOperation(this, evt, msg);
            _writerQueue.Enqueue(new TransferQueueItem(sendOperation));

            Unlock();

            await evt.WaitAsync();

            if (_isClosed)
                throw new ChannelClosedException();

            return true;
        }

        protected async Task<object> RecvCoreAsync()
        {
            if (_isClosed)
                throw new ChannelClosedException();

            Lock();

            TransferQueueItem writerQueueItem;
            if (_writerQueue.TryDequeue(out writerQueueItem))
            {
                Unlock();

                var msg = writerQueueItem.ChannelOperation.Msg;
                writerQueueItem.ChannelOperation.Notify();
                return msg;
            }

            if (IsBuffered && _msgQueue.Any())
            {
                var msg = _msgQueue.Dequeue();
                Unlock();

                return msg;
            }

            var evt = new CompletionEvent();
            var recvOperation = new RecvChannelOperation(this, evt);
            _readerQueue.Enqueue(new TransferQueueItem(recvOperation));

            Unlock();

            await evt.WaitAsync();

            if (_isClosed)
                throw new ChannelClosedException();

            return recvOperation.Msg;
        }


        protected bool SendCore(object msg)
        {
            try
            {
                return SendCoreAsync(msg).Result;
            }
            catch (AggregateException aggregateException)
            {
                throw aggregateException.InnerExceptions[0];
            }
        }

        protected object RecvCore()
        {
            try
            {
                return RecvCoreAsync().Result;
            }
            catch (AggregateException aggregateException)
            {
                throw aggregateException.InnerExceptions[0];
            }
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

            _readerQueue.DequeueNotifyAll();
            _writerQueue.DequeueNotifyAll();

            Unlock();
        }

        internal bool SendFast(SendChannelOperation sendChannelOperation, Action unlock)
        {
            TransferQueueItem readerQueueItem;
            if (_readerQueue.TryDequeue(out readerQueueItem))
            {
                unlock();

                readerQueueItem.ChannelOperation.Msg = sendChannelOperation.Msg;
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

                recvChannelOperation.Msg = writerQueueItem.ChannelOperation.Msg;
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

        internal void YieldingSendFast(object msg)
        {
            Lock();

            TransferQueueItem readerQueueItem;
            if (_readerQueue.TryDequeue(out readerQueueItem))
            {
                Unlock();

                readerQueueItem.ChannelOperation.Msg = msg;
                readerQueueItem.ChannelOperation.Notify();
                return;
            }

            if (IsBuffered && _msgQueue.Count < _queueSize)
            {
                _msgQueue.Enqueue(msg);
            }

            Unlock();
        }

        internal void Enqueue(ChannelOperation channelOperation, SelectFireContext selectFireContext)
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

        internal void Reset()
        {
            Lock();

            _readerQueue.DequeueNotifyAll();
            _writerQueue.DequeueNotifyAll();
            _msgQueue.Clear();            

            Unlock();
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
