using System;

namespace GoSharp
{
    public sealed class ChannelClosedException : Exception
    {
        public ChannelClosedException() : base("The channel is closed")
        {
            
        }
    }
}
