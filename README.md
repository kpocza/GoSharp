# Go\# project
Go-like DSL for C#

This library provides a mini embedded DSL for C# that mimics Go concurrency statements like channels and select.

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

looks like this in C# powered by Go#:


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

