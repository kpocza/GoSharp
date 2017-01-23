using System;

namespace GoSharp
{
    public static class Select
    {
        public static SelectSet CaseRecv<T>(Channel<T> channel, Action<T> action)
        {
            var selectSet = new SelectSet();
            selectSet.CaseRecv(channel, action);
            return selectSet;
        }

        public static SelectSet CaseSend<T>(Channel<T> channel, T msg)
        {
            var selectSet = new SelectSet();
            selectSet.CaseSend(channel, msg);
            return selectSet;
        }
    }
}
