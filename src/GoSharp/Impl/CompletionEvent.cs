using System.Threading.Tasks;

namespace GoSharp.Impl
{
    internal sealed class CompletionEvent
    {
        private readonly TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();

        internal void Notify()
        {
            _tcs.SetResult(true);
        }

        internal Task WaitAsync()
        {
            return _tcs.Task;
        }
    }
}
