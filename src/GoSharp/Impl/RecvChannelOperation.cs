using System;

namespace GoSharp.Impl
{
    internal sealed class RecvChannelOperation : ChannelOperation
    {
        private readonly Action<object> _action;

        internal RecvChannelOperation(ChannelBase channel, CompletionEvent evt, Action<object> action): base(channel, evt)
        {
            _action = action;
        }

        internal RecvChannelOperation(ChannelBase channel, CompletionEvent evt) : base(channel, evt)
        {
            _action = null;
        }

        internal void ExecuteAction()
        {
            _action(Msg);
        }
    }
}
