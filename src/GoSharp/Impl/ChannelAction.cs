using System.Threading;

namespace Go.Impl
{
    internal abstract class ChannelAction
    {
        internal ISelectableChannel Channel { get; }

        internal object Token { get; set; }

        internal ChannelAction(ISelectableChannel channel)
        {
            Channel = channel;
        }

        internal abstract WaitHandle GetWaitHandle();

        internal abstract void SelectPrepare();

        internal abstract object SelectChosen();

        internal abstract void SelectNotChosen();
    }
}
