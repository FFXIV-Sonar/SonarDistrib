using SonarUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Sonar.Threading
{
    public static class ThreadUtils
    {
        public static void RunAsThreads(bool join, IEnumerable<Action> actions)
        {
            if (join)
            {
                var threads = actions
                    .Select(action =>
                    {
                        var thread = new Thread(_ => action());
                        thread.Start();
                        return thread;
                    })
                    .ToArray();
                for (var index = 0; index < threads.Length; index++) threads![index]!.Join();
            }
            else
            {
                actions
                    .ForEach(action =>
                    {
                        var thread = new Thread(_ => action());
                        thread.Start();
                    });
            }
        }

        public static void RunAsThreads(bool join, params Action[] actions) // TODO: params ReadOnlySpan<Action> actions
        {
            var threads = join ? new Thread[actions.Length] : default;
            for (var index = 0; index < actions.Length; index++)
            {
                var thread = new Thread(index => actions[(int)index!].Invoke());
                thread.Start(index);
                if (join) threads![index] = thread;
            }
            if (join)
            {
                for (var index = 0; index < threads!.Length; index++) threads[index].Join();
            }
        }

        public static void RunAsThreads(params Action[] actions) => RunAsThreads(true, actions); // TODO: params ReadOnlySpan<Action> actions
    }
}
