package main

import "fmt"
import "sync"
import "time"

type account struct {
	balance int
}

var accounts [2]account;
var mutex = &sync.Mutex{}

func transfer(src *account, dest *account, amount int) {
	time.Sleep(500*time.Microsecond);
	mutex.Lock()
	if(src.balance >= amount) {
		src.balance-= amount;
		dest.balance+= amount;
	}
	mutex.Unlock()
}

func main() {
	var gocount int = 1000;
	var cnt int = 20000;

	accounts[0].balance = 1000;
	accounts[1].balance = 1000;

	var wg sync.WaitGroup;

	wg.Add(gocount);

	for t:= 0; t < gocount; t++ {
		if t%2 == 0 {
			go func() {
				fmt.Print("first2second started, ");
				for i:= 0; i < cnt;i++ {
					transfer(&accounts[0], &accounts[1], 1000)
				}
				fmt.Print("firsrt2second ended, ");
				wg.Done();
			}();
		} else {
			go func() {
				fmt.Print("second2first started, ");
				for i:= 0; i < cnt;i++ {
					transfer(&accounts[1], &accounts[0], 1000)
				}
				fmt.Print("second2first ended, ");
				wg.Done();
			}();
		}
	}

	wg.Wait();

	fmt.Println("balance1:", accounts[0].balance);
	fmt.Println("balance2:", accounts[1].balance);
}
