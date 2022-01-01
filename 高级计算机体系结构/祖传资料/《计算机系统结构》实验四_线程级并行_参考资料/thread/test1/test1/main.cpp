#include <omp.h>
#include <iostream>
#include <chrono>
#include <thread>
#include <vector>
#include <iomanip>

using namespace std;

void doNothing(){}

double run(int which){
	auto start = chrono::system_clock::now();
	int n = 16;

	for (int i = 1; i < 1000; i++){
		if (which == 1){//thread
			vector<thread> v;

			for (int j = 0; j < n; j++)
				v.push_back(thread(doNothing));
			for (auto &t : v)
				t.join();
		}
		else if (which == 2){//openmp
#pragma omp parallel for num_threads(n)
			for (int i = 0; i < n; i++)
				doNothing();
		}
	}
	auto end = chrono::system_clock::now();

	chrono::duration<double> elapsed = end - start;
	return elapsed.count();
}

int main(){



	double t2 = run(2);
	cout << "OpenMP : " << setprecision(5) << t2 << endl;

	double t1 = run(1);

	cout << "c++11 thread: " << setprecision(5)<< t1 << endl;
	
	system("pause");

	return 0;
}