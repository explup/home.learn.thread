using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace home.learn.thread
{
    static class ThreadControlExamples
    {
        private static AutoResetEvent _workerEvent = new AutoResetEvent(false);
        private static AutoResetEvent _mainEvent = new AutoResetEvent(false);
        private static ManualResetEventSlim _manualEvent = new ManualResetEventSlim(false);

        private static Barrier barrier = new Barrier(2, b => TDebug.Msg($"End of phase {b.CurrentPhaseNumber + 1}"));

        private static CountdownEvent countdownEvent = new CountdownEvent(2);

        public static void SimulateAutoResetEvent()
        {
            var t = new Thread(s => AutoResetEvent(10));
            t.Start();

            TDebug.Msg("Waiting for another thread to complete work");
            _workerEvent.WaitOne();

            TDebug.Msg("First operation is completed");
            TDebug.Msg("Performing an operation on a main thread");
            Thread.Sleep(TimeSpan.FromSeconds(5));
            _mainEvent.Set();

            TDebug.Msg("Now running the second operation on a second thread");
            _workerEvent.WaitOne();
            TDebug.Msg("Second operation is completed");

        }
        public static void SimulateManualResetevent()
        {
            var t1 = new Thread(s => ManualresetEvent(5));
            t1.Name = "Thread 1";

            var t2 = new Thread(s => ManualresetEvent(6));
            t2.Name = "Thread 2";

            var t3 = new Thread(s => ManualresetEvent(12));
            t3.Name = "Thread 3";

            t1.Start();
            t2.Start();
            t3.Start();
            Thread.Sleep(TimeSpan.FromSeconds(6));
            TDebug.Msg("The gates are now open");
            _manualEvent.Set();

            Thread.Sleep(TimeSpan.FromSeconds(2));
            _manualEvent.Reset();
            TDebug.Msg("The gates have been closed");
            Thread.Sleep(TimeSpan.FromSeconds(10));
            TDebug.Msg("The gates are now open for the second time");
            _manualEvent.Set();
            Thread.Sleep(TimeSpan.FromSeconds(2));
            TDebug.Msg("the gates have been closed!");
            _manualEvent.Reset();
            
        }

        public static void Countdown() {
            TDebug.Msg("starting two operations");
            var t1 = new Thread(s => PerformCountdown("operation 1 is completed", 4));
            var t2 = new Thread(s => PerformCountdown("operation 2i s completed", 8));

            t1.Start();
            t2.Start();

            countdownEvent.Wait();
            TDebug.Msg("Both operations have been completed");
            countdownEvent.Dispose();   

        }

        public static void SimulateBarrier()
        {
            var t1 = new Thread(s => PerformBarrier("the guitarist", "play an amazing solo", 5));
            var t2 = new Thread(s => PerformBarrier("the singer", "sing his song", 2));

            t1.Start();
            t2.Start();
        }
        private static void PerformBarrier(string name, string msg, int seconds)
        {
            for (int i = 1; i < 3; i++)
            {
                TDebug.Msg("-------------------------");
                Thread.Sleep(TimeSpan.FromSeconds(seconds));
                TDebug.Msg($"{name} starts to {msg}");
                Thread.Sleep(TimeSpan.FromSeconds(seconds));
                TDebug.Msg($"{name} finishes to {msg}");
                barrier.SignalAndWait();
            }
        }

        private static void PerformCountdown(string msg, int seconds) {
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
            TDebug.Msg(msg);
            countdownEvent.Signal();    

        }

        private static void ManualresetEvent(int seconds)
        {
            TDebug.Log("falls to sleep");
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
            TDebug.Log("waits for the gates to open");
            _manualEvent.Wait();
            TDebug.Log("enters the gates");
            
        }
        private static void AutoResetEvent(int seconds)
        {
            TDebug.Msg("Starting a long running work...");
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
            TDebug.Msg("Work is done");
            _workerEvent.Set();

            TDebug.Msg("Wariting for a main thread to complete its work");
            _mainEvent.WaitOne();

            TDebug.Msg("Starting second operation....");
            Thread.Sleep(TimeSpan.FromSeconds(seconds));

            TDebug.Msg("Work is done");
            _workerEvent.Set();
        
        }
    }
}
