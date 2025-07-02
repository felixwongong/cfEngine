using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using cfEngine.Extension;

namespace cfEngine
{
    public static class Validation
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Validation<T> Success<T>(T value)
        {
            return new Success<T>(value);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Validation<T> Failure<T>(Exception exception)
        {
            return new Failure<T>(exception);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Validation<T> Pending<T>(T value)
        {
            return new Pending<T>(value);
        }

        public static Task<Validation<T>> VContinuedWith<T>(this Task<Validation<T>> task, Action<Validation<T>> continuation)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            if (continuation == null) throw new ArgumentNullException(nameof(continuation));

            return task.ContinueWithSynchronized(result =>
            {
                continuation(result.Result);
                return result.Result;
            });
        }
    }
}