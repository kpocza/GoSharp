namespace GoSharp.Impl
{
    internal sealed class SendChannelOperation : ChannelOperation
    {
        internal object Msg { get; private set; }

        internal SendChannelOperation(ChannelBase channel, CompletionEvent evt, object msg) : base(channel, evt, null)
        {
            Msg = msg;
        }
    }
}
