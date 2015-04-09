using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
namespace ImperativeSoccer
{
    public class WorkerThread<T>
    {
        private object workerStateLock;
        private MessageQueue<T> queue;
        private object state;        
        private WorkerThreadStart<T> threadStart;
      

        ManualResetEvent threadCondition;
        public ManualResetEvent ThreadCondition
        {
            get
            {
                lock(workerStateLock)
                { return threadCondition; }
            }
        }

        Thread workerThread; 
        public Thread Thread
        {
            get
            {
                lock(workerStateLock)
                {
                    return workerThread; 
                }
            }
        }
        ExceptionHandler exceptionDelegate;
        public event ExceptionHandler ExceptionHandler
        {
            add
            {
                lock(workerStateLock)
                {
                    exceptionDelegate += value; 
                }
            }
            remove
            {
                lock(workerStateLock)
                {
                    exceptionDelegate -= value;
                }

            }
        }
        ThreadProgress onCompleteDelegate; 
        public event ThreadProgress OnCompleteHandler
        {
            add
            {
                lock(workerStateLock)
                {
                    onCompleteDelegate += value;
                }
            }
            remove
            {
                lock(workerStateLock)
                {
                    onCompleteDelegate -= value;
                }
            }
        }
        

        public WorkerThread(WorkerThreadStart<T> starter,object obj)
        {
            if (starter == null)
                throw new ArgumentNullException("threadstart missing");
            threadStart = starter;            
            queue = new MessageQueue<T>();
            threadCondition = new ManualResetEvent(false);
            state = obj;
            workerStateLock = new object(); 
        }
        public WorkerThread(WorkerThreadStart<T> starter): this(starter,null)
        { }


        public void postMessage(T msg)
        {
            queue.Enqueue(msg);
        }
        public T extractMessage()
        {
            return queue.Dequeue();
        }
        public void Start()
        {
            lock (workerStateLock)
            {
                if (workerThread != null)
                    throw new InvalidOperationException("thread exists");
                workerThread = new Thread(new ThreadStart(Run));
                workerThread.Start();
            }           
            
        }
        private void Run()
        {
            try
            {
                threadStart(this, state);
            }catch(Exception e)
            {
                Console.WriteLine("Worker Thread with tid:{0} threw Exception {1}", Thread.CurrentThread.ManagedThreadId, e.ToString());
                ExceptionHandler handler;
                lock(workerStateLock)
                {
                    handler = exceptionDelegate;
                }
                if(handler !=null)
                    handler(this, e);
            }
            finally 
            {
                ThreadProgress handler;
                lock(workerStateLock)
                {
                    handler = onCompleteDelegate;
                }
                if(handler !=null)
                    onCompleteDelegate(this);
            }


        }
        
        
    }
}
