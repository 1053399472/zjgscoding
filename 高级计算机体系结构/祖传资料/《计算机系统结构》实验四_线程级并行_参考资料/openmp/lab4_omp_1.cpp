#include <iostream>
#include <omp.h>
using namespace std;

/* run this program using the console pauser or add your own getch, system("pause") or input loop */

int main(int argc, char** argv) {
#pragma omp parallel for
for(int i = 0;i < 10;i++){
	cout << "i = " << i << endl;
}

	return 0;
}
