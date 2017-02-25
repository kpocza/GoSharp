namespace GoSharp.Impl
{
    internal abstract class ChannelOperation
    {
        internal ChannelBase Channel { get; private set; }

        internal object Msg { get; set; }

        protected readonly CompletionEvent _evt;

        internal ChannelOperation(ChannelBase channel, CompletionEvent evt)
        {
            Channel = channel;
            _evt = evt;
        }

        internal void Notify()
        {
            _evt.Notify();
        }
    }
}
