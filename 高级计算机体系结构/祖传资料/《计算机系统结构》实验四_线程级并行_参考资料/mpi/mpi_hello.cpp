#include <iostream>
#include <mpi.h>
#include <stdio.h>
#include <string.h>
using namespace std;

double f(double x){
	return 4.0 / (1 + x * x);
}
int main(int argc, char** argv) {
	int id,n,N;
	MPI_Status status;
	int i;
	
	double mpi,pi,sum,dx,x;

	MPI_Init(&argc,&argv);
	
	MPI_Comm_rank(MPI_COMM_WORLD, &id);
	MPI_Comm_size(MPI_COMM_WORLD, &n);
	
	if( id == 0){
		cout << "ÇëÊäÈëN:\n"; 
		cin >> N;
		
	} 
	
	MPI_Bcast(&N,1,MPI_INT,0,MPI_COMM_WORLD);
	
	dx = (1.0 - 0)/ N;
	sum = 0.0;
	
	for(i = id + 1;i <= N;i += n){
		x = dx * (i - 0.5);
		sum = sum + f(x);
	}
	
	mpi = dx * sum;
	
	cout << id << " : My PI sum = " << mpi << endl;

	MPI_Reduce(&mpi,&pi,1,MPI_DOUBLE,MPI_SUM,0,MPI_COMM_WORLD);
	
	if(id == 0){
		cout <<"PI = "<< pi << endl;
	}
	
	MPI_Finalize();
	
	cout << "exit!\n";
	
	return 0;
}
