package main

import "fmt"
import "time"

func main() {
	ex1SendRecv()
	ex1SendRecvBuf()
	chanSync()
	simpleSelect()
	noActSelect()
	defSelect()
	closeChannel()
	rangeChannel()
}

func ex1SendRecv() {
	messages := make(chan string)

	go func() { 
		messages <- "ping"
	}()

	msg := <-messages
	fmt.Println(msg)
}

func ex1SendRecvBuf() {
	messages := make(chan string, 2)

	messages <- "ping"
	messages <- "ping2"

	fmt.Println(<-messages)
	fmt.Println(<-messages)
}

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

