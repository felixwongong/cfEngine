using System;
using System.Threading;
using System.Threading.Tasks;

namespace cfEngine.Extension
{
    public static class TaskExtension
    {
        public static Task ContinueWithSynchronized(this Task t, Action<Task> action, CancellationToken token = default)
        {
            return t.ContinueWith(action, token, TaskContinuationOptions.None,
                TaskScheduler.FromCurrentSynchronizationContext());
        }

        public static Task ContinueWithSynchronized<T>(this Task<T> t, Action<Task<T>> action,
            CancellationToken token = default)
        {
            return t.ContinueWith(action, token, TaskContinuationOptions.None,
                TaskScheduler.FromCurrentSynchronizationContext());
        }

        public static Task<T> ContinueWithSynchronized<T>(this Task<T> t, Func<Task<T>, T> action,
            CancellationToken token = default)
        {
            return t.ContinueWith(action, token, TaskContinuationOptions.None,
                TaskScheduler.FromCurrentSynchronizationContext());
        }

        public static void DisposeIfCompleted(this Task t)
        {
            if (t.IsCompleted)
            {
                t.Dispose();
            }
        }
    }
}