using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ImperativeSoccer
{
    class MessageQueue<T>
    {
        private LinkedList<T> queue = new LinkedList<T>();
        private readonly object queueLock = new object();

        public void Enqueue(T element)
        {
            lock(queueLock)
            {
                bool pulse = queue.Count == 0 ? true : false;
                queue.AddLast(element);
                if (pulse)
                    Monitor.Pulse(queueLock);
                    
            }
        }

        public T Dequeue()
        {
            lock(queueLock)
            {
                while (queue.Count == 0)
                {
                    Monitor.Wait(queueLock);
                }
                    
                LinkedListNode<T> node = queue.First;
                queue.RemoveFirst();
                return node.Value;
            }
        }


    }
}
