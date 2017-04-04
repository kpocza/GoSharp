using System;
using System.Threading;
using System.Threading.Tasks;

namespace GoSharp.PerfTest
{
    public class Examples
    {
        public void Run()
        {
            sendRecv().Wait();
            sendRecvSync();
            sendRecvBuf().Wait(); 
            chanSync().Wait();
            simpleSelect().Wait();
            simpleSelectSync();
            noActSelect().Wait();
            defSelect().Wait();
            closeChannel().Wait();
            rangeChannel().Wait();
            timer().Wait();
            ticker().Wait();
            timeoutSimpleSelect().Wait();
        }

        private async Task sendRecv()
        {
            var messages = Channel<string>.CreateNonBuffered();

            Go.Run(async () =>
            {
                await messages.SendAsync("ping");
            });

            var msg = await messages.RecvAsync();
            Console.WriteLine(msg);
        }

        private void sendRecvSync()
        {
            var messages = Channel<string>.CreateNonBuffered();

            Go.Run(() =>
            {
                messages.Send("ping");
            });

            var msg = messages.Recv();
            Console.WriteLine(msg);
        }

        private async Task sendRecvBuf()
        {
            var messages = Channel<string>.CreateBuffered(2);

            await messages.SendAsync("ping");
            await messages.SendAsync("ping2");

            Console.WriteLine(await messages.RecvAsync());
            Console.WriteLine(await messages.RecvAsync());
        }

        private async Task chanSync()
        {
            var done = Channel<bool>.CreateNonBuffered();

            Go.Run(async () =>
            {
                Console.WriteLine("begin");

                await Task.Delay(TimeSpan.FromSeconds(1));

                Console.WriteLine("done");

                await done.SendAsync(true);
            });

            await done.RecvAsync();
            Console.WriteLine("end");
        }

        private async Task simpleSelect()
        {
            var chan1 = Channel<int>.CreateNonBuffered();
            var chan2 = Channel<string>.CreateNonBuffered();

            Go.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                await chan1.SendAsync(1);
            });

            Go.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
                await chan2.SendAsync("two");
            });

            for(int i = 0;i < 2;i++)
            {
                await Select
                    .CaseRecv(chan1, msg1 => Console.WriteLine($"recvd: {msg1}"))
                    .CaseRecv(chan2, msg2 => Console.WriteLine($"recvd: {msg2}"))
                    .GoAsync();
            }
        }

        private void simpleSelectSync()
        {
            var chan1 = Channel<int>.CreateNonBuffered();
            var chan2 = Channel<string>.CreateNonBuffered();

            Go.Run(() =>
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
                chan1.Send(1);
            });

            Go.Run(() =>
            {
                Thread.Sleep(TimeSpan.FromSeconds(2));
                chan2.Send("two");
            });

            for (int i = 0; i < 2; i++)
            {
                Select
                    .CaseRecv(chan1, msg1 => Console.WriteLine($"recvd: {msg1}"))
                    .CaseRecv(chan2, msg2 => Console.WriteLine($"recvd: {msg2}"))
                    .Go();
            }
        }

        private async Task noActSelect()
        {
            var chan1 = Channel<string>.CreateNonBuffered();
            var chan2 = Channel<string>.CreateNonBuffered();

            await Select
                .CaseRecv(chan1, msg => Console.WriteLine($"rcvd: {msg}"))
                .DefaultAsync(() => Console.WriteLine("nothing rcvd"));

            await Select
                .CaseSend(chan1, "hi")
                .DefaultAsync(() => Console.WriteLine("nothing sent"));

            await Select
                .CaseRecv(chan1, msg1 => Console.WriteLine($"rcvd: {msg1}"))
                .CaseRecv(chan2, msg2 => Console.WriteLine($"rcvd: {msg2}"))
                .DefaultAsync(() => Console.WriteLine("no activity"));
        }

        private async Task defSelect()
        {
            var ch = Channel<string>.CreateBuffered(1);

            await ch.SendAsync("hi");

            await Select
                .CaseRecv(ch, msg => Console.WriteLine($"rcvd: {msg}"))
                .DefaultAsync(() => Console.WriteLine("nothing rcvd"));
        }

        private async Task closeChannel()
        {
            var messages = Channel<int>.CreateNonBuffered();

            Go.Run(async () =>
            {
                try
                {
                    while (true)
                    {
                        Console.WriteLine("waiting...");

                        var msg = await messages.RecvAsync();
                        Console.WriteLine(msg);
                    }
                }
                catch (ChannelClosedException)
                {
                    Console.WriteLine("ChannelClosedException caught");
                }
            });

            for (int i = 0; i < 10; i++)
            {
                await messages.SendAsync(i);
            }
            messages.Close();

            Console.WriteLine("closed");
        }

        private async Task rangeChannel()
        {
            var messages = Channel<int>.CreateNonBuffered();

            Go.Run(() =>
            {
                foreach (var msg in messages.Range)
                {
                    Console.WriteLine(msg);
                }
            });

            for (int i = 0; i < 10; i++)
            {
                await messages.SendAsync(i);
            }
            messages.Close();
        }

        private async Task timer()
        {
            var timer = new Timer(TimeSpan.FromSeconds(1));
            timer.Start();
            Console.WriteLine("Waiting for timer to expire");
            await timer.RecvAsync();
            Console.WriteLine("Timer expired");
        }

        private async Task ticker()
        {
            var ticker = new Ticker(TimeSpan.FromMilliseconds(100));
            Go.Run(() =>
            {
                foreach (var t in ticker.Range)
                {
                    Console.WriteLine($"Tick at: {t}");
                }
            });
            ticker.Start();

            await Task.Delay(TimeSpan.FromSeconds(1));
            ticker.Stop();
        }

        private async Task timeoutSimpleSelect()
        {
            var chan1 = Channel<int>.CreateNonBuffered();
            var chan2 = Channel<string>.CreateNonBuffered();

            Go.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                await chan1.SendAsync(1);
            });

            Go.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
                await chan2.SendAsync("two");
            });

            for (int i = 0; i < 2; i++)
            {
                await Select
                    .CaseRecv(chan1, msg1 => Console.WriteLine($"recvd: {msg1}"))
                    .CaseRecv(chan2, msg2 => Console.WriteLine($"recvd: {msg2}"))
                    .TimeoutAsync(TimeSpan.FromSeconds(0.4), () => Console.WriteLine("timeout"));
            }
        }
    }
}
