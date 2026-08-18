using System.Threading;
using System.Threading.Tasks;

namespace SonarUtils
{
    public static class TaskExtensions
    {
        public static Task WithExceptionObserved(this Task task)
        {
            task.ObserveException();
            return task;
        }

        public static Task<T> WithExceptionObserved<T>(this Task<T> task)
        {
            task.ObserveException();
            return task;
        }

        public static Task WithExceptionObserved(this ValueTask vtask)
        {
            var task = vtask.AsTask();
            task.ObserveException();
            return task;
        }

        public static Task<T> WithExceptionObserved<T>(this ValueTask<T> vtask)
        {
            var task = vtask.AsTask();
            task.ObserveException();
            return task;
        }

        public static void ObserveException(this Task task) => task.ContinueWith(static task => task.Exception, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default);
    }
}
