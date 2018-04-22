# High-level Go\# examples

Examples shown below are from the unit test cases located in GoTest.cs and PiplelineTest.cs. Merge, Zip, Buffer, Broadcast and Pipeline constructs are shown below.

## Merge operation

The Merge operation is reading data from variable number of input channels in round-robin way and forwards the messages to a single output channel.

Example:
​	input1: 1, 2 ,3
​	input2: a, b, c
​	output: 1, a, 2, b, 3, c

Code sample:

```c#
[TestMethod]
public void Merge2Test()
{
    var input1 = Channel<int>.CreateNonBuffered();
    var input2 = Channel<int>.CreateNonBuffered();
    var output = Channel<int>.CreateNonBuffered();
    var res = new List<int>();
    int cnt = 100;

    Go.Run(async () =>
    {
        for (int i = 0; i < cnt; i++)
        {
            await input1.SendAsync(i);
        }
    });

    Go.Run(async () =>
    {
        for (int i = 0; i < cnt; i++)
        {
            await input2.SendAsync(-i);
        }
    });

    Go.Merge(input1, input2, output);

    while (res.Count < cnt * 2)
    {
        res.Add(output.Recv());
    }

    input1.Close();
    input2.Close();
    output.Close();

    Assert.AreEqual(0, res.Sum());
}
```



## Zip operation

This operation is reading data from 2 to 4 channels of variable types while outputting a tuple of 2 to 4 items having items with types of the input channels. One can imagine this as the zip of a trouser.

Example:
​	input1: 1, 2 ,3
​	input2: a, b, c
​	output: (1, a), (2, b), (3, c)

Code sample:

```c#
[TestMethod]
public void Zip2Test()
{
    var input1 = Channel<int>.CreateNonBuffered();
    var input2 = Channel<int>.CreateNonBuffered();
    var output = Channel<Tuple<int, int>>.CreateNonBuffered();
    int cnt = 100;

    Go.Run(async () =>
    {
        for (int i = 0; i < cnt; i++)
        {
            await input1.SendAsync(i);
        }
    });

    Go.Run(async () =>
    {
        for (int i = 0; i < cnt; i++)
        {
            await input2.SendAsync(-i);
        }
    });

    Go.Zip(input1, input2, output);

    for (int i = 0; i < cnt; i++)
    {
        var item = output.Recv();
        Assert.AreEqual(-item.Item1, item.Item2);
    }

    input1.Close();
    input2.Close();
    output.Close();
}
```


## Buffer operation

Buffer operation is reading items from an input channel of T and creating a collection of Ts that has at most the specified number of elements (buffer size) or after a given timeout has elapsed. These two conditions can be applied individually or combined.

Example:
​	input: 1,2,3,4,5,6,7,8,9,10
​	buffer size: 5
​	output (1,2,3,4,5),(6,7,8,9,10)

The following code demonstrates a buffer with 5 elements:

```c#
[TestMethod]
public void Buffer1Test()
{
    var input = Channel<int>.CreateNonBuffered();
    var output = Channel<ICollection<int>>.CreateNonBuffered();

    int cnt = 100;
    int buffer = 5;

    Go.Run(async () =>
    {
        for (int i = 0; i < cnt; i++)
        {
            await input.SendAsync(i);
        }
    });

    Go.Buffer(input, output, buffer);

    var res = new List<int>();
    for (int i = 0; i < cnt / buffer; i++)
    {
        var part = output.Recv();
        res.AddRange(part);
    }

    input.Close();
    output.Close();

    Assert.IsTrue(Enumerable.SequenceEqual(Enumerable.Range(0, 100), res));
}
```

The following code demonstrates a buffer with 5 elements or less if less than 5 elements arrive in 30 milliseconds:

```c#
[TestMethod]
public void Buffer2Test()
{
    var input = Channel<int>.CreateNonBuffered();
    var output = Channel<ICollection<int>>.CreateNonBuffered();

    int cnt = 100;
    int buffer = 5;

    Go.Run(async () =>
    {
        for (int i = 0; i < cnt; i++)
        {
            await Task.Delay(10);
            await input.SendAsync(i);
        }
    });

    Go.Buffer(input, output, buffer, TimeSpan.FromMilliseconds(30));

    int parts = 0;
    var res = new List<int>();
    while(res.Count < cnt)
    {
        var part = output.Recv();
        res.AddRange(part);
        parts++;
    }

    input.Close();
    output.Close();

    Assert.IsTrue(parts > cnt / buffer);
    Assert.IsTrue(Enumerable.SequenceEqual(Enumerable.Range(0, 100), res));
}
```



