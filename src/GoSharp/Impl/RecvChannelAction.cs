using System.Reflection;
using System.Threading;

namespace Go.Impl
{
    internal sealed class RecvChannelAction : ChannelAction
    {
        internal MethodInfo Action { get; }
        internal object Target { get; }

        internal RecvChannelAction(ISelectableChannel channel, MethodInfo action, object target) : base(channel)
        {
            Target = target;
            Action = action;
        }

        internal override WaitHandle GetWaitHandle()
        {
            return Channel.SelectRecvGetItemAvailable(Token);
        }

        internal override void SelectPrepare()
        {
            Token = Channel.SelectRecvPrepare();
        }

        internal override object SelectChosen()
        {
            return Channel.SelectRecvChosen(Token);
        }

        internal override void SelectNotChosen()
        {
            Channel.SelectRecvNotChosen(Token);
        }
    }
}
