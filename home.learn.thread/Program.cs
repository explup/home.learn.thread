using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace home.learn.thread
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            args = new string[] { "-Example", "SimulateConcurrentDic" };
         
#endif
            Thread.CurrentThread.Name = "MainThread";

            var dic = CreateDic();

            if (args == null)
            {
                Console.WriteLine("args is null");
            }
            else
            {
                if(args.Length != 2)
                {
                    Console.WriteLine("Please give argument '-Example SimulateLock'");
                }
               else if(args[0] != "-Example" || string.IsNullOrWhiteSpace(args[1]))
                {
                    Console.WriteLine("Please give argument '-Example SimulateLock'");
                }else if (!dic.Keys.Contains(args[1])){
                    Console.WriteLine("Please give example as following name: " + dic.Keys.Aggregate((i,j) => { return i + "," + j; }));

                }
                else
                {
                    //execute example
                    dic[args[1]]();
                }
            }
            
            Console.ReadLine();
        }
       
        private static Dictionary<string, Action> CreateDic()
        {
            Dictionary<string, Action> dic = new Dictionary<string, Action>();

            AddAction(dic, ThreadBasicExamples.JoinAbortExample);

            AddAction(dic, ThreadSafeExamples.SimulateLock);
            AddAction(dic, ThreadSafeExamples.SimulateNoLock);
            AddAction(dic, ThreadSafeExamples.SimulateDeadLock);
            AddAction(dic, ThreadSafeExamples.SimulateMutex);
            AddAction(dic, ThreadSafeExamples.SimulateSemaphore);
            AddAction(dic, ThreadSafeExamples.SimulateReadWriteLock);

            AddAction(dic, ThreadControlExamples.SimulateAutoResetEvent);
            AddAction(dic, ThreadControlExamples.SimulateManualResetevent);
            AddAction(dic, ThreadControlExamples.SimulateBarrier);
            AddAction(dic, ThreadControlExamples.Countdown);

            AddAction(dic, ThreadPoolExamples.SimulateAsynchronousProgrammingModel);
            AddAction(dic, ThreadPoolExamples.SimulateCancelThread);
            AddAction(dic, ThreadPoolExamples.CompareThreadsAndPool);
            AddAction(dic, ThreadPoolExamples.AsyncOperationInThreadPool);
            AddAction(dic, ThreadPoolExamples.SimulateTimer);
            AddAction(dic, ThreadPoolExamples.SimulateEventBasedAsynchronousPattern);


            AddAction(dic, TaskExamples.TaskBasicOperations);
            AddAction(dic, TaskExamples.TaskCancel);
            AddAction(dic, TaskExamples.SimulateTaskWithException);
            AddAction(dic, TaskExamples.SimulateComplexTasks);

            AddAction(dic, AwaitExamples.SimulateAsyncWithAwait);
            AddAction(dic, AwaitExamples.SimulateAsyncLambda);
            AddAction(dic, AwaitExamples.SimulateAsyncWithTwoAwait);

            AddAction(dic, ConcurrentExamples.SimulateConcurrentDic);

            return dic;
        }
      
        private static void AddAction(Dictionary<string, Action> dic, Action action)
        {
            if (dic == null)
            {
                throw new ArgumentNullException();
            }

            dic.Add(action.Method.Name, action);
        }
      


       

    

       
    }
}
