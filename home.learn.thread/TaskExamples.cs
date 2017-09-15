using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace home.learn.thread
{
    static class TaskExamples
    {
        public static void TaskBasicOperations()
        {
            var t1 = new Task(()=> TaskMethod("Task 1"));
            var t2 = new Task(() => TaskMethod("Task 2"));

            t2.Start();
            t1.Start();

            Task.Run(() => TaskMethod("Task 3"));
            Task.Factory.StartNew(() => TaskMethod("Task 4"));
            Task.Factory.StartNew(() => TaskMethod("Task 5"),
            TaskCreationOptions.LongRunning);

            Thread.Sleep(TimeSpan.FromSeconds(1));
            TDebug.Msg("------------------------------");

            TaskMethod2("Main Thread Task");
            Task<int> task = CreateTask("Task 11");
            task.Start();
            TaskMethod2("Main Thread Task11");
            int result = task.Result;
            TDebug.Msg($"Result is: {result}");

            task = CreateTask("Task 22");
            task.RunSynchronously();
            TaskMethod2("Main Thread Task2");
            result = task.Result;
            TDebug.Msg($"Result is: {result}");

            task = CreateTask("Task 333");
            TDebug.Msg(task.Status.ToString());
            task.Start();

            while (!task.IsCompleted)
            {
                TDebug.Msg(task.Status.ToString());
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
            }
            TDebug.Msg(task.Status.ToString());
            result = task.Result;
            TDebug.Msg($"Result is:{result}");


            TDebug.Msg("------------------------------");

            var firstTask = new Task<int>(() => TaskMethod("First task", 3));
            var secondTask = new Task<int>(() => TaskMethod("Second task", 2));

            firstTask.ContinueWith(
                t => TDebug.Msg(
                    $"the first anwser is {t.Result}. Thread id " +
                    $"{Thread.CurrentThread.ManagedThreadId}, is thread pool thread: " +
                    $"{Thread.CurrentThread.IsThreadPoolThread}"),
                TaskContinuationOptions.OnlyOnRanToCompletion
                );

            firstTask.Start();
            secondTask.Start();

            Thread.Sleep(TimeSpan.FromSeconds(4));

            Task continuation = secondTask.ContinueWith(
               t => TDebug.Msg(
                   $"the second anwser is {t.Result}. Thread id " +
                   $"{Thread.CurrentThread.ManagedThreadId}, is thread pool thread: " +
                   $"{Thread.CurrentThread.IsThreadPoolThread}"),
               TaskContinuationOptions.OnlyOnRanToCompletion |
               TaskContinuationOptions.ExecuteSynchronously
               );
            continuation.GetAwaiter().OnCompleted(() =>
            TDebug.Msg($"Continuation task completed!. Thread id " +
                   $"{Thread.CurrentThread.ManagedThreadId}, is thread pool thread: " +
                   $"{Thread.CurrentThread.IsThreadPoolThread}"));
            Thread.Sleep(TimeSpan.FromSeconds(2));

            firstTask = new Task<int>(() =>
            {
                var innerTask = Task.Factory.StartNew(() => TaskMethod("Second task", 5), TaskCreationOptions.AttachedToParent);
                innerTask.ContinueWith(t => TaskMethod("Third task", 2),
                    TaskContinuationOptions.AttachedToParent);
                return TaskMethod("First task", 2);

            });

            firstTask.Start();
            while (!firstTask.IsCompleted)
            {
                TDebug.Msg(firstTask.Status.ToString());
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
            }
            TDebug.Msg(firstTask.Status.ToString());

        }

        public static void TaskCancel()
        {
            var cts = new CancellationTokenSource();
            var longTask = new Task<int>(() =>
            CancelableTaskMethod("task1", 10, cts.Token), cts.Token);

            TDebug.Msg(longTask.Status.ToString());
            cts.Cancel();

            TDebug.Msg(longTask.Status.ToString());
            TDebug.Msg($"First task has been cancelled before execution");

            cts = new CancellationTokenSource();
            longTask = new Task<int>(() =>
            CancelableTaskMethod("Task2", 10, cts.Token), cts.Token);
            longTask.Start();

            for (int i = 0; i < 5; i++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(0.1));
                TDebug.Msg(longTask.Status.ToString());
            }
            cts.Cancel();
            for (int i = 0; i < 5; i++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
                TDebug.Msg(longTask.Status.ToString());
            }

            TDebug.Msg($"a task has been completed with result {longTask.Result}");

        }

        public static void SimulateTaskWithException()
        {
            Task<int> task;
            try
            {
                task = Task.Run(() => TaskMethodException("task 1", 2));
                int result = task.Result;
                TDebug.Msg($"result: {result}");
            }
            catch (Exception ex)
            {
                TDebug.Msg($"Exception caught: {ex}");
            }

            TDebug.Msg("--------------------------------");

            try
            {
                task = Task.Run(() => TaskMethodException("Task 2", 2));
                int result = task.GetAwaiter().GetResult();
                TDebug.Msg($"result: {result}");
            }
            catch (Exception ex)
            {
                TDebug.Msg($"Exception caught: {ex}");
            }

            TDebug.Msg("------------------------------------");

            var t1 = new Task<int>(() => TaskMethodException("task 3", 3));
            var t2 = new Task<int>(() => TaskMethodException("task 4", 4));
            var complexTask = Task.WhenAll(t1, t2);
            var exceptionhandler = complexTask.ContinueWith(t =>
                TDebug.Msg($"Exception caught:{t.Exception}"),
                TaskContinuationOptions.OnlyOnFaulted);

            t1.Start();
            t2.Start();
        }

        public static void SimulateComplexTasks()
        {
            var firstTask = new Task<int>(() => TaskMethod("task 1", 3));
            var secondTask = new Task<int>(() => TaskMethod("task 2", 2));
            var whenAllTask = Task.WhenAll(firstTask, secondTask);

            whenAllTask.ContinueWith(t =>
            TDebug.Msg($"the first answer is {t.Result[0]}, the second is {t.Result[1]}"),
            TaskContinuationOptions.OnlyOnRanToCompletion);

            firstTask.Start();
            secondTask.Start();

            Thread.Sleep(TimeSpan.FromSeconds(4));

            var tasks = new List<Task<int>>();
            for (int i = 0; i < 4; i++)
            {
                int counter = i;
                var task = new Task<int>(() => TaskMethod($"Task {counter}", counter));
                tasks.Add(task);
                task.Start();
            }

            while(tasks.Count > 0)
            {
                var completedTask = Task.WhenAny(tasks).Result;
                tasks.Remove(completedTask);
                TDebug.Msg($"a task has been completed with result {completedTask.Result}");
            }
        }

        private static int TaskMethodException(string name,int seconds)
        {
            TDebug.Msg($"Task {name} is running on a thread id {Thread.CurrentThread.ManagedThreadId}");
            TDebug.Msg($"is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}");

            Thread.Sleep(TimeSpan.FromSeconds(seconds));

            throw new Exception("test exception");

            return 42 * seconds;
        }

        private static int CancelableTaskMethod(string name, int seconds, CancellationToken token)
        {
            TDebug.Msg($"Task {name} is running on a thread id {Thread.CurrentThread.ManagedThreadId}");
            TDebug.Msg($"is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}");

            for (int i = 0; i < seconds; i++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
                if (token.IsCancellationRequested) return -1;
            }
            return 42 * seconds;
        }
        private static int TaskMethod(string name, int seconds)
        {
            TDebug.Msg($"Task {name} is running on a thread id {Thread.CurrentThread.ManagedThreadId}");
            TDebug.Msg($"is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}");

            Thread.Sleep(TimeSpan.FromSeconds(seconds));
            return 42 * seconds;
        }

        private static Task<int> CreateTask(string name)
        {
            return new Task<int>(() => TaskMethod2(name));
        }
        private static int TaskMethod2(string name)
        {
            TDebug.Msg($"Task {name} is running on a thread id {Thread.CurrentThread.ManagedThreadId}");
            TDebug.Msg($"is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}");

            Thread.Sleep(TimeSpan.FromSeconds(2));
            return 42;
        }

        private static void TaskMethod(string name)
        {
            TDebug.Msg($"Task {name} is running on a thread id {Thread.CurrentThread.ManagedThreadId}");
            TDebug.Msg($"is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}");
        }
    }
}
