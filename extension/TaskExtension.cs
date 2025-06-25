using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using cfEngine.Logging;

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
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task LogIfFaulted(this Task t)
        {
            return t.ContinueWith(result =>
            {
                if (result.IsFaulted)
                {
                    Log.LogException(result.Exception);
                }
            });
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<T> LogIfFaulted<T>(this Task<T> t)
        {
            return t.ContinueWith(result =>
            {
                if (result.IsFaulted)
                {
                    Log.LogException(result.Exception);
                }
                return result.Result;
            });
        }
    }
}