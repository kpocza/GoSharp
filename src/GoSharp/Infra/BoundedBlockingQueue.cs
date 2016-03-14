using System;
using System.Collections.Generic;
using System.Threading;
using Go.Impl;

namespace Go.Infra
{
    sealed class BoundedBlockingQueue<T> : IDisposable
    {
        private readonly Queue<T> _queue;
        private Semaphore _itemsAvailable;
        private Semaphore _spaceAvailable;

        internal BoundedBlockingQueue(int size)
        {
            _itemsAvailable = new Semaphore(0, size);
            _spaceAvailable = new Semaphore(size, size);
            _queue = new Queue<T>(size);
        }

        internal void Enqueue(T data)
        {
            _spaceAvailable.WaitOne();

            PreAssuredEnqueue(data);
        }

        internal T Enqueue(Func<T> func)
        {
            _spaceAvailable.WaitOne();

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
            _itemsAvailable.WaitOne();

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

        internal WaitHandle ItemAvailable => _itemsAvailable;
        internal WaitHandle SpaceAvailable => _spaceAvailable;
    }
}