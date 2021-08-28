using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SyncTool.Logger;

namespace SyncTool.Implementations
{
    class Threading
    {
        //private static readonly Semaphore semaphore = new Semaphore(256, 256);

        public ConcurrentQueue<Thread> Queue { get; set; } = new ConcurrentQueue<Thread>();

        public ConcurrentQueue<Thread> Finished { get; set; } = new ConcurrentQueue<Thread>();

        public bool IsRunning { get; set; } = false;

        public void AddThread(Thread thread)
        {
            
                Queue.Enqueue(thread);
            
        }

        public void AddAllThreads(List<Thread> threads)
        {
            threads.ForEach(AddThread);
        }

        public void StartWork()
        {
            IsRunning = true;
            foreach(var thread in Queue)
            {
                ThreadStart(thread);
            }           
            IsRunning = false;
        }

        private void ThreadFinish(Thread th)
        {
          
            th.Join();
            AddToFinished(th);
            //semaphore.Release();
        }

        private void ThreadStart(Thread th)
        {
            //semaphore.WaitOne();
            th.Start();
            ThreadFinish(th);

        }

        private void AddToFinished(Thread th)
        {
          
                Finished.Enqueue(th);
            
        }


       
    }
}
