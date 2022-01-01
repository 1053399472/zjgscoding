#include <iostream>
#include <thread>
#include <condition_variable>
#include <queue>
#include <mutex>
#include <atomic>
using namespace std;

queue<int> buf;
mutex bufLock;

condition_variable_any cvProducer;
condition_variable_any cvConsumer;

void producer(){
	atomic<int> lock;
	
	lock = 1;
	
	while(true){
		this_thread::sleep_for(chrono::seconds (2));
		int e = lock.fetch_add(1);
		
		bufLock.lock();
		
		while(buf.size()>= 10){
			cvProducer.wait(bufLock);
		}
		
		buf.push(e);
		
		cout<< "Producer : an item produced " << e << "("<< buf.size() << ")"<< endl;
		
		bufLock.unlock();
		
		cvConsumer.notify_one();
	}	
}


void consumer(){
	while(true){
		this_thread::sleep_for(chrono::seconds (1));
		
		bufLock.lock();
		
		while(buf.empty())
			cvConsumer.wait(bufLock);
		
		int e = buf.front();
		buf.pop();
		
		cout << "Consumer : an item consumed " << e << "("<< buf.size() << ")"<< endl;
		
		bufLock.unlock();
		cvProducer.notify_one();
		
	}
}
int main() {
	thread p(producer);
	thread c(consumer);
	
	p.join();
	c.join();
	
	return 0;
}
