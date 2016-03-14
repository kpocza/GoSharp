using System;
using System.Collections.Generic;
using System.Reflection;
using Go.Impl;

namespace Go
{
    public class SelectCase
    {
        private readonly List<ChannelAction> _channelActions;
        private Action _defaultAction;

        internal SelectCase()
        {
            _channelActions = new List<ChannelAction>();
            _defaultAction = null;
        }

        public SelectCase CaseRecv<T>(Channel<T> channel, Action<T> action)
        {
            if (channel == null)
                throw new ArgumentNullException(nameof(channel));
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            CheckCaseCount();

            channel.EnsureNotClosed();

            _channelActions.Add(new RecvChannelAction(channel, action.GetMethodInfo(), action.Target));
            return this;
        }

        public SelectCase CaseSend<T>(Channel<T> channel, T item)
        {
            if (channel == null)
                throw new ArgumentNullException(nameof(channel));
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            CheckCaseCount();

            channel.EnsureNotClosed();

            _channelActions.Add(new SendChannelAction(channel, item));
            return this;
        }

        public SelectCase CaseSend<T>(Channel<T> channel, Func<T> item)
        {
            if (channel == null)
                throw new ArgumentNullException(nameof(channel));
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            CheckCaseCount();

            channel.EnsureNotClosed();

            _channelActions.Add(new SendChannelAction(channel, item.GetMethodInfo(), item.Target));
            return this;
        }

        public void Default(Action defaultAction)
        {
            if(defaultAction == null)
                throw new ArgumentNullException(nameof(defaultAction));

            _defaultAction = defaultAction;
            Go();
        }

        public void Go()
        {
            var selectImpl = new SelectImpl(_channelActions, _defaultAction);
            selectImpl.Go();
        }

        private void CheckCaseCount()
        {
            if (_channelActions.Count >= 64)
                throw new TooManyCasesException();
        }
    }
}
