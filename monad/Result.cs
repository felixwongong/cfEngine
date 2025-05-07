using System.Runtime.CompilerServices;

namespace cfEngine.Monad
{
    public struct Result<T, E>
    {
        public T value { get; private set; }
        public E errorValue { get; private set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<T, E> Ok(T value)
        {
            return new Result<T, E> { value = value };
        }

        public static Result<T, E> Err(E errorValue)
        {
            return new Result<T, E> { errorValue = errorValue };
        }
    }
}