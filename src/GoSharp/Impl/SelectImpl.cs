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

        internal SelectImpl(List<ChannelAction> channelActions, Action defaultAction)
        {
            _channelActions = channelActions;
            _defaultAction = defaultAction;
        }

        internal void Go()
        {
            if (!_channelActions.Any())
                return;

            Shuffle();

            PrepareSelectCases();

            var waitHandles =_channelActions.Select(ca => ca.GetWaitHandle()).ToArray();

            var waitIndex = _defaultAction == null
                ? WaitHandle.WaitAny(waitHandles)
                : WaitHandle.WaitAny(waitHandles, 0);

            var result = FinalizeSelectCases(waitHandles, waitIndex);

            if (waitIndex != WaitHandle.WaitTimeout)
            {
                ExecuteAction(waitIndex, result);
            }
            else
            {
                _defaultAction?.Invoke();
            }
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
    }
}
