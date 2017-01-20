using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace GoSharp.Impl
{
    internal class TransferQueue
    {
        private readonly ConcurrentQueue<TransferQueueItem> _transferQueue;

        internal TransferQueue()
        {
            _transferQueue = new ConcurrentQueue<TransferQueueItem>();
        }

        internal void Enqueue(TransferQueueItem transferQueueItem)
        {
            _transferQueue.Enqueue(transferQueueItem);
        }

        internal bool TryDequeue(out TransferQueueItem transferQueueItem)
        {
            TransferQueueItem tqi;
            transferQueueItem = null;

            while (_transferQueue.TryDequeue(out tqi))
            {
                if (tqi.SelectFireContext == null ||
                    Interlocked.CompareExchange(ref tqi.SelectFireContext.Fired, tqi, null) == null)
                {
                    transferQueueItem = tqi;
                    return true;
                }
            }

            return false;
        }

        internal bool HasItem => _transferQueue.Any();
    }
}
