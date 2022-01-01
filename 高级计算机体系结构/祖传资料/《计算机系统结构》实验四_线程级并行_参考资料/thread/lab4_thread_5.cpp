#include <iostream>
#include <vector>
#include <future>
#include <algorithm>
#include <numeric>
#include <mutex>
#include <thread>

/*
Òì²½»Øµ÷ 
*/

using namespace std;

mutex m;

template <typename Iter>
int para_sum(Iter begin,Iter end){
	typename Iter::difference_type len = end - begin;
	
	if(len < 1000)
		return accumulate(begin,end,0);
	
	auto mid = begin + len / 2;
	
	auto handle = async(launch::async,
			para_sum<Iter>,mid,end);
		
	int sum = para_sum(begin,mid);
	
	m.lock();
	cout<< this_thread::get_id() << " sum = " << sum << endl; 
	m.unlock();

	return sum + handle.get();
}

int main() {
	
	vector<int> v(10000,1);
	
	cout << "sum = " << para_sum(v.begin(),v.end()) << endl;
	return 0;
}
