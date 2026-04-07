using System;
using System.Collections.Generic;

namespace SonarUtils.Collections
{
    public static class QueueExtensions
    {
        public static void EnqueueRange<T>(this Queue<T> queue, IEnumerable<T> items)
        {
            foreach (var item in items) queue.Enqueue(item);
        }

        public static void EnqueueRange<T>(this Queue<T> queue, ReadOnlySpan<T> items)
        {
            foreach (var item in items) queue.Enqueue(item);
        }
    }
}
