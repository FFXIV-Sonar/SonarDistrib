using AG.Collections.Concurrent;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SonarUtils.Threading
{
    /// <summary>Assist in running asynchronous tasks and ensure they finish properly when disposed.</summary>
    public sealed class Tasker : IDisposable, IAsyncDisposable, IEnumerable<Task>
    {
        private readonly ConcurrentTrieSet<Task> _tasks = [];

        #region User functions
        /// <summary>Adds a task to the tasks list, with some checks within it.</summary>
        /// <param name="task">Task to add.</param>
        public bool AddTask(Task task)
        {
            // Make sure task doesn't already exist
            if (!this._tasks.Add(task)) return false;

            // Adds exception handling
            task.ContinueWith(static (task, obj) =>
            {
                var self = Unsafe.As<Tasker>(obj)!;
                if (!self._tasks.Remove(task))
                {
                    if (task.Exception is not null) throw task.Exception!;
                    return;
                }
                if (task.IsCompletedSuccessfully) self.InvokeEvent(self.Success, task);
                if (task.IsFaulted) self.InvokeEvent(self.Faulted, task);
                if (task.IsCanceled) self.InvokeEvent(self.Canceled, task);
                self.InvokeEvent(self.Complete, task);
            }, this, TaskScheduler.Default);

            this.InvokeEvent(this.Added, task);
            return true;
        }

        /// <summary>Removes a task from the task list (will not wait when disposing).</summary>
        /// <param name="task">Task to remove.</param>
        public bool RemoveTask(Task task)
        {
            // Check if it exists and remove task
            if (!this._tasks.Remove(task)) return false;
            this.InvokeEvent(this.Removed, task);
            return true;
        }

        /// <summary>Number of tasks this TaskerService is currently holding.</summary>
        public int Count => this._tasks.Count;

        /// <summary>Tasks tracker by this <see cref="Tasker"/>.</summary>
        public IEnumerable<Task> Tasks => this._tasks;

        /// <summary>Resets the task list (DANGEROUS).</summary>
        public void Clear() => this._tasks.Clear();
        #endregion

        #region Event Handlers
        /// <summary>Fired when a task is added.</summary>
        public event Action<Tasker, Task>? Added;

        /// <summary>Fired when a task is removed.</summary>
        public event Action<Tasker, Task>? Removed;

        /// <summary>Fired when a task is complete.</summary>
        public event Action<Tasker, Task>? Complete;

        /// <summary>Fired when a task is successful.</summary>
        public event Action<Tasker, Task>? Success;

        /// <summary>Fired when an exception occurs in a task.</summary>
        public event Action<Tasker, Task>? Faulted;

        /// <summary>Fired when a task is cancelled.</summary>
        public event Action<Tasker, Task>? Canceled;

        /// <summary>Fired when an exception happen during event handling.</summary>
        /// <remarks>Exceptions thrown here are swallowed.</remarks>
        public event Action<Tasker, Task, Exception>? EventException;

        private void InvokeEvent(Action<Tasker, Task>? handler, Task task)
        {
            if (handler is null) return;
            foreach (var action in Delegate.EnumerateInvocationList(handler))
            {
                try
                {
                    action(this, task);
                }
                catch (Exception ex)
                {
                    try
                    {
                        this.EventException?.Invoke(this, task, ex);
                    }
                    catch (Exception ex2)
                    {
                        // Debug breakpoint dummy
                        GC.KeepAlive(ex2);
                    }
                }
            }
        }

        #endregion

        #region Disposable Pattern
        /// <summary>Await all tasks</summary>
        public async ValueTask DisposeAsync()
        {
            var tasks = this._tasks.ToArray();
            try { await Task.WhenAll(tasks).ConfigureAwait(false); } catch { /* Swallow */ }
        }

        /// <summary>Wait all tasks</summary>
        public void Dispose()
        {
            var tasks = this._tasks.ToArray();
            try { Task.WaitAll(tasks); } catch { /* Swallow */ }
        }
        #endregion

        #region IEnumerator implementation
        public IEnumerator<Task> GetEnumerator()
        {
            return this._tasks.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        #endregion
    }
}
