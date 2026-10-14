using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SonarUtils
{
    public static class SemaphoreSlimExtensions
    {
        public static void ReleaseSwallowDisposeException(this SemaphoreSlim semaphore)
        {
            try { semaphore.Release(); } catch (ObjectDisposedException) { /* Swallow */ }
        }

        public static void ReleaseSwallowDisposeException(this SemaphoreSlim semaphore, int releaseCount)
        {
            try { semaphore.Release(releaseCount); } catch (ObjectDisposedException) { /* Swallow */ }
        }
    }
}
