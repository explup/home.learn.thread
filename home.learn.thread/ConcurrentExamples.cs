using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace home.learn.thread
{
    static class ConcurrentExamples
    {
        public static void SimulateConcurrentDic()
        {
            string item = "item";
            int iterations = 10000000;
            string currentItem;

            var concurrentDic = new ConcurrentDictionary<int, string>();
            var dic = new Dictionary<int, string>();

            var sw = new Stopwatch();

            sw.Start();
            for (int i = 0; i < iterations; i++)
            {
                lock (dic)
                {
                    dic[i] = item;
                }
            }

            sw.Stop();

            TDebug.Msg($"writing to dictionary with a lock: {sw.Elapsed}");
            sw.Restart();

            for (int i = 0; i < iterations; i++)
            {
                concurrentDic[i] = item;
            }
            sw.Stop();

            TDebug.Msg($"writing to concurrent dictionary: {sw.Elapsed}");

            sw.Restart();

            for (int i = 0; i < iterations; i++)
            {
                lock (dic)
                {
                   currentItem = dic[i];
                }
            }

            sw.Stop();

            TDebug.Msg($"reading from a dictionary with a lock: {sw.Elapsed}");

            sw.Restart();

            for (int i = 0; i < iterations; i++)
            {
                currentItem = dic[i];
            }
            sw.Stop();

            TDebug.Msg($"reading from a concurrent dictionary: {sw.Elapsed}");

        }
    }
}
