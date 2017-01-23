namespace GoSharp.Impl
{
    internal abstract class ChannelOperation
    {
        internal ChannelBase Channel { get; private set; }

        protected readonly object _target;

        protected readonly AsyncAutoResetEvent _evt;

        internal ChannelOperation(ChannelBase channel, AsyncAutoResetEvent evt,  object target)
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
