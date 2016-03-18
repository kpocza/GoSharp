using System;
using System.Threading;
using Go.Infra;

namespace Go.Impl
{
    internal sealed class NonBufferedChannel<T> : IChannelImpl<T>
    {
        private readonly BoundedBlockingQueue<Passer<T>> _queue;
        private readonly ManualResetEvent _closedEvent;

        internal NonBufferedChannel()
        {
            _queue = new BoundedBlockingQueue<Passer<T>>(1);
            _closedEvent = new ManualResetEvent(false);
        }

        public void Send(T message)
        {
            Passer<T> passer;
            do
            {
                try
                {
                    passer = _queue.Dequeue();
                }
                catch (QueueStoppedException)
                {
                    throw new ChannelClosedException();
                }
                SendBody(passer, message);
            } while (passer.IsSelect && passer.SelectResult == SelectResult.NotSelected);
        }

        public T Recv()
        {
            Passer<T> passer;
            try
            {
                passer = _queue.Enqueue(() => new Passer<T> {Signal = new AutoResetEvent(false), IsSelect = false});
            }
            catch(QueueStoppedException)
            {
                throw new ChannelClosedException();
            }
            WaitOrClosed(passer.Signal);
            passer.Signal.Dispose();
            return passer.Message;
        }

        public void Close()
        {
            _queue.Stop();
            _closedEvent.Set();
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
            SendBody(passer, item);
        }

        public void SelectSendNotChosen(object token)
        {
        }

        public WaitHandle SelectSendGetItemAvailable(object token)
        {
            return _queue.ItemAvailable;
        }

        private void SendBody(Passer<T> passer, T message)
        {
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
                WaitOrClosed(passer.SelectSignal);
                passer.SelectSignal.Dispose();
            }
        }

        private void WaitOrClosed(WaitHandle waitHandle)
        {
            int waitHandleindex = WaitHandle.WaitAny(new WaitHandle[] {waitHandle, _closedEvent});
            if(waitHandleindex == 1)
                throw new ChannelClosedException();
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
