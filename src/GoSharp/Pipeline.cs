using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoSharp
{
    public sealed class Pipeline
    {
        private List<INodeAction> _nodes;

        public Pipeline()
        {
            _nodes = new List<INodeAction>();
        }

        public void Chain<TIn, TOut>(Func<TIn, Task<TOut>> func, int weight = 1, int bufferSize = 0)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));
            if (weight < 1)
                throw new ArgumentOutOfRangeException(nameof(weight));
            if (bufferSize < 0)
                throw new ArgumentOutOfRangeException(nameof(bufferSize));

            Channel<TIn> chIn = _nodes.Any()
                ? (Channel<TIn>)_nodes.Last().ChannelOut
                : Channel<TIn>.CreateNonBuffered();

            var chOut = Channel<TOut>.CreateBuffered(bufferSize);

            _nodes.Add(new NodeAction<TIn, TOut>(func, chIn, chOut, weight));
        }

        public void ChainMulti<TIn, TOut>(Func<TIn, Task<IEnumerable<TOut>>> func, int weight = 1, int bufferSize = 0)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));
            if (weight < 1)
                throw new ArgumentOutOfRangeException(nameof(weight));
            if (bufferSize < 0)
                throw new ArgumentOutOfRangeException(nameof(bufferSize));

            Channel<TIn> chIn = _nodes.Any()
                ? (Channel<TIn>)_nodes.Last().ChannelOut
                : Channel<TIn>.CreateNonBuffered();

            var chOut = Channel<TOut>.CreateBuffered(bufferSize);

            _nodes.Add(new NodeAction<TIn, TOut>(func, chIn, chOut, weight));
        }

        public void Chain<TIn, TOut>(Func<TIn, TOut> func, int weight = 1, int bufferSize = 0)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));
            if (weight < 1)
                throw new ArgumentOutOfRangeException(nameof(weight));
            if (bufferSize < 0)
                throw new ArgumentOutOfRangeException(nameof(bufferSize));

            Channel<TIn> chIn = _nodes.Any()
                ? (Channel<TIn>)_nodes.Last().ChannelOut
                : Channel<TIn>.CreateNonBuffered();

            var chOut = Channel<TOut>.CreateBuffered(bufferSize);

            _nodes.Add(new NodeAction<TIn, TOut>(func, chIn, chOut, weight));
        }

        public void ChainMulti<TIn, TOut>(Func<TIn, IEnumerable<TOut>> func, int weight = 1, int bufferSize = 0)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));
            if (weight < 1)
                throw new ArgumentOutOfRangeException(nameof(weight));
            if (bufferSize < 0)
                throw new ArgumentOutOfRangeException(nameof(bufferSize));

            Channel<TIn> chIn = _nodes.Any()
                ? (Channel<TIn>)_nodes.Last().ChannelOut
                : Channel<TIn>.CreateNonBuffered();

            var chOut = Channel<TOut>.CreateBuffered(bufferSize);

            _nodes.Add(new NodeAction<TIn, TOut>(func, chIn, chOut, weight));
        }
        public void Start()
        {
            foreach(var node in _nodes)
            {
                node.Start();
            }
        }

        public void Stop()
        {
            foreach (var node in _nodes)
            {
                node.Stop();
            }
        }

        public Channel<TIn> GetInChannel<TIn>()
        {
            return (Channel<TIn>)_nodes.First().ChannelIn;
        }

        public Channel<TOut> GetOutChannel<TOut>()
        {
            return (Channel<TOut>)_nodes.Last().ChannelOut;
        }

        interface INodeAction
        {
            object ChannelIn { get; }
            object ChannelOut { get; }

            void Start();

            void Stop();
        }

        class NodeAction<TIn, TOut> : INodeAction
        {
            private readonly Func<TIn, Task<TOut>> _funcAsync;
            private readonly Func<TIn, Task<IEnumerable<TOut>>> _funcMultiAsync;
            private readonly Func<TIn, TOut> _func;
            private readonly Func<TIn, IEnumerable<TOut>> _funcMulti;
            private readonly Channel<TIn> _chIn;
            private readonly Channel<TOut> _chOut;
            private readonly int _weight;
            private readonly Task[] _tasks;
            private volatile bool _running;

            internal NodeAction(Func<TIn, Task<TOut>> func, Channel<TIn> chIn, Channel<TOut> chOut, int weight) : this(chIn, chOut, weight)
            {
                _funcAsync = func;
            }

            internal NodeAction(Func<TIn, Task<IEnumerable<TOut>>> funcMulti, Channel<TIn> chIn, Channel<TOut> chOut, int weight) : this(chIn, chOut, weight)
            {
                _funcMultiAsync = funcMulti;
            }

            internal NodeAction(Func<TIn, TOut> func, Channel<TIn> chIn, Channel<TOut> chOut, int weight) : this(chIn, chOut, weight)
            {
                _func = func;
            }

            internal NodeAction(Func<TIn, IEnumerable<TOut>> funcMulti, Channel<TIn> chIn, Channel<TOut> chOut, int weight) : this(chIn, chOut, weight)
            {
                _funcMulti = funcMulti;
            }

            private NodeAction(Channel<TIn> chIn, Channel<TOut> chOut, int weight)
            {
                _chIn = chIn;
                _chOut = chOut;
                _weight = weight;
                _tasks = new Task[weight];
            }

            void INodeAction.Start()
            {
                _running = true;
                for (int i = 0;i < _weight;i++)
                {
                    _tasks[i] = Task.Run(Body);
                }
            }

            void INodeAction.Stop()
            {
                _running = false;
                _chIn.Close();
                Task.WaitAll(_tasks);
            }

            object INodeAction.ChannelIn => _chIn;

            object INodeAction.ChannelOut => _chOut;

            private async Task Body()
            {
                while(_running)
                {
                    TIn inp;

                    try
                    {
                        inp = await _chIn.RecvAsync();
                    }
                    catch(ChannelClosedException)
                    {
                        break;
                    }

                    if (_funcAsync != null)
                    {
                        var outp = await _funcAsync(inp);
                        await _chOut.SendAsync(outp);
                    }
                    else if (_func != null)
                    {
                        var outp = _func(inp);
                        await _chOut.SendAsync(outp);
                    }
                    else if (_funcMultiAsync != null)
                    {
                        var outpMulti = await _funcMultiAsync(inp);

                        foreach (var outp in outpMulti)
                        {
                            await _chOut.SendAsync(outp);
                        }
                    }
                    else if (_funcMulti != null)
                    {
                        var outpMulti = _funcMulti(inp);

                        foreach (var outp in outpMulti)
                        {
                            await _chOut.SendAsync(outp);
                        }
                    }
                    else
                        throw new InvalidOperationException();
                }
            }
        }
    }
}
