using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoSharp.Impl
{
    internal sealed class SelectLogic
    {
        private readonly List<ChannelOperation> _channelOperations;
        private readonly CompletionEvent _evt;
        private readonly SelectFireContext _selectFireContext;
        private Action _defaultAction;
        private Timer _timer;
        private Action _timeoutAction;

        internal SelectLogic()
        {
            _channelOperations = new List<ChannelOperation>();
            _evt = new CompletionEvent();
            _selectFireContext = new SelectFireContext();
        }

        internal void AddRecv<T>(Channel<T> channel, Action<T> action)
        {
            if(_channelOperations.Any(co => co.Channel == channel))
                throw new InvalidOperationException("Channel has been already registered in an other case");

            var recvChannelOperation = new RecvChannelOperation(channel, _evt, msg => action((T)msg));
            _channelOperations.Add(recvChannelOperation);
        }

        internal void AddSend<T>(Channel<T> channel, object msg)
        {
            if (_channelOperations.Any(co => co.Channel == channel))
                throw new InvalidOperationException("Channel has been already registered in an other case");

            var sendChannelOperation = new SendChannelOperation(channel, _evt, msg);
            _channelOperations.Add(sendChannelOperation);
        }

        internal void AddDefault(Action defaultAction)
        {
            _defaultAction = defaultAction;
        }

        internal void AddTimeout(TimeSpan timeout, Action timeoutAction)
        {
            _timer = new Timer(timeout);
            _timeoutAction = timeoutAction;
        }

        internal async Task GoAsync()
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

            if (_defaultAction != null || _channelOperations.All(c => c.Channel.IsClosed))
            {
                UnlockAllChannels();
                _defaultAction?.Invoke();
                return;
            }

            foreach (var channelOperation in _channelOperations)
            {
                if (channelOperation.Channel.IsClosed)
                    continue;

                if (channelOperation is RecvChannelOperation)
                {
                    var recvChannelOperation = (RecvChannelOperation)channelOperation;
                    var channel = channelOperation.Channel;

                    channel.Enqueue(recvChannelOperation, _selectFireContext);
                }
                else
                {
                    var sendChannelOperation = (SendChannelOperation)channelOperation;
                    var channel = channelOperation.Channel;

                    channel.Enqueue(sendChannelOperation, _selectFireContext);
                }
            }

            UnlockAllChannels();

            if (_timer != null)
            {
                var timerChannelOperation = new RecvChannelOperation(_timer.Channel, _evt, _ => _timeoutAction());
                _channelOperations.Add(timerChannelOperation);
                _timer.Channel.Enqueue(timerChannelOperation, _selectFireContext);
                _timer.Start();
            }

            if (_channelOperations.All(c => c.Channel.IsClosed))
                return;

            await _evt.WaitAsync();

            var firedChannelOperation = _selectFireContext.Fired.ChannelOperation;

            if (firedChannelOperation != null)
            {
                if (!firedChannelOperation.Channel.IsClosed)
                {
                    var firedRecvChannelOperation = firedChannelOperation as RecvChannelOperation;
                    firedRecvChannelOperation?.ExecuteAction();
                }
            }

            _timer?.Close();
        }

        private void LockAllChannels()
        {
            foreach (var channelOperation in _channelOperations.OrderBy(c => c.Channel.Id))
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
    }
}
