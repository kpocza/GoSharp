#include <stdio.h>
#include <pthread.h>
#include <unistd.h>

typedef struct account_t {
	int balance;
} account_t;

pthread_mutex_t lock;

account_t accounts[2];
int cnt = 20000;

void transfer(account_t *src, account_t *dst, int amount) {
	usleep(500);
	pthread_mutex_lock(&lock);

	if(src->balance >= amount) {
		src->balance-= amount;
		dst->balance+= amount;
	}

	pthread_mutex_unlock(&lock);
}

void* first2second(void *args) {
	printf("first2second started, ");
	for(int i = 0;i < cnt;i++) {
		transfer(&accounts[0], &accounts[1], 1000);
	}
	printf("first2second ended, ");
}

void* second2first(void * args) {
	printf("second2first started, ");
	for(int i = 0;i < cnt;i++) {
		transfer(&accounts[1], &accounts[0], 1000);
	}
	printf("second2first ended, ");
}

int main() {
	int threadcnt = 1000;
	pthread_t tid[threadcnt];

	accounts[0].balance = 1000;
	accounts[1].balance = 1000;

	pthread_mutex_init(&lock, NULL);

	for(int t = 0;t < threadcnt;t++) {
		if(t%2 == 0) 
			pthread_create(&tid[t], NULL, &first2second, NULL);
		else
			pthread_create(&tid[t], NULL, &second2first, NULL);
	}

	for(int t = 0;t < threadcnt;t++)
		pthread_join(tid[t], NULL);
	pthread_mutex_destroy(&lock);

	printf("first balance: %d\n", accounts[0].balance);
	printf("second balance: %d\n", accounts[1].balance);
	return 0;
}
