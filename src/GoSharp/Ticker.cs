using System;
using GoSharp.Impl;

namespace GoSharp
{
    public sealed class Ticker: SchedulingBase, IRecvChannel<DateTime>
    {
        public Ticker(TimeSpan interval) : base(interval)
        {
        }

        protected override void StartCore()
        {
            _coreTimer = new System.Threading.Timer(_ =>
            {
                _channel.YieldingSendFast(DateTime.Now);
            }, null, _timeout, _timeout);
        }
    }
}
