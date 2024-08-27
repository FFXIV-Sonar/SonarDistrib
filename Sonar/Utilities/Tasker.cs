using AG.Collections.Concurrent;
using SonarUtils.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Sonar.Utilities
{
    // License for Tasker: MIT, feel free to use

    /// <summary>
    /// Assist in running asynchronous tasks and ensure they finish properly when disposed
    /// </summary>
    public sealed class Tasker : IDisposable, IAsyncDisposable ,IEnumerable<Task>
    {
        private readonly ConcurrentTrieSet<Task> _tasks = new();

        #region User functions
        /// <summary>
        /// Adds a task to the tasks list, with some checks within it
        /// </summary>
        /// <param name="task">Task to add</param>
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
                if (task.IsCompletedSuccessfully) self.InvokeEvent(self.TaskSuccessful, task);
                if (task.IsFaulted) self.InvokeEvent(self.TaskFaulted, task);
                if (task.IsCanceled) self.InvokeEvent(self.TaskCanceled, task);
                self.InvokeEvent(self.TaskComplete, task);
            }, this, TaskScheduler.Default);

            this.InvokeEvent(this.TaskAdded, task);
            return true;
        }

        /// <summary>
        /// Removes a task from the task list (will not wait when disposing)
        /// </summary>
        /// <param name="task">Task to remove</param>
        public bool RemoveTask(Task task)
        {
            // Check if it exists and remove task
            if (!this._tasks.Remove(task)) return false;
            this.InvokeEvent(this.TaskRemoved, task);
            return true;
        }

        /// <summary>
        /// Number of tasks this TaskerService is currently holding
        /// </summary>
        public int Count => this._tasks.Count;

        /// <summary>
        /// Tasks tracker by this <see cref="Tasker"/>
        /// </summary>
        public IEnumerable<Task> Tasks => this._tasks;

        /// <summary>
        /// Resets the task list (DANGEROUS)
        /// </summary>
        public void Clear() => this._tasks.Clear();
        #endregion

        #region Event Handlers
        /// <summary>
        /// Fired when a task is added
        /// </summary>
        public event Action<Tasker, Task>? TaskAdded;
        /// <summary>
        /// Fired when a task is removed
        /// </summary>
        public event Action<Tasker, Task>? TaskRemoved;
        /// <summary>
        /// Fired when a task is complete
        /// </summary>
        public event Action<Tasker, Task>? TaskComplete;
        /// <summary>
        /// Fired when a task is successful
        /// </summary>
        public event Action<Tasker, Task>? TaskSuccessful;
        /// <summary>
        /// Fired when an exception occurs in a task
        /// </summary>
        public event Action<Tasker, Task>? TaskFaulted;
        /// <summary>
        /// Fired when a task is cancelled
        /// </summary>
        public event Action<Tasker, Task>? TaskCanceled;

        /// <summary>
        /// Fired when an exception happen during event handling
        /// </summary>
        public event Action<Tasker, Task, Exception>? OnEventException;

        private void InvokeEvent(Action<Tasker, Task>? handler, Task task)
        {
            if (handler is null) return;
            foreach (var d in Unsafe.As<Action<Tasker, Task>[]>(handler.GetInvocationList()))
            {
                try
                {
                    d.Invoke(this, task);
                }
                catch (Exception ex)
                {
                    this.OnEventException?.Invoke(this, task, ex);
                }
            }
        }

        #endregion

        #region Disposable Pattern
        /// <summary>Await all tasks</summary>
        public async ValueTask DisposeAsync()
        {
            var tasks = this._tasks.ToArray();
            try { await Task.WhenAll(tasks); } catch { /* Swallow */ }
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
