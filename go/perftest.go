package main

import "fmt"
import "os"
import "time"
import "sync"
import "math/rand"

func main() {
	fmt.Println("Non-buffered:");
	sendRecvRun(1, 200000, 1, 200000, 0);
	sendRecvRun(2, 100000, 1, 200000, 0);
	sendRecvRun(1, 200000, 2, 100000, 0);
	sendRecvRun(10, 50000, 10, 50000, 0);
	sendRecvRun(50, 10000, 50, 10000, 0);
	sendRecvRun(100, 10000, 100, 10000, 0);
	sendRecvRun(10, 100000, 100, 10000, 0);
	sendRecvRun(100, 10000, 10, 100000, 0);
	sendRecvRun(500, 1000, 500, 1000, 0);
	sendRecvRun(1000, 1000, 1000, 1000, 0);
	sendRecvRun(10, 100000, 1000, 1000, 0);
	sendRecvRun(1000, 1000, 10, 100000, 0);
	sendRecvRun(2000, 1000, 2000, 1000, 0);
	fmt.Println();

	fmt.Println("Buffered:");
	sendRecvRun(1, 200000, 1, 200000, 1000);
	sendRecvRun(2, 100000, 1, 200000, 1000);
	sendRecvRun(1, 200000, 2, 100000, 1000);
	sendRecvRun(10, 50000, 10, 50000, 1000);
	sendRecvRun(50, 10000, 50, 10000, 1000);
	sendRecvRun(100, 10000, 100, 10000, 1000);
	sendRecvRun(10, 100000, 100, 10000, 1000);
	sendRecvRun(100, 10000, 10, 100000, 1000);
	sendRecvRun(500, 1000, 500, 1000, 500);
	sendRecvRun(1000, 1000, 1000, 1000, 500);
	sendRecvRun(10, 100000, 1000, 1000, 500);
	sendRecvRun(1000, 1000, 10, 100000, 500);
	sendRecvRun(2000, 1000, 2000, 1000, 500);
	fmt.Println();
	
	fmt.Println("Select non-buffered:");
	selectSendRecvRun(10, 1, 100000, 1, 100000, 0);
	selectSendRecvRun(10, 2, 100000, 1, 200000, 0);
	selectSendRecvRun(10, 1, 200000, 2, 100000, 0);
	selectSendRecvRun(10, 10, 10000, 10, 10000, 0);
	selectSendRecvRun(10, 50, 2000, 50, 2000, 0);
	selectSendRecvRun(10, 100, 1000, 100, 1000, 0);
	selectSendRecvRun(10, 10, 10000, 100, 1000, 0);
	selectSendRecvRun(10, 100, 1000, 10, 10000, 0);
	selectSendRecvRun(10, 500, 250, 500, 250, 0);
	selectSendRecvRun(10, 1000, 100, 1000, 100, 0);
	selectSendRecvRun(10, 10, 10000, 1000, 100, 0);
	selectSendRecvRun(10, 1000, 100, 10, 10000, 0);
	selectSendRecvRun(10, 2000, 100, 2000, 100, 0);
	fmt.Println();
	
	fmt.Println("Select buffered:");
	selectSendRecvRun(10, 1, 100000, 1, 100000, 500);
	selectSendRecvRun(10, 2, 100000, 1, 200000, 500);
	selectSendRecvRun(10, 1, 200000, 2, 100000, 500);
	selectSendRecvRun(10, 10, 10000, 10, 10000, 500);
	selectSendRecvRun(10, 50, 2000, 50, 2000, 500);
	selectSendRecvRun(10, 100, 1000, 100, 1000, 500);
	selectSendRecvRun(10, 10, 10000, 100, 1000, 500);
	selectSendRecvRun(10, 100, 1000, 10, 10000, 500);
	selectSendRecvRun(10, 500, 250, 500, 250, 100);
	selectSendRecvRun(10, 1000, 100, 1000, 100, 50);
	selectSendRecvRun(10, 10, 10000, 1000, 100, 50);
	selectSendRecvRun(10, 1000, 100, 10, 10000, 50);
	selectSendRecvRun(10, 2000, 100, 2000, 100, 50);
	fmt.Println();
}

func sendRecvRun(senderCount, sendItemCount, recvCount, recvItemCount, bufferSize int) {
	channel:= make(chan int, bufferSize);
	var wg sync.WaitGroup;

	if(senderCount * sendItemCount!= recvCount*recvItemCount) {
		fmt.Println("Invalid counts");
		os.Exit(1);
	}

	wg.Add(recvCount + senderCount);

	start:= time.Now();
	for i:= 0;i < recvCount;i++ {
		go func() {
			for j:=0;j < recvItemCount;j++ {
				<- channel;
			}
			wg.Done();
		}()
	}

	for i:= 0;i < senderCount;i++ {
		go func() {
			for j:=0;j < sendItemCount;j++ {
				channel <- i;
			}
			wg.Done();
		}()
	}

	wg.Wait();
	end:= time.Now();

	diff:= end.Sub(start);
	fmt.Println("Sender go:", senderCount, ", items/go:", sendItemCount, 
			", Recv go:", recvCount, ", items/go:", recvItemCount, ":", 
			diff.Nanoseconds()/1000000, "ms");
}

func selectSendRecvRun(chnCnt, senderCount, sendItemCount, recvCount, recvItemCount, bufferSize int) {
	var channels = make([]chan int, chnCnt);
	for i:=0;i < chnCnt;i++ {
		channels[i] = make(chan int, bufferSize);
	}
	var wg sync.WaitGroup;

	if(senderCount * sendItemCount!= recvCount*recvItemCount) {
		fmt.Println("Invalid counts");
		os.Exit(1);
	}

	wg.Add(recvCount + senderCount);

	start:= time.Now();
	for i:= 0;i < recvCount;i++ {
		go func() {
			for j:=0;j < recvItemCount;j++ {
				select {
					case <- channels[0]:
					case <- channels[1]:
					case <- channels[2]:
					case <- channels[3]:
					case <- channels[4]:
					case <- channels[5]:
					case <- channels[6]:
					case <- channels[7]:
					case <- channels[8]:
					case <- channels[9]:
				}
			}
			wg.Done();
		}()
	}

	for i:= 0;i < senderCount;i++ {
		go func() {
			
			for j:=0;j < sendItemCount;j++ {
				idx:= rand.Intn(chnCnt);
				channels[idx] <- i;
			}
			wg.Done();
		}()
	}

	wg.Wait();
	end:= time.Now();

	diff:= end.Sub(start);
	fmt.Println("Sender go:", senderCount, ", items/go:", sendItemCount, 
			", Recv go:", recvCount, ", items/go:", recvItemCount, ":", 
			diff.Nanoseconds()/1000000, "ms");
}

func createActionGo(count int, fnc func()) {
	for i:=0;i < count;i++ {
		fnc();
	}
}
