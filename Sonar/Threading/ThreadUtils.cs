using Loyc.Collections;
using Loyc.Collections.Impl;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sonar.Threading
{
    public static class ThreadUtils
    {
        public static void RunAsThreads(bool join, params Action[] actions)
        {
            InternalList<Thread> threads = join ? new(actions.Length) : default;
            foreach (var action in actions)
            {
                var thread = new Thread(_ => action.Invoke());
                thread.Start();
                if (join) threads.Add(thread);
            }
            if (join) threads.ForEach(t => t.Join());
        }
        public static void RunAsThreads(params Action[] actions) => RunAsThreads(true, actions);
    }
}
