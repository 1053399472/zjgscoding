#include <iostream>
#include <omp.h>

#include <stdio.h>
#include <windows.h>

using namespace std;

double f(double x){
	return 4.0 / (1.0 + x * x);
}

int main() {

	int n = 1000000;
	double step = 1.0 / n;
	double pi, sum;

	DWORD start, end;
	start = GetTickCount();

#pragma omp for reduction(+:sum)
	for (int i = 0; i < n; i++)
	{
		double x,sum;
		x = (i + 0.5) * step;
		sum = sum + f(x);
	}

	end = GetTickCount();

	pi = sum * step;

	printf("pi = %lf, elapsed time = %d\n", pi, end - start);

	return 0;
}