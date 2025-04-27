/***
	
	This program is the solution to the Producer-Consumer Problem introduced in Chapter 3.

	The parent process spawns two child processes: a producer and a consumer.
	The producer produces alphabet characters and inserts them into the buffer.
	The consumer deletes a character from the buffer and displays it on the screen.

	However, this program does not work correctly.
	Correct the problems to behave as shown in the following example.
		- Indicate each correction with a line comments similar to Problem 1.
		- Check error for all system calls that can fail: on failure, display an error message.

	DO NOT MAKE UNNECESSARY MODIFICATION TO OTHER PARTS.

	Example) ./a.out
		Producer started...
		[Producer] new item = A
		[Producer] new item = B
		[Producer] new item = C
		Consumer started...
		[Consumer] retrieved item = A
		[Producer] new item = D
		[Consumer] retrieved item = B
		[Producer] new item = E
		[Producer] new item = F
		[Consumer] retrieved item = C
		[Producer] new item = G
		[Consumer] retrieved item = D
		[Consumer] retrieved item = E
		[Producer] new item = H
		[Producer] new item = I
		[Consumer] retrieved item = F
		[Producer] new item = J
		[Consumer] retrieved item = G
		[Consumer] retrieved item = H
		Producer finishing...
		[Consumer] retrieved item = I
		[Consumer] retrieved item = J
		[Parent] terminating threads...		// This message can be printed before or after the "Producer/Consomer finishing..." messages
		Consumer finishing...
		Bye!

***/


#include <stdio.h>
#include <stdlib.h>

#include <unistd.h>
#include <pthread.h>
#include <wait.h>
#include <errno.h>

#include <fcntl.h>
#include <sys/shm.h>
#include <sys/mman.h>

#define TRUE 1
#define FALSE 0

#define UPPER 10

#define BUFFER_SIZE 6
typedef struct {
	int in, out;
	char data[BUFFER_SIZE];
} Buffer;

void* Producer(void *vparam);
void* Consumer(void *vparam);


int main()
{
	// DO NOT MODIFY THE FOLLOWING TWO LINES.
	int no_thread = 2;
	pthread_t tid[2] = { 0 };
	
	int fd = shm_open("msg", O_CREAT | O_RDWR, 0666);
	if(fd == -1){
		perror("Error: ");
		return -1;
	}

	ftruncate(fd, sizeof(Buffer));
	Buffer *buffer = (Buffer*)mmap(NULL, sizeof(Buffer), PROT_READ | PROT_WRITE, MAP_SHARED, fd, 0);


	if(buffer == NULL){
		printf("Failed to allocate memory in Line %d\n", __LINE__);
		exit(-1);
	}
	buffer->in = buffer->out = 0;

	pid_t prod = fork();
	if(prod < 0){
		fprintf(stderr, "Failed to create Producer in Line %d\n", __LINE__);
		exit(-1);
	} else if(prod == 0){
		Producer(buffer);
		exit(0);
	}


	sleep(3);			 // wait for 3 seconds

	pid_t cons = fork();
	if(cons < 0){
		fprintf(stderr, "Failed to create Consumer in Line %d\n", __LINE__);
		kill(prod, SIGINT);
		exit(-1);
	} else if(cons == 0){
		Consumer(buffer);
		exit(0);
	}

	sleep(10);

	printf("[Parent] terminating threads...\n");
	fflush(stdout);

	wait(NULL);
	wait(NULL);


	munmap(buffer, sizeof(Buffer));
	shm_unlink("msg");

	printf("Bye!\n");

	return 0;
}

void* Producer(void *vparam)
// DO NOT MODIFY THIS FUNCTION
{
	printf("Producer started...\n");
	fflush(stdout);

	Buffer *buf = (Buffer *)vparam;
	int index = 0;
	for(int i = 0; i < UPPER; i++){
		char new_item = index + 'A';
		index = (index + 1) % 26;

		printf("[Producer] new item = %c\n", new_item);
		fflush(stdout);
		sleep(1);				// delay

		while((buf->in + 1) % BUFFER_SIZE == buf->out)
			usleep(100000);
		
		buf->data[buf->in] = new_item;
		buf->in = (buf->in + 1) % BUFFER_SIZE;
	}

	printf("Producer finishing...\n");
	fflush(stdout);

	return NULL;
}

void* Consumer(void *vparam)
// DO NOT MODIFY THIS FUNCTION
{
	printf("Consumer started...\n");
	fflush(stdout);

	Buffer *buf = (Buffer *)vparam;

	for(int i = 0; i < UPPER || buf->in != buf->out; i++){
		while(buf->in == buf->out)
			usleep(100000);

		char new_item = buf->data[buf->out];
		buf->out = (buf->out + 1) % BUFFER_SIZE;

		printf("[Consumer] item = %c\n", new_item);
		fflush(stdout);
		sleep(1);				// delay
	}

	printf("Consumer finishing...\n");
	fflush(stdout);

	return NULL;
}

