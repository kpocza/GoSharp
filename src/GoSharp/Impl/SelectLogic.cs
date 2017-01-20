using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace GoSharp.Impl
{
    internal sealed class SelectLogic
    {
        private readonly List<ChannelOperation> _channelOperations;
        private readonly AutoResetEvent _evt;
        private readonly SelectFireContext _selectFireContext;
        private Action _defaultAction;

        internal SelectLogic()
        {
            _channelOperations = new List<ChannelOperation>();
            _evt = new AutoResetEvent(false);
            _selectFireContext = new SelectFireContext();
        }

        internal void AddRecv<T>(Channel<T> channel, Action<T> action)
        {
            var recvChannelOperation = new RecvChannelOperation(channel, _evt, action.GetMethodInfo(), action.Target);
            _channelOperations.Add(recvChannelOperation);
        }

        internal void AddSend<T>(Channel<T> channel, object msg)
        {
            var sendChannelOperation = new SendChannelOperation(channel, _evt, msg);
            _channelOperations.Add(sendChannelOperation);
        }

        internal void Go()
        {
            Shuffle();

            LockAllChannels();

            foreach (var channelOperation in _channelOperations)
            {
                if (channelOperation is RecvChannelOperation)
                {
                    var recvChannelOperation = (RecvChannelOperation)channelOperation;
                    var channel = channelOperation.Channel;

                    if (channel.ReceiveFast(recvChannelOperation, UnlockAllChannels))
                    {
                        recvChannelOperation.ExecuteAction();
                        return;
                    }
                }
                else
                {
                    var sendChannelOperation = (SendChannelOperation)channelOperation;
                    var channel = channelOperation.Channel;

                    if (channel.IsClosed)
                        continue;

                    if (channel.SendFast(sendChannelOperation, UnlockAllChannels))
                        return;
                }
            }

            if (_channelOperations.All(c => c.Channel.IsClosed))
            {
                UnlockAllChannels();
                _defaultAction?.Invoke();
                return;
            }

            if (_defaultAction != null)
            {
                UnlockAllChannels();
                _defaultAction();
                return;
            }

            foreach (var channelOperation in _channelOperations)
            {
                if (channelOperation is RecvChannelOperation)
                {
                    var recvChannelOperation = (RecvChannelOperation)channelOperation;
                    var channel = channelOperation.Channel;

                    channel.Enqueue(recvChannelOperation, _selectFireContext, _evt);
                }
                else
                {
                    var sendChannelOperation = (SendChannelOperation)channelOperation;
                    var channel = channelOperation.Channel;

                    channel.Enqueue(sendChannelOperation, _selectFireContext, _evt);
                }
            }

            UnlockAllChannels();

            _evt.WaitOne();

            // TODO: handle closure, timeout

            var firedChannelOperation = _selectFireContext.Fired.ChannelOperation;

            if (firedChannelOperation != null)
            {
                var firedRecvChannelOperation = firedChannelOperation as RecvChannelOperation;
                firedRecvChannelOperation?.ExecuteAction();
            }
        }

        private void LockAllChannels()
        {
            foreach (var channelOperation in _channelOperations.OrderBy(c => c.Channel.Uid))
            {
                channelOperation.Channel.Lock();
            }
        }

        private void UnlockAllChannels()
        {
            foreach (var channelOperation in _channelOperations)
            {
                channelOperation.Channel.Unlock();
            }
        }

        private void Shuffle()
        {
            var rnd = new Random();
            int n = _channelOperations.Count;
            while (n > 1)
            {
                n--;
                var k = rnd.Next(n + 1);
                var value = _channelOperations[k];
                _channelOperations[k] = _channelOperations[n];
                _channelOperations[n] = value;
            }
        }

        public void AddDefault(Action defaultAction)
        {
            _defaultAction = defaultAction;
        }
    }
}
