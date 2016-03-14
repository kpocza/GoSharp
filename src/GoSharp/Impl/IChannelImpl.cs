using System.Threading;

namespace Go.Impl
{
    public interface IChannelImpl<T>
    {
        #region Standard methods

        void Send(T message);
        T Recv();

        #endregion

        #region Select Recv
        object SelectRecvPrepare();

        T SelectRecvChosen(object token);

        void SelectRecvNotChosen(object token);

        WaitHandle SelectRecvGetItemAvailable(object token);

        #endregion

        #region Select Send
        object SelectSendPrepare();

        void SelectSendChosen(object token, T item);

        void SelectSendNotChosen(object token);

        WaitHandle SelectSendGetItemAvailable(object token);

        #endregion
    }
}
