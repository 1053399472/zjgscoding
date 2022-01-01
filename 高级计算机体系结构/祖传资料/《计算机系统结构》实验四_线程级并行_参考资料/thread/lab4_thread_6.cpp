#include <iostream>       // std::cout  
#include <functional>     // std::ref  
#include <thread>         // std::thread  
#include <future>         // std::promise, std::future  
using namespace std;

void print_int (future<int>& fut) {  
  int x = fut.get();//当promise::set_value()设置了promise的共享状态值后，fut将会通过future::get()获得该共享状态值，若promise没有设置该值那么fut.get()将会阻塞线程直到共享状态值被promise设置  
  cout << "value: " << x << '\n';//输出：<span style="font-family: monospace; white-space: pre; background-color: rgb(231, 231, 231);">value: 10</span>  
}  
int main ()  
{  
  promise<int> prom;                      //创建一个promise对象  
  future<int> fut = prom.get_future();    //获取promise内部的future，fut将和promise共享promise中的共享状态，该共享状态用于返回计算结果  
  thread t1 (print_int, ref(fut));  //创建一个线程，并通过引用方式将fut传到print_int中  
  prom.set_value (10);                         //设置共享状态值  
                                               //  
  t1.join();//等待子线程  
  return 0;  
}
