using System;
using System.Threading.Tasks;
using GoSharp.Impl;

namespace GoSharp
{
    public class SelectSet
    {
        private readonly SelectLogic _selectLogic;

        internal SelectSet()
        {
            _selectLogic = new SelectLogic();
        }

        public SelectSet CaseRecv<T>(Channel<T> channel, Action<T> action)
        {
            if(channel.IsClosed)
                throw new ChannelClosedException();

            _selectLogic.AddRecv(channel, action);
            return this;
        }

        public SelectSet CaseSend<T>(Channel<T> channel, T msg)
        {
            if (channel.IsClosed)
                throw new ChannelClosedException();

            _selectLogic.AddSend(channel, msg);
            return this;
        }

        public void Default(Action defaultAction)
        {
            _selectLogic.AddDefault(defaultAction);
            Go();
        }

        public async Task DefaultAsync(Action defaultAction)
        {
            _selectLogic.AddDefault(defaultAction);
            await GoAsync();
        }

        public void Go()
        {
            _selectLogic.Go();
        }

        public async Task GoAsync()
        {
            await _selectLogic.GoAsync();
        }
    }
}