## Broadcast operation

Broadcast operation is multiplexing data from a single input channel to multiple output channels while the same data is sent to every channel.

Example:
​	input: 1, 2, 3
​	output1: 1, 2, 3
​	output2: 1, 2, 3

Code sample:

```c#
[TestMethod]
public void Broadcast4Test()
{
    var input = Channel<int>.CreateNonBuffered();
    var output1 = Channel<int>.CreateNonBuffered();
    var output2 = Channel<int>.CreateNonBuffered();
    var output3 = Channel<int>.CreateNonBuffered();
    var output4 = Channel<int>.CreateNonBuffered();
    var done = Channel<bool>.CreateNonBuffered();

    int cnt = 100;

    Go.Run(async () =>
    {
        for (int i = 0; i < cnt; i++)
        {
            await input.SendAsync(i);
        }
    });

    Go.Broadcast(input, new[] { output1, output2, output3, output4 });

    var res1 = new List<int>();
    var res2 = new List<int>();
    var res3 = new List<int>();
    var res4 = new List<int>();

    Go.Run(async () =>
    {
        for (int i = 0; i < cnt; i++)
        {
            res1.Add(await output1.RecvAsync());
        }
        await done.SendAsync(true);
    });

    Go.Run(async () =>
    {
        for (int i = 0; i < cnt; i++)
        {
            res2.Add(await output2.RecvAsync());
        }
        await done.SendAsync(true);
    });

    Go.Run(async () =>
    {
        for (int i = 0; i < cnt; i++)
        {
            res3.Add(await output3.RecvAsync());
        }
        await done.SendAsync(true);
    });

    Go.Run(async () =>
    {
        for (int i = 0; i < cnt; i++)
        {
            res4.Add(await output4.RecvAsync());
        }
        await done.SendAsync(true);
    });

    done.Recv();
    done.Recv();
    done.Recv();
    done.Recv();

    input.Close();
    output1.Close();
    output2.Close();
    output3.Close();
    output4.Close();

    Assert.IsTrue(Enumerable.SequenceEqual(Enumerable.Range(0, 100), res1));
    Assert.IsTrue(Enumerable.SequenceEqual(Enumerable.Range(0, 100), res2));
    Assert.IsTrue(Enumerable.SequenceEqual(Enumerable.Range(0, 100), res3));
    Assert.IsTrue(Enumerable.SequenceEqual(Enumerable.Range(0, 100), res4));
}
```



## Pipeline operation

Pipeline is implementing a chain of operations that are passing data between each other via channels. Each operation can return a single element or a list of elements. This feature is work in progress.

```c#
[TestMethod]
public void SimpleProc()
{
    var list = new List<int>();
    var pipeline = new Pipeline();

    pipeline.Chain(async (int a) => await Task.Run(() => a.ToString()));
    pipeline.Chain((string a) => int.Parse(a));

    var chIn = pipeline.GetInChannel<int>();
    var chOut = pipeline.GetOutChannel<int>();

    pipeline.Start();

    Task.Run(() => { for (int i = 0; i < 3; i++) list.Add(chOut.Recv()); });

    chIn.Send(1);
    chIn.Send(2);
    chIn.Send(3);

    Thread.Sleep(100);

    pipeline.Stop();

    Assert.IsTrue(new[] { 1, 2, 3 }.SequenceEqual(list));
}

[TestMethod]
public void MultiProc()
{
    var list = new List<int>();
    var pipeline = new Pipeline();

    pipeline.ChainMulti<int, int>(async (int a) => await Task.Run(() => new[] { a, a * a }));
    pipeline.ChainMulti((int a) => new[] { a, a * a });

    var chIn = pipeline.GetInChannel<int>();
    var chOut = pipeline.GetOutChannel<int>();

    pipeline.Start();

    Task.Run(() => { for (int i = 0; i < 12; i++) list.Add(chOut.Recv()); });

    chIn.Send(1);
    chIn.Send(2);
    chIn.Send(3);

    Thread.Sleep(100);

    pipeline.Stop();

    Assert.IsTrue(new[] { 1, 1, 1, 1, 2, 4, 4, 16, 3, 9, 9, 81}.SequenceEqual(list));
}
```

