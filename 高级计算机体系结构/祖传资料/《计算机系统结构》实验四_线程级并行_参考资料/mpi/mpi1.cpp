#include <iostream>
#include "mpi.h"
using namespace std;


int main1(int argc, char** argv) {
	
	MPI_Init(&argc,&argv);
	
	cout << "Hello world\n";
	
	MPI_Finalize();
	
	return 0;
}
