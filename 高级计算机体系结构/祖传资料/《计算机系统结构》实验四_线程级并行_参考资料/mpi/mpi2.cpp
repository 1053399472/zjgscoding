#include <iostream>
#include <string.h>
#include <stdio.h>
#include "mpi.h"
using namespace std;


int main(int argc, char** argv) {
	
	MPI_Init(&argc,&argv);
	int rank,total;
	char msg[128],recv[128];
	
	MPI_Status state;
	
	MPI_Comm_rank(MPI_COMM_WORLD,&rank);
	MPI_Comm_size(MPI_COMM_WORLD,&total);
	
	if(rank == 0){ // Ö÷Ïß³Ì
		for(int i = 1;i < total;i++){
			sprintf(msg,"Hello [%d], send from main[0]",i);
			MPI_Send(msg,128,MPI_CHAR,i,0,MPI_COMM_WORLD);
		}
	} else {
		MPI_Recv(recv,128,MPI_CHAR,0,0,MPI_COMM_WORLD,&state);
		cout << rank << " get : " << recv<< endl;	 
	}
	
	MPI_Finalize();
	
	cout << rank << endl;
	return 0;
}
