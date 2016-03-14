using System.Threading;
using Go.Infra;

namespace Go.Impl
{
    internal sealed class NonBufferedChannel<T> : IChannelImpl<T>
    {
        private readonly BoundedBlockingQueue<Passer<T>> _queue;

        internal NonBufferedChannel()
        {
            _queue = new BoundedBlockingQueue<Passer<T>>(1);
        }

        public void Send(T message)
        {
            Passer<T> passer;
            do
            {
                passer = _queue.Dequeue();
                passer.Message = message;
                Interlocked.MemoryBarrier();
                try
                {
                    passer.Signal?.Set();
                }
                catch
                {
                }
                if (passer.IsSelect)
                {
                    passer.SelectSignal.WaitOne();
                    passer.SelectSignal.Dispose();
                }
            } while (passer.IsSelect && passer.SelectResult == SelectResult.NotSelected);
        }

        public T Recv()
        {
            var passer = _queue.Enqueue(() => new Passer<T> {Signal = new AutoResetEvent(false), IsSelect = false});
            passer.Signal.WaitOne();
            passer.Signal.Dispose();
            return passer.Message;
        }

        public object SelectRecvPrepare()
        {
            var passer = new Passer<T>
            {
                Signal = new AutoResetEvent(false),
                SelectSignal = new AutoResetEvent(false),
                IsSelect = true,
                SelectResult = SelectResult.None
            };
            _queue.Enqueue(passer);

            return passer;
        }

        public T SelectRecvChosen(object token)
        {
            var passer = (Passer<T>) token;
            passer.Signal.Dispose();
            passer.Signal = null;
            passer.SelectResult = SelectResult.Selected;
            passer.SelectSignal.Set();
            return passer.Message;
        }

        public void SelectRecvNotChosen(object token)
        {
            var passer = (Passer<T>)token;
            passer.Signal.Dispose();
            passer.Signal = null;
            passer.SelectResult = SelectResult.NotSelected;
            passer.SelectSignal.Set();

            _queue.SafeRemoveItem(passer);
        }

        public WaitHandle SelectRecvGetItemAvailable(object token)
        {
            var passer = (Passer<T>)token;
            return passer.Signal;
        }

        public object SelectSendPrepare()
        {
            return null;
        }

        public void SelectSendChosen(object token, T item)
        {
            var passer = _queue.PreAssuredDequeue();
            passer.Message = item;
            Interlocked.MemoryBarrier();
            try
            {
                passer.Signal?.Set();
            }
            catch
            {
            }
            if (passer.IsSelect)
            {
                passer.SelectSignal.WaitOne();
                passer.SelectSignal.Dispose();
            }
        }

        public void SelectSendNotChosen(object token)
        {
        }

        public WaitHandle SelectSendGetItemAvailable(object token)
        {
            return _queue.ItemAvailable;
        }

        class Passer<TP>
        {
            public TP Message;
            public AutoResetEvent Signal;
            public AutoResetEvent SelectSignal;
            public bool IsSelect;
            public SelectResult SelectResult;
        }

        enum SelectResult
        {
            None = 0,
            Selected = 1,
            NotSelected = 2
        }
    }
}
