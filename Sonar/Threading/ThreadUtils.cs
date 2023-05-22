using System;
using System.Threading;

namespace Sonar.Threading
{
    public static class ThreadUtils
    {
        public static void RunAsThreads(bool join, params Action[] actions)
        {
            var threads = join ? new Thread[actions.Length] : default;
            for (var index = 0; index < actions.Length; index++)
            {
                var thread = new Thread(index => actions[(int)index!].Invoke());
                thread.Start(index);
                if (join) threads![index] = thread;
            }
            for (var index = 0; index < actions.Length; index++) threads![index].Join();
        }
        public static void RunAsThreads(params Action[] actions) => RunAsThreads(true, actions);
    }
}
