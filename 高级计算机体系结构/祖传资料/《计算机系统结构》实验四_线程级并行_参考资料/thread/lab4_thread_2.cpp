#include <iostream>
#include <thread>
#include <string>
#include <vector>
using namespace std;
/*

多个线程

参数传递

*/

void run(int id,string msg,int repeat){
	for(int i = 0;i < repeat;i++)
		cout << id << ":" << msg << endl;
}

int main(int argc, char** argv) {

	thread t1(run,1,"Hello from 1", 1);
	
	t1.join();
	
	vector<thread> v;
	
	for(int i = 2;i < 10;i++){
		string msg = "hello from " + i;
	
		v.push_back(thread(run,i,msg,i));	 
	}
	
	for(auto &t:v)
		t.join();
	
	return 0;
}
