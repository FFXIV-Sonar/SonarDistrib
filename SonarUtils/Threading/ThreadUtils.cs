using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SonarUtils.Threading
{
    public static class ThreadUtils
    {
        public static void RunAsThreads(bool join, params ReadOnlySpan<Action> actions)
        {
            var threads = join ? new Thread[actions.Length] : default;
            for (var index = 0; index < actions.Length; index++)
            {
                var thread = new Thread(action => Unsafe.As<Action>(action!).Invoke());
                thread.Start(actions[index]);
                if (join) threads![index] = thread;
            }
            if (join)
            {
                for (var index = 0; index < threads!.Length; index++) threads[index].Join();
            }
        }

        public static void RunAsThreads(params ReadOnlySpan<Action> actions) => RunAsThreads(true, actions);

        public static Task RunAsThreadsAsync(ReadOnlySpan<Action> actions, CancellationToken cancellationToken = default)
        {
            var tasks = new Task[actions.Length];
            for (var index = 0; index < actions.Length; index++)
            {
                // Use LongRunning tasks instead of threads
                tasks[index] = Task.Factory.StartNew(actions[index], cancellationToken, TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
            }
            return Task.WhenAll(tasks);
        }

        public static Task RunAsThreadsAsync(params ReadOnlySpan<Action> actions) => RunAsThreadsAsync(actions, CancellationToken.None);
    }
}
