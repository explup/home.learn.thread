using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace home.learn.thread
{
    static class TDebug
    {
         public static void LogIsLocked(object obj)
        {
            if (Monitor.IsEntered(obj))
            {
                Console.WriteLine($"State of {obj} in thread {Thread.CurrentThread.Name} is: locking");
            }
            else
            {
                Console.WriteLine($"State of {obj} in thread {Thread.CurrentThread.Name} is: unlocked");

            }

        }


        public static void LogThreadState(Thread t)
        {
            Console.WriteLine(string.Format("{0} state: {1}", t.Name, t.ThreadState));
        }

        public static void Log(Thread thread, string msg)
        {
            Console.WriteLine($"{thread.Name} : {msg}");

        }

        public static void Log(string msg)
        {
            Console.WriteLine($"{Thread.CurrentThread.Name} : {msg}");

        }

        public static void Msg(string msg)
        {
            Console.WriteLine(msg);

        }
    }
}
