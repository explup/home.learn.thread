using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace home.learn.thread
{
    static class ThreadSafeExamples
    {
        /// <summary>
        /// example for unsafe thread counter and using lock to solve this problem
        /// </summary>
        public static void SimulateLock ()
        {
            TDebug.Msg("Incorrect counter");
            var c = new Counter();

            var t1 = new Thread(t => TestCounter(c));
            var t2 = new Thread(t => TestCounter(c));
            var t3 = new Thread(t => TestCounter(c));

            t1.Start();
            t2.Start();
            t3.Start();

            t1.Join();
            t2.Join();
            t3.Join();

            TDebug.Msg($"Total count : {c.Count}");
            TDebug.Msg("-----------------------------");

            TDebug.Msg("Correct coutner");
            var c1 = new CounterWithLock();

            t1 = new Thread(t => TestCounter(c1));
            t2 = new Thread(t => TestCounter(c1));
            t3 = new Thread(t => TestCounter(c1));


            t1.Start();
            t2.Start();
            t3.Start();

            t1.Join();
            t2.Join();
            t3.Join();

            TDebug.Msg($"Total count : {c1.Count}");
            TDebug.Msg("-----------------------------");
        }

        public static void SimulateNoLock()
        {
            TDebug.Msg("Incorrect counter");
            var c = new Counter();

            var t1 = new Thread(t => TestCounter(c));
            var t2 = new Thread(t => TestCounter(c));
            var t3 = new Thread(t => TestCounter(c));

            t1.Start();
            t2.Start();
            t3.Start();

            t1.Join();
            t2.Join();
            t3.Join();

            TDebug.Msg($"Total count : {c.Count}");
            TDebug.Msg("-----------------------------");

            TDebug.Msg("Correct coutner");
            var c1 = new CounterNoLock();

            t1 = new Thread(t => TestCounter(c1));
            t2 = new Thread(t => TestCounter(c1));
            t3 = new Thread(t => TestCounter(c1));


            t1.Start();
            t2.Start();
            t3.Start();

            t1.Join();
            t2.Join();
            t3.Join();

            TDebug.Msg($"Total count : {c1.Count}");
            TDebug.Msg("-----------------------------");
        }


        public static void SimulateDeadLock()
        {
            Console.WriteLine("Starting simuate dead lock ...");

            object locker1 = new object();
            object locker2 = new object();
            object locker3 = new object();

            Thread t1 = new Thread(s => LockTooMuch(locker1, locker2));
            t1.Name = "Montior Sub Thread";
            t1.Start();

            TDebug.Log("locking object 2");
            lock (locker3)
            {
                lock (locker2)
                {
                    TDebug.LogIsLocked(locker1);
                    TDebug.LogIsLocked(locker2);
                    TDebug.LogIsLocked(locker3);
                    Thread.Sleep(1000);
                    if (Monitor.TryEnter(locker1, TimeSpan.FromSeconds(5)))
                    {
                        TDebug.Log("accquired a protected resource succesfully : object1");
                    }
                    else
                    {
                        TDebug.Log("timeout acruiring a resource : object1");
                        //Debug("released locking on object2");
                        //Monitor.Exit(locker2);
                    }
                }
            }


            TDebug.LogIsLocked(locker1);
            TDebug.LogIsLocked(locker2);
            TDebug.LogIsLocked(locker3);

            Thread t2 = new Thread(s => LockTooMuch(locker1, locker2));
            t2.Name = "Dead Lock Sub Thread";
            TDebug.Log(t2, "starting");
            t2.Start();

            TDebug.Log("locking object 2");
            lock (locker2)
            {
                Thread.Sleep(1000);
                TDebug.Log("locking object 1");
                lock (locker1)
                {
                    TDebug.Log("accquired the protected succesfully : object1");
                }
            }
            TDebug.Log("end simuate dead lock ...");
        }


        public static void SimulateMutex()
        {
            string mutextname = "mutext_name";
            Mutex mutex = new Mutex(false, mutextname);

            Thread t1 = new Thread(s => MutexThreadSafe(mutex));
            t1.Name = "thread 1";
            t1.Start();

            Thread t2 = new Thread(s=>MutexThreadSafe(mutex));
            t2.Name = "thread 2";
            t2.Start();

        }

        public static void SimulateSemaphore()
        {
            for (int i = 0; i < 7; i++)
            {
                string threadName = "Thread" + i;
                int secondsToWait = 2 + 2 * i;
                var t = new Thread(s => SemaphoreThreadSafe(secondsToWait));
                t.Name = threadName;
                t.Start();
            }
        }

        public static void SimulateReadWriteLock()
        {
            new Thread(Read) { IsBackground = true }.Start();
            new Thread(Read) { IsBackground = true }.Start();
            new Thread(Read) { IsBackground = true }.Start();

            new Thread(s => Write("Thread 1")) { IsBackground = true }.Start();
            new Thread(s => Write("Thread 2")) { IsBackground = true }.Start();

            Thread.Sleep(TimeSpan.FromSeconds(30));
        }
        private static SemaphoreSlim _semaphore = new SemaphoreSlim(4);
        private static ReaderWriterLockSlim _rw = new ReaderWriterLockSlim();
        private static Dictionary<int, int> _items = new Dictionary<int, int>();

        private static void Read()
        {
            TDebug.Msg("Reading contents of a dic");
            while (true)
            {
                try
                {
                    _rw.EnterReadLock();
                    //TDebug.Msg($"entered upgradeable read lock");

                    foreach (var item in _items.Keys)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(0.1));

                    }
                }
                finally
                {
                    _rw.ExitReadLock();
                }
            }
        }

        private static void Write(string threadName)
        {
            while (true)
            {
                try
                {
                    int newKey = new Random().Next(10);
                    _rw.EnterUpgradeableReadLock();
                    if (!_items.ContainsKey(newKey))
                    {
                        try
                        {
                            _rw.EnterWriteLock();
                            _items[newKey] = 1;
                            TDebug.Msg($"new key {newKey} is added to a dic by a {threadName}");

                        }
                        finally
                        {
                            _rw.ExitWriteLock();
                        }
                    }
                    Thread.Sleep(TimeSpan.FromSeconds(0.1));
                }
                
                finally
                {
                    _rw.ExitUpgradeableReadLock();
                }
            }
        }

        private static void SemaphoreThreadSafe(int seconds)
        {
            TDebug.Log("waits to access a block of code");
            _semaphore.Wait();
            TDebug.Log("was granted an access");
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
            TDebug.Log("is completed");
            _semaphore.Release();
        }
        private static void MutexThreadSafe(Mutex mutex)
        {
            mutex.WaitOne();   // Wait until it is safe to enter.
            TDebug.Log("enters thread safe");
            // Place code to access non-reentrant resources here.
            Thread.Sleep(500);    // Wait until it is safe to enter.
            TDebug.Log("leaved thread safe");
            mutex.ReleaseMutex();    // Release the Mutex.
        }
        private static void LockTooMuch(object locker1, object locker2)
        {
            TDebug.Log("locking object 1");
            lock (locker1)
            {
                TDebug.Log("sleeping");
                Thread.Sleep(1000);
                TDebug.Log("is locking object 2");
                lock (locker2)
                {
                    TDebug.Log("locker2 is released");
                }
            }

        }
        private static void TestCounter(CounterBase cb)
        {
            for (int i = 0; i < 100000; i++)
            {
                cb.Increment();
                cb.Decrement();
            }
        }
    }
}
