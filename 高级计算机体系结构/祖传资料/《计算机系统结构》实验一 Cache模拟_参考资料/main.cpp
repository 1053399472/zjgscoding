#include <iostream>

#include <stdio.h>
#include <stdlib.h>

using namespace std;

struct block{
	bool is_valid;
	int tag;
	void *data;
};

#include <vector> 

#define MEM_READ_TIME 100
#define CACHE_READ_TIME 1

class Cache{
public:
	
	int cache_size;
	int block_size;
	int set_size;
	
	int set_count;
	int hits,reads,writes,replaces,total,total_time;
	
	vector<struct block> blocks;
	
	void init(int p_cache_size,int p_block_size,int p_set_size){
		cache_size = p_cache_size;
		block_size = p_block_size;
		set_size = p_set_size;
		
		total = 0;
		reads = 0;
		writes = 0;
		hits = 0;
		replaces = 0;
		
		total_time = 0;
		
		set_count = cache_size / block_size / set_size;
		for(int i = 0;i < set_count * set_size;i++){
			struct block b;
			
			b.is_valid = false;
			b.tag = -1;
			
			blocks.push_back(b);
		}
			
	}
	
	void read(int addr){
		cout << "-- read from " << addr ;
		
		reads++;
		total++;
		
		//found the block
		int tag = addr / block_size ;
		
		int index = (tag/ set_size) % set_count ; 
		
		total_time += CACHE_READ_TIME;
		
		for(int i = index;i < index + set_size;i++){
			if(blocks[i].is_valid && blocks[i].tag == tag){//hit
				hits++;
				cout << " hit" << endl;
				return;
			}
		}
		
		cout << " miss" << endl;
		
		//load from memory
		total_time += MEM_READ_TIME;
		
		for(int i = index;i < index + set_size;i++){
			if(!blocks[i].is_valid){//found a free block
				blocks[i].is_valid = true;
				blocks[i].tag = tag;
				
				cout << " -- --- --- memory loaded " << endl;
				return;
			}
		}
		
		//random replace
		cout << "cache is full! Need replace!" << endl;
		int r = rand() % set_size + index;
		
		replaces++;
		
		blocks[r].is_valid = true;
		blocks[r].tag = tag;
		
		return;
			
	}
	
	void write(int addr){
		cout << "-- write to " << addr << endl;
		
		writes++;
		total++;
			
	}
	
	
	void show_statistic(){
		cout << "Cache size:" << cache_size << endl;
		cout << "block size:" << block_size << endl;
		cout << "set size:" << set_size << endl << endl;
		
		cout << "total r/w:" << total << endl;
		cout << "hit r/w:" << hits << endl ;
		cout << "replace :" << replaces << endl << endl;
				
		if(total == 0) total = 1;
		
		cout << "hit rate:" << hits/total << endl << endl;
		
		cout << "total time:" << total_time << endl;
		
		cout << "average time: " << total_time * 1.0 / total << endl;
		
	}
	
	
	Cache(int p_cache_size,int p_block_size,int p_set_size)	{
		init(p_cache_size,p_block_size,p_set_size);
	}
	
	~Cache(){
		//release the blocks
		if(set_count){
			blocks.clear();
		}
	}
	

};

int main(int argc, char** argv) {
	//init cache
	Cache c(1024,4,2);
	
	char rw;
	int addr;
	
	//load memory trace file
	while(scanf("%x %c",&addr,&rw) == 2 ){
		//cin	>> rw >> addr;
		
		/*
		if(rw == -1 && addr == -1)
			break;
		*/	
		if(rw == 'R')
			c.read(addr);
		else if(rw == 'W')
			c.write(addr);
	}
	
	c.show_statistic();	
	
	return 0;
}
