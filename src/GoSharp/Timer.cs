using System;
using GoSharp.Impl;

namespace GoSharp
{
    public sealed class Timer : SchedulingBase, IRecvChannel<DateTime>
    {
        public Timer(TimeSpan timeout) : base(timeout)
        {
        }

        protected override void StartCore()
        {
            _coreTimer = new System.Threading.Timer(_ =>
            {
                _channel.YieldingSendFast(DateTime.Now);
                lock (_lock)
                {
                    DropTimer();
                    _coreTimer = null;
                }
            }, null, _timeout, TimeSpan.Zero);
        }
    }
}
