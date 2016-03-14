using System;

namespace Go
{
    public class ChannelClosedException : Exception
    {
        public ChannelClosedException() : base("The channel is already closed")
        {
            
        }
    }
}
