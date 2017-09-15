using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.ComponentModel;

namespace home.learn.thread
{
    static class ThreadPoolExamples
    {
        private delegate string RunOnThreadPool(out int threadId);

        public static void SimulateAsynchronousProgrammingModel()
        {
            int threadId = 0;
            RunOnThreadPool poolDelegate = Test;

            var t = new Thread(s => Test(out threadId));
            t.Start();

            t.Join();

            TDebug.Msg($"Thread id : {threadId}");
            IAsyncResult r = poolDelegate.BeginInvoke(out threadId, Callback, "a delegate asynchronous call");
            r.AsyncWaitHandle.WaitOne();

            string result = poolDelegate.EndInvoke(out threadId, r);

            TDebug.Msg($"Thread pool worker thread id: {threadId}");
            TDebug.Msg(result);

            Thread.Sleep(TimeSpan.FromSeconds(2));
        }

        public static void AsyncOperationInThreadPool()
        {
            int x = 1;
            int y = 2;
            string lambdaState = "lambda state 2";

            ThreadPool.QueueUserWorkItem(AsyncOperation);
            Thread.Sleep(TimeSpan.FromSeconds(2));

            ThreadPool.QueueUserWorkItem(AsyncOperation, "async state");
            Thread.Sleep(TimeSpan.FromSeconds(2));

            ThreadPool.QueueUserWorkItem(state =>
            {
                TDebug.Msg($"Operation state: {state}");
                TDebug.Msg($"Worker thread id: {Thread.CurrentThread.ManagedThreadId}");
                Thread.Sleep(TimeSpan.FromSeconds(1));

            }, "lambda state");

            Thread.Sleep(TimeSpan.FromSeconds(3));

            ThreadPool.QueueUserWorkItem(state =>
            {
                TDebug.Msg($"Operation state: {x + y}, {lambdaState}");
                TDebug.Msg($"Worker thread id: {Thread.CurrentThread.ManagedThreadId}");
                Thread.Sleep(TimeSpan.FromSeconds(1));

            }, "lambda state");
        }

        public static void SimulateCancelThread()
        {
            using(var cts = new CancellationTokenSource())
            {
                CancellationToken token = cts.Token;
                ThreadPool.QueueUserWorkItem(_ => AsyncOperationCancelByRequest(token));
                Thread.Sleep(TimeSpan.FromSeconds(2));
                cts.Cancel();
            }

            using (var cts = new CancellationTokenSource())
            {
                CancellationToken token = cts.Token;
                ThreadPool.QueueUserWorkItem(_ => AsyncOperationCancelByException(token));
                Thread.Sleep(TimeSpan.FromSeconds(2));
                cts.Cancel();
            }

            using (var cts = new CancellationTokenSource())
            {
                CancellationToken token = cts.Token;
                ThreadPool.QueueUserWorkItem(_ => AsyncOperationByRegister(token));
                Thread.Sleep(TimeSpan.FromSeconds(2));
                cts.Cancel();
            }
        }
        public static void CompareThreadsAndPool()
        {
            int numberOfOperations = 500;
            var sw = new Stopwatch();
            sw.Start();
            UseThreads(numberOfOperations);
            sw.Stop();
            TDebug.Log($"Execution time using threads: {sw.ElapsedMilliseconds}");

            sw.Reset();
            sw.Start();
            UseThreadPool(numberOfOperations);
            sw.Stop();
            TDebug.Msg($"Execution time using thread pool: {sw.ElapsedMilliseconds}");

        }

        public static void SimulateTimer()
        {
            TDebug.Msg("Press 'Enter' to stop the timer...");
            DateTime start = DateTime.Now;

            Timer timer = new Timer(_ =>
            {
                TimeSpan elapsed = DateTime.Now - start;
                TDebug.Msg($"{elapsed.Seconds} seconds from {start} . Timer thread pool thread id: {Thread.CurrentThread.ManagedThreadId}");

            }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));

            try
            {
                Thread.Sleep(TimeSpan.FromSeconds(6));
                timer.Change(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(4));

                Console.ReadLine();
            }
            finally
            {
                timer.Dispose();
            }
        }

        private static void UseThreads(int numberOfOperations)
        {
            using (var countdown = new CountdownEvent(numberOfOperations))
            {
                TDebug.Msg("Scheduling work by creating threads");
                for (int i = 0; i < numberOfOperations; i++)
                {
                    var thread = new Thread(s =>
                    {
                        TDebug.Msg($"{Thread.CurrentThread.ManagedThreadId},");
                        Thread.Sleep(TimeSpan.FromSeconds(0.1));
                        countdown.Signal();
                    });
                    thread.Start();
                }
                countdown.Wait();

            }
        }

