#include <iostream>       // std::cout  
#include <functional>     // std::ref  
#include <thread>         // std::thread  
#include <future>         // std::promise, std::future  
using namespace std;

void print_int (future<int>& fut) {  
  int x = fut.get();//��promise::set_value()������promise�Ĺ���״ֵ̬��fut����ͨ��future::get()��øù���״ֵ̬����promiseû�����ø�ֵ��ôfut.get()���������߳�ֱ������״ֵ̬��promise����  
  cout << "value: " << x << '\n';//�����<span style="font-family: monospace; white-space: pre; background-color: rgb(231, 231, 231);">value: 10</span>  
}  
int main ()  
{  
  promise<int> prom;                      //����һ��promise����  
  future<int> fut = prom.get_future();    //��ȡpromise�ڲ���future��fut����promise����promise�еĹ���״̬���ù���״̬���ڷ��ؼ�����  
  thread t1 (print_int, ref(fut));  //����һ���̣߳���ͨ�����÷�ʽ��fut����print_int��  
  prom.set_value (10);                         //���ù���״ֵ̬  
                                               //  
  t1.join();//�ȴ����߳�  
  return 0;  
}
