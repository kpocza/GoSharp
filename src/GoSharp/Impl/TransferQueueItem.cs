﻿namespace GoSharp.Impl
{
    internal sealed class TransferQueueItem
    { 
        internal ChannelOperation ChannelOperation { get; private set; }

        internal SelectFireContext SelectFireContext { get; private set; }

        internal TransferQueueItem(ChannelOperation channelOperation)
        {
            ChannelOperation = channelOperation;
        }

        internal TransferQueueItem(ChannelOperation channelOperation, SelectFireContext selectFireContext) : this(channelOperation)
        {
            SelectFireContext = selectFireContext;
        }
    }
}
