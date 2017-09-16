using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace home.learn.thread
{
    static class AwaitExamples
    {
        public static void SimulateAsyncWithAwait()
        {
            Task t =  AsynchronyWithAwait();
            t.Wait();
        }

        public static void SimulateAsyncLambda()
        {
            Task t = AsyncLambdaPrcessing();
            t.Wait();
        }

        public static void SimulateAsyncWithTwoAwait()
        {
            Task t = AsyncWithTwoAwait();
            t.Wait();
        }

        private static async Task AsyncWithTwoAwait()
        {
            try
            {
                string result = await GetInfoAsync("Task 1");
                TDebug.Msg(result);

                result = await GetInfoAsync("Task 2");
                TDebug.Msg(result);
            }
            catch (Exception ex)
            {
                TDebug.Msg(ex.ToString());
            }
        }
        private static async Task AsyncLambdaPrcessing()
        {
            Func<string, Task<string>> asyncLambda = async name =>
             {
                 await Task.Delay(TimeSpan.FromSeconds(2));
                 return $"Task {name} is running on a thre id {Thread.CurrentThread.ManagedThreadId}"
                 + $"is therad pool thread: {Thread.CurrentThread.IsThreadPoolThread}";
             };
            string result = await asyncLambda("async lambda");
            TDebug.Msg(result);
        }
        private static async Task AsynchronyWithAwait()
        {
            try
            {
                string result = await GetInfoAsync("Task 1");
                TDebug.Msg(result);
            }
            catch (Exception ex)
            {
                TDebug.Msg(ex.ToString());
            }
        }

        private static async Task<string> GetInfoAsync(string name)
        {
            await Task.Delay(TimeSpan.FromSeconds(2));
            //throw new Exception("test");
            return $"Task {name} is running on a thre id {Thread.CurrentThread.ManagedThreadId}"
                + $"is therad pool thread: {Thread.CurrentThread.IsThreadPoolThread}";
        }
    }
}
