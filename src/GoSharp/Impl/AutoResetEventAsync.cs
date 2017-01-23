using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoSharp.Impl
{
    // based on https://blogs.msdn.microsoft.com/pfxteam/2012/02/11/building-async-coordination-primitives-part-2-asyncautoresetevent/
    internal class AsyncAutoResetEvent
    {
        private static readonly Task Completed = Task.FromResult(true);
        private readonly Queue<TaskCompletionSource<bool>> _waits = new Queue<TaskCompletionSource<bool>>();
        private volatile bool _signaled;

        internal AsyncAutoResetEvent(bool signaled)
        {
            _signaled = signaled;
        }

        internal Task WaitOneAsync()
        {
            lock (_waits)
            {
                if (_signaled)
                {
                    _signaled = false;
                    return Completed;
                }

                var tcs = new TaskCompletionSource<bool>();
                _waits.Enqueue(tcs);
                return tcs.Task;
            }
        }

        internal void Set()
        {
            TaskCompletionSource<bool> toRelease = null;

            lock (_waits)
            {
                if (_waits.Count > 0)
                    toRelease = _waits.Dequeue();
                else
                    _signaled = true;
            }

            toRelease?.SetResult(true);
        }
    }
}
