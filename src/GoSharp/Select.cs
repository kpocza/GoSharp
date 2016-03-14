using System;

namespace Go
{
    public static class Select
    {
        public static SelectCase CaseRecv<T>(Channel<T> channel, Action<T> action)
        {
            var selectCase = new SelectCase();
            selectCase.CaseRecv(channel, action);
            return selectCase;
        }

        public static SelectCase CaseSend<T>(Channel<T> channel, T item)
        {
            var selectCase = new SelectCase();
            selectCase.CaseSend(channel, item);
            return selectCase;
        }

        public static SelectCase CaseSend<T>(Channel<T> channel, Func<T> item)
        {
            var selectCase = new SelectCase();
            selectCase.CaseSend(channel, item);
            return selectCase;
        }
    }
}
