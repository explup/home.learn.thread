using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace home.learn.thread
{
    static class ThreadBasicExamples
    {  /// <summary>
       /// example for join and abort
       /// </summary>
       public static void JoinAbortExample()
        {
            string delaySubThreadName = "DelaySubThread";
            string subThreadname = "SubThread";

            Thread delayThread = new Thread(s => PrintNumbersWithDelay());
            delayThread.Name = delaySubThreadName;

            delayThread.Start();
            TDebug.LogThreadState(delayThread);

            Thread.Sleep(TimeSpan.FromSeconds(6));
            delayThread.Abort();
            TDebug.LogThreadState(delayThread);

            TDebug.Log(delayThread, "aborted");

            Thread t = new Thread(s => PrintNumbers());
            t.Name = subThreadname;

            t.Start();
            t.Join();
            TDebug.Log(t, "completed");

            TDebug.LogThreadState(t);

            PrintNumbers();
        }

        private static void PrintNumbers()
        {
            TDebug.Log("Starting");
            for (int i = 0; i < 100; i++)
            {
                TDebug.Log($"writing {i}");
            }
        }

        private static void PrintNumbersWithDelay()
        {
            TDebug.Log("Starting");
            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(2));
                TDebug.Log($"writing {i}");
            }
        }

    }
}
