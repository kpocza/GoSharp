using System.Threading;
using Go.Infra;

namespace Go.Impl
{
    internal sealed class BufferedChannel<T> : IChannelImpl<T>
    {
        private readonly BoundedBlockingQueue<T> _queue;

        internal BufferedChannel(int size)
        {
            _queue = new BoundedBlockingQueue<T>(size);
        }

        public void Send(T message)
        {
            _queue.Enqueue(message);
        }

        public T Recv()
        {
            return _queue.Dequeue();
        }

        public object SelectRecvPrepare()
        {
            return null;
        }

        public T SelectRecvChosen(object token)
        {
            return _queue.PreAssuredDequeue();
        }

        public void SelectRecvNotChosen(object token)
        {
        }

        public WaitHandle SelectRecvGetItemAvailable(object token)
        {
            return _queue.ItemAvailable;
        }

        public object SelectSendPrepare()
        {
            return null;
        }

        public void SelectSendChosen(object token, T item)
        {
            _queue.PreAssuredEnqueue(item);
        }

        public void SelectSendNotChosen(object token)
        {
        }

        public WaitHandle SelectSendGetItemAvailable(object token)
        {
            return _queue.SpaceAvailable;
        }
    }
}
