using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoSharp.Impl
{
    public abstract class SchedulingBase
    {
        protected readonly object _lock;
        protected readonly TimeSpan _timeout;
        protected readonly Channel<DateTime> _channel;
        protected System.Threading.Timer _coreTimer;

        protected SchedulingBase(TimeSpan timeout)
        {
            if (timeout.TotalMilliseconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(timeout));

            _lock = new object();
            _timeout = timeout;
            _channel = Channel<DateTime>.CreateBuffered(1);
        }

        public void Start()
        {
            lock (_lock)
            {
                if (_channel.IsClosed)
                    throw new ChannelClosedException();

                if (_coreTimer != null)
                    throw new InvalidOperationException("Already started");

                StartCore();
            }
        }

        public void Stop()
        {
            lock (_lock)
            {
                if (_channel.IsClosed)
                    throw new ChannelClosedException();

                lock (_lock)
                {
                    DropTimer();
                    _coreTimer = null;
                }
                _channel.Reset();
            }
        }

        public void Close()
        {
            lock (_lock)
            {
                DropTimer();
                _channel.Close();
            }
        }

        protected abstract void StartCore();

        public DateTime Recv()
        {
            return _channel.Recv();
        }

        public async Task<DateTime> RecvAsync()
        {
            return await _channel.RecvAsync();
        }

        public IEnumerable<DateTime> Range => _channel.Range;

        protected void DropTimer()
        {
            if (_coreTimer != null)
            {
                _coreTimer.Change(-1, -1);
                _coreTimer.Dispose();
            }
        }
    }
}
