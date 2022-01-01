#include <iostream>
#include <thread>
using namespace std;

void run(){
	cout << "Hello world from thread\n";
}

int main() {
	
	thread t(run);
	
	t.join();
	
	cout << "Hello world from main thread.\n";
}
