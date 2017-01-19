using System;

namespace GoSharp
{
    public class ChannelClosedException : Exception
    {
        public ChannelClosedException() : base("The channel is already closed")
        {
            
        }
    }
}
