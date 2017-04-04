# Go\# examples

Examples shown below are put here from [go/examples.go](../go/examples.go) and [src/GoSharp.PerfTest/Examples.cs](../src/GoSharp.PerfTest/Examples.cs) in Go and C#, respectively. Most of the samples originate from [http://gobyexample.com](http://gobyexample.com).

The following Go code fragment creates a non-buffered channel, start a goroutine that send "ping" to the channel while in the same time the main function receives this message and prints it. The receiver is waiting for the message for unlimited amount of time while the sender is doing the same to be able to send the message.

```go
func sendRecv() {
	messages := make(chan string)

	go func() { 
		messages <- "ping"
	}()
	
	msg := <-messages
	fmt.Println(msg)
}
```

Semantically equivalent implementation in C# powered by Go# uses asynchronous languages elements (async/await) to achieve the same effect while goroutines are simulated by tasks behind the scenes:

```c#
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
```

Blocking version of the same as above (still behind the Go.Run is a task):

```c#
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
```



Go supports buffered channels where a given number of items can be sent to the channel without blocking the sender afterwards these messages are received:


```go
func sendRecvBuf() {
	messages := make(chan string, 2)

	messages <- "ping"
	messages <- "ping2"
	
	fmt.Println(<-messages)
	fmt.Println(<-messages)
}
```

Buffered channel in Go#:

```c#
private async Task sendRecvBuf()
{
    var messages = Channel<string>.CreateBuffered(2);

    await messages.SendAsync("ping");
    await messages.SendAsync("ping2");

    Console.WriteLine(await messages.RecvAsync());
    Console.WriteLine(await messages.RecvAsync());
}
```



It's easy to wait for the completion of a goroutine with the following trick:


```go
func chanSync() {
	done := make(chan bool)

	go func() {
		fmt.Println("begin")
		time.Sleep(time.Second)
		fmt.Println("done")
		done <- true
	}()
	
	<- done
	fmt.Println("end")
}
```

Similar version using Go#:

```c#
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
```



The select statement is borrowed from the socket programming. The case that is able to send or receive data first will be processed. Select in this form is waiting for unlimited amount of time. In this example two goroutines send messages to two channels with 1 and 2 seconds delay while the select is waiting twice to receive both messages:


```go
func simpleSelect() {
	chan1 := make(chan int)
	chan2 := make(chan string)
	
	go func() {
		time.Sleep(time.Second)
		chan1 <- 1
	}()
	
	go func() {
		time.Sleep(time.Second * 2)
		chan2 <- "two"
	}()
	
	for i := 0; i < 2; i++ {
		select {
			case msg1:= <- chan1:
				fmt.Println("recvd:", msg1)
			case msg2:= <- chan2:
				fmt.Println("recvd:", msg2)
		}
	}
}
```

Similar implementation powered by Go#:


```c#
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
```

This is the synchronous version of the same:


```c#
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
```



It's possible to skip waiting if there is no message available to be received or none of the channels in the cases can send data. In this case the default case will be executed immediately:


```go
func noActSelect() {
	chan1 := make(chan string)
	chan2 := make(chan string)
	
	select {
		case msg:= <- chan1:
			fmt.Println("rcvd:", msg);
		default:
			fmt.Println("nothing rcvd")
	}
	
	select {
		case chan1 <- "hi":
			fmt.Println("msg sent");
		default:
			fmt.Println("nothing sent")
	}
	
	select {
		case msg1:= <- chan1:
			fmt.Println("rcvd1:", msg1)
		case msg2:= <- chan2:
			fmt.Println("rcvd2:", msg2)
		default:
			fmt.Println("no activity")
	}
}
```

The Go# implementation:


```c#
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
```



In this case for example the Select can receive immediately so that the first case will be executed and not the default:


```go
func defSelect() {
	ch := make(chan string, 1)

	ch <- "hi"

	select {
		case msg:= <- ch:
			fmt.Println("rcvd:", msg);
		default:
			fmt.Println("nothing rcvd")
	}
}
```

Similarly in Go#:


```c#
private async Task defSelect()
{
    var ch = Channel<string>.CreateBuffered(1);

    await ch.SendAsync("hi");

    await Select
        .CaseRecv(ch, msg => Console.WriteLine($"rcvd: {msg}"))
        .DefaultAsync(() => Console.WriteLine("nothing rcvd"));
}
```



Channels can be closed that will cause the goroutine listening on the closed channel to be stopped:


```go
func closeChannel() {
	messages := make(chan int)

	go func() {
		for {
			fmt.Println("waiting...")
			msg:= <- messages
			fmt.Println(msg)
		}
	}()
	
	for i:= 0;i <= 10; i++ {
		messages <- i
	}
	
	close(messages)
	fmt.Println("closed");
}
```

In Go# we have to catch the ChannelClosedException to avoid program termination:


```c#
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
```



Via range we can iterate incoming messages of a channel while close will stop the range operation:


```go
func rangeChannel() {
	messages := make(chan int)

	go func() {
		for msg:= range messages {
			fmt.Println(msg)
		}
	}()
	
	for i:= 0;i <= 10; i++ {
		messages <- i
	}
	
	close(messages)
}
```

In Go# the Range operator won't throw ChannelClosedException but will stop the enumeration gracefully:


```c#
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
```



Every timer is elapsing only once after the specified timeout:


```go
func timerEx() {
	timer:= time.NewTimer(time.Second)
	fmt.Println("waiting for the timer to expired")
	<- timer.C
	fmt.Println("Timer expired")
}
```

This is how it looks like in Go# (the only real difference is that the timer has to be started):


```c#
private async Task timer()
{
    var timer = new Timer(TimeSpan.FromSeconds(1));
    timer.Start();
    Console.WriteLine("Waiting for timer to expire");
    await timer.RecvAsync();
    Console.WriteLine("Timer expired");
}
```



Tickers fire multiple times periodically after the given timeout:


```go
func tickerEx() {
	ticker:= time.NewTicker(time.Millisecond * 100)
	go func() {
		for t:= range ticker.C {
			fmt.Println("Tick at", t)
		}
	}()

	time.Sleep(time.Second)
	ticker.Stop()
}
```

Tickers have to be also started but the code structure generally looks similar to the Go version:


```c#
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
```



Select statement also supports timeout. If none of the channels/cases fire in the given timeout then the timeout case will do so:


```go
func timeoutSelect() {
	chan1 := make(chan int)
	chan2 := make(chan string)

	go func() {
		time.Sleep(time.Second)
		chan1 <- 1
	}()

	go func() {
		time.Sleep(time.Second * 2)
		chan2 <- "two"
	}()

	for i := 0; i < 2; i++ {
		select {
			case msg1:= <- chan1:
				fmt.Println("recvd:", msg1)
			case msg2:= <- chan2:
				fmt.Println("recvd:", msg2)
			case <- time.After(time.Millisecond * 100):
				fmt.Println("timeout")
		}
	}
}
```

The Go# implementation is almost the same:


```c#
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
```

