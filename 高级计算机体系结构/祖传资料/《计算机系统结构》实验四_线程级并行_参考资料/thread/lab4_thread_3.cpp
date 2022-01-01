#include <iostream>
#include <thread>
#include <string>
#include <vector>
#include <mutex>

using namespace std;
/*

多个线程

参数传递

互斥量 

*/

mutex console;

void run(int id,string msg,int repeat){
	
//	lock_guard<mutex> lock(console);

	for(int i = 0;i < repeat;i++)
	{
		console.lock();
		cout << id << ":" << msg << endl;
		console.unlock();
	}
}

int main(int argc, char** argv) {

	thread t1(run,1,"Hello from ", 1);
	
	t1.join();
	
	vector<thread> v;
	
	for(int i = 2;i < 10;i++){
	
		v.push_back(thread(run,i,"Echo from ",i));	 
	}
	
	for(auto &t:v)
		t.join();
	
	return 0;
}
