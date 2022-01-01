#include <iostream>
#include <thread>
#include <mutex>
using namespace std;

/*
callable ¿‡–Õ 
*/ 

mutex console;

class background_task{
public:
	void operator()()const{
		
		console.lock();
		
		cout << "Hello world\n";

		console.unlock();

	}
private:
}; 

int main() {
	background_task b1;
	background_task b2;
	
	thread t1(b1);
	thread t2(b2);
	t1.join();
	t2.join();
	
	
	return 0;
}
