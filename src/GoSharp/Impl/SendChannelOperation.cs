namespace GoSharp.Impl
{
    internal sealed class SendChannelOperation : ChannelOperation
    {
        internal SendChannelOperation(ChannelBase channel, CompletionEvent evt, object msg) : base(channel, evt)
        {
            Msg = msg;
        }
    }
}
