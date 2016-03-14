using System;
using System.Reflection;
using System.Threading;

namespace Go.Impl
{
    internal sealed class SendChannelAction : ChannelAction
    {
        private readonly MethodInfo _methodInfo;
        private readonly object _target;
        internal object Item { get; }

        internal SendChannelAction(ISelectableChannel channel, object item) : base(channel)
        {
            Item = item;
        }

        internal SendChannelAction(ISelectableChannel channel, MethodInfo methodInfo, object target) : base(channel)
        {
            _methodInfo = methodInfo;
            _target = target;
        }

        internal override WaitHandle GetWaitHandle()
        {
            return Channel.SelectSendGetItemAvailable(Token);
        }

        internal override void SelectPrepare()
        {
            Token = Channel.SelectSendPrepare();
        }

        internal override object SelectChosen()
        {
            if (_target == null)
            {
                Channel.SelectSendChosen(Token, Item);
            }
            else
            {
                var item = _methodInfo.Invoke(_target, null);
                Channel.SelectSendChosen(Token, item);
            }
            return null;
        }

        internal override void SelectNotChosen()
        {
            Channel.SelectSendNotChosen(Token);
        }
    }
}
