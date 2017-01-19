using System.Threading;

namespace GoSharp.Impl
{
    internal abstract class ChannelOperation
    {
        internal ChannelBase Channel { get; private set; }

        protected readonly object _target;

        protected readonly AutoResetEvent _evt;

        internal ChannelOperation(ChannelBase channel, AutoResetEvent evt,  object target)
        {
            Channel = channel;
            _evt = evt;
            _target = target;
        }

        internal void Notify()
        {
            _evt.Set();
        }
    }
}
