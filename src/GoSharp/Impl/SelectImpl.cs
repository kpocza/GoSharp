using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Go.Impl
{
    internal class SelectImpl
    {
        private List<ChannelAction> _channelActions;
        private readonly Action _defaultAction;
        private readonly TimeSpan _timeout;
        private readonly Action _timeoutAction;

        internal SelectImpl(List<ChannelAction> channelActions, Action defaultAction, TimeSpan timeout, Action timeoutAction)
        {
            _channelActions = channelActions;
            _defaultAction = defaultAction;
            _timeout = timeout;
            _timeoutAction = timeoutAction;
        }

        internal void Go()
        {
            if (!_channelActions.Any())
                return;

            Shuffle();

            PrepareSelectCases();

            var waitHandles =_channelActions.Select(ca => ca.GetWaitHandle()).ToArray();
            var waitIndex = Wait(waitHandles);

            var result = FinalizeSelectCases(waitHandles, waitIndex);

            if (waitIndex != WaitHandle.WaitTimeout)
            {
                ExecuteAction(waitIndex, result);
            }
            else
            {
                if (HasDefaultAction)
                {
                    _defaultAction.Invoke();
                }

                if(HasTimeout)
                {
                    _timeoutAction.Invoke();
                }
            }
        }

        private int Wait(WaitHandle[] waitHandles)
        {
            int waitIndex = -1;

            if (!HasDefaultAction && !HasTimeout)
            {
                waitIndex = WaitHandle.WaitAny(waitHandles);
            }
            else if (HasDefaultAction && !HasTimeout)
            {
                waitIndex = WaitHandle.WaitAny(waitHandles, 0);
            }
            else if (!HasDefaultAction && HasTimeout)
            {
                waitIndex = WaitHandle.WaitAny(waitHandles, _timeout);
            }
            return waitIndex;
        }

        private void Shuffle()
        {
            if (_channelActions.Count <= 1)
                return;

            var tmpList = new List<ChannelAction>(_channelActions.Count);

            var rnd = new Random(Environment.TickCount * 32 + Thread.CurrentThread.ManagedThreadId);
            while (_channelActions.Any())
            {
                var nextIndex = rnd.Next()%_channelActions.Count;
                var item = _channelActions[nextIndex];
                _channelActions.RemoveAt(nextIndex);
                tmpList.Add(item);
            }

            _channelActions = tmpList;
        }

        private object FinalizeSelectCases(WaitHandle[] waitHandles, int waitIndex)
        {
            object result = null;
            for (var idx = 0; idx < waitHandles.Length; idx++)
            {
                var channelAction = _channelActions[idx];
                if (idx == waitIndex)
                {
                    result = channelAction.SelectChosen();
                }
                else
                {
                    channelAction.SelectNotChosen();
                }
            }
            return result;
        }

        private void PrepareSelectCases()
        {
            foreach (var channelAction in _channelActions)
            {
                channelAction.SelectPrepare();
            }
        }

        private void ExecuteAction(int waitIndex, object result)
        {
            var channelAction = _channelActions[waitIndex] as RecvChannelAction;
            channelAction?.Action.Invoke(channelAction.Target, new[] {result});
        }

        private bool HasDefaultAction => _defaultAction != null;
        private bool HasTimeout => _timeoutAction != null;
    }
}
