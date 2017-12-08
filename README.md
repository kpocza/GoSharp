# Go\# project
Go-like DSL for C#

This library provides a mini embedded DSL for C# that mimics Go concurrency statements like channels, select, timers and tickers.

The official NuGet package called GoSharp is hosted by nuget.org and supports .NET Standard 2.0 (.NET Core 2.0, .NET Framework 4.6.1), .NET Standard 1.2 (.NET Core 1.0, .NET Framework 4.5.1) and .NET Framework 4.5.2.

For example the following Go code

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

looks like this way in C# powered by Go#:


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

For more examples and detailed explanation [go here](docs/examples.md).



TODO:

- Select close behaviour fine tuning
- Performance docs