        public static void SimulateEventBasedAsynchronousPattern()
        {
            var bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;

            bw.DoWork += Worker_DoWork;
            bw.ProgressChanged += Worker_ProgressChange;
            bw.RunWorkerCompleted += Worker_Completed;

            bw.RunWorkerAsync();

            TDebug.Msg("Press C to cancel work");
            do
            {
                if (Console.ReadKey(true).KeyChar == 'C')
                {
                    bw.CancelAsync();
                }
            }
            while(bw.IsBusy);
        }

        private static void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            TDebug.Msg($"DoWork thread pool thread id :{Thread.CurrentThread.ManagedThreadId}");
            var bw = (BackgroundWorker)sender;
            for (int i = 0; i < 100; i++)
            {
                if (bw.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                if(i%10 == 0)
                {
                    bw.ReportProgress(i);
                }

                Thread.Sleep(TimeSpan.FromSeconds(0.1));
            }
            e.Result = 42;
        }
        private static void Worker_ProgressChange(object sender, ProgressChangedEventArgs e)
        {
            TDebug.Msg($"{e.ProgressPercentage}% completed." + $"Progress thread pool thread id: {Thread.CurrentThread.ManagedThreadId}");
        }

        private static void Worker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            TDebug.Msg($"Completed thread pool thread id:{Thread.CurrentThread.ManagedThreadId}");
            if(e.Error != null)
            {
                TDebug.Msg($"Exception {e.Error.Message} has occured.");
            }
            else if (e.Cancelled)
            {
                TDebug.Msg("Operation has been canceled");
            }
            else
            {
                TDebug.Msg($"The answer is : {e.Result}");
            }
        }

        private static void UseThreadPool(int numberOfOperations)
        {
            using (var countdown = new CountdownEvent(numberOfOperations))
            {
                TDebug.Msg("Scheduling work by creating threads");
                for (int i = 0; i < numberOfOperations; i++)
                {
                    ThreadPool.QueueUserWorkItem(s =>
                    {
                        TDebug.Msg($"{Thread.CurrentThread.ManagedThreadId},");
                        Thread.Sleep(TimeSpan.FromSeconds(0.1));
                        countdown.Signal();
                    });
                }
                countdown.Wait();

            }
        }
        private static void AsyncOperationCancelByRequest(CancellationToken token)
        {
            TDebug.Msg("starting the first task");
            for (int i = 0; i < 5; i++)
            {
                if (token.IsCancellationRequested)
                {
                    TDebug.Msg("The first task has been canceled");
                    return;
                }
                Thread.Sleep(TimeSpan.FromSeconds(1));

            }
            TDebug.Msg("The first task has completed succesfully");
        }

        private static void AsyncOperationCancelByException(CancellationToken token)
        {
            try
            {
                TDebug.Msg("starting the second task");
                for (int i = 0; i < 5; i++)
                {
                    token.ThrowIfCancellationRequested();
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }
                TDebug.Msg("the second task has completed successfully");
            }
            catch (OperationCanceledException)
            {

                TDebug.Msg("the second task has been canceled");
            }
        }
        private static void AsyncOperationByRegister(CancellationToken token)
        {
            bool cancellationFlag = false;
            token.Register(() => cancellationFlag = true);
            TDebug.Msg("Starting the third task");
            for (int i = 0; i < 5; i++)
            {
                if (cancellationFlag)
                {
                    TDebug.Msg("The third task has been canceled");
                    return;
                }
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
            TDebug.Msg("the third task has completed succesfully");
        }

        private static void AsyncOperation(object state)
        {
            TDebug.Msg($"Operation state: {state ?? "(null)"}");
            TDebug.Msg($"WOrker thread id: {Thread.CurrentThread.ManagedThreadId}");
            Thread.Sleep(TimeSpan.FromSeconds(2));

        }

        private static void Callback(IAsyncResult asyncResult)
        {
            TDebug.Msg("Starting a callback...");
            TDebug.Msg($"State passed to a callback: { asyncResult.AsyncState}");
            TDebug.Msg($"Is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}");
            TDebug.Msg($"Thread pool worker thread id : {Thread.CurrentThread.ManagedThreadId}");
            
        }

        private static string Test(out int threadId)
        {
            TDebug.Msg("Starting....");
            TDebug.Msg($"Is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}");
            Thread.Sleep(TimeSpan.FromSeconds(2));
            threadId = Thread.CurrentThread.ManagedThreadId;
            return $"Thread pool worker thread id was : {threadId}";
        }
    }
}
