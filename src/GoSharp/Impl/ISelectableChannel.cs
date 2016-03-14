using System.Threading;

namespace Go.Impl
{
    internal interface ISelectableChannel
    {
        #region Select Recv
        object SelectRecvPrepare();

        object SelectRecvChosen(object token);

        void SelectRecvNotChosen(object token);

        WaitHandle SelectRecvGetItemAvailable(object token);

        #endregion

        #region Select Send
        object SelectSendPrepare();

        void SelectSendChosen(object token, object item);

        void SelectSendNotChosen(object token);

        WaitHandle SelectSendGetItemAvailable(object token);

        #endregion
    }
}
