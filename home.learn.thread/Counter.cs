using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace home.learn.thread
{

    abstract class CounterBase
    {
        public abstract void Increment();
        public abstract void Decrement();
    }

    class Counter : CounterBase
    {
        public int Count { get; private set;}


        public override void Decrement()
        {
            Count++;
        }

        public override void Increment()
        {
            Count--;
        }
    }

    class CounterWithLock : CounterBase
    {
        private readonly object _syncRoot = new object();

        public int Count { get; private set; }

        public override void Decrement()
        {
            lock (_syncRoot)
            {
                Count++;
            }
        }

        public override void Increment()
        {
            lock (_syncRoot)
            {
                Count--;
            }
        }
    }

    class CounterNoLock : CounterBase
    {
        private int _count;

        public int Count => _count;

        
        public override void Decrement()
        {
            Interlocked.Increment(ref _count);
        }

        public override void Increment()
        {
            Interlocked.Decrement(ref _count);
        }
    }
}
