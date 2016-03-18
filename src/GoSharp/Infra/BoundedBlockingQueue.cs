using System;
using System.Collections.Generic;
using System.Threading;

namespace Go.Infra
{
    sealed class BoundedBlockingQueue<T> : IDisposable
    {
        private readonly Queue<T> _queue;
        private Semaphore _itemsAvailable;
        private Semaphore _spaceAvailable;
        private ManualResetEvent _stoppedEvent;

        internal BoundedBlockingQueue(int size)
        {
            _itemsAvailable = new Semaphore(0, size);
            _spaceAvailable = new Semaphore(size, size);
            _queue = new Queue<T>(size);
            _stoppedEvent = new ManualResetEvent(false);
        }

        internal void Enqueue(T data)
        {
            WaitOrClose(_spaceAvailable);

            PreAssuredEnqueue(data);
        }

        internal T Enqueue(Func<T> func)
        {
            WaitOrClose(_spaceAvailable);

            T item;
            lock (_queue)
            {
                item = func();
                _queue.Enqueue(item);
            }

            _itemsAvailable.Release();
            return item;
        }

        internal T Dequeue()
        {
            WaitOrClose(_itemsAvailable);

            return PreAssuredDequeue();
        }

        internal T PreAssuredDequeue()
        {
            T item;
            lock (_queue)
            {
                item = _queue.Dequeue();
            }

            _spaceAvailable.Release();
            return item;
        }

        internal void PreAssuredEnqueue(T data)
        {
            lock (_queue)
            {
                _queue.Enqueue(data);
            }

            _itemsAvailable.Release();
        }

        internal void SafeRemoveItem(T item)
        {
            if (_itemsAvailable.WaitOne(0))
            {
                lock (_queue)
                {
                    var deQueueItem = _queue.Dequeue();
                    _spaceAvailable.Release();

                    if (!object.ReferenceEquals(item, deQueueItem))
                    {
                        if (_spaceAvailable.WaitOne(0))
                        {
                            _queue.Enqueue(deQueueItem);
                            _itemsAvailable.Release();
                        }
                    }
                }
            }
        }

        void IDisposable.Dispose()
        {
            if (_itemsAvailable != null)
            {
                _itemsAvailable.Dispose();
                _spaceAvailable.Dispose();
                _itemsAvailable = null;
                _spaceAvailable = null;
            }
        }

        public void Stop()
        {
            _stoppedEvent.Set();
        }

        private void WaitOrClose(Semaphore waitSemaphore)
        {
            int waitHandleIndex = WaitHandle.WaitAny(new WaitHandle[] {waitSemaphore, _stoppedEvent});

            if(waitHandleIndex == 1)
                throw new QueueStoppedException();
        }

        internal WaitHandle ItemAvailable => _itemsAvailable;
        internal WaitHandle SpaceAvailable => _spaceAvailable;
    }
}