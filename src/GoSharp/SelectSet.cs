using System;
using System.Threading.Tasks;
using GoSharp.Impl;

namespace GoSharp
{
    public sealed class SelectSet
    {
        private readonly SelectLogic _selectLogic;

        internal SelectSet()
        {
            _selectLogic = new SelectLogic();
        }

        public SelectSet CaseRecv<T>(Channel<T> channel, Action<T> action)
        {
            if (channel == null)
                throw new ArgumentNullException(nameof(channel));
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (channel.IsClosed)
                throw new ChannelClosedException();

            _selectLogic.AddRecv(channel, action);
            return this;
        }

        public SelectSet CaseSend<T>(Channel<T> channel, T msg)
        {
            if (channel == null)
                throw new ArgumentNullException(nameof(channel));

            if (channel.IsClosed)
                throw new ChannelClosedException();

            _selectLogic.AddSend(channel, msg);
            return this;
        }

        public Task GoAsync()
        {
            return _selectLogic.GoAsync();
        }

        public Task DefaultAsync(Action defaultAction)
        {
            if (defaultAction == null)
                throw new ArgumentNullException(nameof(defaultAction));

            _selectLogic.AddDefault(defaultAction);
            return GoAsync();
        }

        public Task TimeoutAsync(TimeSpan timeout, Action timeoutAction)
        {
            if (timeout.TotalMilliseconds < 1)
                throw new ArgumentOutOfRangeException(nameof(timeout));
            if (timeoutAction == null)
                throw new ArgumentNullException(nameof(timeoutAction));

            _selectLogic.AddTimeout(timeout, timeoutAction);
            return GoAsync();
        }

        public void Default(Action defaultAction)
        {
            DefaultAsync(defaultAction).Wait();
        }

        public void Timeout(TimeSpan timeout, Action timeoutAction)
        {
            TimeoutAsync(timeout, timeoutAction).Wait();
        }

        public void Go()
        {
            GoAsync().Wait();
        }
    }
}
